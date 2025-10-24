using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed;

    private Rigidbody2D rb;
    private Animator anim;

    public Vector2 FacingDirection { get; private set; } = Vector2.down;
    public Vector2 LastFacingDirecion { get; private set; } = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void MoveTowards(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;

        // Stop if close enough
        if (dir.magnitude < 0.1f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        dir.Normalize();
        rb.linearVelocity = dir * moveSpeed;

        if (dir.sqrMagnitude > 0.001f)
        {
            SetFacingDirection(dir);
            FacingDirection = dir;
            LastFacingDirecion = dir;
            UpdateAnimation(dir);
        }
    }

    public void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void SetFacingDirection(Vector2 dir)
    {
        FacingDirection = dir.normalized;
        UpdateAnimation(dir);
    }

    public void UpdateAnimation(Vector2 dir)
    {
        anim.SetFloat("MoveX", dir.x);
        anim.SetFloat("MoveY", dir.y);
        anim.SetBool("isWalking", rb.linearVelocity.magnitude > 0.1f);
    }
}
