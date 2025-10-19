using System.Collections.Generic;
using PawnShop.Models.Npc;

namespace PawnShop.Repositories
{
    public class NavigationRepository : INavigationRepository
    {
        private readonly Dictionary<NpcType, List<NpcTask>> _actionTasks;

        public NavigationRepository()
        {
            _actionTasks = new Dictionary<NpcType, List<NpcTask>>();
        }

        /// <summary>
        /// Register tasks for specific NPC action
        /// </summary>
        /// <param name="action">NPC action type</param>
        /// <param name="tasks">List of tasks for this action</param>
        public void Register(NpcType type, List<NpcTask> tasks)
        {
            _actionTasks[type] = tasks;
        }

        /// <summary>
        /// Clear all registered tasks
        /// </summary>
        public void Clear()
        {
            _actionTasks.Clear();
        }

        /// <summary>
        /// Get navigation tasks for specific NPC action
        /// </summary>
        /// <param name="action">NPC action type</param>
        /// <returns>List of tasks for the action, or empty list if not found</returns>
        public List<NpcTask> GetNavigation(NpcType action)
        {
            return _actionTasks.TryGetValue(action, out var tasks)
                ? tasks
                : new List<NpcTask>();
        }
    }
}
