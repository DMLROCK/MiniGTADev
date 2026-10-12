using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DispensaryEngine : MonoBehaviour
{
    [System.Serializable]
    public class ShopProduct
    {
        public string name;
        public int price;
    }

    [Header("--- STONER HIDEOUT STAFFING ---")]
    public string primaryClerk = "Snoop Dogg";
    public string assistantClerk = "FlightReacts";
    public string securityStaff = "Tactical Security Guard";

    [Header("--- SYSTEM INTERFACES ---")]
    public Transform shopDoorwayMarker;
    public GameObject dashboardUIPanel;
    public Text interfaceTextMesh;
    public PlayerGTAController playerController;
    public BitLifeCustomizer customizerDatabase;

    public List<ShopProduct> dispensaryMenu = new List<ShopProduct>();

    private int selectedProductIndex;
    private bool isPlayerInsideStore;
    private bool playerWasEnabledBeforeStore;

    private void Start()
    {
        // Add defaults only if the list is empty, preventing duplicate products
        // when products have already been configured in the Inspector.
        if (dispensaryMenu == null)
            dispensaryMenu = new List<ShopProduct>();

        if (dispensaryMenu.Count == 0)
        {
            dispensaryMenu.Add(new ShopProduct { name = "Premium Exotic Bud", price = 150 });
            dispensaryMenu.Add(new ShopProduct { name = "Glass Vortex Rig", price = 600 });
            dispensaryMenu.Add(new ShopProduct { name = "Concentrate Vaporizer Pen", price = 350 });
            dispensaryMenu.Add(new ShopProduct { name = "Infused Fudge Brownie", price = 50 });
        }

        if (dashboardUIPanel != null)
            dashboardUIPanel.SetActive(false);
    }

    private void Update()
    {
        if (playerController == null || shopDoorwayMarker == null)
            return;

        float spatialGap = Vector3.Distance(
            playerController.transform.position, shopDoorwayMarker.position);

        if (!isPlayerInsideStore && spatialGap <= 3.5f && !playerController.isDriving)
            TriggerShopMenuOpen();

        if (isPlayerInsideStore)
            ProcessStoreMenuInputs();
    }

    private void TriggerShopMenuOpen()
    {
        isPlayerInsideStore = true;

        if (dashboardUIPanel != null)
            dashboardUIPanel.SetActive(true);

        playerWasEnabledBeforeStore = playerController.enabled;
        playerController.enabled = false;
        RenderDispensaryLayoutScreen();
    }

    private void ProcessStoreMenuInputs()
    {
        if (dispensaryMenu == null || dispensaryMenu.Count == 0)
        {
            if (interfaceTextMesh != null)
                interfaceTextMesh.text = "The shop currently has no products.\nPress Escape / Controller B to leave.";
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                selectedProductIndex = (selectedProductIndex + 1) % dispensaryMenu.Count;
                RenderDispensaryLayoutScreen();
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                selectedProductIndex = (selectedProductIndex - 1 + dispensaryMenu.Count) % dispensaryMenu.Count;
                RenderDispensaryLayoutScreen();
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
                PurchaseSelectedProduct();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
            CloseShop();
    }

    private void PurchaseSelectedProduct()
    {
        if (customizerDatabase == null || dispensaryMenu == null ||
            dispensaryMenu.Count == 0 || selectedProductIndex < 0 ||
            selectedProductIndex >= dispensaryMenu.Count)
            return;

        ShopProduct activeSelection = dispensaryMenu[selectedProductIndex];
        if (activeSelection == null || string.IsNullOrWhiteSpace(activeSelection.name) ||
            activeSelection.price < 0)
            return;

        if (customizerDatabase.cashBalance < activeSelection.price)
        {
            if (interfaceTextMesh != null)
                interfaceTextMesh.text += "\n\nNot enough cash for this item.";
            return;
        }

        if (customizerDatabase.playerInventory == null)
            customizerDatabase.playerInventory = new List<string>();

        customizerDatabase.cashBalance -= activeSelection.price;
        customizerDatabase.playerInventory.Add(activeSelection.name);

        RenderDispensaryLayoutScreen();
        customizerDatabase.UpdateHUD();
    }

    private void CloseShop()
    {
        isPlayerInsideStore = false;

        if (dashboardUIPanel != null)
            dashboardUIPanel.SetActive(false);

        if (playerController != null)
        {
            playerController.enabled = playerWasEnabledBeforeStore;
            playerController.transform.position += new Vector3(0f, 0f, -3f);
        }
    }

    private void RenderDispensaryLayoutScreen()
    {
        if (interfaceTextMesh == null || customizerDatabase == null)
            return;

        string clerkDialogue = selectedProductIndex == 0
            ? "[" + primaryClerk + "]: 'Welcome! Take a look at today's selection.'"
            : "[" + assistantClerk + "]: 'Take a look at the selection!'";

        interfaceTextMesh.text =
            "=== STONER HIDEOUT SHOP ===\n" +
            "[STAFF]: " + securityStaff + "\n\n" +
            clerkDialogue + "\n\n" +
            "YOUR CURRENT WALLET: $" + customizerDatabase.cashBalance +
            "\n--------------------------------------------------\n";

        if (dispensaryMenu == null || dispensaryMenu.Count == 0)
        {
            interfaceTextMesh.text += "No products available.\n";
        }
        else
        {
            selectedProductIndex = Mathf.Clamp(selectedProductIndex, 0, dispensaryMenu.Count - 1);
            for (int i = 0; i < dispensaryMenu.Count; i++)
            {
                ShopProduct product = dispensaryMenu[i];
                if (product == null)
                    continue;

                interfaceTextMesh.text += i == selectedProductIndex
                    ? " ==> [" + product.name + " - $" + product.price + "] <==\n"
                    : "     " + product.name + " - $" + product.price + "\n";
            }
        }

        interfaceTextMesh.text +=
            "\n[Space / Controller A] Purchase  |  [Escape / Controller B] Leave";
    }
}
