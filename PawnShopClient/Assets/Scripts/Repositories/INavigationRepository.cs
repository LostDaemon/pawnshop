using System.Collections.Generic;
using PawnShop.Models.Npc;

namespace PawnShop.Repositories
{
    public interface INavigationRepository
    {
        /// <summary>
        /// Register transforms for specific NPC action
        /// </summary>
        /// <param name="action">NPC action type</param>
        /// <param name="tasks">List of transforms for this action</param>
        void Register(NpcType action, List<NpcTask> tasks);

        /// <summary>
        /// Clear all registered transforms
        /// </summary>
        void Clear();

        /// <summary>
        /// Get navigation transforms for specific NPC action
        /// </summary>
        /// <param name="action">NPC action type</param>
        /// <returns>List of transforms for the action, or empty list if not found</returns>
        List<NpcTask> GetNavigation(NpcType type);
    }
}
