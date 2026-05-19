using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    private void FixedUpdate()
    {
        if (target == null) return;

        // Calculate the camera's target position based on Purly's X position
        Vector3 desiredPosition = target.position + offset;
        
        // ASSIGNMENT REQUIREMENT: Tracks Purly horizontally (X-axis)
        // We lock the Y and Z axes to the initial offset to ensure smooth side-scrolling.
        desiredPosition.y = transform.position.y; 
        
        // Apply smooth interpolation (Lerp) to avoid jittery tracking
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}