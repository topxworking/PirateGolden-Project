using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private float riseSpeed = 90f;
    [SerializeField] private float lifttime = 1.1f;

    public void Play(string text)
    {
        if (label) label.text = text;
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        var rt = GetComponent<RectTransform>();
        float elapsed = 0f;

        while (elapsed < lifttime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifttime;

            rt.anchoredPosition += Vector2.up * riseSpeed * Time.deltaTime;

            if (canvasGroup)
                canvasGroup.alpha = 1f - Mathf.Clamp01((t - 0.5f) * 2f);

            float scale = t < 0.1f ? Mathf.Lerp(0f, 1.2f, t / 0.1f)
                : t < 0.2f ? Mathf.Lerp(1.2f, 1f, (t - 0.1f) / 0.1f)
                : 1f;
            rt.localScale = Vector3.one * scale;

            yield return null;
        }

        Destroy(gameObject);
    }
}
