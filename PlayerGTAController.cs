using UnityEngine;

public class PlayerGTAController : MonoBehaviour
{
    [Header("--- GTA PHYSICS & MOVEMENT ---")]
    public CharacterController controller;
    public Transform mainCamera;
    public float walkSpeed = 5.0f;
    public float sprintSpeed = 9.0f;
    public float jumpHeight = 2.0f;
    public float gravityForce = -25.0f; 

    [Header("--- INTERACTIVE ITEM HANDLING ---")]
    public ParticleSystem bluntSmokeSystem;
    public ParticleSystem bongSmokeSystem;
    public BitLifeCustomizer inventoryData;

    private Vector3 velocity;
    private bool isGrounded;
    [HideInInspector] public bool isDriving = false;

    // Camera High Effect Variables
    private bool isCurrentlyHigh = false;
    private float highTimerDuration = 0f;
    private float originalCameraFieldOfView;
    private Camera actualCameraComponent;

    private void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        if (mainCamera != null)
        {
            actualCameraComponent = mainCamera.GetComponent<Camera>();
            if (actualCameraComponent != null) originalCameraFieldOfView = actualCameraComponent.fieldOfView;
        }
    }

    private void Update()
    {
        if (isDriving) return;
        
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f; 

        ProcessGTAMovementInput();
        ProcessActionMechanicsInput();
        CheckForNearbyVehicles();
        HandleTrippyCameraVisuals();
    }

    private void ProcessGTAMovementInput()
    {
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        
        // If high, slightly sway inputs side to side to simulate poor balance
        if (isCurrentlyHigh)
        {
            horiz += Mathf.Sin(Time.time * 2f) * 0.15f;
        }

        Vector3 direction = new Vector3(horiz, 0f, vert).normalized;

        if (direction.magnitude >= 0.1f)
        {
            bool isSprinting = Input.GetKey(KeyCode.JoystickButton8) || Input.GetKey(KeyCode.LeftShift);
            float currentMoveSpeed = isSprinting ? sprintSpeed : walkSpeed;
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            controller.Move(moveDir.normalized * currentMoveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, targetAngle, 0f), 0.25f);
        }

        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.JoystickButton0)) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityForce);
        }

        velocity.y += gravityForce * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void ProcessActionMechanicsInput()
    {
        if (inventoryData == null) return;
        if (Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.E))
        {
            if (inventoryData.playerInventory.Contains("Premium Exotic Bud") || inventoryData.playerInventory.Contains("Infused Fudge Brownie"))
            {
                if (inventoryData.playerInventory.Contains("Glass Vortex Rig") && bongSmokeSystem != null) bongSmokeSystem.Play();
                else if (bluntSmokeSystem != null) bluntSmokeSystem.Play();

                // Trigger the visual high effect for 15 seconds
                TriggerHighVisualEffect(15.5f);
            }
        }
    }

    private void TriggerHighVisualEffect(float duration)
    {
        isCurrentlyHigh = true;
        highTimerDuration = duration;
    }

    private void HandleTrippyCameraVisuals()
    {
        if (!isCurrentlyHigh || actualCameraComponent == null) return;

        highTimerDuration -= Time.deltaTime;

        if (highTimerDuration > 0)
        {
            // Warps FOV smoothly back and forth to look psychedelic
            float dynamicFOVModifier = Mathf.Sin(Time.time * 3.5f) * 8f; 
            actualCameraComponent.fieldOfView = originalCameraFieldOfView + dynamicFOVModifier;

            // Roll camera lens left and right subtly
            float subtleTiltRoll = Mathf.Cos(Time.time * 2f) * 4f;
            mainCamera.localRotation = Quaternion.Euler(mainCamera.localEulerAngles.x, mainCamera.localEulerAngles.y, subtleTiltRoll);
        }
        else
        {
            // Effect finished, snap safely back to normal settings
            isCurrentlyHigh = false;
            actualCameraComponent.fieldOfView = Mathf.Lerp(actualCameraComponent.fieldOfView, originalCameraFieldOfView, 0.1f);
            mainCamera.localRotation = Quaternion.Lerp(mainCamera.localRotation, Quaternion.Euler(mainCamera.localEulerAngles.x, mainCamera.localEulerAngles.y, 0f), 0.1f);
        }
    }

    private void CheckForNearbyVehicles()
    {
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            RealisticVehicleEngine[] vehicles = FindObjectsByType<RealisticVehicleEngine>(FindObjectsSortMode.None);
            foreach (RealisticVehicleEngine vehicle in vehicles)
            {
                float distance = Vector3.Distance(transform.position, vehicle.transform.position);
                if (distance <= 3.5f)
                {
                    vehicle.EnterVehicle();
                    break;
                }
            }
        }
    }

    public void TeleportAndExitVehicle(Vector3 exitPosition)
    {
        transform.position = exitPosition;
        isDriving = false;
        gameObject.SetActive(true);
    }
}
