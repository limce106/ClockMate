using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class LocalDataManager : Singleton<LocalDataManager>
{
    private const string RelativeLocalDataFolder = "/Resources/LocalData";
    private readonly string AbsoluteLocalDataFolder = Application.dataPath + RelativeLocalDataFolder;

    public LocalDataList<LDStage> Stage { get; private set; }

    public LocalDataManager()
    {
        Stage = new LocalDataList<LDStage>();
        
        LoadAll();
    }
    
    /// <summary>
    /// 로컬 데이터 폴더 하위에 있는 모든 데이터를 로드한다.
    /// </summary>
    public void LoadAll()
    {
        if (Directory.Exists(AbsoluteLocalDataFolder) == false)
        {
            Directory.CreateDirectory(AbsoluteLocalDataFolder);
        }

        // 로컬 경로 내에 있는 모든 폴더들 가져오기
        DirectoryInfo[] directories = new DirectoryInfo(AbsoluteLocalDataFolder).GetDirectories();

        foreach (DirectoryInfo directory in directories)
        {
            Load(directory);
        }
    }

    /// <summary>
    /// 로컬 데이터를 로드한다
    /// </summary>
    private void Load(DirectoryInfo directoryInfo)
    {
        // 폴더 이름
        string folderName = directoryInfo.Name;
        // 파싱할 대상의 클래스 타입: LD + 폴더 이름
        Type classType = Type.GetType($"LD{folderName}");
        // 파싱한 데이터를 저장할 타깃 프로퍼티 
        PropertyInfo targetProperty = GetType().GetProperty(folderName);
        // 타깃 프로퍼티에 값을 세팅할 함수
        MethodInfo targetInitMethod = targetProperty.GetValue(this).GetType().GetMethod("Init");
        
        FileInfo[] files = directoryInfo.GetFiles();
        FileInfo csvFileInfo = null;
        
        // 폴더 내에서 csv 파일을 찾아낸다
        foreach (var file in files)
        {
            if (IsCsv(file))
            {
                csvFileInfo = file;
                break;
            }
        }

        // 폴더 내에 파일이 없는경우
        if (csvFileInfo == null)
        {
            Debug.LogError($"파일이 존재하지 않습니다. {folderName}");
            return;
        }

        // csv파일을 라인 별로 나눠 저장한다
        StreamReader sr = new StreamReader(csvFileInfo.FullName);
        var lines = sr.ReadToEnd().Split(Environment.NewLine);
        sr.Close();

        // 첫 번째 라인에는 어떤 프로퍼티에 저장할지 이름이 명시되어있다.
        string[] propertyNames = lines[0].Split(",");
        List<PropertyInfo> propertyTypes = GetPropertyTypeList(classType, lines[0]);

        // 파싱으로 생성될 모든 데이터가 저장될 리스트
        object dataList = Activator.CreateInstance(typeof(List<>).MakeGenericType(classType));
        // 위의 리스트에 데이터 추가하는 함수
        MethodInfo dataListAddMethod = dataList.GetType().GetMethod("Add");

        // 두 번째 라인부터 파싱을 시작
        for (int i = 1; i < lines.Length; ++i)
        {
            // 데이터 객체 생성
            object newData = Activator.CreateInstance(classType);
            
            string[] lineList = lines[i].Split(",");
            for(int j = 0; j < lineList.Length; ++j)
            {
                Type currentType = propertyTypes[j].PropertyType;
                PropertyInfo currentProperty = newData.GetType().GetProperty(propertyNames[j]);
                
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    // 리스트인 경우
                    
                    // 리스트 객체를 생성
                    object newList = Activator.CreateInstance(currentProperty.PropertyType);
                    Type genericType = currentType.GetGenericArguments()[0];
                    
                    // 데이터를 |를 기준으로 나눈다 (콤마는 이미 사용중이므로 가장 덜 사용될만한 문자를 선정함)
                    string[] listData = lineList[j].Split("|");
                    // 리스트에 추가하는 함수
                    MethodInfo dataAddMethod = newList.GetType().GetMethod("Add");
                    
                    foreach (string data in listData)
                    {
                        dataAddMethod.Invoke(newList, new object[] { Convert.ChangeType(data, genericType) });
                    }
                    
                    // 프로퍼티에 리스트를 넣어준다.
                    currentProperty.SetValue(newData, newList);
                }
                else if (currentType.IsEnum)
                {
                    // 열거형인 경우
                    currentProperty.SetValue(newData, Enum.Parse(currentType, lineList[j]));
                }
                else
                {
                    // 그 외 타입인경우
                    currentProperty.SetValue(newData, Convert.ChangeType(lineList[j], currentType));
                }
            }

            // 파싱된 데이터를 집어넣는다.
            dataListAddMethod.Invoke(dataList, new object[] { newData });
        }
        
        // 파싱한 모든 데이터를 최종 리스트에 넣는다.
        targetInitMethod.Invoke(targetProperty.GetValue(this), new object[] { dataList });
    }

    private bool IsCsv(FileInfo _fileInfo)
    {
        return _fileInfo.Extension == ".csv";
    }

    /// <summary>
    /// 문자열을 읽고 해당하는 프로퍼티 정보를 리스트에 담아 반환한다.
    /// </summary>
    private List<PropertyInfo> GetPropertyTypeList(Type classType, string firstLine)
    {
        string[] firstLineNames = firstLine.Split(",");
        List<PropertyInfo> result = new List<PropertyInfo>(firstLineNames.Length);

        foreach (string name in firstLineNames)
        {
            PropertyInfo propertyType = classType.GetProperty(name);
            if (propertyType == null)
            {
                Debug.LogError("해당 타입은 존재하지 않습니다.");
                return null;
            }
            result.Add(propertyType);
        }
        
        return result;
    }
}