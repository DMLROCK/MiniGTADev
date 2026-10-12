using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RealisticVehicleEngine : MonoBehaviour
{
    public enum VehicleType { TeslaCybertruck, DodgeScatPack }

    [Header("--- AUTOMOTIVE SELECTION ---")]
    public VehicleType specificModel = VehicleType.DodgeScatPack;

    [Header("--- CORE PHYSICS CONTROLLER ---")]
    public Rigidbody vehicleRigidbody;
    public PlayerGTAController playerScript;
    public Transform mainCamera;

    [Header("--- VEHICLE TUNING ---")]
    [Tooltip("Approximate maximum speed in metres per second.")]
    public float cybertruckTopSpeed = 58f;
    public float scatPackTopSpeed = 78f;
    public float acceleration = 18f;
    public float brakingAcceleration = 28f;
    public float steeringDegreesPerSecond = 75f;

    private float driftHandlingTraction;
    private float electronicTopSpeed;

    private void Awake()
    {
        if (vehicleRigidbody == null)
            vehicleRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (specificModel == VehicleType.TeslaCybertruck)
        {
            driftHandlingTraction = 0.85f;
            electronicTopSpeed = cybertruckTopSpeed;
        }
        else
        {
            driftHandlingTraction = 0.40f;
            electronicTopSpeed = scatPackTopSpeed;
        }
    }

    private void FixedUpdate()
    {
        if (playerScript == null || !playerScript.isDriving || vehicleRigidbody == null)
            return;

        float steeringInput = Input.GetAxis("Horizontal");
        float acceleratorInput = GetAxisIfAvailable("Xbox_RightTrigger");
        float brakeInput = GetAxisIfAvailable("Xbox_LeftTrigger");

        // These custom axes are not present in Unity by default. Keyboard fallback
        // is used when they are missing or have no input.
        if (acceleratorInput <= 0.01f && brakeInput <= 0.01f)
        {
            acceleratorInput = Input.GetKey(KeyCode.W) ? 1f : 0f;
            brakeInput = Input.GetKey(KeyCode.S) ? 1f : 0f;
        }

        Vector3 forwardVelocity = Vector3.Project(vehicleRigidbody.velocity, transform.forward);
        float forwardSpeed = forwardVelocity.magnitude;

        if (acceleratorInput > 0.05f && vehicleRigidbody.velocity.magnitude < electronicTopSpeed)
            vehicleRigidbody.AddForce(transform.forward * acceleratorInput * acceleration, ForceMode.Acceleration);

        if (brakeInput > 0.05f)
            vehicleRigidbody.AddForce(-transform.forward * brakeInput * brakingAcceleration, ForceMode.Acceleration);

        // Limit forward speed without abruptly blocking reverse movement.
        Vector3 horizontalVelocity = new Vector3(vehicleRigidbody.velocity.x, 0f, vehicleRigidbody.velocity.z);
        if (horizontalVelocity.magnitude > electronicTopSpeed)
        {
            Vector3 limited = horizontalVelocity.normalized * electronicTopSpeed;
            vehicleRigidbody.velocity = new Vector3(limited.x, vehicleRigidbody.velocity.y, limited.z);
        }

        if (vehicleRigidbody.velocity.magnitude > 1f)
        {
            float directionSign = Vector3.Dot(transform.forward, vehicleRigidbody.velocity) >= 0f ? 1f : -1f;
            float tractionSteering = Mathf.Lerp(steeringDegreesPerSecond, steeringDegreesPerSecond * 0.65f,
                driftHandlingTraction);
            transform.Rotate(Vector3.up, steeringInput * tractionSteering * directionSign * Time.fixedDeltaTime);
        }
    }

    private float GetAxisIfAvailable(string axisName)
    {
        try
        {
            return Mathf.Clamp01(Input.GetAxis(axisName));
        }
        catch (System.ArgumentException)
        {
            return 0f;
        }
    }

    private void LateUpdate()
    {
        if (playerScript == null || !playerScript.isDriving || mainCamera == null)
            return;

        Vector3 targetPosition = transform.position +
                                 transform.rotation * new Vector3(0f, 4.5f, -8.5f);
        mainCamera.position = Vector3.Lerp(mainCamera.position, targetPosition, 8f * Time.deltaTime);
        mainCamera.LookAt(transform.position + Vector3.up);
    }
}
