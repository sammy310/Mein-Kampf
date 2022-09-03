using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickSlotManager
{
    public static QuickSlotManager Instance { get; private set; } = new QuickSlotManager();


    public int CurrentQuickSlotIndex { get; private set; } = 0;


    public void SetQuickSlotIndex(int index)
    {
        CurrentQuickSlotIndex = index;
    }

    public void IncreaseQuickSlotIndex()
    {
        if (++CurrentQuickSlotIndex >= Inventory.QuickSlotSize)
        {
            CurrentQuickSlotIndex = 0;
        }
    }

    public void DecreaseQuickSlotIndex()
    {
        if (--CurrentQuickSlotIndex < 0)
        {
            CurrentQuickSlotIndex = Inventory.QuickSlotSize - 1;
        }
    }


    public void UseCurrentItem()
    {
        Item item = Inventory.Instance.GetQuickSlotItem(CurrentQuickSlotIndex);
        if (item == null)
        {
            return;
        }

        if (item.IsBlock)
        {
            var blockPos = WorldManager.Instance.PlayerController.GetLookAtOppsiteBlockPosition();
            if (blockPos.IsNull == false)
            {
                if (WorldManager.Instance.IsAnyPlayerInBlockPosition(blockPos) == false)
                {
                    if (item.UseItem(1) == 0)
                    {
                        WorldManager.Instance.ChunkManager.SetBlock(blockPos, item.ItemType);
                    }
                }
            }
        }
    }
}
