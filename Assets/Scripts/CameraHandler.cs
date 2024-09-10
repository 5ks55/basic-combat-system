using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform viewTransform;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private float lookSensitivity = 0.5f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    [SerializeField] private PlayerController playerController;

    public Transform ViewTransform { get; private set; }
    public GameObject MainCamera { get; private set; }

    private void Awake()
    {
        ViewTransform = viewTransform;
        MainCamera = mainCamera;
    }

    private void Start()
    {
        OnMouseVisibilityChanged();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        Vector2 lookInput = playerController.GetPlayerInput().actions["Look"].ReadValue<Vector2>();
        xRotation -= lookInput.y * lookSensitivity;
        yRotation += lookInput.x * lookSensitivity;

        xRotation = Mathf.Clamp(xRotation, -30f, 70f);

        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);
        viewTransform.rotation = rotation;
    }

    public void OnMouseVisibilityChanged()
    {
        if (playerController.GetPlayerInput().actions["MouseVisibility"].IsPressed())
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}