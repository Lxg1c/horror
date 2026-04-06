using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] private float sensitivity = 0.15f;

    [Header("Pitch Clamp")]
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private InputAction lookAction;
    private float pitch;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        lookAction = InputManager.Instance.actions.Player.Look;
        lookAction.Enable();
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        lookAction.Disable();
    }

    private void LateUpdate()
    {
        Vector2 delta = lookAction.ReadValue<Vector2>();

        // Мышь влево/вправо — крутим тело игрока (parent)
        transform.parent.Rotate(Vector3.up * delta.x * sensitivity);

        // Мышь вверх/вниз — наклоняем только камеру
        pitch -= delta.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
