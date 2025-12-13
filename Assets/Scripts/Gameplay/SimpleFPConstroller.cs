using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleFPController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject arOverlay;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.08f;
    [SerializeField] private float pitchClamp = 85f;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float yaw;
    private float pitch;

    private bool uiOpen;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // Initialize from current rotation
        yaw = transform.eulerAngles.y;
        pitch = cameraTransform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        SetUIOpen(false);
    }

    void Update()
    {
        // Movement (WASD) in XZ plane
        Vector3 forward = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;
        Vector3 right = new Vector3(cameraTransform.right.x, 0f, cameraTransform.right.z).normalized;

        Vector3 move = (forward * moveInput.y + right * moveInput.x) * (moveSpeed * Time.deltaTime);
        transform.position += move;

        // Look (mouse) only when UI is closed
        if (!uiOpen)
        {
            yaw += lookInput.x * mouseSensitivity;
            pitch -= lookInput.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    // --- Input System "Send Messages" callbacks ---
    // Names must match actions: OnMove, OnLook, OnToggleUI

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnToggleUI(InputValue value)
    {
        // Only trigger on press, not release
        if (value.isPressed)
            SetUIOpen(!uiOpen);
    }

    private void SetUIOpen(bool open)
    {
        uiOpen = open;

        if (arOverlay != null)
            arOverlay.SetActive(open);

        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
