using UnityEngine;
using UnityEngine.EventSystems;

public class ShipClickHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Feedback")]
    [SerializeField] private Animator shipAnimator;
    [SerializeField] private string clickAnimTrigger = "Click";
    [SerializeField] private ParticleSystem splashParticle;

    [Header("Scale Punch")]
    [SerializeField] private bool usePunchScale = true;
    [SerializeField] private float punchAmount = 0.13f;
    [SerializeField] private float punchSpeed = 12f;

    private Vector3 _baseScale;
    private bool _punching;
    private float _punchT;

    private void Start() => _baseScale = transform.localScale;

    private void Update()
    {
        if (!_punching || !usePunchScale) return;

        _punchT += Time.deltaTime * punchSpeed;
        float t = Mathf.Sin(_punchT * Mathf.PI);
        transform.localScale = _baseScale * (1f + punchAmount * t);

        if (_punchT >= 1f)
        {
            _punchT = 0f;
            _punching = false;
            transform.localScale = _baseScale;
        }
    }

    public void OnShipClick()
    {
        GameManager.Instance?.OnShipClicked();
        PlayFeedback();
    }

    public void OnPointerClick(PointerEventData _) => OnShipClick();

    public void OnPointerDown(PointerEventData _)
        => transform.localScale = _baseScale * 0.92f;

    public void OnPointerUp(PointerEventData _)
        => transform.localScale = _baseScale;

    private void PlayFeedback()
    {
        if (usePunchScale) { _punching = true; _punchT = 0f; }
        if (shipAnimator && !string.IsNullOrEmpty(clickAnimTrigger))
            shipAnimator.SetTrigger(clickAnimTrigger);
        if (splashParticle) splashParticle.Play();
    }
}
