using System;
using System.Collections.Generic;
using PawnShop.Models;

namespace PawnShop.Services
{
    public class NegotiationHistoryService : INegotiationHistoryService
    {
        private readonly List<IHistoryRecord> _history = new();
        public event Action<IHistoryRecord> OnRecordAdded;

        public IReadOnlyList<IHistoryRecord> History => _history.AsReadOnly();

        public void Add(IHistoryRecord record)
        {
            _history.Add(record);
            OnRecordAdded?.Invoke(record);
        }
    }
}