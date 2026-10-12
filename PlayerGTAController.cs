using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerGTAController : MonoBehaviour
{
    [Header("--- GTA PHYSICS & MOVEMENT ---")]
    public CharacterController controller;
    public Transform mainCamera;
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 2f;
    public float gravityForce = -25f;

    [Header("--- INTERACTIVE ITEM HANDLING ---")]
    public ParticleSystem bluntSmokeSystem;
    public ParticleSystem bongSmokeSystem;
    public BitLifeCustomizer inventoryData;

    private Vector3 velocity;
    private bool isGrounded;

    [HideInInspector] public bool isDriving;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (isDriving || controller == null)
            return;

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        ProcessGTAMovementInput();
        ProcessActionMechanicsInput();
    }

    private void ProcessGTAMovementInput()
    {
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horiz, 0f, vert).normalized;

        if (direction.sqrMagnitude >= 0.01f)
        {
            bool isSprinting = Input.GetKey(KeyCode.JoystickButton8) ||
                               Input.GetKey(KeyCode.LeftShift);
            float currentMoveSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // Allow movement even if no camera has been assigned.
            float cameraYaw = mainCamera != null ? mainCamera.eulerAngles.y : transform.eulerAngles.y;
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraYaw;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            controller.Move(moveDir.normalized * currentMoveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler(0f, targetAngle, 0f),
                Mathf.Clamp01(10f * Time.deltaTime));
        }

        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.JoystickButton0)) && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityForce);

        velocity.y += gravityForce * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void ProcessActionMechanicsInput()
    {
        if (inventoryData == null || inventoryData.playerInventory == null)
            return;

        if (Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.E))
        {
            bool hasConsumable =
                inventoryData.playerInventory.Contains("Premium Exotic Bud") ||
                inventoryData.playerInventory.Contains("Infused Fudge Brownie");

            if (!hasConsumable)
                return;

            if (inventoryData.playerInventory.Contains("Glass Vortex Rig") && bongSmokeSystem != null)
                bongSmokeSystem.Play();
            else if (bluntSmokeSystem != null)
                bluntSmokeSystem.Play();
        }
    }
}
