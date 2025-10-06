using UnityEngine;

namespace PawnShop.Controllers
{
    public class TeleportClientController : MonoBehaviour
    {
        [SerializeField] private float searchRadius = 0.5f;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                TryTeleportToNext();
            }
            
            if (Input.GetKeyDown(KeyCode.S))
            {
                TryTeleportToPrevious();
            }
        }
        
        private void TryTeleportToNext()
        {
            TeleportController teleportController = FindNearbyTeleportController();
            if (teleportController != null)
            {
                Debug.Log("Found teleport controller for Next teleportation");
                teleportController.TeleportMeToNext(gameObject);
            }
            else
            {
                Debug.Log("No teleport controller found for Next teleportation");
            }
        }
        
        private void TryTeleportToPrevious()
        {
            TeleportController teleportController = FindNearbyTeleportController();
            if (teleportController != null)
            {
                Debug.Log("Found teleport controller for Previous teleportation");
                teleportController.TeleportMeToPrevious(gameObject);
            }
            else
            {
                Debug.Log("No teleport controller found for Previous teleportation");
            }
        }
        
        private TeleportController FindNearbyTeleportController()
        {
            TeleportController[] allTeleportControllers = FindObjectsOfType<TeleportController>();
            
            foreach (TeleportController teleportController in allTeleportControllers)
            {
                float distance = Vector2.Distance(transform.position, teleportController.transform.position);
                if (distance <= searchRadius)
                {
                    return teleportController;
                }
            }
            
            return null;
        }
        
    }
}
