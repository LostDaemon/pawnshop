using UnityEngine;

public class NavigationController : MonoBehaviour
{
    private INavigationService navigationService;

    public void Initialize(INavigationService service)
    {
        navigationService = service;

        // Инициализируем все панели на сцене
        var uiScreens = FindObjectsByType<ScreenUIController>(FindObjectsSortMode.None);
        foreach (var screen in uiScreens)
        {
            screen.Initialize(navigationService);
            Debug.Log($"[UI] Found screen: {screen.name}");
        }
    }

    private void Update()
    {
        if (navigationService == null)
            return;

        if (Input.GetKeyDown(KeyCode.W))
            navigationService.MoveUp();

        if (Input.GetKeyDown(KeyCode.S))
            navigationService.MoveDown();

        if (Input.GetKeyDown(KeyCode.A))
            navigationService.MoveLeft();

        if (Input.GetKeyDown(KeyCode.D))
            navigationService.MoveRight();
    }

    public INavigationService GetService() => navigationService;
}