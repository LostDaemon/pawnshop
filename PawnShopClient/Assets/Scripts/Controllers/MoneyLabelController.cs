using PawnShop.Models;
using PawnShop.Services;
using UnityEngine;
using Zenject;

namespace PawnShop.Controllers
{
    public class MoneyLabelController : MonoBehaviour
    {
        [SerializeField] private IndicatorController _indicator;

        private IWalletService _wallet;

        [Inject]
        public void Construct(IWalletService wallet)
        {
            _wallet = wallet;
            _wallet.OnBalanceChanged += OnBalanceChanged;

            // init
            _indicator.SetValue(_wallet.GetBalance(CurrencyType.Money), animate: false);
        }

        private void OnDestroy()
        {
            if (_wallet != null)
                _wallet.OnBalanceChanged -= OnBalanceChanged;
        }

        private void OnBalanceChanged(CurrencyType currency, long newValue)
        {
            if (currency == CurrencyType.Money)
                _indicator.SetValue(newValue, animate: true);
        }
    }
}