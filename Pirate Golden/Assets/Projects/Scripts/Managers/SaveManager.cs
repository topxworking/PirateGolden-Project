using System;
using System.IO;
using System.Text;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SAVE_FILE = "pirate_save.dat";
    private const string PREFS_HAS_SAVE = "has_save";
    private const string PREFS_SAVE_VERSION = "save_version";
    private const string PREFS_LAST_SAVED = "last_saved";
    private const int CURRENT_VERSION = 1;
    private const string ENCRYPT_KEY = "PirateSecretKey!";

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
        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: false);
            byte[] encrypted = Encrypt(json);
            File.WriteAllBytes(_savePath, encrypted);

            PlayerPrefs.SetInt(PREFS_HAS_SAVE, 1);
            PlayerPrefs.SetInt(PREFS_SAVE_VERSION, CURRENT_VERSION);
            PlayerPrefs.SetString(PREFS_LAST_SAVED, DateTime.UtcNow.ToString("o"));
            PlayerPrefs.Save();

            Debug.Log("Game saved successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public GameData Load()
    {
        bool hasSave = PlayerPrefs.GetInt(PREFS_HAS_SAVE, 0) == 1;

        if (!hasSave || !File.Exists(_savePath))
        {
            Debug.Log("No save found. Creating default data.");
            return CreateDefaultData();
        }

        try
        {
            byte[] encrypted = File.ReadAllBytes(_savePath);
            string json = Decrypt(encrypted);
            GameData data = JsonUtility.FromJson<GameData>(json);

            if (data == null) throw new Exception("Decrypted data is null");

            string lastSaved = PlayerPrefs.GetString(PREFS_LAST_SAVED, "Unknown");
            Debug.Log($"Save loaded. Version: {PlayerPrefs.GetInt(PREFS_SAVE_VERSION, 0)}, Last saved: {lastSaved}");
            return data;
        }
        catch
        {
            try
            {
                string json = File.ReadAllText(_savePath);
                GameData data = JsonUtility.FromJson<GameData>(json);
                if (data != null)
                {
                    Debug.Log("Loaded legacy plain JSON save. Re-saving as encrypted.");
                    Save(data);
                    return data;
                }
            }
            catch { }

            Debug.LogWarning("Corrupt save. Returning default.");
            return CreateDefaultData();
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);
            Debug.Log("Save .dat file deleted.");
        }

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("Save data fully cleared.");
    }

    public bool HasSave()
    {
        return PlayerPrefs.GetInt(PREFS_HAS_SAVE, 0) == 1 && File.Exists(_savePath);
    }

    public string GetLastSavedTime()
    {
        return PlayerPrefs.GetString(PREFS_LAST_SAVED, "Never");
    }

    private byte[] Encrypt(string plainText)
    {
        byte[] data = Encoding.UTF8.GetBytes(plainText);
        byte[] key = Encoding.UTF8.GetBytes(ENCRYPT_KEY);
        for (int i = 0; i < data.Length; i++)
            data[i] ^= key[i % key.Length];
        return data;
    }

    private string Decrypt(byte[] encryptedData)
    {
        byte[] key = Encoding.UTF8.GetBytes(ENCRYPT_KEY);
        byte[] data = new byte[encryptedData.Length];
        for (int i = 0; i < encryptedData.Length; i++)
            data[i] = (byte)(encryptedData[i] ^ key[i % key.Length]);
        return Encoding.UTF8.GetString(data);
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

    public void ResetSave()
    {
        DeleteSave();
        GameData defaultData = CreateDefaultData();
        Save(defaultData);
    }
}