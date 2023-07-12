using UnityEngine;

public class Player_Look : MonoBehaviour
{
    // Public variables
    public float _verticalMouseSensitivity;
    public float _horizontalMouseSensitivity;
    public Transform _playerBody;

    // Private variables
    float xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Get mouse Input
        float mouseX = Input.GetAxis("Mouse X") * _horizontalMouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _verticalMouseSensitivity * Time.deltaTime;

        // Apply vertical mouse Input
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Apply horizontal mouse Input
        _playerBody.Rotate(Vector3.up * mouseX);
    }
}