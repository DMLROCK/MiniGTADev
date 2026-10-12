using UnityEngine;
using UnityEngine.UI;

public class MissionQuestGiver : MonoBehaviour
{
    [Header("--- MISSION DESCRIPTION CONFIG ---")]
    public string missionTitle = "The Green Run";
    public string missionObjectiveText = "Buy a Glass Vortex Rig from the shop to complete delivery.";
    public int completionCashPayout = 1500;

    [Header("--- LINKED PROFILE MANAGER ---")]
    public BitLifeCustomizer customizerData;
    public GameObject promptTooltipUI;

    private bool isPlayerInZone = false;

    private void Update()
    {
        if (isPlayerInZone && customizerData != null)
        {
            if (promptTooltipUI != null)
            {
                promptTooltipUI.SetActive(true);
                promptTooltipUI.GetComponent<Text>().text = "Press [E] to Accept Mission: " + missionTitle;
            }

            // Accept quest via E or Controller Right Bumper
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                customizerData.AssignNewMission(missionTitle, missionObjectiveText, completionCashPayout);
                Debug.Log("Mission Accepted: " + missionTitle);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            if (promptTooltipUI != null) promptTooltipUI.SetActive(false);
        }
    }
}
