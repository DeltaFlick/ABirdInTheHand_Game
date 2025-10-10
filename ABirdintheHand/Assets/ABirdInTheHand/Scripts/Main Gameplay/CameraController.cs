using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <Summary>
/// 1st Person Camera Controller
/// </Summary>

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float controllerSensitivity = 250f;

    public Transform orientation;

    private float xRotation;
    private float yRotation;

    private PlayerInput playerInput;
    private InputAction lookAction;
    private InputDevice lastUsedDevice;

    private void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();
        lookAction = playerInput.actions["Look"];
    }

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (lookAction.activeControl != null)
        {
            lastUsedDevice = lookAction.activeControl.device;
        }

        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        float sensitivity = mouseSensitivity;
        if (lastUsedDevice != null && lastUsedDevice is Gamepad)
        {
            sensitivity = controllerSensitivity;
        }

        float mouseX = lookInput.x * Time.deltaTime * sensitivity;
        float mouseY = lookInput.y * Time.deltaTime * sensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        if (orientation != null)
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void SetImmediateOrientation(Quaternion cameraWorldRotation, Quaternion orientationWorldRotation)
    {
        if (orientation != null)
            orientation.rotation = orientationWorldRotation;

        transform.rotation = cameraWorldRotation;

        Vector3 camEuler = transform.eulerAngles;

        float pitch = camEuler.x;
        if (pitch > 180f) pitch -= 360f;

        xRotation = pitch;
        yRotation = camEuler.y;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }
}
