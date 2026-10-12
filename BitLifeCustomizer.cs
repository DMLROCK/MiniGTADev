using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BitLifeCustomizer : MonoBehaviour
{
    [Header("--- CHARACTER PROFILE ---")]
    public string characterName = "Jay";
    public int cashBalance = 4200;
    public List<string> playerInventory = new List<string>();

    [Header("--- UI WINDOWS ---")]
    public GameObject customizerPanel;
    public Text customizerText;
    public Text mainGameplayHUD;
    public GameObject pausePanel; // Drag your pause menu background here

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
        
        // Check if a save game already exists on the player's computer
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
        // Press Escape or Start Button on controller to toggle Pause Menu & Trigger Auto-Save
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (!isCustomizing)
            {
                TogglePauseGame();
            }
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
            SaveGameData(); // Initial auto-save upon spawning
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
        customizerText.text += "\n<i>Press [Space/A Button] to Spawn Into World</i>";
    }

    public void TogglePauseGame()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // Freezes all physics, animations, and movement instantly
            if (pausePanel != null) pausePanel.SetActive(true);
            
            SaveGameData(); // <--- AUTOMATICALLY SAVES THE GAME UPON PAUSING!
            Debug.Log("Game Paused. Progress automatically saved to disk.");
        }
        else
        {
            Time.timeScale = 1f; // Resumes game physics clock speed back to normal
            if (pausePanel != null) pausePanel.SetActive(false);
        }
    }

    // --- ENGINES FOR COMPUTER HARD DRIVE SAVING ---
    public void SaveGameData()
    {
        PlayerPrefs.SetString("Saved_CharacterName", characterName);
        PlayerPrefs.SetInt("Saved_CashBalance", cashBalance);
        PlayerPrefs.SetString("Saved_ChosenOutfit", chosenOutfit);

        // Compress inventory strings together split by a comma to store list values safely
        string compressedInventory = string.Join(",", playerInventory.ToArray());
        PlayerPrefs.SetString("Saved_PlayerInventory", compressedInventory);

        PlayerPrefs.Save(); // Forces data write out to local system registries
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
            string[] items = compressedInventory.Split(',');
            foreach (string item in items)
            {
                if (!string.IsNullOrEmpty(item)) playerInventory.Add(item);
            }
        }
    }

    public void UpdateHUD()
    {
        if (mainGameplayHUD == null) return;
        string inventoryString = playerInventory.Count > 0 ? string.Join(", ", playerInventory.ToArray()) : "Empty";
        
        string wantedStars = "";
        if (movementScript != null)
        {
            for (int i = 0; i < 5; i++)
            {
                wantedStars += (i < movementScript.wantedLevel) ? "★" : "☆";
            }
        }
        else { wantedStars = "☆☆☆☆☆"; }

        mainGameplayHUD.text = "<b>NAME:</b> " + characterName + "  |  <b>OUTFIT:</b> " + chosenOutfit + "  |  <b>WALLET:</b> $" + cashBalance + "  |  <color=yellow><b>WANTED:</b> " + wantedStars + "</color>\n<b>ITEMS:</b> " + inventoryString;
    }
}
