using System;
using PawnShop.Models;

namespace PawnShop.Services
{
    public interface IWalletService
    {
        long GetBalance(CurrencyType currency);
        bool TransactionAttempt(CurrencyType currency, long amount);
        event Action<CurrencyType, long> OnBalanceChanged;
    }
}