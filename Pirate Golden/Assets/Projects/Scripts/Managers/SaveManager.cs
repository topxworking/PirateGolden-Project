using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SAVE_FILE = "pirate_save.json";

    private string _savePath;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE);
        Debug.Log($"Save path: {_savePath}");
    }

    public void Save(GameData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(_savePath, json);
        Debug.Log("Saved");
    }

    public GameData Load()
    {
        if (!File.Exists(_savePath))
        {
            Debug.Log("No save file found.");
            return CreateDefaultData();
        }

        try
        {
            string json = File.ReadAllText(_savePath);
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
        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);
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
