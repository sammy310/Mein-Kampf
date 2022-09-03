using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    None, Air, Dirt,
}

public class Item
{
    public static int MaxStack = 64;
    
    public int InventoryIndex { get; set; }

    public ItemType ItemType { get; set; }
    public int ItemIndex => (int)ItemType;

    public int Stack { get; set; }
    public bool IsItemEmpty => (Stack <= 0);

    public bool IsBlock => true;

    public Item(int inventoryIndex, ItemType itemType)
    {
        InventoryIndex = inventoryIndex;
        ItemType = itemType;
        Stack = 1;
    }


    public int SetItemCount(int count)
    {
        if (MaxStack < count)
        {
            Stack = 64;
            return count - MaxStack;
        }

        Stack = count;
        return 0;
    }

    public int AddItem(int count)
    {
        Stack += count;
        int gap = Stack - MaxStack;
        if (gap > 0)
        {
            Stack = MaxStack;
            return gap;
        }
        return 0;
    }

    public int UseItem(int count)
    {
        Stack -= count;
        if (Stack <= 0)
        {
            int remain = -Stack;
            Stack = 0;
            return remain;
        }
        return 0;
    }

    public class SortingByInventoryIndex : IComparer<Item>
    {
        public int Compare(Item x, Item y)
        {
            return x.ItemIndex.CompareTo(y.ItemIndex);
        }
    }
}

public class Inventory
{
    public static Inventory Instance { get; private set; } = new Inventory();


    public const int QuickSlotSize = 10;
    public const int InventorySize = 40;

    public Item[] QuickSlotList = new Item[QuickSlotSize];
    public Item[] InventoryList = new Item[InventorySize];

    public Dictionary<ItemType, SortedList<int, Item>> ItemDict = new Dictionary<ItemType, SortedList<int, Item>>();
    public Dictionary<ItemType, int> ItemCountDict = new Dictionary<ItemType, int>();


    public Item GetItem(ItemType itemType)
    {
        if (ItemDict.TryGetValue(itemType, out var itemList))
        {
            return itemList[itemList.Count - 1];
        }
        return null;
    }

    public Item GetQuickSlotItem(int quickSlotIndex)
    {
        return QuickSlotList[quickSlotIndex];
    }

    public void AddItem(ItemType itemType, int itemCount)
    {
        int requestAddCount = itemCount;

        SortedList<int, Item> itemList = null;
        if (ItemDict.TryGetValue(itemType, out itemList))
        {
            foreach (var item in itemList.Values)
            {
                if (itemCount > 0)
                {
                    itemCount = item.AddItem(itemCount);
                }
                else
                {
                    break;
                }
            }
        }

        while (itemCount > 0)
        {
            Item item = AddNewItem(itemType);
            if (item != null)
            {
                itemCount = item.SetItemCount(itemCount);
            }
            else
            {
                break;
            }
        }

        requestAddCount -= itemCount;
        if (ItemCountDict.TryGetValue(itemType, out int count))
        {
            ItemCountDict[itemType] += requestAddCount;
        }
        else
        {
            ItemCountDict.Add(itemType, requestAddCount);
        }
    }

    public Item AddNewItem(ItemType itemType)
    {
        int inventoryIndex = GetEmptyInventoryIndex();
        Debug.Log($"Add new {itemType.ToString()} to {inventoryIndex}");

        if (inventoryIndex < 0)
        {
            return null;
        }

        Item item = new Item(inventoryIndex, itemType);
        SetItemToInventory(item);
        
        return item;
    }

    public void UseItem(ItemType itemType, int itemCount)
    {
        int requestUseCount = itemCount;

        List<Item> removeItems = new List<Item>();
        if (ItemDict.TryGetValue(itemType, out var itemList))
        {
            foreach (var itemValue in itemList)
            {
                Item item = itemValue.Value;

                itemCount = item.UseItem(itemCount);
                if (item.IsItemEmpty)
                {
                    removeItems.Add(item);
                }

                if (itemCount == 0)
                {
                    break;
                }
            }
        }

        foreach (var item in removeItems)
        {
            RemoveItemFromInventory(item);
        }

        requestUseCount -= itemCount;
        if (ItemCountDict.TryGetValue(itemType, out int count))
        {
            ItemCountDict[itemType] -= requestUseCount;
        }
    }

    private void SetItemToInventory(Item item)
    {
        int inventoryIndex = item.InventoryIndex;
        if (inventoryIndex < QuickSlotSize)
        {
            QuickSlotList[inventoryIndex] = item;
        }
        else
        {
            InventoryList[inventoryIndex - QuickSlotSize] = item;
        }

        SortedList<int, Item> itemList = null;
        if (ItemDict.TryGetValue(item.ItemType, out itemList) == false)
        {
            itemList = new SortedList<int, Item>();
            ItemDict.Add(item.ItemType, itemList);
        }
        itemList.Add(inventoryIndex, item);
    }

    private void RemoveItemFromInventory(Item item)
    {
        int inventoryIndex = item.InventoryIndex;
        if (inventoryIndex < QuickSlotSize)
        {
            QuickSlotList[inventoryIndex] = null;
        }
        else
        {
            InventoryList[inventoryIndex - QuickSlotSize] = null;
        }

        if (ItemDict.TryGetValue(item.ItemType, out var itemList))
        {
            itemList.Remove(inventoryIndex);
        }
    }


    private int GetEmptyInventoryIndex()
    {
        for (int i = 0; i < QuickSlotSize; i++)
        {
            if (QuickSlotList[i] == null)
            {
                return i;
            }
        }

        for (int i = 0; i < InventorySize; i++)
        {
            if (InventoryList[i] == null)
            {
                return QuickSlotSize + i;
            }
        }

        return -1;
    }
}
