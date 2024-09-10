using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    [Header("Movement")]
    [SerializeField] private MovementHandler movementHandler;

    [Header("Camera")]
    [SerializeField] private CameraHandler cameraHandler; 
    
    [Header("Flip")]
    [SerializeField] private FlipHandler flipHandler;

    [Header("Dash")]
    [SerializeField] private DashHandler dashHandler;

    [Header("Player Body")]
    [SerializeField] private GameObject playerBody; 

    [Header("Climbing")]
    [SerializeField] private ClimbingHandler climbingHandler;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        InitializeInputActions(); // Sets up input actions for player control
    }

    private void InitializeInputActions()
    {
        // Bind input actions to corresponding handlers
        playerInput.actions["Jump"].performed += ctx => movementHandler.OnJump();
        playerInput.actions["Dash"].performed += ctx => dashHandler.OnDash();
        playerInput.actions["MouseVisibility"].performed += ctx => cameraHandler.OnMouseVisibilityChanged();
        playerInput.actions["FlipForward"].performed += ctx => flipHandler.OnFlip(1f);
        playerInput.actions["FlipBackward"].performed += ctx => flipHandler.OnFlip(-1f);
    }  
    
    public PlayerInput GetPlayerInput()
    {
        return playerInput;
    }

    public InputAction GetAction(string nameAction)
    {
        return playerInput.actions[nameAction];
    }

    public GameObject GetPlayerBody()
    {
        return playerBody;
    }
}