using PawnShop.Services;
using UnityEngine;
using Zenject;

namespace PawnShop.Controllers
{
    public class TimeDriverController : MonoBehaviour
    {
        [Inject] private ITimeService _timeService;

        private void Update()
        {
            _timeService.Tick(Time.deltaTime);
        }
    }
}