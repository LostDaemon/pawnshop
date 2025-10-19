using System;
using PawnShop.Models.Characters;
using PawnShop.Models.Npc;

namespace PawnShop.Services
{
    public interface ICustomerService
    {
        Customer CurrentCustomer { get; }
        void NextCustomer();
        void RequestSkip();
        void ClearCustomer();
        void ChangeCustomerPatience(float changeAmount);
        void TriggerNpcAction(string npcId, NpcAction action);
        event Action<Customer> OnCustomerChanged;
        event Action<float> OnPatienceChanged;
        event Action<string, NpcAction> OnNpcAction;
    }
}