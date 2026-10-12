using UnityEngine;

public class CityTrafficAI : MonoBehaviour
{
    [Header("--- IMMERSIVE POPULATION LOGISTICS ---")]
    public GameObject civilianNPCBlueprint;
    public GameObject policeCruiserBlueprint;
    public PlayerGTAController playerTracker;
    
    public int absoluteMaxCivilians = 15;
    private float spawnClock = 0f;

    private void Update()
    {
        spawnClock += Time.deltaTime;
        if (spawnClock >= 6.0f)
        {
            spawnClock = 0f;
            PopulateCitySectors();
        }
    }

    private void PopulateCitySectors()
    {
        // Count existing ambient NPCs to avoid performance drops
        int currentCount = GameObject.FindGameObjectsWithTag("Civilian").Length;
        if (currentCount >= absoluteMaxCivilians || civilianNPCBlueprint == null || playerTracker == null) return;

        // Spawn a random NPC within a safe bubble near the player
        Vector3 randomBubbleOffset = playerTracker.transform.position + new Vector3(Random.Range(-35f, 35f), 0f, Random.Range(-35f, 35f));
        GameObject activeCivilian = Instantiate(civilianNPCBlueprint, randomBubbleOffset, Quaternion.identity);
        activeCivilian.tag = "Civilian";

        // Give the civilian primitive forward wandering movement
        Rigidbody rb = activeCivilian.GetComponent<Rigidbody>();
        if (rb == null) rb = activeCivilian.AddComponent<Rigidbody>();
        rb.useGravity = true;
        
        Destroy(activeCivilian, 45f); // Automatically cleanup far-away clones
    }

    // Called automatically by the event layer when a crime occurs
    public void DispatchStateInterceptors(Vector3 crimeSceneLocation)
    {
        if (policeCruiserBlueprint == null) return;

        Vector3 tacticalFlankSpawn = crimeSceneLocation + (playerTracker.transform.forward * 40f) + (Vector3.up * 1f);
        GameObject dispatchSquadCar = Instantiate(policeCruiserBlueprint, tacticalFlankSpawn, Quaternion.identity);
        
        RealisticVehicleEngine carEngine = dispatchSquadCar.GetComponent<RealisticVehicleEngine>();
        if (carEngine != null)
        {
            carEngine.specificModel = RealisticVehicleEngine.VehicleType.PoliceCruiser;
            carEngine.playerScript = playerTracker;
        }
    }
}
