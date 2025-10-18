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

    bool seesPlayer;

    private Transform playerPosition;
    private Animator anim;
    private Rigidbody2D rb;
    private EnemyState enemyState;
    private Vector2 facingDirection;
    private float chaseTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        ChangeState(EnemyState.Idle);
        facingDirection = -transform.up;
    }

    void Update()
    {
        seesPlayer = CanSeePlayer();
        HandlePlayerDetection(seesPlayer);

        //CheckForPlayer();

        if (enemyState == EnemyState.Searching)
        {
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= maxChaseTimer)
            {
                ChangeState(EnemyState.Idle);
            }
        }

        if (enemyState == EnemyState.Chasing)
        {
            ChasePlayer();
        }
    }

    bool CanSeePlayer()
    {
        Collider2D target = Physics2D.OverlapCircle(transform.position, viewDistance, playerLayer);

        if (target != null)
        {
            Vector2 dirToTarget = (target.transform.position - transform.position).normalized;
            Vector2 forward = facingDirection;
            float angleToTarget = Vector2.Angle(forward, dirToTarget);

            if (angleToTarget < viewAngle)
            {
                // Check if there are obstacles blocking view
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToTarget, viewDistance, obstacleLayer);
                if (hit.collider == null || hit.collider == target)
                {
                    playerPosition = target.transform;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }
    
    void HandlePlayerDetection(bool isDetected)
    {
        if (isDetected)
        {
            ChangeState(EnemyState.Chasing);
            chaseTimer = 0f;
        }
        else
        {
            if (enemyState == EnemyState.Chasing)
            {
                ChangeState(EnemyState.Searching);
            }
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (playerPosition.transform.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
        facingDirection = direction;
        UpdateAnimation(direction);
        anim.SetBool("isWalking", true);
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

        Vector2 forward = facingDirection;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, viewAngle) * forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -viewAngle) * forward * viewDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }

    void ChangeState(EnemyState newState)
    {
        //Exit current animation
        if (enemyState == EnemyState.Idle)
        {
            anim.SetBool("isWalking", false);
        }
        else if (enemyState == EnemyState.PlayerDetected)
        {
            anim.SetBool("isWalking", false);
        }
        else if (enemyState == EnemyState.Chasing)
        {
            anim.SetBool("isWalking", false);
        }
        else if (enemyState == EnemyState.Searching)
        {
            anim.SetBool("isWalking", false);
        }

        //Update current state
        enemyState = newState;

        if (enemyState == EnemyState.Idle)
        {
            anim.SetBool("isWalking", false);
            rb.linearVelocity = Vector2.zero;
            facingDirection = -transform.up;
        }
        else if (enemyState == EnemyState.PlayerDetected)
        {
            anim.SetBool("isWalking", false);
        }
        else if (enemyState == EnemyState.Chasing)
        {
            anim.SetBool("isWalking", true);
        }
        else if (enemyState == EnemyState.Searching)
        {
            anim.SetBool("isWalking", true);
        }
    }
}

public enum EnemyState
{
    Idle,
    PlayerDetected,
    Chasing,
    Searching
}
