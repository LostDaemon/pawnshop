using UnityEngine;
using Zenject;

public class SceneUIAnchorController : MonoBehaviour
{
    private DiContainer _container;
    [SerializeField] private GameObject _uiAnchorPrefab;
    [SerializeField] private Canvas _uiCanvas;
    [SerializeField] private Transform _parent;
    private GameObject _uiObjectInstance;

    [Inject]
    public void Construct(DiContainer container)
    {
        _container = container;
    }

    private void Awake()
    {
        _uiObjectInstance = _container.InstantiatePrefab(_uiAnchorPrefab, _parent);
    }

    public void LateUpdate()
    {
        //TODO: Ensure screen was moved
        PositionChildren();
    }

    private void PositionChildren()
    {
        if (_uiCanvas == null || _uiObjectInstance == null)
            return;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _uiCanvas.transform as RectTransform,
            screenPos,
            _uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _uiCanvas.worldCamera,
            out Vector2 localPos);

        _uiObjectInstance.GetComponent<RectTransform>().localPosition = localPos;
    }
}