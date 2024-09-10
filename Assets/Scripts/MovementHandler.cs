using UnityEngine;

public class MovementHandler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f; 
    [SerializeField] private float sprintSpeed = 40f; 
    [SerializeField] private float groundDrag = 5f; 
    [SerializeField] private float airMultiplier = 0.4f; 


    private Rigidbody rb;
    private Vector3 moveDirection;

    [Header("Bunny Hop")]
    [SerializeField] private float bunnyHopBonus = 2f; 
    [SerializeField] private float bunnyHopWindow = 0.3f; 
    [SerializeField] private float maxBunnyHopBonus = 10f; 
    private float currentBunnyHopBonus = 0f;
    private bool isBunnyHopping = false;
    private float lastJumpTime = 0f;

    private float speed; // Current movement speed


    [SerializeField] private PlayerController playerController;
    [SerializeField] private ChargedJumpHandler chargedJumpHandler;
    [SerializeField] private ClimbingHandler climbingHandler;
    [SerializeField] private CameraHandler cameraHandler;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f; // Height of the player for ground check
    [SerializeField] private LayerMask whatIsGround; // Layers considered as ground
    private bool isGrounded;

    private bool readyToJump = true;
    [SerializeField] private float jumpForce = 12f; // Default jump force
    [SerializeField] private float jumpCooldown = 0.25f; // Cooldown between jumps

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForce = 10f;  
    [SerializeField] private Vector3 wallJumpDirection = new Vector3(1, 1, 0);  


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevents the rigidbody from rotating due to physics
    }

    private void Start()
    {
        ResetJump();
    }

    private void Update()
    {
        isGrounded = IsGrounded(); // Check if the player is grounded
        SpeedControl(); // Adjusts the player's speed based on state

        if (isGrounded)
        {
            rb.drag = groundDrag; // Applies drag when on the ground

            // Resets bunny hop bonus if the player has not jumped again within the window
            if (isBunnyHopping && Time.time - lastJumpTime > bunnyHopWindow)
            {
                currentBunnyHopBonus = 0f;
                isBunnyHopping = false;
            }
        }
        else
            rb.drag = 0; // No drag in the air
    }

    private void FixedUpdate()
    {
        if(!climbingHandler.IsClimbing())
            MovePlayer(); // Move player only if not climbing
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
    public float GetSprintSpeed()
    {
        return sprintSpeed;
    }

    public Rigidbody GetRigidbody() 
    {
        return rb;
    }

    public void SetSpeed(float _speed)
    {
        speed = _speed;
    }

    public float GetJumpForce()
    {
        return jumpForce;
    }

    private void MovePlayer()
    {
        // Get movement input from player
        Vector2 moveInput = playerController.GetAction("Move").ReadValue<Vector2>();
        Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y);

        if (inputDirection != Vector3.zero)
        {
            // Calculate and set player rotation based on input direction
            float targetRotation = Quaternion.LookRotation(inputDirection).eulerAngles.y + cameraHandler.MainCamera.transform.rotation.eulerAngles.y;
            Quaternion targetRotationQuat = Quaternion.Euler(0, targetRotation, 0);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotationQuat, 20f * Time.deltaTime));

            // Calculate forward and right vectors based on camera orientation
            Vector3 forward = cameraHandler.MainCamera.transform.forward;
            forward.y = 0; 
            forward.Normalize();

            Vector3 right = cameraHandler.MainCamera.transform.right;
            right.y = 0; 
            right.Normalize();

            // Combine forward and right vectors with input to get movement direction
            moveDirection = (forward * moveInput.y + right * moveInput.x).normalized * (speed + currentBunnyHopBonus);
        }
        else
        {
            moveDirection = Vector3.zero; // No movement if no input
        }

        // Apply movement force with different multipliers for ground and air
        if (isGrounded)
        {
            rb.AddForce(moveDirection.normalized * (speed + currentBunnyHopBonus) * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * (speed + currentBunnyHopBonus) * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Clamp the player's horizontal speed to max speed plus bunny hop bonus
        if (flatVel.magnitude > moveSpeed + currentBunnyHopBonus)
        {
            Vector3 limitedVel = flatVel.normalized * (moveSpeed + currentBunnyHopBonus);
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }

        // Adjust speed based on sprint input when grounded
        if (readyToJump && isGrounded)
        {
            speed = playerController.GetAction("Sprint").IsPressed() ? sprintSpeed : moveSpeed;
        }

    }

    public bool IsGrounded()
    {
        // Raycast to check if player is grounded
        return Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
    }

    public void OnJump()
    {
        // Jump only if ready and not currently charging a charged jump
        if (!readyToJump || chargedJumpHandler.IsChargingJump())
            return;

        if (isGrounded || climbingHandler.IsTouchingWall()) 
        {
            readyToJump = false;
            if (isGrounded)
                Jump();
            else
                WallJump(); 
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // Apply jump force with current  charged force
            rb.AddForce(transform.up * chargedJumpHandler.GetCurrentJumpForce(), ForceMode.Impulse);

            // Apply bunny hop bonus if within time window of last jump
            if (Time.time - lastJumpTime <= bunnyHopWindow)
            {
                currentBunnyHopBonus = Mathf.Min(currentBunnyHopBonus + bunnyHopBonus, maxBunnyHopBonus); 
                isBunnyHopping = true;
            }
            else
            {
                currentBunnyHopBonus = 0f; 
                isBunnyHopping = false;
            }

            lastJumpTime = Time.time; // Record the time of the jump
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
        chargedJumpHandler.SetCurrentJumpForce(jumpForce);
    }

    private void WallJump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Reset vertical velocity before wall jump

        // Calculate and apply wall jump force based on player and wall orientation
        Vector3 jumpDirection = (transform.up + climbingHandler.GetFrontWallHit().normal).normalized;
        rb.AddForce(jumpDirection * wallJumpForce, ForceMode.Impulse);

        rb.useGravity = true;
    }
}