using UnityEngine;

public class ShipAnimator : MonoBehaviour
{
    [Header("Bobbing")]
    [SerializeField] private bool enableBob = true;
    [SerializeField] private float bobHeight = 0.18f;
    [SerializeField] private float bobSpeed = 1.4f;

    [Header("Sway")]
    [SerializeField] private bool enableSway = true;
    [SerializeField] private float swayAngle = 4f;
    [SerializeField] private float swaySpeed = 1.1f;

    [Header("Phase Offet")]
    [SerializeField] private float phaseOffset = 0f;

    private Vector3 _startPos;
    private float _startRotZ;

    private void Start()
    {
        _startPos = transform.localPosition;
        _startRotZ = transform.localEulerAngles.z;
    }

    private void Update()
    {
        float t = Time.time + phaseOffset;

        if (enableBob)
        {
            float y = _startPos.y + Mathf.Sin(t * bobSpeed) * bobHeight;
            transform.localPosition = new Vector3(_startPos.x, y, _startPos.z);
        }

        if (enableSway)
        {
            float rot = _startRotZ + Mathf.Sin(t * swaySpeed) * swayAngle;
            transform.localEulerAngles = new Vector3(0f, 0f, rot);
        }
    }
}
