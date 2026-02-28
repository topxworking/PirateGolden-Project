using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SAVE_FILE = "pirate_save.json";
    private string SavePath => Path.Combine(Application.dataPath, SAVE_FILE);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Save(GameData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SAVE_FILE, json);
        Debug.Log($"Saved > {SavePath}");
    }

    public GameData Load()
    {
        if (!File.Exists(SAVE_FILE))
        {
            Debug.Log("No save file found.");
            return CreateDefaultData();
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("Save loaded successfully.");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load save: {e.Message}.");
            return CreateDefaultData();
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save file deleted.");
        }
    }

    private GameData CreateDefaultData()
    {
        return new GameData
        {
            totalCoins = 0,
            coinsPerClick = 1,
            coinsPerSecond = 0,
            totalCoinsEarned = 0,
            totalClicks = 0
        };
    }
}
