using System.Collections.Generic;
using UnityEngine;

public class FloatingTextPool : MonoBehaviour
{
    public static FloatingTextPool Instance { get; private set; }

    [SerializeField] private FloatingTextUI prefab;
    [SerializeField] private RectTransform parent;
    [SerializeField] protected int poolSize = 10;

    private readonly Queue<FloatingTextUI> _pool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public FloatingTextUI Get()
    {
        if (_pool.Count == 0)
        {
            var extra = Instantiate(prefab, parent);
            extra.gameObject.SetActive(false);
            return extra;
        }
        var obj = _pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(FloatingTextUI obj)
    {
        obj.gameObject.SetActive(false);
        obj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        var cg = obj.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 1f;
        _pool.Enqueue(obj);
    }
}
