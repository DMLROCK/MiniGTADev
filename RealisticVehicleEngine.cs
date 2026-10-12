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

    private float engineHorsepower;
    private float driftHandlingTraction;
    private float electronicTopSpeed;

    private void Start()
    {
        if (specificModel == VehicleType.TeslaCybertruck)
        {
            engineHorsepower = 3200f;       
            driftHandlingTraction = 0.85f;  
            electronicTopSpeed = 130f;
        }
        else if (specificModel == VehicleType.DodgeScatPack)
        {
            engineHorsepower = 2200f;       
            driftHandlingTraction = 0.40f;  
            electronicTopSpeed = 175f;
        }
    }

    private void Update()
    {
        // Listen for exit command if currently driving the car
        if (playerScript != null && playerScript.isDriving)
        {
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                ExitVehicle();
            }
        }
    }

    private void FixedUpdate()
    {
        if (playerScript == null || !playerScript.isDriving) return;

        float steeringInput = Input.GetAxis("Horizontal");
        float acceleratorTrigger = Input.GetAxis("Xbox_RightTrigger");
        float brakeTrigger = Input.GetAxis("Xbox_LeftTrigger");

        if (acceleratorTrigger == 0 && brakeTrigger == 0)
        {
            acceleratorTrigger = Input.GetKey(KeyCode.W) ? 1.0f : 0.0f;
            brakeTrigger = Input.GetKey(KeyCode.S) ? 1.0f : 0.0f;
        }

        if (vehicleRigidbody.velocity.magnitude < electronicTopSpeed)
        {
            if (acceleratorTrigger > 0.05f) vehicleRigidbody.AddForce(transform.forward * acceleratorTrigger * engineHorsepower * Time.fixedDeltaTime, ForceMode.VelocityChange);
            if (brakeTrigger > 0.05f) vehicleRigidbody.AddForce(-transform.forward * brakeTrigger * engineHorsepower * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        if (vehicleRigidbody.velocity.magnitude > 1.0f)
        {
            float dynamicTractionModifier = Vector3.Dot(transform.forward, vehicleRigidbody.velocity.normalized);
            float turningForce = steeringInput * (specificModel == VehicleType.DodgeScatPack ? 40f : 25f);
            transform.Rotate(Vector3.up, turningForce * driftHandlingTraction * Time.fixedDeltaTime * dynamicTractionModifier);
        }

        // Lock camera back relative to the speeding vehicle
        if (mainCamera != null)
        {
            Vector3 targetCamPos = transform.position - (transform.forward * 6f) + (Vector3.up * 2.5f);
            mainCamera.position = Vector3.Lerp(mainCamera.position, targetCamPos, 0.1f);
            mainCamera.LookAt(transform.position + (transform.forward * 2f));
        }
    }

    public void EnterVehicle()
    {
        if (playerScript == null) return;

        playerScript.isDriving = true;
        playerScript.gameObject.SetActive(false); // Hide the driver model inside the car
        
        // Attach driver transform to car so they stay anchored together
        playerScript.transform.SetParent(this.transform); 
    }

    public void ExitVehicle()
    {
        if (playerScript == null) return;

        playerScript.transform.SetParent(null); // Detach driver from car hierarchy
        
        // Calculate safe ground point out to the left side of the vehicle door
        Vector3 exitSideOffset = transform.position - (transform.right * 1.8f) + (Vector3.up * 0.5f);
        
        playerScript.TeleportAndExitVehicle(exitSideOffset);
    }
}
