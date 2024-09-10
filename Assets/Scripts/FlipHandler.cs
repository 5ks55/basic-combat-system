using System.Collections;
using UnityEngine;

public class FlipHandler : MonoBehaviour
{
    [Header("Flip")]
    [SerializeField] private float flipDuration = 0.5f; // Duration of the flip
    [SerializeField] private float flipCooldown = 0.5f; // Time between flips
    private bool isFlipping = false;
    private float nextFlipTime = 0f;
    private Vector3 initialRotation;
    private Vector3 flipAxis;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private MovementHandler movementHandler;
    [SerializeField] private ClimbingHandler climbingHandler;

    public void OnFlip(float direction)
    {
        if (Time.time >= nextFlipTime && !isFlipping && !movementHandler.IsGrounded() && !climbingHandler.IsClimbing())
        {
            isFlipping = true;
            nextFlipTime = Time.time + flipCooldown;

            flipAxis = new Vector3(direction, 0, 0);

            initialRotation = playerController.GetPlayerBody().transform.localEulerAngles;

            StartCoroutine(PerformFlip());
        }
    }

    private IEnumerator PerformFlip()
    {
        float elapsedTime = 0f;
        float fullRotationAmount = 360f; 
        Vector3 targetRotation = initialRotation + (flipAxis * fullRotationAmount);

        while (elapsedTime < flipDuration)
        {
            // Smoothly interpolate rotation
            playerController.GetPlayerBody().transform.localEulerAngles = Vector3.Lerp(initialRotation, targetRotation, elapsedTime / flipDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation
        playerController.GetPlayerBody().transform.localEulerAngles = targetRotation;

        isFlipping = false;
    }
}