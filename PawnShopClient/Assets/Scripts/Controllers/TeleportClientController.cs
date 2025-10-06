using System.Linq;
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

        public void TryTeleportToTarget(Transform target, float verticalTreshold)
        {
            var teleportController = FindNearbyTeleportController();
            if (teleportController != null)
            {
                Debug.Log("[TeleportClientController] Teleporting to target");

                var teleportStations = teleportController.MapTeleportStations();
                
                // Find station closest to target Y level
                Transform bestStation = null;
                float bestDistance = float.MaxValue;
                
                foreach (var station in teleportStations)
                {
                    float distance = Mathf.Abs(station.position.y - target.position.y);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestStation = station;
                    }
                }
                
                if (bestStation != null && bestDistance <= verticalTreshold)
                {
                    Debug.Log($"[TeleportClientController] Teleporting to station: {bestStation.name}");
                    transform.position = bestStation.position;
                }
                else
                {
                    Debug.LogWarning($"[TeleportClientController] No suitable station found for target Y: {target.position.y}");
                }
            }
        }


        public void TryTeleportToNext()
        {
            var teleportController = FindNearbyTeleportController();
            if (teleportController != null)
            {
                Debug.Log("[TeleportClientController] Teleporting up");
                teleportController.TeleportMeToNext(gameObject);

                // Get teleport stations map
                var teleportStations = teleportController.MapTeleportStations();
                Debug.Log($"[NpcController] Teleport stations: {string.Join(", ", teleportStations?.Select(t => t.name) ?? new string[0])}");
            }
        }

        public void TryTeleportToPrevious()
        {
            var teleportController = FindNearbyTeleportController();
            if (teleportController != null)
            {
                Debug.Log("[TeleportClientController] Teleporting down");
                teleportController.TeleportMeToPrevious(gameObject);
            }
        }

        private TeleportController FindNearbyTeleportController()
        {
            TeleportController[] allTeleportControllers = FindObjectsByType<TeleportController>(FindObjectsSortMode.None);

            foreach (TeleportController teleportController in allTeleportControllers)
            {
                float distance = Vector2.Distance(transform.position, teleportController.transform.position);
                if (distance <= searchRadius)
                {
                    return teleportController;
                }
            }
            Debug.Log("[TeleportClientController] No teleport controller found");
            return null;
        }

    }
}
