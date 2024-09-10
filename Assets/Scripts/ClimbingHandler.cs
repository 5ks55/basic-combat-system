using UnityEngine;

public class ClimbingHandler : MonoBehaviour
{
    [Header("Climbing")]
    [SerializeField] private float climbSpeed = 4f;
    [SerializeField] private float maxClimbTime = 3f;
    private float climbTimer;
    private bool climbing;

    [Header("Wall Detection")]
    [SerializeField] private float detectionLength = 0.3f;
    [SerializeField] private float sphereCastRadius = 0.5f;
    [SerializeField] private float maxWallLookAngle = 45f;

    [SerializeField] private LayerMask whatIsWall;
    private RaycastHit frontWallHit;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private MovementHandler movementHandler; 
    [SerializeField] private CameraHandler cameraHandler;

    private void Update()
    {
        SetClimbTimerOnGround(); // Reset timer when grounded
        StateMachine(); // Handles climbing logic
    }

    private void FixedUpdate()
    {
        if (climbing)
            ClimbingMovement(); // Apply climbing movement
    }

    public bool IsClimbing()
    {
        return climbing;
    }

    public RaycastHit GetFrontWallHit()
    {
        return frontWallHit;
    }

    private void StateMachine()
    {
        // Start climbing if facing a wall, moving forward, and within allowable angle
        if (IsWallFront() && playerController.GetAction("Move").ReadValue<Vector2>().y > 0 && GetWallLookAngle() < maxWallLookAngle)
        {
            if (!climbing && climbTimer > 0) StartClimbing();

            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) StopClimbing();
        }
        else
        {
            if (climbing) StopClimbing(); // Stop if conditions aren't met
        }
    }

    private void SetClimbTimerOnGround()
    {
        if (movementHandler.IsGrounded())
        {
            climbTimer = maxClimbTime; // Refill climbing time
        }
    }

    private float GetWallLookAngle()
    {
        return Vector3.Angle(cameraHandler.ViewTransform.forward, -frontWallHit.normal);
    }

    private bool IsWallFront()
    {
        // Detects if there's a wall in front using a sphere cast
        return Physics.SphereCast(transform.position, sphereCastRadius, cameraHandler.ViewTransform.forward, out frontWallHit, detectionLength, whatIsWall);
    }

    public bool IsTouchingWall()
    {
        return IsWallFront() && GetWallLookAngle() < maxWallLookAngle;
    }

    private void StartClimbing()
    {
        climbing = true;
        movementHandler.GetRigidbody().useGravity = false;
    }

    private void ClimbingMovement()
    {
        Rigidbody rb = movementHandler.GetRigidbody();
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z); // Move up while climbing
    }

    private void StopClimbing()
    {
        climbing = false;
        movementHandler.GetRigidbody().useGravity = true;
    }
}