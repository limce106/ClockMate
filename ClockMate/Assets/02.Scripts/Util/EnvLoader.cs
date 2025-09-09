using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class EnvLoader
{
    private static Dictionary<string, string> _envVars = new Dictionary<string, string>();
    private static bool _isLoaded = false;

    public static void LoadEnv()
    {
        if (_isLoaded) return;

        string envPath = Path.Combine(Application.dataPath, "../.env");

        if (!File.Exists(envPath))
        {
            Debug.LogWarning("'.env' 파일이 존재하지 않습니다. 환경 변수가 로드되지 않습니다.");
            return;
        }

        foreach (var line in File.ReadAllLines(envPath))
        {
            var parts = line.Split('=', System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) continue;

            string key = parts[0].Trim();
            string value = parts[1].Trim().Trim('"');

            _envVars[key] = value;
        }
        _isLoaded = true;
    }

    public static string GetEnv(string key)
    {
        LoadEnv();
        if (_envVars.ContainsKey(key))
        {
            return _envVars[key];
        }
        Debug.LogError($"Environment variable '{key}' not found.");
        return null;
    }
}
