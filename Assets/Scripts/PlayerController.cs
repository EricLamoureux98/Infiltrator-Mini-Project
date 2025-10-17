using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed;

    Rigidbody2D rb;
    Animator anim;
    Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * speed;
    }
    
    public void Move(InputAction.CallbackContext context)
    {
        anim.SetBool("isWalking", true);

        if (context.canceled)
        {
            anim.SetBool("isWalking", false);
            anim.SetFloat("LastInputX", moveInput.x);
            anim.SetFloat("LastInputY", moveInput.y);
        }

        moveInput = context.ReadValue<Vector2>(); // Must be after if statement for idle to work
        anim.SetFloat("InputX", moveInput.x);
        anim.SetFloat("InputY", moveInput.y);
    }
}
