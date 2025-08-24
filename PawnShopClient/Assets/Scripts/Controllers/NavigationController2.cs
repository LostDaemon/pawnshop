using UnityEngine;
using PawnShop.Controllers;
using PawnShop.Services;
using Zenject;
using PawnShop.Models;

namespace PawnShop.Controllers
{
    public class NavigationController2 : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera _camera;
        [SerializeField] private CartController[] _navigationPoints;

        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _cameraZDistance = 10f;

        [Header("Size Animation")]
        [SerializeField] private float _originalSize = 5f;
        [SerializeField] private float _maxSize = 8f;
        [SerializeField] private float _sizeSpeed = 3f;
        [SerializeField] private float _transitionOverlap = 0.1f; // How much overlap between phases (0-1)

                private int _currentPointIndex = 0;
        private Vector3 _targetPosition;
        private Vector3 _startPosition;
        private bool _isAnimating = false;
        private int _currentPhase = 0; // 0 = size increase, 1 = move, 2 = size decrease
        private int _pointMovingAwayFrom = 0; // Point we're moving away from
        
        private NavigationService2 navigationService;

        [Inject]
        private void Construct(NavigationService2 navigation)
        {
            navigationService = navigation;
        }

        private void Start()
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_navigationPoints.Length > 0)
            {
                _targetPosition = GetPositionWithZDistance(_navigationPoints[0].transform.position);
                _camera.transform.position = _targetPosition;
                if (_camera.orthographic)
                    _camera.orthographicSize = _originalSize;
            }
        }

        private void Update()
        {
            if (!_isAnimating)
            {
                HandleInput();
            }

            UpdateAnimation();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.A))
                MoveToPreviousPoint();
            else if (Input.GetKeyDown(KeyCode.D))
                MoveToNextPoint();
        }

        private void MoveToPreviousPoint()
        {
            if (_navigationPoints.Length <= 1) return;

            _currentPointIndex--;
            if (_currentPointIndex < 0)
                _currentPointIndex = _navigationPoints.Length - 1;

            StartAnimation();
        }

        private void MoveToNextPoint()
        {
            if (_navigationPoints.Length <= 1) return;

            _currentPointIndex++;
            if (_currentPointIndex >= _navigationPoints.Length)
                _currentPointIndex = 0;

            StartAnimation();
        }

                private void StartAnimation()
        {
            _startPosition = _camera.transform.position;
            _targetPosition = GetPositionWithZDistance(_navigationPoints[_currentPointIndex].transform.position);
            _isAnimating = true;
            _currentPhase = 0; // Start with size increase
            
            // Store the point we're moving away from (current camera position)
            _pointMovingAwayFrom = GetCurrentPointIndexFromPosition(_startPosition);
            
            // Скрыть UI сразу при начале перехода от текущего вагона
            if (navigationService != null)
            {
                Debug.Log($"[NavigationController2] Starting transition. Hiding current UI");
                navigationService.SetCart(CartType.Undefined);
            }
        }

        private void UpdateAnimation()
        {
            if (!_isAnimating) return;

            switch (_currentPhase)
            {
                case 0: // Phase 1: Increase size (отдаление от вагона А)
                    if (_camera.orthographic && _camera.orthographicSize < _maxSize - 0.01f)
                    {
                        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _maxSize, _sizeSpeed * Time.deltaTime);

                        // Start moving camera when size is 90% complete
                        if (_camera.orthographic && _camera.orthographicSize >= _maxSize * (1f - _transitionOverlap))
                        {
                                                     // Hide point we're moving away from (вагон А)
                         if (_navigationPoints[_pointMovingAwayFrom] != null)
                         {
                             _navigationPoints[_pointMovingAwayFrom].Hide();
                         }
                            _currentPhase = 1;
                        }
                    }
                    else
                    {
                        if (_camera.orthographic)
                            _camera.orthographicSize = _maxSize;

                        // Hide point we're moving away from (вагон А) when size increase is complete
                        if (_navigationPoints[_pointMovingAwayFrom] != null)
                        {
                            _navigationPoints[_pointMovingAwayFrom].Hide();
                        }
                        _currentPhase = 1;
                    }
                    break;

                case 1: // Phase 2: Move camera
                    if (Vector3.Distance(_camera.transform.position, _targetPosition) > 0.01f)
                    {
                        _camera.transform.position = Vector3.Lerp(_camera.transform.position, _targetPosition, _moveSpeed * Time.deltaTime);

                        // Start decreasing size when movement is 90% complete
                        float totalDistance = Vector3.Distance(_startPosition, _targetPosition);
                        float currentDistance = Vector3.Distance(_camera.transform.position, _targetPosition);
                        float moveProgress = 1f - (currentDistance / totalDistance);

                        if (moveProgress >= 1f - _transitionOverlap)
                        {
                            _currentPhase = 2;

                                                         // Start showing target point (вагон B) when starting to approach
                             if (_navigationPoints[_currentPointIndex] != null)
                             {
                                 _navigationPoints[_currentPointIndex].Show();
                                 
                                 // Переключить тип вагона когда стенка становится прозрачной (показывает содержимое)
                                 if (navigationService != null)
                                 {
                                     CartType newCartType = (CartType)(_currentPointIndex + 1);
                                     Debug.Log($"[NavigationController2] Showing cart contents. Setting cart type: {newCartType}");
                                     navigationService.SetCart(newCartType);
                                 }
                             }
                        }
                    }
                    else
                    {
                        _camera.transform.position = _targetPosition;
                        _currentPhase = 2;
                    }
                    break;

                case 2: // Phase 3: Decrease size (приближение к вагону B)
                    if (_camera.orthographic && _camera.orthographicSize > _originalSize + 0.01f)
                    {
                        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _originalSize, _sizeSpeed * Time.deltaTime);

                        // Continue moving camera if it's not at target yet
                        if (Vector3.Distance(_camera.transform.position, _targetPosition) > 0.01f)
                        {
                            _camera.transform.position = Vector3.Lerp(_camera.transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
                        }

                        
                    }
                    else
                    {
                        if (_camera.orthographic)
                            _camera.orthographicSize = _originalSize;

                        // Ensure camera reaches final position
                        if (Vector3.Distance(_camera.transform.position, _targetPosition) > 0.01f)
                        {
                            _camera.transform.position = Vector3.Lerp(_camera.transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
                        }
                        else
                        {
                            _camera.transform.position = _targetPosition;
                            _isAnimating = false; // Animation complete
                        }
                    }
                    break;


            }
        }

        private Vector3 GetPositionWithZDistance(Vector3 pointPosition)
        {
            return new Vector3(pointPosition.x, pointPosition.y, _cameraZDistance);
        }

        // Public methods
        public void MoveToPoint(int pointIndex)
        {
            if (pointIndex >= 0 && pointIndex < _navigationPoints.Length)
            {
                _currentPointIndex = pointIndex;
                StartAnimation();
            }
        }

        public int GetCurrentPointIndex() => _currentPointIndex;
        public bool IsAnimating() => _isAnimating;

        private int GetPreviousPointIndex()
        {
            int previousIndex = _currentPointIndex - 1;
            if (previousIndex < 0)
                previousIndex = _navigationPoints.Length - 1;
            return previousIndex;
        }

        private int GetCurrentPointIndexFromPosition(Vector3 position)
        {
            if (_navigationPoints.Length == 0) return 0;

            float minDistance = float.MaxValue;
            int closestIndex = 0;

            for (int i = 0; i < _navigationPoints.Length; i++)
            {
                if (_navigationPoints[i] != null)
                {
                    Vector3 pointPosition = GetPositionWithZDistance(_navigationPoints[i].transform.position);
                    float distance = Vector3.Distance(position, pointPosition);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestIndex = i;
                    }
                }
            }

            return closestIndex;
        }
    }
}
