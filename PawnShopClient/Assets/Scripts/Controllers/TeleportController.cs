using UnityEngine;

namespace PawnShop.Controllers
{
    public class TeleportController : MonoBehaviour
    {
        [SerializeField] private TeleportController nextTeleportController;
        [SerializeField] private TeleportController previousTeleportController;
        
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
