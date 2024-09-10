using System.Collections;
using UnityEngine;

public class ChargedJumpHandler : MonoBehaviour
{
    [Header("Charged Jump")]
    [SerializeField] private float maxJumpForce = 20f; // Maximum force for the charged jump
    [SerializeField] private float chargeThreshold = 1f; // Time threshold to reach full charge
    [SerializeField] private float chargeIncreaseRate = 5f; // Rate at which jump force increases while charging
    private float currentJumpForce; 
    private bool isChargingJump = false; 
    private float jumpHoldTime = 0f; 

    [Header("Player Body Scaling")]
    [SerializeField] private float minScaleY = 0.6f; // Minimum Y scale during charging
    [SerializeField] private float scaleSpeed = 2f; // Speed at which body scales down during charge
    [SerializeField] private float resetScaleSpeed = 10f; // Speed at which body scales back to original size
    private Vector3 originalScale; 

    [SerializeField] private PlayerController playerController;
    [SerializeField] private MovementHandler movementHandler;

    private void Awake()
    {
        originalScale = playerController.GetPlayerBody().transform.localScale; // Store the original scale of the player body
    }

    private void Update()
    {
        HandleChargedJump(); // Process input and handle charged jump
    }

    public float GetCurrentJumpForce()
    {
        return currentJumpForce;
    }
    public void SetCurrentJumpForce(float value)
    {
        currentJumpForce = value;
    }

    public bool IsChargingJump()
    {
        return isChargingJump;
    }

    private void HandleChargedJump()
    {
        if (!movementHandler.IsGrounded()) return; // Ensure jump only happens when grounded


        if (playerController.GetAction("ChargedJump").IsPressed())
        {
            isChargingJump = true;
            jumpHoldTime += Time.deltaTime;

            // Increase jump force based on hold time, but clamp to maxJumpForce
            currentJumpForce = Mathf.Min(movementHandler.GetJumpForce() + chargeIncreaseRate * jumpHoldTime, maxJumpForce);

            // Scale down player body proportionally to jump hold time
            float targetScaleY = Mathf.Max(minScaleY, originalScale.y - (originalScale.y - minScaleY) * (jumpHoldTime / chargeThreshold));
            Vector3 targetScale = new Vector3(originalScale.x, targetScaleY, originalScale.z);
            playerController.GetPlayerBody().transform.localScale = Vector3.Lerp(playerController.GetPlayerBody().transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }
        else if (playerController.GetAction("ChargedJump").WasReleasedThisFrame() && isChargingJump)
        {
            // Trigger jump if charged sufficiently
            if (jumpHoldTime > chargeThreshold)
            {
                isChargingJump = false;
                movementHandler.OnJump();
            }
            ResetChargedJump(); // Reset the jump variables and scale
        }
    }

    private void ResetChargedJump()
    {
        isChargingJump = false;
        jumpHoldTime = 0f;
        currentJumpForce = movementHandler.GetJumpForce(); 

        StartCoroutine(ResetScale()); // Smoothly reset the player body scale to original
    }

    private IEnumerator ResetScale()
    {
        GameObject playerBody = playerController.GetPlayerBody();
        while (playerBody.transform.localScale != originalScale)
        {
            playerBody.transform.localScale = Vector3.Lerp(playerBody.transform.localScale, originalScale, Time.deltaTime * resetScaleSpeed);
            yield return null;
        }

        playerBody.transform.localScale = originalScale; // Ensure the scale is exactly reset
    }
}