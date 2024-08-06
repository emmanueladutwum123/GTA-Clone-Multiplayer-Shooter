using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [Header("Player Movement")]
    public float playerSpeed = 1.1f;
    public float playerSprint = 5f;

    [Header("Player Animator & Gravity")]
    public CharacterController cC;
    public float gravity = -9.81f;
    public Animator animator;

    [Header("Player Script Camera")]
    public Transform playerCamera;

    [Header("Player Jumping & Velocity")]
    public float jumpHeight = 1f;
    public float turnCalmTime = 0.1f;
    private float turnCalmVelocity;
    private Vector3 velocity;
    public Transform surfaceCheck;
    private bool onSurface;
    public float surfaceDistance = 0.4f;
    public LayerMask surfaceMask;

    private void Start()
    {
        if (cC == null)
        {
            cC = GetComponent<CharacterController>();
        }
    }

    private void Update()
    {
        if (surfaceCheck != null)
        {
            onSurface = Physics.CheckSphere(surfaceCheck.position, surfaceDistance, surfaceMask);
        }
        else
        {
            Debug.LogWarning("surfaceCheck Transform has not been assigned in the Inspector", this);
        }

        if (onSurface && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        ApplyGravity();
        playerMove();

        if (Input.GetKeyDown(KeyCode.Space) && onSurface)
        {
            Jump();
        }

        Sprint();
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        cC.Move(velocity * Time.deltaTime);
    }

    void playerMove()
    {
        float horizontal_axis = Input.GetAxisRaw("Horizontal");
        float vertical_axis = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal_axis, 0f, vertical_axis).normalized;

        if (direction.magnitude >= 0.1f)
        {   
            animator.SetBool("Walk", true);
            animator.SetBool("Running", false);

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalmVelocity, turnCalmTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            cC.Move(moveDirection.normalized * playerSpeed * Time.deltaTime);
            jumpHeight = 0f;
        }
        else
        {   
            animator.SetBool("Walk", false);
            animator.SetBool("Running", false);
            jumpHeight = 1f;
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && onSurface)
        {
            animator.SetBool("Idle", false);
            animator.SetTrigger("Jump");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            animator.SetBool("Idle", true);
            animator.ResetTrigger("Jump");
        }
    }

    void Sprint()
    {
        if (Input.GetButton("Sprint") && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && onSurface)
        {
            float horizontal_axis = Input.GetAxisRaw("Horizontal");
            float vertical_axis = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal_axis, 0f, vertical_axis).normalized;

            if (direction.magnitude >= 0.1f)
            {   
                animator.SetBool("Walk", false);
                animator.SetBool("Running", true);

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalmVelocity, turnCalmTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                cC.Move(moveDirection.normalized * playerSprint * Time.deltaTime);
                jumpHeight = 0f;
            }
            else
            {
               animator.SetBool("Walk", true);
               animator.SetBool("Running", false);
               jumpHeight = 1f;
            }
        }
    }
}
