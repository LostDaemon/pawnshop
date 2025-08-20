using System;

public interface ICustomerService
{
    Customer CurrentCustomer { get; }
    void ChangeMood(float delta);
    void IncreaseUncertainty(float delta);
    void ShowNextCustomer();
    long EvaluateCurrentItem();
    event Action<Customer> OnCustomerChanged;
}