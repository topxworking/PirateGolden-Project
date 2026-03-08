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

    private RectTransform _rt;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    public void Play(string text, Vector2 position)
    {
        if (label) label.text = text;
        _rt.anchoredPosition = position;
        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        Vector2 startPos = _rt.anchoredPosition;

        if (canvasGroup) canvasGroup.alpha = 1f;
        _rt.localScale = Vector3.zero;

        while (elapsed < lifttime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifttime;

            _rt.anchoredPosition = startPos + Vector2.up * riseSpeed * elapsed;

            if (canvasGroup)
                canvasGroup.alpha = 1f - Mathf.Clamp01((t - 0.5f) * 2f);

            float scale = t < 0.1f ? Mathf.Lerp(0f, 1.2f, t / 0.1f)
                : t < 0.2f ? Mathf.Lerp(1.2f, 1f, (t - 0.1f) / 0.1f)
                : 1f;
            _rt.localScale = Vector3.one * scale;

            yield return null;
        }

        FloatingTextPool.Instance?.Return(this);
    }
}
