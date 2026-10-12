using UnityEngine;

public class PlayerAndCar : MonoBehaviour
{
    [Header("--- PLAYER ---")]
    public GameObject player3D;
    public Rigidbody playerRb;
    public float walkSpeed = 6.0f;

    [Header("--- CAR ---")]
    public GameObject sportsCar;
    public Rigidbody carRb;
    public float horsepower = 1800f;
    public float steeringSpeed = 40f;

    [Header("--- CAMERA ---")]
    public Transform mainCamera;
    
    [HideInInspector] public bool isDriving = false;

    private void Awake()
    {
        // Keep graphics running smooth at 60FPS on Xbox
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        
        if (carRb != null) carRb.isKinematic = true;
    }

    private void Update()
    {
        // Press Xbox Y button to enter/exit car
        if (Input.GetKeyDown(KeyCode.JoystickButton3) || Input.GetKeyDown(KeyCode.Y))
        {
            ToggleDrivingState();
        }
    }

    private void FixedUpdate()
    {
        if (!isDriving)
        {
            // 3D Player Movement (Left Thumbstick)
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            Vector3 move = new Vector3(moveX, 0f, moveZ).normalized;
            if (move.magnitude >= 0.1f && playerRb != null)
            {
                player3D.transform.Translate(move * walkSpeed * Time.deltaTime, Space.World);
                player3D.transform.rotation = Quaternion.Slerp(player3D.transform.rotation, Quaternion.LookRotation(move), 0.25f);
            }
        }
        else
        {
            // 3D Car Driving (RT to accelerate, LT to reverse, Left Stick to steer)
            float steer = Input.GetAxis("Horizontal");
            float gas = Input.GetAxis("Xbox_RightTrigger");
            float brake = Input.GetAxis("Xbox_LeftTrigger");

            // Keyboard fallbacks for testing
            if (gas == 0 && brake == 0)
            {
                gas = Input.GetKey(KeyCode.W) ? 1f : 0f;
                brake = Input.GetKey(KeyCode.S) ? 1f : 0f;
            }

            if (gas > 0.05f) carRb.AddForce(sportsCar.transform.forward * gas * horsepower * Time.deltaTime, ForceMode.Acceleration);
            if (brake > 0.05f) carRb.AddForce(-sportsCar.transform.forward * brake * horsepower * Time.deltaTime, ForceMode.Acceleration);
            
            if (carRb.velocity.magnitude > 0.5f)
            {
                float direction = Vector3.Dot(sportsCar.transform.forward, carRb.velocity) >= 0 ? 1f : -1f;
                sportsCar.transform.Rotate(Vector3.up, steer * steeringSpeed * direction * Time.deltaTime);
            }
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // Cinematic camera tracking that won't stutter
        if (!isDriving && player3D != null)
        {
            mainCamera.position = Vector3.Lerp(mainCamera.position, player3D.transform.position + new Vector3(0, 4.5f, -7f), 0.15f);
            mainCamera.LookAt(player3D.transform.position + Vector3.up);
        }
        else if (isDriving && sportsCar != null)
        {
            mainCamera.position = Vector3.Lerp(mainCamera.position, sportsCar.transform.position + (sportsCar.transform.rotation * new Vector3(0, 5f, -9f)), 0.15f);
            mainCamera.LookAt(sportsCar.transform.position + Vector3.up);
        }
    }

    private void ToggleDrivingState()
    {
        if (player3D == null || sportsCar == null) return;

        float distance = Vector3.Distance(player3D.transform.position, sportsCar.transform.position);

        if (!isDriving && distance <= 4.0f)
        {
            isDriving = true;
            player3D.SetActive(false);
            if (carRb != null) carRb.isKinematic = false;
        }
        else if (isDriving)
        {
            isDriving = false;
            if (carRb != null) carRb.isKinematic = true;
            player3D.transform.position = sportsCar.transform.position + (sportsCar.transform.right * -2.5f);
            player3D.SetActive(true);
        }
    }
}
