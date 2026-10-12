using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BitLifeCustomizer : MonoBehaviour
{
    [Header("--- CHARACTER PROFILE ---")]
    public string characterName = "Jay";
    public int cashBalance = 4200;
    public List<string> playerInventory = new List<string>();

    [Header("--- CURRENT MISSION DATA ---")]
    public string activeMissionName = "None";
    public string activeMissionObjective = "Explore the city.";
    public int missionRewardCash = 0;

    [Header("--- UI WINDOWS ---")]
    public GameObject customizerPanel;
    public Text customizerText;
    public Text mainGameplayHUD;
    public GameObject pausePanel;

    [Header("--- SCRIPTS ---")]
    public PlayerGTAController movementScript;

    private string[] outfits = { "Casual Hoodie", "Streetwear Tracksuit", "Retro Tie-Dye", "Biker Jacket" };
    private int outfitIndex = 0;
    private bool isCustomizing = true;
    private string chosenOutfit = "Casual Hoodie";
    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        
        if (PlayerPrefs.HasKey("Saved_CashBalance"))
        {
            LoadGameData();
            isCustomizing = false;
            if (customizerPanel != null) customizerPanel.SetActive(false);
            if (movementScript != null) movementScript.enabled = true;
            UpdateHUD();
        }
        else
        {
            if (movementScript != null) movementScript.enabled = false; 
            if (customizerPanel != null) customizerPanel.SetActive(true);
            RenderCustomizer();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (!isCustomizing) TogglePauseGame();
        }

        if (!isCustomizing) return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            outfitIndex = (outfitIndex + 1) % outfits.Length;
            RenderCustomizer();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            outfitIndex = (outfitIndex - 1 + outfits.Length) % outfits.Length;
            RenderCustomizer();
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            chosenOutfit = outfits[outfitIndex];
            isCustomizing = false;
            if (customizerPanel != null) customizerPanel.SetActive(false);
            if (movementScript != null) movementScript.enabled = true; 
            SaveGameData();
            UpdateHUD();
        }
    }

    private void RenderCustomizer()
    {
        if (customizerText == null) return;
        customizerText.text = "<b>=== CHARACTER CUSTOMIZER ===</b>\nNAME: " + characterName + "\nSTARTING FUNDS: $" + cashBalance + "\n\nCHOOSE OUTFIT STYLE:\n";
        for (int i = 0; i < outfits.Length; i++)
        {
            if (i == outfitIndex) customizerText.text += " > [" + outfits[i] + "] <\n";
            else customizerText.text += "   " + outfits[i] + "\n";
        }
        customizerText.text += "\n<i>Press [Space/A] to Spawn Into World</i>";
    }

    public void TogglePauseGame()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
            SaveGameData();
        }
        else
        {
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
        }
    }

    public void AssignNewMission(string name, string objective, int reward)
    {
        activeMissionName = name;
        activeMissionObjective = objective;
        missionRewardCash = reward;
        UpdateHUD();
    }

    public void CompleteActiveMission()
    {
        if (activeMissionName == "None") return;
        cashBalance += missionRewardCash;
        Debug.Log("MISSION COMPLETE: " + activeMissionName + " | Earned $" + missionRewardCash);
        activeMissionName = "None";
        activeMissionObjective = "Explore the city.";
        missionRewardCash = 0;
        SaveGameData();
        UpdateHUD();
    }

    public void SaveGameData()
    {
        PlayerPrefs.SetString("Saved_CharacterName", characterName);
        PlayerPrefs.SetInt("Saved_CashBalance", cashBalance);
        PlayerPrefs.SetString("Saved_ChosenOutfit", chosenOutfit);
        PlayerPrefs.SetString("Saved_PlayerInventory", string.Join(",", playerInventory.ToArray()));
        PlayerPrefs.Save();
    }

    public void LoadGameData()
    {
        characterName = PlayerPrefs.GetString("Saved_CharacterName", "Jay");
        cashBalance = PlayerPrefs.GetInt("Saved_CashBalance", 4200);
        chosenOutfit = PlayerPrefs.GetString("Saved_ChosenOutfit", "Casual Hoodie");
        string compressedInventory = PlayerPrefs.GetString("Saved_PlayerInventory", "");
        playerInventory.Clear();
        if (!string.IsNullOrEmpty(compressedInventory))
        {
            foreach (string item in compressedInventory.Split(','))
            {
                if (!string.IsNullOrEmpty(item)) playerInventory.Add(item);
            }
        }
    }

    public void UpdateHUD()
    {
        if (mainGameplayHUD == null) return;
        string inventoryString = playerInventory.Count > 0 ? string.Join(", ", playerInventory.ToArray()) : "Empty";
        string currentWeapon = (movementScript != null) ? movementScript.activeWeaponName : "Unarmed";
        
        string wantedStars = "";
        int stars = (movementScript != null) ? movementScript.wantedLevel : 0;
        for (int i = 0; i < 5; i++) wantedStars += (i < stars) ? "★" : "☆";

        mainGameplayHUD.text = "<b>NAME:</b> " + characterName + " | <b>OUTFIT:</b> " + chosenOutfit + " | <b>WALLET:</b> $" + cashBalance + " | <color=yellow><b>WANTED:</b> " + wantedStars + "</color>\n" +
                               "<b>WEAPON:</b> " + currentWeapon + " | <b>ITEMS:</b> " + inventoryString + "\n" +
                               "<color=orange><b>[MISSION]: " + activeMissionName + "</b> - " + activeMissionObjective + "</color>";
    }
}
