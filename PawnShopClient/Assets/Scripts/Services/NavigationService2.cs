using System;
using PawnShop.Models;

namespace PawnShop.Services
{
    public class NavigationService2
    {
        private CartType _currentCart = CartType.Locomotive;
        
        public CartType CurrentCart => _currentCart;
        
        public event Action<CartType> OnCartChanged;

        public void SetCart(CartType cartType)
        {
            _currentCart = cartType;
            OnCartChanged?.Invoke(_currentCart);
        }

        public void NextCart()
        {
            var nextCart = (CartType)((int)_currentCart + 1);
            SetCart(nextCart);
        }

        public void PreviousCart()
        {
            var prevCart = (CartType)((int)_currentCart - 1);
            SetCart(prevCart);
        }
    }
}
