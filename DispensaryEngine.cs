using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DispensaryEngine : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string name;
        public int cost;
    }

    public Transform storeTrigger;
    public GameObject shopUIPlane;
    public Text shopMenuText;
    public PlayerAndCar movementScript;
    public BitLifeCustomizer characterData;

    public List<ShopItem> stonerHideoutMenu = new List<ShopItem>();
    private int selectedIndex = 0;
    private bool insideShop = false;

    private void Start()
    {
        stonerHideoutMenu.Add(new ShopItem { name = "Premium Exotic Bud", cost = 150 });
        stonerHideoutMenu.Add(new ShopItem { name = "Glass Vortex Rig", cost = 600 });
        stonerHideoutMenu.Add(new ShopItem { name = "Concentrate Vaporizer Pen", cost = 350 });
        stonerHideoutMenu.Add(new ShopItem { name = "Infused Fudge Brownie", cost = 50 });
        
        if (shopUIPlane != null) shopUIPlane.SetActive(false);
    }

    private void Update()
    {
        if (movementScript == null || movementScript.player3D == null || storeTrigger == null) return;

        float distance = Vector3.Distance(movementScript.player3D.transform.position, storeTrigger.position);

        // Enter Stoner Hideout
        if (!insideShop && distance <= 3.5f && !movementScript.isDriving)
        {
            EnterShop();
        }

        if (insideShop)
        {
            HandleShopMenuInput();
        }
    }

    private void EnterShop()
    {
        insideShop = true;
        if (shopUIPlane != null) shopUIPlane.SetActive(true);
        if (movementScript != null) movementScript.enabled = false; // Freeze 3D movement while shopping
        RenderShopText();
    }

    private void HandleShopMenuInput()
    {
        // Scroll Menu (D-Pad Up/Down)
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedIndex = (selectedIndex + 1) % stonerHideoutMenu.Count;
            RenderShopText();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedIndex = (selectedIndex - 1 + stonerHideoutMenu.Count) % stonerHideoutMenu.Count;
            RenderShopText();
        }

        // Buy Item (Xbox A Button)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            ShopItem item = stonerHideoutMenu[selectedIndex];
            if (characterData != null && characterData.cashBalance >= item.cost)
            {
                characterData.cashBalance -= item.cost;
                characterData.playerInventory.Add(item.name);
                RenderShopText();
                characterData.UpdateHUD();
            }
        }

        // Exit Shop (Xbox B Button)
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            insideShop = false;
            if (shopUIPlane != null) shopUIPlane.SetActive(false);
            if (movementScript != null)
            {
                movementScript.enabled = true;
                movementScript.player3D.transform.position += new Vector3(0f, 0f, -2.5f); // Step back from door
            }
        }
    }

    private void RenderShopText()
    {
        if (shopMenuText == null || characterData == null) return;
        
        shopMenuText.text = "<b>=== WELCOME TO THE STONER HIDEOUT ===</b>\n" +
                            "CASH: $" + characterData.cashBalance + "\n\n";

        for (int i = 0; i < stonerHideoutMenu.Count; i++)
        {
            if (i == selectedIndex)
                shopMenuText.text += " > [" + stonerHideoutMenu[i].name + " - $" + stonerHideoutMenu[i].cost + "] <\n";
            else
                shopMenuText.text += "   " + stonerHideoutMenu[i].name + " - $" + stonerHideoutMenu[i].cost + "\n";
        }
        shopMenuText.text += "\n<i>[A] Buy  |  [B] Exit Shop</i>";
    }
}

