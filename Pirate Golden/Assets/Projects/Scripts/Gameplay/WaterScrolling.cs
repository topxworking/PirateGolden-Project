using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WaterScrolling : MonoBehaviour
{
    [Header("Scroll Speed")]
    [SerializeField] private float speedX = 0.04f;
    [SerializeField] private float speedY = 0.0f;

    private Material _mat;
    private Vector2 _offset;

    private void Start() => _mat = GetComponent<Renderer>().material;

    private void Update()
    {
        _offset.x += speedX * Time.deltaTime;
        _offset.y += speedY * Time.deltaTime;
        _mat.SetTextureOffset("_MainTex", _offset);
    }
}
