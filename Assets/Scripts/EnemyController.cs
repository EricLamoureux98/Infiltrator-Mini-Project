using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Detection Stats")]
    [SerializeField] float speed;
    [SerializeField] float maxSpottedTime;
    [SerializeField] float maxChaseTime;
    [SerializeField] float maxSearchTime;

    [Header("FOV Cone")]
    [SerializeField] float viewDistance = 8f;
    [SerializeField] float viewAngle = 30f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] LayerMask playerLayer;

    [Header("UI Components")]
    [SerializeField] GameObject UIDetected;
    [SerializeField] GameObject UIFound;     

    // Components
    private Transform playerTransform;
    private Animator anim;
    private Rigidbody2D rb;
    private EnemyState enemyState;

    // Player Detection
    private Vector2 facingDirection;
    private Vector2 lastFacingDirection;
    private Vector2 lastKnowPlayerPos;
    private bool seesPlayer;

    // Timers
    private float chaseTimer;
    private float spottedTimer;

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

        if (enemyState == EnemyState.Chasing)
        {
            ChasePlayer();
        }

        if (enemyState == EnemyState.Searching)
        {
            Vector2 dir = lastKnowPlayerPos - (Vector2)transform.position;
            if (dir.magnitude > 0.1f)
            {
                rb.linearVelocity = dir.normalized * speed;
                facingDirection = dir.normalized;
                UpdateAnimation(dir);
            }
            
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= maxChaseTime)
            {
                ChangeState(EnemyState.Idle);
            }
        }
        
    }

    bool CanSeePlayer()
    {
        // Returns false if no player is detected in circles radius
        Collider2D target = Physics2D.OverlapCircle(transform.position, viewDistance, playerLayer);

        if (target != null)
        {
            /* Creates a direction vector from the enemy to the player
               .normalized makes the vector length 1 so it can be used for angle math and raycasts. */
            Vector2 dirToTarget = (target.transform.position - transform.position).normalized;
            Vector2 forward = facingDirection;
            float angleToTarget = Vector2.Angle(forward, dirToTarget);

            if (angleToTarget < viewAngle)
            {
                // Check if there are obstacles blocking view
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToTarget, viewDistance, obstacleLayer);

                //If it hits nothing (air) or hits the player first
                if (hit.collider == null || hit.collider == target)
                {
                    playerTransform = target.transform;
                    lastKnowPlayerPos = target.transform.position;
                    return true;
                }
            }
        }
        return false;
    }
    
    void HandlePlayerDetection(bool isDetected)
    {
        if (isDetected)
        {
            UIDetected.SetActive(true);
            spottedTimer += Time.deltaTime;
            if (spottedTimer >= maxSpottedTime)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
        else
        {
            UIDetected.SetActive(false);
            spottedTimer = 0f;
            if (enemyState == EnemyState.Chasing)
            {
                ChangeState(EnemyState.Searching);
            }
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
        facingDirection = direction;
        lastFacingDirection = facingDirection;
        UpdateAnimation(direction);
        anim.SetBool("isWalking", true);
    }

    IEnumerator SearchForPlayer()
    {
        float elapsedTime = 0f;
        float maxAngle = 60f; // how far left/right to rotate
        float rotationSpeed = 90f; // degrees per second
        float direction = 1f; // 1 for clockwise, -1 for counterclockwise
        float currentAngle = 0f;

        while (elapsedTime < maxSearchTime)
        {
            elapsedTime += Time.deltaTime;

            // gradually rotate
            currentAngle += rotationSpeed * direction * Time.deltaTime;

            // flip direction if we hit a boundary of maxAngle (sweeping motion)
            if (Mathf.Abs(currentAngle) >= maxAngle)
            {
                currentAngle = Mathf.Clamp(currentAngle, -maxAngle, maxAngle);
                direction *= -1f; // reverse rotation
            }

            facingDirection = Quaternion.Euler(0, 0, currentAngle) * lastFacingDirection;

            if (CanSeePlayer())
            {
                ChangeState(EnemyState.Chasing);
                yield break; // stop searching early if player is found
            }

            yield return null;
        }

        // if timer runs out, stop searching
        ChangeState(EnemyState.Idle);
    }

    void UpdateAnimation(Vector2 direction)
    {
        anim.SetFloat("MoveX", direction.x);
        anim.SetFloat("MoveY", direction.y);
    }

    void ChangeState(EnemyState newState)
    {
        enemyState = newState;

        // walking is true if chasing or searching
        bool walking = newState == EnemyState.Chasing || newState == EnemyState.Searching;
        anim.SetBool("isWalking", walking);

        if (newState == EnemyState.Searching)
        {
            StartCoroutine(SearchForPlayer());
        }

        if (newState == EnemyState.Idle)
        {
            StopCoroutine(SearchForPlayer());
            rb.linearVelocity = Vector2.zero;
            anim.SetFloat("LastMoveX", lastFacingDirection.x);
            anim.SetFloat("LastMoveY", lastFacingDirection.y);
        }

        chaseTimer = 0f;
        spottedTimer = 0f;
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

        Gizmos.DrawWireSphere(lastKnowPlayerPos, 0.5f);
    }
}

public enum EnemyState
{
    Idle,
    PlayerDetected,
    Chasing,
    Searching
}
