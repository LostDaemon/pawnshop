using UnityEngine;
using UnityEngine.UI;
using PawnShop.Services;
using Zenject;
using PawnShop.Models;

namespace PawnShop.Controllers
{
    public class CartNavigationButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private bool isForward = true; // true = вперед, false = назад

        private NavigationService navigationService;

        [Inject]
        private void Construct(NavigationService navigation)
        {
            navigationService = navigation;

            if (button == null)
                button = GetComponent<Button>();

            button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            if (isForward)
            {
                // Переключить на следующий вагон
                var currentCart = navigationService.CurrentCart;
                var nextCart = (CartType)((int)currentCart + 1);
                navigationService.SetCart(nextCart);
            }
            else
            {
                // Переключить на предыдущий вагон
                var currentCart = navigationService.CurrentCart;
                var prevCart = (CartType)((int)currentCart - 1);
                navigationService.SetCart(prevCart);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnButtonClick);
        }
    }
}
