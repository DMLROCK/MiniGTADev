using UnityEngine;
using UnityEngine.UI;

public class DispensaryEngine : MonoBehaviour
{
    [System.Serializable]
    public struct DispensaryItem
    {
        public string itemName;
        public int itemsPrice;
    }

    [Header("--- SHOP INVENTORY ---")]
    public DispensaryItem[] menuItems = new DispensaryItem[]
    {
        new DispensaryItem { itemName = "Premium Exotic Bud", itemsPrice = 250 },
        new DispensaryItem { itemName = "Infused Fudge Brownie", itemsPrice = 45 },
        new DispensaryItem { itemName = "Glass Vortex Rig", itemsPrice = 600 }
    };

    [Header("--- LINKED GAME DATA ---")]
    public BitLifeCustomizer playerData;
    public PlayerGTAController playerScript;

    [Header("--- DISPENSARY UI WINDOWS ---")]
    public GameObject shopPanel;
    public Text shopText;

    private int shopSelectionIndex = 0;
    private bool isInsideShop = false;

    private void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    private void Update()
    {
        // Toggle the shop menu open or closed manually using the P key for testing
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isInsideShop) CloseDispensary();
            else OpenDispensary();
        }

        if (!isInsideShop) return;

        // Navigate shop items using Up/Down Arrow keys or W/S
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

        // Buy item using Space key or Xbox A Button
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            TryPurchaseSelectedItem();
        }
    }

    public void OpenDispensary()
    {
        if (playerData == null) return;
        
        isInsideShop = true;
        shopSelectionIndex = 0;
        
        if (playerScript != null) playerScript.enabled = false; // Freeze player movement while shopping
        if (shopPanel != null) shopPanel.SetActive(true);
        
        RenderShopMenu();
    }

    public void CloseDispensary()
    {
        isInsideShop = false;
        
        if (shopPanel != null) shopPanel.SetActive(false);
        if (playerScript != null) playerScript.enabled = true; // Give control back to movement
    }

    private void RenderShopMenu()
    {
        if (shopText == null || playerData == null) return;

        shopText.text = "<b>=== GREEN LEAF DISPENSARY ===</b>\nYOUR CASH: $" + playerData.cashBalance + "\n\nAVAILABLE PRODUCTS:\n";
        
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (i == shopSelectionIndex)
            {
                shopText.text += " > [" + menuItems[i].itemName + "] - $" + menuItems[i].itemsPrice + " <\n";
            }
            else
            {
                shopText.text += "   " + menuItems[i].itemName + " - $" + menuItems[i].itemsPrice + "\n";
            }
        }
        
        shopText.text += "\n<i>Press [Space] to Buy | Press [P] to Exit Shop</i>";
    }

    private void TryPurchaseSelectedItem()
    {
        if (playerData == null) return;

        DispensaryItem selectedProduct = menuItems[shopSelectionIndex];

        if (playerData.cashBalance >= selectedProduct.itemsPrice)
        {
            // Deduct money and add product directly to the profile inventory list
            playerData.cashBalance -= selectedProduct.itemsPrice;
            playerData.playerInventory.Add(selectedProduct.itemName);
            
            // Instantly tell the HUD canvas to display the new item and money count
            playerData.UpdateHUD();
            RenderShopMenu();
            
            Debug.Log("Purchased: " + selectedProduct.itemName);
        }
        else
        {
            Debug.LogWarning("Not enough money to buy " + selectedProduct.itemName);
        }
    }
}
