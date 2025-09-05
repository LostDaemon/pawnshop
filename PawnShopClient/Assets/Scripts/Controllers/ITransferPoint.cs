using System;
using UnityEngine.EventSystems;
using PawnShop.Models;

namespace PawnShop.Controllers
{
    public interface ITransferPoint<TPayload> : IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler where TPayload : class
    {
        TPayload GetPayload();
        void SetPayload(TPayload payload);
        StorageType StorageType { get; }
    }
}
