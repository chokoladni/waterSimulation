using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public float sensitivity = 1.0f;

    public float zoomStrength = 0.2f;
    public float zoomSpeed = 3.0f;
    public float minCameraDistance = 5.0f;
    public float maxCameraDistance = 50.0f;

    private Vector2 scroll;
    private Vector2 delta;

    private CameraInputActions cameraInputs;

    private Transform cameraTransform;

    private float mousePosX, mousePosY;

    private float targetCameraDistance;

    private bool locked;

    private void Awake() {
        cameraInputs = new CameraInputActions();
        cameraInputs.Enable();

        cameraInputs.Camera.zoom.performed += ctx => scroll = ctx.ReadValue<Vector2>();
        cameraInputs.Camera.rotate.performed += ctx => delta = ctx.ReadValue<Vector2>();
        cameraInputs.Camera.disableMovement.performed += ctx => locked = !locked;

        Cursor.lockState = CursorLockMode.Locked;

        cameraTransform = GetComponentInChildren<Camera>().transform;

        targetCameraDistance = Mathf.Clamp(targetCameraDistance, minCameraDistance, maxCameraDistance);
    }


    private void Update() {
        transform.position = target.position;

        if (delta.magnitude > 0 && !locked) {
            mousePosX += delta.x * sensitivity;
            mousePosY += delta.y * sensitivity;
            mousePosY = Mathf.Clamp(mousePosY, -90, -10);
            transform.localRotation = Quaternion.Euler(mousePosY, mousePosX, 0);
        }

        if (scroll.y != 0 && !locked) {
            float direction = scroll.y > 0 ? -1 : 1;

            targetCameraDistance = Mathf.Clamp(cameraTransform.localPosition.z + direction * zoomSpeed, minCameraDistance, maxCameraDistance);
        }

        float distance = Mathf.Abs(cameraTransform.localPosition.z - targetCameraDistance);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, new Vector3(cameraTransform.localPosition.x, cameraTransform.localPosition.y, targetCameraDistance), zoomStrength);
    }
}
