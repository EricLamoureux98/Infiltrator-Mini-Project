using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float maxSpottedTime;
    [SerializeField] float maxChaseTimer;

    [Header("FOV Cone")]
    [SerializeField] float viewDistance = 8f;
    [SerializeField] float viewAngle = 30f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] LayerMask playerLayer;

    private Transform playerPosition;
    private Animator anim;
    private Rigidbody2D rb;
    private float chaseTimer;
    private float spottedTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        CheckForPlayer();
    }

    void CheckForPlayer()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, viewDistance, playerLayer);

        if (playerPosition)
        {
            ChasePlayer();
        }

        foreach (Collider2D target in targets)
        {
            Vector2 forward = -transform.up;
            Vector2 dirToTarget = (target.transform.position - transform.position).normalized;
            float angleToTarget = Vector2.Angle(forward, dirToTarget);

            if (angleToTarget < viewAngle)
            {
                // Check if there are obstacles blocking view
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToTarget, viewDistance, obstacleLayer);

                if (hit.collider == null || hit.collider == target)
                {
                    playerPosition = target.transform;
                    Debug.Log("Player spotted!");
                }
            }
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (playerPosition.transform.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
        anim.SetBool("isWalking", true);
        UpdateAnimation(direction);
    }

    void UpdateAnimation(Vector2 direction)
    {
        anim.SetFloat("MoveX", direction.x);
        anim.SetFloat("MoveY", direction.y);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector2 forward = -transform.up;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, viewAngle) * forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -viewAngle) * forward * viewDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
