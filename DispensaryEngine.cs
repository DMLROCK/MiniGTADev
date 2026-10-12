using UnityEngine;
using UnityEngine.UI;

public class DispensaryEngine : MonoBehaviour
{
    [System.Serializable]
    public struct DispensaryItem
    {
        public string itemName;
        public int basePrice;
        [HideInInspector] public int currentPrice;
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
    public GameObject promptTooltipUI; // Text pop-up: "Press E to Interact"

    private int shopSelectionIndex = 0;
    private bool isPlayerNearCounter = false;
    private bool isPlayerNearATM = false;
    private bool isInsideShopMenu = false;
    private int discountItemIndex = -1;

    private void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (promptTooltipUI != null) promptTooltipUI.SetActive(false);
        
        for (int i = 0; i < menuItems.Length; i++)
        {
            menuItems[i].currentPrice = menuItems[i].basePrice;
        }
    }

    private void Update()
    {
        // Handle physical ATM interaction zone
        if (isPlayerNearATM && !isInsideShopMenu)
        {
            if (promptTooltipUI != null)
            {
                promptTooltipUI.SetActive(true);
                promptTooltipUI.GetComponent<Text>().text = "Press [E] to use Dispensary ATM";
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                InteractWithATM();
            }
            return;
        }

        // Handle physical Counter interaction zone
        if (isPlayerNearCounter && !isInsideShopMenu)
        {
            if (promptTooltipUI != null)
            {
                promptTooltipUI.SetActive(true);
                promptTooltipUI.GetComponent<Text>().text = "Press [E] to talk to Budtender";
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                OpenDispensary();
            }
        }
        else if (!isPlayerNearATM && !isPlayerNearCounter)
        {
            if (promptTooltipUI != null) promptTooltipUI.SetActive(false);
        }

        if (!isInsideShopMenu) return;

        // Shop Menu Navigation
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

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            TryPurchaseSelectedItem();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            CloseDispensary();
        }
    }

    // Dynamic checks using Object Names to distinguish Counter vs ATM zones
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameObject.name.Contains("ATM")) isPlayerNearATM = true;
            else isPlayerNearCounter = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearATM = false;
            isPlayerNearCounter = false;
            CloseDispensary();
        }
    }

    public void OpenDispensary()
    {
        if (playerData == null) return;
        isInsideShopMenu = true;
        shopSelectionIndex = 0;
        
        if (playerScript != null) playerScript.enabled = false;
        if (shopPanel != null) shopPanel.SetActive(true);

        CalculateDailyDeals();
        RenderShopMenu();
    }

    private void CalculateDailyDeals()
    {
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
        if (playerScript != null) playerScript.enabled = true;
    }

    private void RenderShopMenu()
    {
        if (shopText == null || playerData == null) return;

        shopText.text = "<b>=== GREEN LEAF DISPENSARY ===</b>\nCUSTOMER WALLET: $" + playerData.cashBalance + "\n\n";
        for (int i = 0; i < menuItems.Length; i++)
        {
            string dealTag = (i == discountItemIndex) ? " <color=green>[🔥 DAILY SPECIAL - 20% OFF]</color>" : "";
            if (i == shopSelectionIndex)
                shopText.text += " > [" + menuItems[i].itemName + "] - $" + menuItems[i].currentPrice + dealTag + " <\n";
            else
                shopText.text += "   " + menuItems[i].itemName + " - $" + menuItems[i].currentPrice + dealTag + "\n";
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

    public void InteractWithATM()
    {
        if (playerData == null) return;
        playerData.cashBalance += 500; // Adds $500 cash from bank vault
        playerData.UpdateHUD();
        Debug.Log("Withdrew $500 cash from the dispensary wall ATM.");
    }
}
