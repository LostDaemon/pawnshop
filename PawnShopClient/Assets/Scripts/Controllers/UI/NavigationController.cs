using UnityEngine;

public class NavigationController : MonoBehaviour
{
    private INavigationService navigationService;

    public void Initialize(INavigationService service)
    {
        navigationService = service;
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
}