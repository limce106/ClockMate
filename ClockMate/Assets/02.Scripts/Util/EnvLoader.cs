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
            Debug.LogWarning("'.env' ������ �������� �ʽ��ϴ�. ȯ�� ������ �ε���� �ʽ��ϴ�.");
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
