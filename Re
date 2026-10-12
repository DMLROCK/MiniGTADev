using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DispensaryEngine : MonoBehaviour
{
    [System.Serializable]
    public class ShopProduct { public string name; public int price; }

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
    private int selectedProductIndex = 0;
    private bool isPlayerInsideStore = false;

    private void Start()
    {
        dispensaryMenu.Add(new ShopProduct { name = "Premium Exotic Bud", price = 150 });
        dispensaryMenu.Add(new ShopProduct { name = "Glass Vortex Rig", price = 600 });
        dispensaryMenu.Add(new ShopProduct { name = "Concentrate Vaporizer Pen", price = 350 });
        dispensaryMenu.Add(new ShopProduct { name = "Infused Fudge Brownie", price = 50 });

        if (dashboardUIPanel != null) dashboardUIPanel.SetActive(false);
    }

    private void Update()
    {
        if (playerController == null || shopDoorwayMarker == null) return;

        float spatialGap = Vector3.Distance(playerController.transform.position, shopDoorwayMarker.position);

        if (!isPlayerInsideStore && spatialGap <= 3.5f && !playerController.isDriving)
        {
            TriggerShopMenuOpen();
        }

        if (isPlayerInsideStore)
        {
            ProcessStoreMenuInputs();
        }
    }

    private void TriggerShopMenuOpen()
    {
        isPlayerInsideStore = true;
        if (dashboardUIPanel != null) dashboardUIPanel.SetActive(true);
        playerController.enabled = false; // Freeze physical movement while interacting with inventory grids
        RenderDispensaryLayoutScreen();
    }

    private void ProcessStoreMenuInputs()
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

        // Buy (Xbox A Button / Joystick Button 0)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            ShopProduct activeSelection = dispensaryMenu[selectedProductIndex];
            if (customizerDatabase != null && customizerDatabase.cashBalance >= activeSelection.price)
            {
                customizerDatabase.cashBalance -= activeSelection.price;
                customizerDatabase.playerInventory.Add(activeSelection.name);
                RenderDispensaryLayoutScreen();
                customizerDatabase.UpdateHUD();
            }
        }

        // Exit Store (Xbox B Button / Joystick Button 1)
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            isPlayerInsideStore = false;
            if (dashboardUIPanel != null) dashboardUIPanel.SetActive(false);
            playerController.enabled = true;
            playerController.transform.position += new Vector3(0f, 0f, -3.0f); // Push character outside guard checkpoint safely
        }
    }

    private void RenderDispensaryLayoutScreen()
    {
        if (interfaceTextMesh == null || customizerDatabase == null) return;

        // Custom, dynamic character voice generation blocks if no outside models are loaded yet
        string clerkDialogue = (selectedProductIndex == 0) 
            ? "<b>[" + primaryClerk + "]:</b> 'Drop it like it's hot, nephew. That's top shelf organic right there.'" 
            : "<b>[" + assistantClerk + "]:</b> 'Look at the detail! No way man, look at the scaling! Is that real?! LETS GOOO!'";

        interfaceTextMesh.text = "<b>=== STONER HIDEOUT SHOP MAIN OFFICE ===</b>\n" +
                                  "<b>[SECURITY STATUS]:</b> " + securityStaff + " standing guard at main entrance.\n\n" +
                                  clerkDialogue + "\n\n" +
                                  "<b>YOUR CURRENT WALLET:</b> $" + customizerDatabase.cashBalance + "\n" +
                                  "--------------------------------------------------\n";

        for (int i = 0; i < dispensaryMenu.Count; i++)
        {
            if (i == selectedProductIndex)
                interfaceTextMesh.text += "  ==> [" + dispensaryMenu[i].name + " - $" + dispensaryMenu[i].price + "] <==\n";
            else
                interfaceTextMesh.text += "      " + dispensaryMenu[i].name + " - $" + dispensaryMenu[i].price + "\n";
        }
        interfaceTextMesh.text += "\n\n<i>[Xbox A] Purchase Selected Good  |  [Xbox B] Walk Out Past Security</i>";
    }
}
