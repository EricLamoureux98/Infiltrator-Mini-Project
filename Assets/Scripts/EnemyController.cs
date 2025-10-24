using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Behavior Timers")]
    [SerializeField] float maxSpottedTime;
    [SerializeField] float maxChaseTime;
    [SerializeField] float maxSearchTime;

    [Header("UI Components")]
    //[SerializeField] GameObject UIDetected;
    //[SerializeField] GameObject UIFound;     

    private EnemyMovement movement;
    private EnemyVision vision;

    private EnemyState currentState;
    private Coroutine searchCoroutine;

    // Timers
    private float spottedTimer;
    private float chaseTimer;


    void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        vision = GetComponent<EnemyVision>();
    }

    void Start()
    {
        ChangeState(EnemyState.Idle);
    }

    void Update()
    {
        bool seesPlayer = vision.CanSeePlayer();
        HandleStateLogic(seesPlayer);        
    }

    void HandleStateLogic(bool seesPlayer)
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle(seesPlayer);
                break;

            case EnemyState.Chasing:
                HandleChasing(seesPlayer);
                break;

            case EnemyState.Searching:
                HandleSearching(seesPlayer);
                break;
        }
    }

    void HandleIdle(bool seesPlayer)
    {
        movement.StopMoving();
        //UIDetected.SetActive(false);

        if (seesPlayer)
        {
            spottedTimer += Time.deltaTime;

            if (spottedTimer >= maxSpottedTime)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
        else
        {
            spottedTimer = 0;
        }
    }
    
    void HandleChasing(bool seesPlayer)
    {
        if (seesPlayer)
        {
            chaseTimer = 0f; // Reset since enemy still sees player
            movement.MoveTowards(vision.PlayerTransform.position);
            //UIFound.SetActive(true);
        }
        else
        {
            chaseTimer += Time.deltaTime;

            // Keep moving toward last known player position
            movement.MoveTowards(vision.LastKnownPlayerPos);

            if (chaseTimer >= maxChaseTime)
            {
                ChangeState(EnemyState.Searching);
            }
        }
    }

   void HandleSearching(bool seesPlayer)
    {

    }    
    
    void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        // stop any running search coroutine
        if (searchCoroutine != null)
        {
            StopCoroutine(searchCoroutine);
            searchCoroutine = null;
        }

        switch (newState)
        {
            case EnemyState.Idle:
                movement.StopMoving();
                //UIFound.SetActive(false);
                spottedTimer = 0f;
                chaseTimer = 0f;
                break;

            case EnemyState.Chasing:
                //UIFound.SetActive(true);
                break;

            case EnemyState.Searching:
                //UIFound.SetActive(false);
                searchCoroutine = StartCoroutine(SearchRoutine());
                break;
        }
    }

    IEnumerator SearchRoutine()
    {
        float elapsedTime = 0f;
        float rotationSpeed = 90f; // degrees per second
        float maxAngle = 60f; // how far left/right to rotate
        float currentAngle = 0f;
        float direction = 1f; // 1 for clockwise, -1 for counterclockwise

        // Move toward last known position
        while (Vector2.Distance(transform.position, vision.LastKnownPlayerPos) > 0.1f)
        {
            movement.MoveTowards(vision.LastKnownPlayerPos);
            yield return null;
        }

        // Stop movement and record the direction enemy arrived with
        movement.StopMoving();
        movement.SetFacingDirection((vision.LastKnownPlayerPos - (Vector2)transform.position).normalized);
        Vector2 baseFacing = movement.FacingDirection;
        yield return new WaitForSeconds(0.2f); // brief pause before scanning

        // Rotate back and forth to search
        while (elapsedTime < maxSearchTime)
        {
            elapsedTime += Time.deltaTime;

            currentAngle += rotationSpeed * direction * Time.deltaTime;

            if (Mathf.Abs(currentAngle) >= maxAngle)
            {
                // Flip direction smoothly at the edges
                currentAngle = Mathf.Clamp(currentAngle, -maxAngle, maxAngle);
                direction *= -1f;
            }

            // Rotate around the *base facing direction* (the one from when search began)
            Vector2 sweepFacing = Quaternion.Euler(0, 0, currentAngle) * baseFacing;
            movement.SetFacingDirection(sweepFacing);

            // Detect player mid-sweep
            if (vision.CanSeePlayer())
            {
                ChangeState(EnemyState.Chasing);
                yield break;
            }

            yield return null;
        }

        // Return to original facing before going idle
        movement.SetFacingDirection(baseFacing);
        ChangeState(EnemyState.Idle);
    }
}

public enum EnemyState
{
    Idle,
    PlayerDetected,
    Chasing,
    Searching
}
