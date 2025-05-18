using UnityEngine;

public class ResponsiveRoomScaler : MonoBehaviour
{
    [SerializeField] private NavigationController navigationController;
    [SerializeField] private ParallaxLayerController[] parallaxLayers;

    private INavigationService navigationService;
    private Vector2 lastSize;

    private void Start()
    {
        UpdateRoomSize();
    }

    private void Update()
    {
        Vector2 currentSize = GetRoomSizeFromCamera();
        if (currentSize != lastSize)
        {
            UpdateRoomSize();
        }
    }

    private void UpdateRoomSize()
    {
        Vector2 roomSize = GetRoomSizeFromCamera();
        lastSize = roomSize;

        navigationService = new NavigationService(roomSize);
        navigationController.Initialize(navigationService);

        foreach (var layer in parallaxLayers)
            layer.Initialize(navigationService);
    }

    private Vector2 GetRoomSizeFromCamera()
    {
        float height = Camera.main.orthographicSize * 2f;
        float width = height * Camera.main.aspect;
        return new Vector2(width, height);
    }
}