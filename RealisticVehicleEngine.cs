using UnityEngine;

public class PlayerGTAController : MonoBehaviour
{
    [Header("--- GTA PHYSICS & MOVEMENT ---")]
    public CharacterController controller;
    public Transform mainCamera;
    public float walkSpeed = 5.0f;
    public float sprintSpeed = 9.0f;
    public float jumpHeight = 2.0f;
    public float gravityForce = -25.0f; // Heavy, deliberate realistic gravity drop like GTA V/VI

    [Header("--- INTERACTIVE ITEM HANDLING ---")]
    public ParticleSystem bluntSmokeSystem;
    public ParticleSystem bongSmokeSystem;
    public BitLifeCustomizer inventoryData;

    // Movement Vectors
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
        if (isDriving) return;

        // Ground Check (Ensures you can't infinitely jump into the sky)
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        ProcessGTAMovementInput();
        ProcessActionMechanicsInput();
    }

    private void ProcessGTAMovementInput()
    {
        // Fetch Xbox Left Stick or WASD Input Axis
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horiz, 0f, vert).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Toggle Sprinting by clicking Left Stick (Joystick Button 8) or Left Shift
            bool isSprinting = Input.GetKey(KeyCode.JoystickButton8) || Input.GetKey(KeyCode.LeftShift);
            float currentMoveSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // Compute direction relative to camera angle like modern open world titles
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            controller.Move(moveDir.normalized * currentMoveSpeed * Time.deltaTime);

            // Smoothly rotate body model to face moving direction
            Quaternion targetRot = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.25f);
        }

        // Jump Physics (Xbox A Button / Joystick Button 0)
        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.JoystickButton0)) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityForce);
        }

        // Apply realistic gravity drop overtime
        velocity.y += gravityForce * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void ProcessActionMechanicsInput()
    {
        if (inventoryData == null) return;

        // Press Right Bumper (RB / Joystick Button 5) to actively USE your purchased items
        if (Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.E))
        {
            if (inventoryData.playerInventory.Contains("Premium Exotic Bud") || inventoryData.playerInventory.Contains("Infused Fudge Brownie"))
            {
                // Check if they own matching hardware to use it
                if (inventoryData.playerInventory.Contains("Glass Vortex Rig") && bongSmokeSystem != null)
                {
                    bongSmokeSystem.Play();
                    Debug.Log("Using Glass Vortex Rig Bong.");
                }
                else if (bluntSmokeSystem != null)
                {
                    // Default to smoking a hand-rolled blunt if no heavy rig is owned
                    bluntSmokeSystem.Play();
                    Debug.Log("Smoking Rolled Blunt.");
                }
            }
            else
            {
                Debug.Log("No consumables available in inventory. Visit Stoner Hideout.");
            }
        }
    }
}
