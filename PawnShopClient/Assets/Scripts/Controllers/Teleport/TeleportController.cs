using UnityEngine;

namespace PawnShop.Controllers.Teleport
{
    public class TeleportController : MonoBehaviour
    {
        [SerializeField] private TeleportController nextTeleportController;
        [SerializeField] private TeleportController previousTeleportController;
        
        public TeleportController Next => nextTeleportController;
        public TeleportController Previous => previousTeleportController;
        
        /// <summary>
        /// Map all teleport stations by traversing in both directions
        /// </summary>
        public System.Collections.Generic.List<Transform> MapTeleportStations()
        {
            var stations = new System.Collections.Generic.List<Transform>();
            var visited = new System.Collections.Generic.HashSet<TeleportController>();
            
            // Add current station
            stations.Add(transform);
            visited.Add(this);
            
            // Go Next
            var current = nextTeleportController;
            while (current != null && !visited.Contains(current))
            {
                stations.Add(current.transform);
                visited.Add(current);
                current = current.nextTeleportController;
            }
            
            // Go Previous
            current = previousTeleportController;
            while (current != null && !visited.Contains(current))
            {
                stations.Add(current.transform);
                visited.Add(current);
                current = current.previousTeleportController;
            }
            
            return stations;
        }
        
        public void TeleportMeToNext(GameObject gameObjectToTeleport)
        {
            if (nextTeleportController != null)
            {
                gameObjectToTeleport.transform.position = nextTeleportController.transform.position;
            }
            else
            {
                Debug.Log("Next teleport controller is not assigned");
            }
        }
        
        public void TeleportMeToPrevious(GameObject gameObjectToTeleport)
        {
            if (previousTeleportController != null)
            {
                gameObjectToTeleport.transform.position = previousTeleportController.transform.position;
            }
            else
            {
                Debug.Log("Previous teleport controller is not assigned");
            }
        }
    }
}
