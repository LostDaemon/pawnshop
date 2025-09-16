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
        public event Action<float> OnPatienceChanged;


        [Inject]
        public CustomerService(ICustomerFactoryService customerFactory, INegotiationHistoryService history, ILocalizationService localizationService, ITimeService timeService)
        {
            _customerFactory = customerFactory;
            _history = history;
            _localizationService = localizationService;
            _timeService = timeService;

            _timeService.OnEventTriggered += OnEventTriggered;
            _timeService.OnTimeChanged += OnTimeChanged;
            ScheduleCustomerEvents();
        }

        public void ChangeCustomerPatience(float changeAmount)
        {
            if (CurrentCustomer == null)
            {
                Debug.LogWarning("[CustomerService] Cannot change patience - no current customer");
                return;
            }

            float previousPatience = CurrentCustomer.Patience;
            CurrentCustomer.Patience = Mathf.Max(0f, CurrentCustomer.Patience + changeAmount);
            
            Debug.Log($"[CustomerService] Customer patience changed by {changeAmount:F1}. Previous: {previousPatience:F1}, Current: {CurrentCustomer.Patience:F1}");
            
            // Update patience and trigger events
            UpdatePatience(previousPatience);
        }

        public void ClearCustomer()
        {
            Debug.Log("[CustomerService] Clearing current customer");
            CurrentCustomer = null;
            OnCustomerChanged?.Invoke(null);
        }

        public void Dispose()
        {
            // Unsubscribe from time service events
            if (_timeService != null)
            {
                _timeService.OnEventTriggered -= OnEventTriggered;
                _timeService.OnTimeChanged -= OnTimeChanged;
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

        private void CheckPatienceThresholds(float previousPatience)
        {
            if (CurrentCustomer == null) return;
            
            // Show dialogue at 50% and 25% thresholds
            if (previousPatience > 50f && CurrentCustomer.Patience <= 50f)
            {
                ShowCustomerPatienceDialogue();
            }
            else if (previousPatience > 25f && CurrentCustomer.Patience <= 25f)
            {
                ShowCustomerPatienceDialogue();
            }
        }

        private void HandleCustomerLeaving()
        {
            if (CurrentCustomer == null) return;
            
            Debug.Log($"[CustomerService] Customer patience reached zero. Customer is leaving.");
            
            // Show customer's final dialogue before leaving
            string leaveDialogue = _localizationService.GetLocalization("dialog_customer_patience_leave");
            _history.Add(new TextRecord(HistoryRecordSource.Customer, leaveDialogue));
            
            // Add system message about customer leaving
            string systemMessage = _localizationService.GetLocalization("system_customer_left_impatient");
            _history.Add(new TextRecord(HistoryRecordSource.System, systemMessage));
            
            // Clear current customer
            ClearCustomer();
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

        private void OnTimeChanged(GameTime currentTime)
        {
            // Reduce customer patience every minute
            if (CurrentCustomer != null)
            {
                float previousPatience = CurrentCustomer.Patience;
                CurrentCustomer.Patience = Mathf.Max(0f, CurrentCustomer.Patience - 0.1f);
                
                // Update patience and trigger events
                UpdatePatience(previousPatience);
                
                // Check if customer patience reached zero
                if (CurrentCustomer.Patience <= 0f)
                {
                    HandleCustomerLeaving();
                }
            }
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

        private void ShowCustomerPatienceDialogue()
        {
            if (CurrentCustomer == null) return;
            
            // Show random dialogue from customer about patience
            string dialogue = _localizationService.GetLocalization("dialog_customer_patience");
            _history.Add(new TextRecord(HistoryRecordSource.Customer, dialogue));
        }

        private void UpdatePatience(float previousPatience)
        {
            // Only proceed if patience actually changed
            if (Mathf.Approximately(previousPatience, CurrentCustomer.Patience))
                return;
            
            // Check patience thresholds and show dialogue
            CheckPatienceThresholds(previousPatience);
            
            // Trigger patience changed event
            OnPatienceChanged?.Invoke(CurrentCustomer.Patience);
        }
    }
}