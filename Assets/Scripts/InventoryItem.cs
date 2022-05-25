using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LootLocker.Requests;

public class InventoryItem : MonoBehaviour
{
    public int assetID;

    public int assetInstanceID;

    public Mesh mesh;

    public string assetName;

    public int attack;

    public int defence;

    public bool isLootBox;

    public string context;

    public Image thumbnail;

    [SerializeField]
    private MeshFilter meshFilter;

    private InventoryManager inventoryManager;

    public bool equipped;

    public Image itemBackground;

    public GameObject equippedToggle;

    public GameObject unequippableMessageObject;

    private bool isLootBoxItem;

    private GameObject effectPrefab;

    public bool isRentalAsset;

    public float duration;

    public AnimationCurve lootBoxPresentationCurve;
    public AnimationCurve lootBoxSpinCurve;

    public Mesh lootBox3DModel;

    public void GenereateForInventory(InventoryItemScriptableObject.Item item, LootLockerInventory inventory, InventoryManager inventoryManager)
    {
        this.assetID = item.assetId;
        this.assetInstanceID = inventory.instance_id;
        this.assetName = item.name;
        this.attack = item.attack;
        this.defence = item.defence;
        this.isLootBox = item.isLootBox;
        this.inventoryManager = inventoryManager;
        // Get color of rarity
        ColorUtility.TryParseHtmlString("#"+inventory.asset.rarity.color, out Color rarityColor);
        this.itemBackground.color = rarityColor;
        this.thumbnail.sprite = item.icon;
        this.context = inventory.asset.context;
        this.isRentalAsset = item.isRentalAsset;
        this.effectPrefab = item.effect;
        if (inventoryManager.IsItemEquipped(assetInstanceID) == false)
        {
            UnEquipInInventory();
        }
        else
        {
            EquipInInventory();
        }
    }

    public void EquipInInventory()
    {
        equippedToggle.SetActive(true);
        equipped = true;
    }

    public void UnEquipInInventory()
    {
        equippedToggle.SetActive(false);
        equipped = false;
    }

    public IEnumerator SpinRoutine()
    {
        float timer = 0f;
        float animationDuraton = 5f;
        Vector3 rotation = transform.localEulerAngles;
        while (timer <= animationDuraton)
        {
            rotation.y = Mathf.Lerp(0f, 1080f, lootBoxSpinCurve.Evaluate(timer / animationDuraton));
            transform.localEulerAngles = rotation;
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localEulerAngles = Vector3.zero;
    }

    public IEnumerator AnimateForLootBoxRoutine()
    {
        float timer = 0f;
        float animationDuraton = 1f;
        while (timer <= animationDuraton)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, lootBoxPresentationCurve.Evaluate(timer / animationDuraton));
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    public void GenerateForLootBox(InventoryItemScriptableObject.Item item)
    {
        this.assetID = item.assetId;
        this.thumbnail.sprite = item.icon;
        itemBackground.enabled = false;
        isLootBoxItem = true;
        meshFilter.mesh = lootBox3DModel;
        meshFilter.transform.localEulerAngles = new Vector3(0, 90, 0);
        StartCoroutine(AnimateForLootBoxRoutine());
        StartCoroutine(SpinRoutine());
    }

    public void GenerateEquipped(InventoryItemScriptableObject.Item item, InventoryManager inventoryManager, int assetInstanceID)
    {
        this.assetID = item.assetId;
        this.mesh = item.mesh;
        this.assetName = item.name;
        this.attack = item.attack;
        this.defence = item.defence;
        this.inventoryManager = inventoryManager;
        this.assetInstanceID = assetInstanceID;
        this.isRentalAsset = item.isRentalAsset;
        meshFilter.mesh = mesh;
    }

    public void GenerateVisualBooster(InventoryItemScriptableObject.Item item, float duration)
    {
        this.effectPrefab = item.effect;
        if (effectPrefab != null)
        {
            Instantiate(effectPrefab, transform);
        }
        StartCoroutine(CountDownBooster(duration));
        StartCoroutine(inventoryManager.ActivateBoosterUIRoutine(duration));
    }

    public void UseItem()
    {
        if(isLootBox)
        {
            inventoryManager.InspectLootBox(assetInstanceID);
        }
        else if(isLootBoxItem == false)
        {
            inventoryManager.EquipAsset(assetInstanceID, inventoryManager.IsItemEquipped(assetInstanceID), this);
        }
    }

    public IEnumerator UnEquippableRoutine()
    {
        unequippableMessageObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        // If the inventory screen was closed
        if (unequippableMessageObject != null)
        {
            unequippableMessageObject.SetActive(false);
        }
    }

    private IEnumerator CountDownBooster(float boosterDuration)
    {
        yield return new WaitForSeconds(boosterDuration);
        Destroy(this.gameObject);
    }
}

