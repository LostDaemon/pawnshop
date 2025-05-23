using System;
using System.Collections.Generic;

public interface INegotiationHistoryService
{
    event Action<IHistoryRecord> OnRecordAdded;
    IReadOnlyList<IHistoryRecord> History { get; }
    void Add(IHistoryRecord record);
}