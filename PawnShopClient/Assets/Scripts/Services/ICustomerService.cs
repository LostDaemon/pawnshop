using System;

public interface ICustomerService
{
    Customer Current { get; }
    void SetCurrent(Customer customer);
    void ChangeMood(float delta);
    void IncreaseUncertainty(float delta);
    event Action<float> OnMoodChanged;
    event Action<float> OnUncertaintyChanged;
}