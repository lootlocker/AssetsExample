using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItemsScriptableObject", menuName = "ScriptableObjects/InventoryItems", order = 1)]
public class InventoryItemScriptableObject : ScriptableObject
{
    public enum EquipmentSlot { Head, Armor, Weapon, None, PowerBooster, Bracelet};
    [System.Serializable]
    public class Item
    {
        public int assetId;

        public Mesh mesh;

        public Sprite icon;

        public string name;

        public int attack;

        public int defence;

        public bool isLootBox;

        public string context;

        public EquipmentSlot equipmentSlot;

        public GameObject effect;

        public bool isRentalAsset;
    }
    public List<Item> items = new List<Item>();
}
