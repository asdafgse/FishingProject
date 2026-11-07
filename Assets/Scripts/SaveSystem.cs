using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath =>
        Path.Combine(
            Application.persistentDataPath,
            "meow_and_reel_save.json"
        );

    public static bool Save(SaveData data)
    {
        if (data == null)
        {
            return false;
        }

        try
        {
            Directory.CreateDirectory(
                Application.persistentDataPath
            );

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);

            Debug.Log("기록 저장 완료!");
            return true;
        }
        catch (Exception error)
        {
            Debug.LogError($"저장 실패: {error.Message}");
            return false;
        }
    }

    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            return new SaveData();
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            SaveData data =
                JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                return new SaveData();
            }

            if (float.IsNaN(data.bestCatchWeight) ||
                float.IsInfinity(data.bestCatchWeight) ||
                data.bestCatchWeight < 0f)
            {
                data.bestCatchWeight = 0f;
            }

            Debug.Log("기록 불러오기 완료!");
            return data;
        }
        catch (Exception error)
        {
            Debug.LogError(
                $"불러오기 실패: {error.Message}"
            );

            return new SaveData();
        }
    }
}