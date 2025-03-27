using UnityEngine;
using Unity.Cinemachine;

public class CameraZoom : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineCamera virtualCamera;

    [Header("Offset Settings")]
    [SerializeField] private float maxHorizontalOffset = 2f; // Max left/right movement
    [SerializeField] private float maxDepthOffset = 2f;    // Max forward/back movement
    [SerializeField] private float offsetChangeTime = 0.3f;

    private CinemachineFollow cinemachineFollow;
    private Vector3 velocity = Vector3.zero;
    private Vector3 initialOffset;
    private Vector2 screenCenter;

    private void Start()
    {
        // Initialize references
        if (mainCamera == null) mainCamera = Camera.main;
        if (virtualCamera != null)
        {
            cinemachineFollow = virtualCamera.GetComponentInChildren<CinemachineFollow>();
            if (cinemachineFollow != null)
            {
                initialOffset = cinemachineFollow.FollowOffset;                
            }
        }
        screenCenter = new Vector2(0.5f, 0.5f);
    }

    private void Update()
    {
        if (cinemachineFollow == null) return;       
        Vector2 mouseViewportPos = mainCamera.ScreenToViewportPoint(Input.mousePosition);
               
        Vector2 directionFromCenter = (mouseViewportPos - screenCenter) * 2f;
        directionFromCenter = Vector2.ClampMagnitude(directionFromCenter, 1f);
        Vector3 targetOffset = new Vector3(
            initialOffset.x + (directionFromCenter.x * maxHorizontalOffset),
            initialOffset.y,
            initialOffset.z + (directionFromCenter.y * maxDepthOffset)
        );       
        if (!Input.GetKey(KeyCode.Mouse1))
        {
            targetOffset = initialOffset;
        }
             
        cinemachineFollow.FollowOffset = Vector3.SmoothDamp(
            cinemachineFollow.FollowOffset,
            targetOffset,
            ref velocity,
            offsetChangeTime
        );
    }
}