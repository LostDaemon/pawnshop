using System;
using UnityEngine;

public class CustomerService : ICustomerService
{
    private readonly ICustomerFactoryService _customerFactory;
    private readonly System.Random _random = new();
    public Customer CurrentCustomer { get; private set; }
    public event Action<Customer> OnCustomerChanged;

    public CustomerService(ICustomerFactoryService customerFactory)
    {
        _customerFactory = customerFactory;
    }

    public void ChangeMood(float delta)
    {
        if (CurrentCustomer == null) return;
        CurrentCustomer.MoodLevel = Mathf.Clamp(CurrentCustomer.MoodLevel + delta, -1f, 1f);
        // OnMoodChanged?.Invoke(Current.MoodLevel);
    }

    public void IncreaseUncertainty(float delta)
    {
        if (CurrentCustomer == null) return;
        CurrentCustomer.UncertaintyLevel = Mathf.Clamp01(CurrentCustomer.UncertaintyLevel + delta);
        //OnUncertaintyChanged?.Invoke(Current.UncertaintyLevel);
        ChangeMood(-0.1f);
    }

    public void ShowNextCustomer()
    {
        var customer = _customerFactory.GenerateRandomCustomer();
        CurrentCustomer = customer;
        OnCustomerChanged?.Invoke(CurrentCustomer);
    }

    public long EvaluateCurrentItem()
    {
        if (CurrentCustomer == null || CurrentCustomer.OwnedItem == null)
            return 0;

        return (long)(CurrentCustomer.OwnedItem.BasePrice * _random.Next(60, 86) / 100f);
    }
}