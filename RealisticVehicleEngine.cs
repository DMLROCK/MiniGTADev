using UnityEngine;

public class RealisticVehicleEngine : MonoBehaviour
{
    public enum VehicleType { TeslaCybertruck, DodgeScatPack, PoliceCruiser }
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
        else if (specificModel == VehicleType.PoliceCruiser)
        {
            engineHorsepower = 2600f;
            driftHandlingTraction = 0.65f;
            electronicTopSpeed = 160f;
        }
    }

    private void Update()
    {
        if (playerScript != null && playerScript.isDriving)
        {
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton2)) ExitVehicle();
        }
    }

    private void FixedUpdate()
    {
        if (specificModel == VehicleType.PoliceCruiser && playerScript != null && playerScript.wantedLevel > 0)
        {
            ExecutePoliceAIChase();
            return;
        }

        if (playerScript == null || !playerScript.isDriving) return;

        float steeringInput = Input.GetAxis("Horizontal");
        float acceleratorTrigger = Input.GetAxis("Xbox_RightTrigger");
        float brakeTrigger = Input.GetAxis("Xbox_LeftTrigger");

        if (acceleratorTrigger == 0 && brakeTrigger == 0)
        {
            acceleratorTrigger = Input.GetKey(KeyCode.W) ? 1.0f : 0.0f;
            brakeTrigger = Input.GetKey(KeyCode.S) ? 1.0f : 0.0f;
        }

        if (vehicleRigidbody.velocity.magnitude * 2.23694f > 90f && playerScript.wantedLevel < 2)
        {
            playerScript.TriggerCrimeAlert(2);
        }

        if (vehicleRigidbody.velocity.magnitude < electronicTopSpeed)
        {
            if (acceleratorTrigger > 0.05f) vehicleRigidbody.AddForce(transform.forward * acceleratorTrigger * engineHorsepower * Time.fixedDeltaTime, ForceMode.VelocityChange);
            if (brakeTrigger > 0.05f) vehicleRigidbody.AddForce(-transform.forward * brakeTrigger * engineHorsepower * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        if (vehicleRigidbody.velocity.magnitude > 1.0f)
        {
            float dynamicTractionModifier = Vector3.Dot(transform.forward, vehicleRigidbody.velocity.normalized);
            float turningForce = steeringInput * (specificModel == VehicleType.DodgeScatPack ? 42f : 28f);
            transform.Rotate(Vector3.up, turningForce * driftHandlingTraction * Time.fixedDeltaTime * dynamicTractionModifier);
        }

        if (mainCamera != null)
        {
            Vector3 targetCamPos = transform.position - (transform.forward * 6.5f) + (Vector3.up * 2.8f);
            mainCamera.position = Vector3.Lerp(mainCamera.position, targetCamPos, 0.12f);
            mainCamera.LookAt(transform.position + (transform.forward * 2.5f));
        }
    }

    private void ExecutePoliceAIChase()
    {
        Vector3 playerPos = playerScript.transform.position;
        transform.LookAt(new Vector3(playerPos.x, transform.position.y, playerPos.z));
        
        if (Vector3.Distance(transform.position, playerPos) > 4f)
        {
            vehicleRigidbody.AddForce(transform.forward * engineHorsepower * 0.45f * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
        else if (!playerScript.isDriving)
        {
            playerScript.wantedLevel = 0;
            playerScript.inventoryData.cashBalance = Mathf.Max(0, playerScript.inventoryData.cashBalance - 1000);
            playerScript.inventoryData.UpdateHUD();
            playerScript.transform.position = new Vector3(0, 1, 0); 
        }
    }

    public void EnterVehicle()
    {
        if (playerScript == null) return;
        playerScript.isDriving = true;
        playerScript.gameObject.SetActive(false);
        playerScript.transform.SetParent(this.transform); 
    }

    public void ExitVehicle()
    {
        if (playerScript == null) return;
        playerScript.transform.SetParent(null); 
        playerScript.TeleportAndExitVehicle(transform.position - (transform.right * 1.8f) + (Vector3.up * 0.5f));
    }
}
