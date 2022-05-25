using LootLocker.Requests;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class InventoryManager : MonoBehaviour
{

    public GameObject inventoryItemPrefab;

    public InventoryItemScriptableObject inventoryItemsScriptableObject;

    public Transform inventoryGrid;

    private List<InventoryItem> inventoryItems = new List<InventoryItem>();

    public List<InventoryItem> equippedItems = new List<InventoryItem>();

    public GameObject lootBoxCanvas;

    public UISwoosh lootBoxCanvasSwoosh;

    public Transform headEquipmentSlot;
    public SkinnedMeshRenderer armorMesh;
    public Transform weaponEquipmentSlot;
    public Transform braceletEquipmentSlot;

    public Transform powerBoosterSlot;

    public bool playerIsEquipped;

    int currentLootBoxAssetInstanceID;

    public TextMeshProUGUI lootboxInfoText;
    public static InventoryManager instance;

    public int characterID;

    public List<GameObject> currentLootBoxItems = new List<GameObject>();

    public Transform lootBoxGrid;

    public GameObject closeLootBoxWindowButton;
    public GameObject openLootBoxButton;

    public Animation getLootBoxAnimation;

    public Animator lootBoxAnimator;

    public Animator playerAnimator;

    public GameObject boosterCountdownObject;
    public Image boosterCountDownImage;

    private void Awake()
    {
        instance = this;
    }

    public void DropTableTest()
    {
        int assetIDToGiveToPlayer = 124926;
        LootLockerSDKManager.TriggeringAnEvent("DropTableTrigger", (triggerResponse) =>
        {
            if (triggerResponse.success)
            {
                LootLockerSDKManager.GetAssetNotification((assetResponse) =>
                {
                    if (assetResponse.success)
                    {
                        LootLockerSDKManager.ComputeAndLockDropTable(assetResponse.objects[0].instance_id, (dropResponse) =>
                        {
                        if (dropResponse.success)
                        {
                            int dropPick = 0;
                                Debug.Log(dropResponse.items.Length);
                            for (int i = 0; i < dropResponse.items.Length; i++)
                            {
                                if(dropResponse.items[i].asset_id == assetIDToGiveToPlayer)
                                {
                                    dropPick = dropResponse.items[i].id;
                                    break;
                                }
                            }
                            int[] picks = { dropPick };
                            LootLockerSDKManager.PickDropsFromDropTable(picks, assetResponse.objects[0].instance_id, (response) =>
                            {
                                if (response.success)
                                {
                                    // Added to inventory
                                }
                            });
                        }
                    });
                }   
                });
            }
        });
    }

    public void GetCharacterList()
    {
        LootLockerSDKManager.ListCharacterTypes((response) => { });
    }

    public void GetCharacterEquippableItems()
    {
        LootLockerSDKManager.GetEquipableContextToDefaultCharacter((response) => { });
    }

    public void GetLootBox()
    {
        string triggerName = "GetLootBox";
        getLootBoxAnimation.Play();
        LootLockerSDKManager.TriggeringAnEvent(triggerName, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully triggered event");
            }
            else
            {
                Debug.Log("Error triggering event");
            }
        });
    }

    public void GetPlayerInventory()
    {
        LootLockerSDKManager.GetInventory((response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully retrieved player inventory: " + response.inventory);
                UpdateInventory(response.inventory);
            }
            else
            {
                Debug.Log("Error getting player inventory");
            }
        });
    }

    public void ClearInventory()
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            // Can be null because of timed items
            if (inventoryItems[i] != null)
            {
                Destroy(inventoryItems[i].gameObject);
            }
        }
        inventoryItems.Clear();
    }

    void UpdateInventory(LootLockerInventory[] playerInventory)
    {
        for (int i = 0; i < playerInventory.Length; i++)
        {
            // If it is an already consumed item, them we shouldn't include it in the inventory
            // the server will remove this in the background for us
            if (playerInventory[i].rental != null && playerInventory[i].rental.time_left != null && int.Parse(playerInventory[i].rental.time_left) <= 0)
            {
                // Do nothing
            }
            else
            {
                HandleItem(playerInventory[i]);
            }
        }
    }

    void HandleItem(LootLockerInventory inventory)
    {
        for (int i = 0; i < inventoryItemsScriptableObject.items.Count; i++)
        {
            if (inventory.asset.id == inventoryItemsScriptableObject.items[i].assetId)
            {
                GameObject newItem = Instantiate(inventoryItemPrefab, inventoryGrid);
                InventoryItem newInventoryItem = newItem.GetComponent<InventoryItem>();
                newInventoryItem.context = inventory.asset.context;
                newInventoryItem.GenereateForInventory(inventoryItemsScriptableObject.items[i], inventory, this);
                inventoryItems.Add(newInventoryItem);
            }
        }
    }

    public IEnumerator HandleEquipmentRoutine()
    {
        bool done = false;
        LootLockerSDKManager.GetCurrentLoadOutToDefaultCharacter((response) =>
       {
           if (response.success)
           {
               for (int i = 0; i < response.loadout.Length; i++)
               {
                   VisuallyEquipItem(response.loadout[i].asset.id, response.loadout[i].instance_id);
               }
               done = true;
               playerIsEquipped = true;
               Debug.Log("Got loadout");
           }
           else
           {
               done = true;
               Debug.Log("Error:" + response.Error);
           }
       });
        yield return new WaitWhile(() => done == false);
    }

    bool IsAssetAlreadyEquipped(int assetID)
    {
        for (int i = 0; i < equippedItems.Count; i++)
        {
            if (equippedItems[i].assetID == assetID)
            {
                return true;
            }
        }
        return false;
    }

    void VisuallyEquipItem(int assetID, int assetInstanceID, bool isBooster = false, float duration = 0)
    {
        if (IsAssetAlreadyEquipped(assetID))
        {
            VisuallyUnEquip(assetID);
        }
        for (int i = 0; i < inventoryItemsScriptableObject.items.Count; i++)
        {
            InventoryItemScriptableObject.Item item = inventoryItemsScriptableObject.items[i];
            if (assetID == item.assetId)
            {
                GameObject newItem = Instantiate(inventoryItemPrefab, inventoryGrid);
                InventoryItem newInventoryItem = newItem.GetComponent<InventoryItem>();
                newInventoryItem.GenerateEquipped(inventoryItemsScriptableObject.items[i], this, assetInstanceID);

                equippedItems.Add(newInventoryItem);
                switch (item.equipmentSlot)
                {
                    case InventoryItemScriptableObject.EquipmentSlot.Head:
                        newItem.transform.SetParent(headEquipmentSlot);
                        break;
                    case InventoryItemScriptableObject.EquipmentSlot.Armor:
                        armorMesh.sharedMesh = item.mesh;
                        newItem.gameObject.SetActive(false);
                        break;
                    case InventoryItemScriptableObject.EquipmentSlot.Weapon:
                        newItem.transform.SetParent(weaponEquipmentSlot);
                        break;
                    case InventoryItemScriptableObject.EquipmentSlot.PowerBooster:
                        newItem.transform.SetParent(powerBoosterSlot);
                        break;
                    case InventoryItemScriptableObject.EquipmentSlot.Bracelet:
                        newItem.transform.SetParent(braceletEquipmentSlot);
                        break;
                    default:
                        break;
                }

                // Set up object to be positioned correctly
                newInventoryItem.transform.GetChild(0).localPosition = Vector3.zero;
                newItem.transform.localPosition = Vector3.zero;
                newItem.transform.localRotation = Quaternion.identity;
                newItem.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Default");
                newItem.GetComponent<RectTransform>().localScale = Vector3.one;
                if (isBooster)
                {
                    newInventoryItem.GenerateVisualBooster(item, duration);
                    playerAnimator.SetTrigger("Powerup");
                }
            }
        }
    }

    void VisuallyUnEquip(int assetID)
    {
        List<InventoryItem> tempItems = new List<InventoryItem>(equippedItems);
        for (int i = 0; i < equippedItems.Count; i++)
        {
            if (assetID == equippedItems[i].assetID)
            {
                if (GetItemInfo(assetID).equipmentSlot == InventoryItemScriptableObject.EquipmentSlot.Armor)
                {
                    armorMesh.sharedMesh = null;
                    tempItems.RemoveAt(i);
                }
                else
                {
                    Destroy(equippedItems[i].gameObject);
                    tempItems.RemoveAt(i);
                }
            }
        }
        equippedItems = tempItems;
    }

    public void InspectLootBox(int assetInstanceID)
    {
        lootboxInfoText.text = "";
        lootBoxAnimator.SetTrigger("Reset");
        lootBoxCanvasSwoosh.Show();
        openLootBoxButton.SetActive(true);
        closeLootBoxWindowButton.SetActive(false);
        LootLockerSDKManager.InspectALootBoxForAssetInstances(assetInstanceID, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully inspected Loot box");
                int totalWeight = 0;
                for (int i = 0; i < response.contents.Length; i++)
                {
                    totalWeight += response.contents[i].weight;
                }
                for (int i = 0; i < response.contents.Length; i++)
                {
                    currentLootBoxAssetInstanceID = assetInstanceID;
                    float percentageChance = ((float)response.contents[i].weight / (float)totalWeight);
                    lootboxInfoText.text += percentageChance.ToString("0.00") + "% chance to get " + GetItemInfo(response.contents[i].asset_id).name + "\n";
                }
            }
            else
            {
                Debug.Log("Error inspecting Loot box");
            }
        });
    }

    InventoryItemScriptableObject.Item GetItemInfo(int assetID)
    {
        for (int i = 0; i < inventoryItemsScriptableObject.items.Count; i++)
        {
            if (inventoryItemsScriptableObject.items[i].assetId == assetID)
            {
                return inventoryItemsScriptableObject.items[i];
            }
        }
        Debug.Log("Error, item does not exist");
        return null;
    }

    public void OpenLootBox()
    {
        ClearInventory();
        lootBoxAnimator.SetTrigger("Open");
        LootLockerSDKManager.OpenALootBoxForAssetInstances(currentLootBoxAssetInstanceID, (response) =>
        {
            if (response.success)
            {
                // Animation playing here
                Debug.Log("Successfully opened Loot box");
                
                GetPlayerInventory();
                lootboxInfoText.text = "";
                LootLockerSDKManager.GetAssetNotification((response) =>
                {
                    if (response.success)
                    {
                        for (int i = 0; i < response.objects.Length; i++)
                        {
                            if (response.objects[i].asset.id != 124928)
                            {
                                GameObject newItem = Instantiate(inventoryItemPrefab, lootBoxGrid);
                                InventoryItem newInventoryItem = newItem.GetComponent<InventoryItem>();
                                newInventoryItem.GenerateForLootBox(GetItemInfo(response.objects[i].asset.id));
                                currentLootBoxItems.Add(newItem);
                            }
                        }
                        lootBoxAnimator.SetTrigger("Opened");
                        openLootBoxButton.SetActive(false);
                        closeLootBoxWindowButton.SetActive(true);
                    }
                    else
                    {
                        Debug.Log("Error showing Loot box content");
                        GetPlayerInventory();
                    }
                });
            }
            else
            {
                Debug.Log("Error opening Loot box");
                GetPlayerInventory();
            }
        });
    }

    public void CloseLootBoxWindow()
    {
        for (int i = 0; i < currentLootBoxItems.Count; i++)
        {
            Destroy(currentLootBoxItems[i]);
        }
        currentLootBoxItems.Clear();
        lootBoxCanvasSwoosh.Hide();
    }

    public class RentalResponse
    {
        public string time_left;
    }

    public void EquipAsset(int assetInstanceID, bool equipped, InventoryItem item)
    {

        if (PlayerManager.instance.CanBeEquippedByPlayer(item.context) == false)
        {
            StartCoroutine(item.UnEquippableRoutine());
            return;
        }

        if (equipped == false)
        {
            //Check if it is a rental asset
            if (item.isRentalAsset)
            { 
                // Get the item included and equip it and activate the rental asset
                LootLockerSDKManager.ActivatingARentalAsset(assetInstanceID, (response) =>
                {
                    if (response.success)
                    {
                        RentalResponse rentalResponse = JsonUtility.FromJson<RentalResponse>(response.text);
                        // For inventory item
                        Destroy(item.gameObject);
                        VisuallyEquipItem(item.assetID, assetInstanceID, true, int.Parse(rentalResponse.time_left));
                    }
                    else
                    {
                        Debug.Log("Error activating rental asset");
                    }
                });
            }
            else
            {
                LootLockerSDKManager.EquipIdAssetToDefaultCharacter(assetInstanceID.ToString(), (response) =>
                {
                    if (response.success)
                    {
                        Debug.Log("Asset equipped");

                        item.equipped = true;
                        VisuallyEquipItem(item.assetID, assetInstanceID);
                        EquipItemInInventory(assetInstanceID, item.context);
                    }
                    else
                    {
                        Debug.Log("Asset equip error");
                    }
                });
            }
        }
        else
        {
            LootLockerSDKManager.UnEquipIdAssetToDefaultCharacter(assetInstanceID.ToString(), (response) =>
            {
                if (response.success)
                {
                    Debug.Log("Asset unequipped");
                    item.equipped = false;
                    VisuallyUnEquip(item.assetID);
                    UnEquipItemInInventory(assetInstanceID);
                }
                else
                {
                    Debug.Log("Asset equip error");
                }
            });
        }

    }

    public void UnEquipItemInInventory(int assetInstanceID)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].assetInstanceID == assetInstanceID)
            {
                inventoryItems[i].UnEquipInInventory();
            }
        }
    }
    public void EquipItemInInventory(int assetInstanceID, string context)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            // Should we unequip anything else in the inventory?
            if (inventoryItems[i].context == context && inventoryItems[i].equipped == true)
            {
                UnEquipItemInInventory(inventoryItems[i].assetInstanceID);
            }
            if (inventoryItems[i].assetInstanceID == assetInstanceID)
            {
                inventoryItems[i].EquipInInventory();
            }
        }
    }

    public IEnumerator ActivateBoosterUIRoutine(float duration)
    {
        float timer = 0f;
        boosterCountdownObject.SetActive(true);
        boosterCountdownObject.GetComponent<UISwoosh>().Show();
        boosterCountDownImage.fillAmount = 0f;
        while(timer <= duration)
        {
            timer += Time.deltaTime;
            boosterCountDownImage.fillAmount = timer / duration;
            yield return null;
        }
        boosterCountDownImage.fillAmount = 0f;
        boosterCountdownObject.GetComponent<UISwoosh>().Hide();
    }

    public bool IsItemEquipped(int assetInstanceID)
    {
        for (int i = 0; i < equippedItems.Count; i++)
        {
            if (equippedItems[i].assetInstanceID == assetInstanceID)
            {
                return true;
            }
        }
        return false;
    }
}
