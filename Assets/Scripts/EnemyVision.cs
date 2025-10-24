using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [SerializeField] float viewDistance = 8f;
    [SerializeField] float viewAngle = 30f;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask obstacleLayer;

    private EnemyMovement movement;

    public Transform PlayerTransform { get; private set; }
    public Vector2 LastKnownPlayerPos { get; private set; }

    void Awake()
    {
        movement = GetComponent<EnemyMovement>();
    }

    public bool CanSeePlayer()
    {
        // Returns false if no player is detected in circles radius
        Collider2D target = Physics2D.OverlapCircle(transform.position, viewDistance, playerLayer);

        if (target == null) return false;

        /* Creates a direction vector from the enemy to the player
        .normalized makes the vector length 1 so it can be used for angle math and raycasts. */
        Vector2 dirToTarget = (target.transform.position - transform.position).normalized;

        float angleToTarget = Vector2.Angle(movement.FacingDirection, dirToTarget);

        if (angleToTarget < viewAngle)
        {
            // Check if there are obstacles blocking view
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToTarget, viewDistance, obstacleLayer);

            //If it hits nothing (air) or hits the player first
            if (hit.collider == null || hit.collider == target)
            {
                PlayerTransform = target.transform;
                LastKnownPlayerPos = target.transform.position;
                return true;
            }
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Safely get movement even when not in play mode
        if (movement == null)
            movement = GetComponent<EnemyMovement>();

        // Use a fallback direction if movement hasn't initialized yet
        Vector2 forward = Application.isPlaying ? movement.FacingDirection : Vector2.down;

        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // Calculate cone boundaries
        Vector2 leftBoundary = Quaternion.Euler(0, 0, viewAngle) * forward * viewDistance;
        Vector2 rightBoundary = Quaternion.Euler(0, 0, -viewAngle) * forward * viewDistance;

        // Draw FOV cone lines
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rightBoundary);

        // Visualize last known player position
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(LastKnownPlayerPos, 0.3f);
        }
    }
#endif

}
