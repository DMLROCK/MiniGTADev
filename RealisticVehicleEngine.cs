using UnityEngine;

public class RealisticVehicleEngine : MonoBehaviour
{
    public enum VehicleType { TeslaCybertruck, DodgeScatPack }
    [Header("--- AUTOMOTIVE SELECTION ---")]
    public VehicleType specificModel = VehicleType.DodgeScatPack;

    [Header("--- CORE PHYSICS CONTROLLER ---")]
    public Rigidbody vehicleRigidbody;
    public PlayerGTAController playerScript;
    public Transform mainCamera;

    // Advanced dynamic handling mechanics based on 2026 specs
    private float engineHorsepower;
    private float driftHandlingTraction;
    private float electronicTopSpeed;

    private void Start()
    {
        Configure2026VehicleProfiles();
    }

    private void Configure2026VehicleProfiles()
    {
        if (specificModel == VehicleType.TeslaCybertruck)
        {
            engineHorsepower = 3200f;       // Instant massive electric torque force
            driftHandlingTraction = 0.85f;  // Heavy, rigid AWD grip distribution
            electronicTopSpeed = 130f;
        }
        else if (specificModel == VehicleType.DodgeScatPack)
        {
            engineHorsepower = 2200f;       // Classic high rev performance curve
            driftHandlingTraction = 0.40f;  // Low rear wheel traction for sliding/burnouts
            electronicTopSpeed = 175f;
        }
    }

    private void FixedUpdate()
    {
        if (playerScript == null || !playerScript.isDriving) return;

        float steeringInput = Input.GetAxis("Horizontal");
        float acceleratorTrigger = Input.GetAxis("Xbox_RightTrigger");
        float brakeTrigger = Input.GetAxis("Xbox_LeftTrigger");

        // Keyboard support backup
        if (acceleratorTrigger == 0 && brakeTrigger == 0)
        {
            acceleratorTrigger = Input.GetKey(KeyCode.W) ? 1.0f : 0.0f;
            brakeTrigger = Input.GetKey(KeyCode.S) ? 1.0f : 0.0f;
        }

        // Apply dynamic driving momentum limits based on current setup metrics
        if (vehicleRigidbody.velocity.magnitude < electronicTopSpeed)
        {
            if (acceleratorTrigger > 0.05f)
                vehicleRigidbody.AddForce(transform.forward * acceleratorTrigger * engineHorsepower * Time.fixedDeltaTime, ForceMode.VelocityChange);
            if (brakeTrigger > 0.05f)
                vehicleRigidbody.AddForce(-transform.forward * brakeTrigger * engineHorsepower * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        // Handle realistic steering and drift calculations
        if (vehicleRigidbody.velocity.magnitude > 1.0f)
        {
            float dynamicTractionModifier = Vector3.Dot(transform.forward, vehicleRigidbody.velocity) >= 0 ? 1.0f : -1.0f;
            float steeringAngleCalculation = steeringInput * (driftHandlingTraction * 50f) * dynamicTractionModifier * Time.fixedDeltaTime;
            transform.Rotate(Vector3.up, steeringAngleCalculation);
        }
    }

    private void LateUpdate()
    {
        // Smooth camera track locked right over your vehicle trunk frame
        if (playerScript == null || !playerScript.isDriving || mainCamera == null) return;

        Vector3 cameraPositionTarget = transform.position + (transform.rotation * new Vector3(0, 4.5f, -8.5f));
        mainCamera.position = Vector3.Lerp(mainCamera.position, cameraPositionTarget, 0.2f);
        mainCamera.LookAt(transform.position + Vector3.up);
    }
}
