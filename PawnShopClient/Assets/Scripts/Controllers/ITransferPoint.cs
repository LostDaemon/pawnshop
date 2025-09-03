using UnityEngine;
using PawnShop.Models;

namespace PawnShop.Controllers
{
    public interface ITransferPoint
    {
        string Id { get; }
        StorageType StorageType { get; }
        ItemModel Item { get; set; }
        bool IsEmpty { get; }
    }
}
