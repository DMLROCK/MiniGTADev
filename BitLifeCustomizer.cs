using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private readonly string[] outfits =
        { "Casual Hoodie", "Streetwear Tracksuit", "Retro Tie-Dye", "Biker Jacket" };
    private int outfitIndex;
    private bool isCustomizing = true;
    private string chosenOutfit = "Casual Hoodie";

    private void Start()
    {
        if (movementScript != null)
            movementScript.enabled = false;

        if (customizerPanel != null)
            customizerPanel.SetActive(true);

        RenderCustomizer();
        UpdateHUD();
    }

    private void Update()
    {
        if (!isCustomizing)
            return;

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

            if (customizerPanel != null)
                customizerPanel.SetActive(false);

            if (movementScript != null)
                movementScript.enabled = true;

            UpdateHUD();
        }
    }

    private void RenderCustomizer()
    {
        if (customizerText == null)
            return;

        customizerText.text =
            "<b>=== CHARACTER CUSTOMIZER ===</b>\n" +
            "NAME: " + characterName + "\n" +
            "STARTING FUNDS: $" + cashBalance + "\n\n" +
            "CHOOSE OUTFIT STYLE:\n";

        for (int i = 0; i < outfits.Length; i++)
        {
            customizerText.text += i == outfitIndex
                ? " > [" + outfits[i] + "] <\n"
                : "   " + outfits[i] + "\n";
        }

        customizerText.text += "\nPress [Space / Controller A] to enter the world";
    }

    public void UpdateHUD()
    {
        if (mainGameplayHUD == null)
            return;

        if (playerInventory == null)
            playerInventory = new List<string>();

        string inventoryString = playerInventory.Count > 0
            ? string.Join(", ", playerInventory)
            : "Empty";

        mainGameplayHUD.text =
            "<b>NAME:</b> " + characterName +
            "  |  <b>OUTFIT:</b> " + chosenOutfit +
            "  |  <b>WALLET:</b> $" + cashBalance +
            "\n<b>ITEMS:</b> " + inventoryString;
    }
}
