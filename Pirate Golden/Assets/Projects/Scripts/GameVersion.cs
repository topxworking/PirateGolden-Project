using TMPro;
using UnityEngine;

public class GameVersion : MonoBehaviour
{
    private TextMeshProUGUI _gameVersion;

    private void Start()
    {
        _gameVersion = GetComponent<TextMeshProUGUI>();
        if (_gameVersion != null)
        {
            _gameVersion.text = "Version " + Application.version;
        }
    }
}
