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

    [Header("--- CRIME & POLICE MECHANICS ---")]
    public int wantedLevel = 0;
    private float crimeCooldownTimer = 0f;

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
        HandlePoliceLogic();
        HandleTrippyCameraVisuals();

        if (isDriving) return;
        
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f; 

        ProcessGTAMovementInput();
        ProcessActionMechanicsInput();
        CheckForNearbyVehicles();
    }

    private void ProcessGTAMovementInput()
    {
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        
        if (isCurrentlyHigh)
        {
            horiz += Mathf.Sin(Time.time * 2.5f) * 0.18f; // Makes player stumble subtly
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

                TriggerHighVisualEffect(20f);
                
                // Smoking illegal substances in public adds a 1-star wanted level if cops are nearby
                if (wantedLevel < 1) TriggerCrimeAlert(1);
            }
        }
    }

    public void TriggerCrimeAlert(int levelsEarned)
    {
        wantedLevel = Mathf.Clamp(wantedLevel + levelsEarned, 0, 5);
        crimeCooldownTimer = 25f; // Cops chase for 25 seconds before losing interest
        if (inventoryData != null) inventoryData.UpdateHUD();

        // Find the dispatcher script and send an interceptor to the player's exact location
        #if UNITY_2023_1_OR_NEWER
        CityTrafficAI trafficManager = FindFirstObjectByType<CityTrafficAI>();
        #else
        CityTrafficAI trafficManager = FindObjectOfType<CityTrafficAI>();
        #endif

        if (trafficManager != null)
        {
            trafficManager.DispatchStateInterceptors(transform.position);
        }
    }

    private void HandlePoliceLogic()
    {
        if (wantedLevel > 0)
        {
            crimeCooldownTimer -= Time.deltaTime;
            if (crimeCooldownTimer <= 0)
            {
                wantedLevel = 0;
                if (inventoryData != null) inventoryData.UpdateHUD();
                Debug.Log("The cops lost your trail.");
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
            float dynamicFOVModifier = Mathf.Sin(Time.time * 3f) * 7.5f; 
            actualCameraComponent.fieldOfView = originalCameraFieldOfView + dynamicFOVModifier;

            float subtleTiltRoll = Mathf.Cos(Time.time * 1.8f) * 3.5f;
            mainCamera.localRotation = Quaternion.Euler(mainCamera.localEulerAngles.x, mainCamera.localEulerAngles.y, subtleTiltRoll);
        }
        else
        {
            isCurrentlyHigh = false;
            actualCameraComponent.fieldOfView = Mathf.Lerp(actualCameraComponent.fieldOfView, originalCameraFieldOfView, 0.1f);
            mainCamera.localRotation = Quaternion.Lerp(mainCamera.localRotation, Quaternion.Euler(mainCamera.localEulerAngles.x, mainCamera.localEulerAngles.y, 0f), 0.1f);
        }
    }

    private void CheckForNearbyVehicles()
    {
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            #if UNITY_2023_1_OR_NEWER
            RealisticVehicleEngine[] vehicles = FindObjectsByType<RealisticVehicleEngine>(FindObjectsSortMode.None);
            #else
            RealisticVehicleEngine[] vehicles = FindObjectsOfType<RealisticVehicleEngine>();
            #endif

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
