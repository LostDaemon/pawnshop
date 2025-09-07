using UnityEngine;

namespace PawnShop.Controllers
{
    public class WorldspaceToCanvasProjectionController : MonoBehaviour
    {
        [SerializeField] private Transform _worldSpaceObject;
        
        private Canvas _targetCanvas;
        private Camera _projectionCamera;
        private RectTransform _canvasRectTransform;
        private RectTransform _thisRectTransform;
        
        private void Awake()
        {
            _projectionCamera = Camera.main;
            _targetCanvas = GetComponentInParent<Canvas>();
            _canvasRectTransform = _targetCanvas?.GetComponent<RectTransform>();
            _thisRectTransform = GetComponent<RectTransform>();
        }
        
        private void LateUpdate()
        {
            UpdateProjection();
        }
        
        public void SetWorldSpaceObject(Transform worldSpaceObject)
        {
            _worldSpaceObject = worldSpaceObject;
        }
        
        private void UpdateProjection()
        {
            if (_worldSpaceObject == null || _canvasRectTransform == null || _thisRectTransform == null)
                return;
                
            // Convert world position to screen position
            Vector2 screenPosition = _projectionCamera.WorldToScreenPoint(_worldSpaceObject.position);
            
            // Convert screen position to canvas local position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTransform,
                screenPosition,
                _targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _projectionCamera,
                out Vector2 localPosition);
                
            _thisRectTransform.localPosition = localPosition;
        }
    }
}
