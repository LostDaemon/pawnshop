using System.Collections.Generic;
using PawnShop.Models;
using PawnShop.Repositories;
using UnityEngine;
using Zenject;

namespace PawnShop.Controllers
{
    /// <summary>
    /// Controller for registering navigation data in NavigationRepository
    /// Allows to configure NPC action to transforms mapping in editor
    /// </summary>
    public class NavigationDataController : MonoBehaviour
    {
        [System.Serializable]
        public class NavigationData
        {
            [SerializeField] private NpcAction action;
            [SerializeField] private List<Transform> transforms;

            public NpcAction Action => action;
            public List<Transform> Transforms => transforms;
        }

        [Header("Navigation Data")]
        [SerializeField] private List<NavigationData> navigationDataList = new List<NavigationData>();

        private INavigationRepository navigationRepository;

        [Inject]
        public void Construct(INavigationRepository repository)
        {
            navigationRepository = repository;
        }

        private void Start()
        {
            RegisterNavigationData();
        }

        /// <summary>
        /// Register all navigation data to repository
        /// </summary>
        private void RegisterNavigationData()
        {
            foreach (var data in navigationDataList)
            {
                if (data.Transforms != null && data.Transforms.Count > 0)
                {
                    navigationRepository.Register(data.Action, data.Transforms);
                    Debug.Log($"[NavigationDataController] Registered {data.Transforms.Count} transforms for action: {data.Action}");
                }
            }
        }

        /// <summary>
        /// Clear all navigation data from repository
        /// </summary>
        [ContextMenu("Clear Navigation Data")]
        public void ClearNavigationData()
        {
            navigationRepository.Clear();
            Debug.Log("[NavigationDataController] Cleared all navigation data");
        }

        /// <summary>
        /// Re-register navigation data (useful for runtime updates)
        /// </summary>
        [ContextMenu("Re-register Navigation Data")]
        public void ReregisterNavigationData()
        {
            navigationRepository.Clear();
            RegisterNavigationData();
        }
    }
}
