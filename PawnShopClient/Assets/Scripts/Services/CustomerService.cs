using System;
using System.Collections.Generic;
using System.Linq;
using PawnShop.Models;
using PawnShop.Models.Characters;
using PawnShop.Models.Events;
using UnityEngine;
using Zenject;

namespace PawnShop.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerFactoryService _customerFactory;
        private readonly INegotiationHistoryService _history;
        private readonly ILocalizationService _localizationService;
        private readonly ITimeService _timeService;
        private readonly System.Random _random = new();

        private Queue<Customer> _customersQueue = new Queue<Customer>();

        public Customer CurrentCustomer { get; private set; }
        public event Action<Customer> OnCustomerChanged;
        public event Action<Customer> OnNewCustomer;


        [Inject]
        public CustomerService(ICustomerFactoryService customerFactory, INegotiationHistoryService history, ILocalizationService localizationService, ITimeService timeService)
        {
            _customerFactory = customerFactory;
            _history = history;
            _localizationService = localizationService;
            _timeService = timeService;

            _timeService.OnEventTriggered += OnEventTriggered;
            ScheduleCustomerEvents();
        }

        private void ScheduleCustomerEvents()
        {
            Debug.Log("[CustomerService] Scheduling customer events...");
            
            var scheduledTimes = new List<string>();
            
            for (int hour = 8; hour <= 18; hour++)
            {
                var baseMinute = 0;
                var deviation = _random.Next(-30, 31); // -30 to +30 minutes
                var actualMinute = Math.Max(0, Math.Min(59, baseMinute + deviation));
                
                var scheduledTime = new GameTime(1, hour, actualMinute);
                
                var gameEvent = new GameEvent
                {
                    EventType = GameEventType.Customer,
                    Time = scheduledTime
                };
                
                _timeService.Schedule(gameEvent);
                scheduledTimes.Add($"{hour}:{actualMinute:D2}");
            }
            
            Debug.Log($"[CustomerService] Scheduled customer events at: {string.Join(", ", scheduledTimes)}");
        }

        private void OnEventTriggered(IGameEvent gameEvent)
        {
            Debug.Log($"[CustomerService] Event triggered: Type={gameEvent.EventType}, Time={gameEvent.Time.Day}:{gameEvent.Time.Hour:D2}:{gameEvent.Time.Minute:D2}");
            
            if (gameEvent.EventType == GameEventType.Customer)
            {
                var newCustomer = _customerFactory.GenerateRandomCustomer();
                _customersQueue.Enqueue(newCustomer);
                
                Debug.Log($"[CustomerService] New customer added to queue: Type={newCustomer?.CustomerType}, Item={newCustomer?.OwnedItem?.Name}");
                OnNewCustomer?.Invoke(newCustomer);
                
                if (CurrentCustomer == null)
                {
                    NextCustomer();
                }
            }
        }

        public void NextCustomer()
        {
            Debug.Log("[CustomerService] NextCustomer called");
            
            if (_customersQueue.Count == 0)
            {
                Debug.LogWarning("[CustomerService] No customers in queue");
                return;
            }
            
            CurrentCustomer = _customersQueue.Dequeue();
            
            Debug.Log($"[CustomerService] Current customer: Type={CurrentCustomer?.CustomerType}, Item={CurrentCustomer?.OwnedItem?.Name}");
            OnCustomerChanged?.Invoke(CurrentCustomer);
        }

        public void RequestSkip()
        {
            _history.Add(new TextRecord(HistoryRecordSource.Player,
                string.Format(_localizationService.GetLocalization("dialog_player_skip_item"), CurrentCustomer?.OwnedItem?.Name)));
            ClearCustomer();
        }

        public void ClearCustomer()
        {
            Debug.Log("[CustomerService] Clearing current customer");
            CurrentCustomer = null;
            OnCustomerChanged?.Invoke(null);
        }
    }
}