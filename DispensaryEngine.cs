using UnityEngine;
using UnityEngine.UI;

public class DispensaryEngine : MonoBehaviour
{
    [System.Serializable]
    public struct DispensaryItem
    {
        public string itemName;
        public int basePrice;
        [HideInInspector] public int currentPrice; // Tracks dynamic daily deals
    }

    [Header("--- SHOP INVENTORY ---")]
    public DispensaryItem[] menuItems = new DispensaryItem[]
    {
        new DispensaryItem { itemName = "Premium Exotic Bud", basePrice = 250 },
        new DispensaryItem { itemName = "Infused Fudge Brownie", basePrice = 45 },
        new DispensaryItem { itemName = "Glass Vortex Rig", basePrice = 600 }
    };

    [Header("--- LINKED GAME DATA ---")]
    public BitLifeCustomizer playerData;
    public PlayerGTAController playerScript;

    [Header("--- IMMERSIVE SHOP UI ---")]
    public GameObject shopPanel;
    public Text shopText;
    public GameObject promptTooltipUI; // "Press E to talk to Budtender" text pop-up

    private int shopSelectionIndex = 0;
    private bool isPlayerNearCounter = false;
    private bool isInsideShopMenu = false;
    private int discountItemIndex = -1;

    private void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (promptTooltipUI != null) promptTooltipUI.SetActive(false);
        
        // Reset prices to base values initially
        for (int i = 0; i < menuItems.Length; i++)
        {
            menuItems[i].currentPrice = menuItems[i].basePrice;
        }
    }

    private void Update()
    {
        // If the player is standing at the counter, show the prompt to talk to the budtender
        if (isPlayerNearCounter && !isInsideShopMenu)
        {
            if (promptTooltipUI != null) promptTooltipUI.SetActive(true);

            // Press E or Xbox RB to talk to budtender and open menu
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                OpenDispensary();
            }
        }
        else
        {
            if (promptTooltipUI != null) promptTooltipUI.SetActive(false);
        }

        // Shop Menu Controls
        if (!isInsideShopMenu) return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            shopSelectionIndex = (shopSelectionIndex + 1) % menuItems.Length;
            RenderShopMenu();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            shopSelectionIndex = (shopSelectionIndex - 1 + menuItems.Length) % menuItems.Length;
            RenderShopMenu();
        }

        // Purchase Item
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            TryPurchaseSelectedItem();
        }

        // Manual exit out of menu window
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            CloseDispensary();
        }
    }

    // --- TRIGGER ZONES (Walk up to the counter) ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearCounter = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearCounter = false;
            CloseDispensary(); // Automatically shut menu if player walks away
        }
    }

    public void OpenDispensary()
    {
        if (playerData == null) return;
        
        isInsideShopMenu = true;
        shopSelectionIndex = 0;
        
        if (playerScript != null) playerScript.enabled = false; // Freeze player physics
        if (shopPanel != null) shopPanel.SetActive(true);

        CalculateDailyDeals();
        RenderShopMenu();
    }

    private void CalculateDailyDeals()
    {
        // Pick one random item to mark down as a "Daily Special" (20% off)
        discountItemIndex = Random.Range(0, menuItems.Length);
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (i == discountItemIndex)
                menuItems[i].currentPrice = Mathf.RoundToInt(menuItems[i].basePrice * 0.80f);
            else
                menuItems[i].currentPrice = menuItems[i].basePrice;
        }
    }

    public void CloseDispensary()
    {
        isInsideShopMenu = false;
        if (shopPanel != null) shopPanel.SetActive(false);
        if (playerScript != null) playerScript.enabled = true; // Return movement controls
    }

    private void RenderShopMenu()
    {
        if (shopText == null || playerData == null) return;

        shopText.text = "<b>=== GREEN LEAF DISPENSARY ===</b>\nCUSTOMER WALLET: $" + playerData.cashBalance + "\n\n";
        
        for (int i = 0; i < menuItems.Length; i++)
        {
            string dealTag = (i == discountItemIndex) ? " <color=green>[🔥 DAILY SPECIAL - 20% OFF]</color>" : "";
            
            if (i == shopSelectionIndex)
            {
                shopText.text += " > [" + menuItems[i].itemName + "] - $" + menuItems[i].currentPrice + dealTag + " <\n";
            }
            else
            {
                shopText.text += "   " + menuItems[i].itemName + " - $" + menuItems[i].currentPrice + dealTag + "\n";
            }
        }
        
        shopText.text += "\n<i>[Space/A] Buy | [Escape/B] Exit Counter</i>";
    }

    private void TryPurchaseSelectedItem()
    {
        if (playerData == null) return;

        DispensaryItem selectedProduct = menuItems[shopSelectionIndex];

        if (playerData.cashBalance >= selectedProduct.currentPrice)
        {
            playerData.cashBalance -= selectedProduct.currentPrice;
            playerData.playerInventory.Add(selectedProduct.itemName);
            
            playerData.UpdateHUD();
            RenderShopMenu();
        }
    }

    // --- REAL LIFE ADDITION: THE DISPENSARY ATM MACHINE ---
    public void InteractWithATM()
    {
        if (playerData == null) return;
        playerData.cashBalance += 500; // Withdraw $500 from bank
        playerData.UpdateHUD();
        if (isInsideShopMenu) RenderShopMenu();
        Debug.Log("Withdrew $500 from Dispensary ATM.");
    }
}
