using UnityEngine;

[ExecuteAlways]
public class RoomDebugVisualizer : MonoBehaviour
{
    [Header("Visual")]
    public Color boundsColor = Color.green;
    public Color centerColor = Color.cyan;
    public float gizmoDepth = 10f;

    [Header("Room Settings")]
    public Vector2 roomSize = new Vector2(16f, 9f);
    public Vector2Int currentRoom = Vector2Int.zero;

    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        // Draw visible camera area
        float height = Camera.main.orthographicSize * 2f;
        float width = height * Camera.main.aspect;

        Vector3 camPos = Camera.main.transform.position;
        Vector3 screenCenter = new Vector3(camPos.x, camPos.y, gizmoDepth);

        Gizmos.color = boundsColor;
        Gizmos.DrawWireCube(screenCenter, new Vector3(width, height, 0f));

        // Draw current room center
        Vector3 roomCenter = new Vector3(
            currentRoom.x * roomSize.x,
            currentRoom.y * roomSize.y,
            gizmoDepth
        );

        Gizmos.color = centerColor;
        Gizmos.DrawSphere(roomCenter, 0.2f);
    }
}