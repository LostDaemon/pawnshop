using PawnShop.Models;
using PawnShop.Services;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace PawnShop.Controllers
{
    public class DayTimeController : MonoBehaviour
    {
        [Header("Sky")]
        [SerializeField] private Transform _sky;
        
        [Header("Lighting Objects")]
        [SerializeField] private List<LightingObject> _lightingObjects = new();
        
        [Header("Time Settings")]
        [SerializeField] private float _skyRotationSpeed = 2f; // lerp speed multiplier
        [SerializeField] private float _skyRotationSmoothness = 0.02f; // smoothness factor
        
        [Inject] private ITimeService _timeService;
        
        private float _currentSkyRotation;
        private float _targetSkyRotation;
        
        [System.Serializable]
        public class LightingObject
        {
            public GameObject gameObject;
            [Range(0f, 1f)] public float lightingCoefficient = 1f;
            [Range(0f, 1f)] public float minBrightness = 0.1f;
            [Range(0f, 1f)] public float maxBrightness = 1f;
            
            private SpriteRenderer[] _spriteRenderers;
            private Color[] _originalColors;
            
            public void Initialize()
            {
                if (gameObject != null)
                {
                    _spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
                    _originalColors = new Color[_spriteRenderers.Length];
                    
                    for (int i = 0; i < _spriteRenderers.Length; i++)
                    {
                        _originalColors[i] = _spriteRenderers[i].color;
                    }
                }
            }
            
            public void UpdateLighting(float timeOfDay)
            {
                if (_spriteRenderers == null || _originalColors == null) return;
                
                float brightness = Mathf.Lerp(minBrightness, maxBrightness, lightingCoefficient * timeOfDay);
                
                for (int i = 0; i < _spriteRenderers.Length; i++)
                {
                    Color newColor = _originalColors[i];
                    newColor.r *= brightness;
                    newColor.g *= brightness;
                    newColor.b *= brightness;
                    _spriteRenderers[i].color = newColor;
                }
            }
        }
        
        private void Start()
        {
            if (_timeService == null)
            {
                Debug.LogError("TimeService not injected into DayTimeController");
                return;
            }
            
            // Initialize all lighting objects
            foreach (var lightingObj in _lightingObjects)
            {
                lightingObj.Initialize();
            }
            
            // Subscribe to time changes
            _timeService.OnTimeChanged += OnTimeChanged;
            
            // Initial update
            UpdateLighting(_timeService.CurrentTime);
            
            // Initialize sky rotation
            if (_sky != null)
            {
                float initialTimeOfDay = CalculateTimeOfDay(_timeService.CurrentTime);
                _targetSkyRotation = initialTimeOfDay * 360f;
                _currentSkyRotation = _targetSkyRotation;
                _sky.rotation = Quaternion.Euler(0f, 0f, _currentSkyRotation);
            }
        }
        
        private void OnDestroy()
        {
            if (_timeService != null)
                _timeService.OnTimeChanged -= OnTimeChanged;
        }
        
        private void OnTimeChanged(GameTime time)
        {
            UpdateLighting(time);
        }
        
        private void UpdateLighting(GameTime time)
        {
            // Calculate time of day (0.0 = midnight, 0.5 = noon, 1.0 = midnight)
            float timeOfDay = CalculateTimeOfDay(time);
            
            // Update sky rotation
            UpdateSkyRotation(timeOfDay);
            
            // Update lighting for all objects
            foreach (var lightingObj in _lightingObjects)
            {
                lightingObj.UpdateLighting(timeOfDay);
            }
        }
        
        private float CalculateTimeOfDay(GameTime time)
        {
            // Convert hour and minute to decimal time (0.0 - 1.0)
            float totalMinutes = time.Hour * 60f + time.Minute;
            float timeOfDay = totalMinutes / (24f * 60f);
            
            // Adjust for better lighting curve (more light during day, less at night)
            // This creates a bell curve with peak at noon
            float adjustedTime = Mathf.Sin(timeOfDay * Mathf.PI * 2f - Mathf.PI / 2f) * 0.5f + 0.5f;
            
            return adjustedTime;
        }
        
        private void UpdateSkyRotation(float timeOfDay)
        {
            if (_sky == null) return;
            
            // Calculate rotation based on time of day
            // 0 degrees = midnight, 180 degrees = noon, 360 degrees = midnight
            _targetSkyRotation = timeOfDay * 360f;
        }
        
        private void Update()
        {
            // Smooth sky rotation every frame for better smoothness
            if (_sky != null)
            {
                _currentSkyRotation = Mathf.LerpAngle(_currentSkyRotation, _targetSkyRotation, _skyRotationSmoothness);
                _sky.rotation = Quaternion.Euler(0f, 0f, _currentSkyRotation);
            }
        }
        
        // Helper method to add lighting object at runtime
        public void AddLightingObject(GameObject gameObject, float lightingCoefficient = 1f, float minBrightness = 0.1f, float maxBrightness = 1f)
        {
            var lightingObj = new LightingObject
            {
                gameObject = gameObject,
                lightingCoefficient = lightingCoefficient,
                minBrightness = minBrightness,
                maxBrightness = maxBrightness
            };
            
            lightingObj.Initialize();
            _lightingObjects.Add(lightingObj);
            
            // Update immediately with current time
            if (_timeService != null)
            {
                UpdateLighting(_timeService.CurrentTime);
            }
        }
        
        // Helper method to remove lighting object
        public void RemoveLightingObject(GameObject gameObject)
        {
            _lightingObjects.RemoveAll(obj => obj.gameObject == gameObject);
        }
    }
}
