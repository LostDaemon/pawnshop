using UnityEngine;

public class RotationController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f; // degrees per second
    
    private void LateUpdate()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
