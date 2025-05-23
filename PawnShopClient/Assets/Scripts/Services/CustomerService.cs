using System;
using UnityEngine;

public class CustomerService : ICustomerService
{
    public Customer Current { get; private set; }
    public event Action<float> OnMoodChanged;
    public event Action<float> OnUncertaintyChanged;

    public void SetCurrent(Customer customer)
    {
        Current = customer;
        OnMoodChanged?.Invoke(Current.MoodLevel);
        OnUncertaintyChanged?.Invoke(Current.UncertaintyLevel);
    }

    public void ChangeMood(float delta)
    {
        if (Current == null) return;

        Current.MoodLevel = Mathf.Clamp(Current.MoodLevel + delta, -1f, 1f);
        OnMoodChanged?.Invoke(Current.MoodLevel);
    }

    public void IncreaseUncertainty(float delta)
    {
        if (Current == null) return;

        Current.UncertaintyLevel = Mathf.Clamp01(Current.UncertaintyLevel + delta);
        OnUncertaintyChanged?.Invoke(Current.UncertaintyLevel);
        ChangeMood(-0.1f);
    }
}