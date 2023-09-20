using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    public float power = 2000.0f;
    public float maxRudderDeviation = 10.0f;
    public float rudderSpeed = 2.0f;

    private BoatInputActions boatInputs;

    private Rigidbody boat;

    private float accelerationInput = 0;
    private float rotationInput = 0;

    private Vector3 startingRotation;

    private void Awake() {
        boat = GetComponentInParent<Rigidbody>();

        boatInputs = new BoatInputActions();
        boatInputs.Enable();
        boatInputs.Boat.Accelerate.performed += ctx => accelerationInput = ctx.ReadValue<float>();
        boatInputs.Boat.Rotate.performed += ctx => rotationInput = ctx.ReadValue<float>();
        boatInputs.Boat.Pause.performed += ctx => togglePause();
        boatInputs.Boat.Quit.performed += ctx => Application.Quit();
        
        startingRotation = transform.localRotation.eulerAngles;
    }

    private void togglePause() {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
    }

    private void FixedUpdate() {
        boat.AddForceAtPosition(accelerationInput * power * transform.forward, transform.position);

        Quaternion targetRotation = Quaternion.Euler(startingRotation.x, startingRotation.y - rotationInput * maxRudderDeviation, startingRotation.z);


        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, rudderSpeed * Time.fixedDeltaTime);
    }
}
