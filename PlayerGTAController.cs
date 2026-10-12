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

    private void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (isDriving) return; // Freeze walking completely if driving
        
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
            }
        }
    }

    private void CheckForNearbyVehicles()
    {
        // Press F on Keyboard or Joystick Button 2 (X on Xbox) to Enter
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            // Find all vehicles in the game scene
            RealisticVehicleEngine[] vehicles = FindObjectsByType<RealisticVehicleEngine>(FindObjectsSortMode.None);
            foreach (RealisticVehicleEngine vehicle in vehicles)
            {
                float distance = Vector3.Distance(transform.position, vehicle.transform.position);
                if (distance <= 3.5f) // Close enough to the car door
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
        gameObject.SetActive(true); // Bring player avatar back to life
    }
}
