using System.Collections.Generic;
using PawnShop.Models;
using UnityEngine;

namespace PawnShop.Repositories
{
    public class NavigationRepository : INavigationRepository
    {
        private readonly Dictionary<NpcAction, List<Transform>> _actionTransforms;

        public NavigationRepository()
        {
            _actionTransforms = new Dictionary<NpcAction, List<Transform>>();
        }

        /// <summary>
        /// Register transforms for specific NPC action
        /// </summary>
        /// <param name="action">NPC action type</param>
        /// <param name="transforms">List of transforms for this action</param>
        public void Register(NpcAction action, List<Transform> transforms)
        {
            _actionTransforms[action] = transforms;
        }

        /// <summary>
        /// Clear all registered transforms
        /// </summary>
        public void Clear()
        {
            _actionTransforms.Clear();
        }

        /// <summary>
        /// Get navigation transforms for specific NPC action
        /// </summary>
        /// <param name="action">NPC action type</param>
        /// <returns>List of transforms for the action, or empty list if not found</returns>
        public List<Transform> GetNavigation(NpcAction action)
        {
            return _actionTransforms.TryGetValue(action, out var transforms) 
                ? transforms 
                : new List<Transform>();
        }
    }
}
