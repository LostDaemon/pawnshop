using PawnShop.Models;
using UnityEngine;

namespace PawnShop.Controllers
{
    public class StorageTypeMarkerController : MonoBehaviour
    {
        [SerializeField] private StorageType storageType;

        public StorageType StorageType => storageType;
    }
}
