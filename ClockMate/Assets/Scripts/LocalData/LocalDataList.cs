using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalDataList<T> where T : LocalDataBase
{
    public List<T> DataList { get; private set; }
    private Dictionary<int, T> dictDataList;
    public int Count => DataList.Count;

    public T this[int _index] => dictDataList[_index];

    /// <summary>
    /// 초기화 함수. 값을 대입한다.
    /// </summary>
    public void Init(List<T> dataList)
    {
        DataList = dataList;
        dictDataList = new Dictionary<int, T>();
        foreach (T data in DataList)
        {
            dictDataList.Add(data.ID, data);
        }
    }
}