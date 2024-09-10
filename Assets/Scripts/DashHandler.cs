using UnityEngine;

public class DashHandler : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private float dashForce = 360f;
    [SerializeField] private float dashCooldown = 1f;
    private bool readyToDash = true;
    private bool isDashing = false;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private MovementHandler movementHandler;

    public void OnDash()
    {
        if (readyToDash && movementHandler.IsGrounded() && !isDashing)
        {
            isDashing = true;
            readyToDash = false;

            // Applies forward force for dashing
            movementHandler.GetRigidbody().AddForce(transform.forward * dashForce, ForceMode.Impulse);

            // Adjusts speed based on sprint state
            movementHandler.SetSpeed(playerController.GetAction("Sprint").IsPressed() ? movementHandler.GetSprintSpeed() : movementHandler.GetMoveSpeed());

            Invoke(nameof(ResetDash), dashCooldown);
        }
    }

    private void ResetDash()
    {
        readyToDash = true;
        isDashing = false;
    }
}