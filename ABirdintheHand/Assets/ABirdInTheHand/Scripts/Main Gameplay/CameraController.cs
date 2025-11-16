using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First-person camera controller with mouse and gamepad support
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float controllerSensitivity = 250f;

    [Header("References")]
    public Transform orientation;

    private float xRotation;
    private float yRotation;
    private PlayerInput playerInput;
    private InputAction lookAction;
    private InputDevice lastUsedDevice;

    private bool isGamepad;

    private void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();

        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("[CameraController] PlayerInput or actions not found!", this);
            enabled = false;
            return;
        }

        lookAction = playerInput.actions["Look"];

        if (lookAction == null)
        {
            Debug.LogError("[CameraController] 'Look' action not found!", this);
            enabled = false;
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (lookAction == null)
            return;

        if (lookAction.activeControl != null)
        {
            InputDevice currentDevice = lookAction.activeControl.device;

            if (currentDevice != lastUsedDevice)
            {
                lastUsedDevice = currentDevice;
                isGamepad = currentDevice is Gamepad;
            }
        }

        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        float sensitivity = isGamepad ? controllerSensitivity : mouseSensitivity;
        float deltaTime = Time.deltaTime;

        float mouseX = lookInput.x * deltaTime * sensitivity;
        float mouseY = lookInput.y * deltaTime * sensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        if (orientation != null)
        {
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }

    public void SetImmediateOrientation(Quaternion cameraWorldRotation, Quaternion orientationWorldRotation)
    {
        if (orientation != null)
        {
            orientation.rotation = orientationWorldRotation;
        }

        transform.rotation = cameraWorldRotation;

        Vector3 camEuler = transform.eulerAngles;

        float pitch = camEuler.x;
        if (pitch > 180f)
        {
            pitch -= 360f;
        }

        xRotation = pitch;
        yRotation = camEuler.y;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }
}