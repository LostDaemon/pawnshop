using System;
using System.Collections.Generic;

public class WalletService : IWalletService
{
    private readonly Dictionary<CurrencyType, long> _balances = new();

    public event Action<CurrencyType, long> OnBalanceChanged;

    public WalletService(long initialMoney)
    {
        _balances[CurrencyType.Money] = initialMoney;
    }

    public long GetBalance(CurrencyType currency)
    {
        return _balances.TryGetValue(currency, out var value) ? value : 0L;
    }

    public bool TransactionAttempt(CurrencyType currency, long amount)
    {
        long current = GetBalance(currency);
        long updated = current + amount;

        if (updated < 0)
            return false;

        _balances[currency] = updated;
        OnBalanceChanged?.Invoke(currency, updated);

        return true;
    }
}