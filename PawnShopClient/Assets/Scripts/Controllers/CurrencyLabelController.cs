using PawnShop.Models;
using PawnShop.Services;
using UnityEngine;
using Zenject;

namespace PawnShop.Controllers
{
    public class CurrencyLabelController : MonoBehaviour
    {
        [SerializeField] private IndicatorController _indicator;
        [SerializeField] private CurrencyType _currencyType = CurrencyType.Money;

        private IWalletService _wallet;

        [Inject]
        public void Construct(IWalletService wallet)
        {
            _wallet = wallet;
            _wallet.OnBalanceChanged += OnBalanceChanged;

            // Initialize with current balance
            _indicator.SetValue(_wallet.GetBalance(_currencyType), animate: false);
        }

        private void OnDestroy()
        {
            if (_wallet != null)
                _wallet.OnBalanceChanged -= OnBalanceChanged;
        }

        private void OnBalanceChanged(CurrencyType currency, long newValue)
        {
            if (currency == _currencyType)
                _indicator.SetValue(newValue, animate: true);
        }
    }
}
