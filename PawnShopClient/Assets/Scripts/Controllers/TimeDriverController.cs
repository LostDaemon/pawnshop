using UnityEngine;
using Zenject;

public class TimeDriverController : MonoBehaviour
{
    [Inject] private ITimeService _timeService;

    private void Update()
    {
        _timeService.Tick(Time.deltaTime);
    }
}