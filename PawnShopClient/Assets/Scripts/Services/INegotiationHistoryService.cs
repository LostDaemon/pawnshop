using System;
using System.Collections.Generic;
using PawnShop.Models;

namespace PawnShop.Services
{
    public interface INegotiationHistoryService
    {
        event Action<IHistoryRecord> OnRecordAdded;
        IReadOnlyList<IHistoryRecord> History { get; }
        void Add(IHistoryRecord record);
    }
}