using PawnShop.Models.EventsSystem;
using PawnShop.Services.EventSystem;
using UnityEngine;
using Zenject;

namespace PawnShop.Services.EventSystem
{
    /// <summary>
    /// Processor for customer arrival events
    /// </summary>
    public class CustomerEventProcessor : EventProcessorBase<CustomerEvent>
    {
        private readonly INegotiationService _negotiationService;

        [Inject]
        public CustomerEventProcessor(INegotiationService negotiationService)
        {
            _negotiationService = negotiationService;
        }

        public override void ProcessEvent(CustomerEvent customerEvent)
        {
            Debug.Log($"[CustomerEventProcessor] CUSTOMER ARRIVAL: Day {customerEvent.Time.Day} at {customerEvent.Time.Hour:00}:{customerEvent.Time.Minute:00}");
            
            // Show next customer when customer arrival event is triggered
            _negotiationService.ShowNextCustomer();
        }

        public override bool CanProcess(GameEventType eventType)
        {
            return eventType == GameEventType.VisitorArrival;
        }
    }
}
