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

    [Header("--- SCRIPTS ---")]
    public PlayerGTAController movementScript;

    private string[] outfits = { "Casual Hoodie", "Streetwear Tracksuit", "Retro Tie-Dye", "Biker Jacket" };
    private int outfitIndex = 0;
    private bool isCustomizing = true;
    private string chosenOutfit = "Casual Hoodie";

    private void Start()
    {
        if (movementScript != null) movementScript.enabled = false; 
        if (customizerPanel != null) customizerPanel.SetActive(true);
        RenderCustomizer();
    }

    private void Update()
    {
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

    public void UpdateHUD()
    {
        if (mainGameplayHUD == null) return;
        string inventoryString = playerInventory.Count > 0 ? string.Join(", ", playerInventory.ToArray()) : "Empty";
        
        // Grab wanted stars from the movement script if available
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
