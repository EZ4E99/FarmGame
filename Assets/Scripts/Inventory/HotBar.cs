using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class HotBar : Panel
{
    private HotBarSlot[] slots = new HotBarSlot[10];
    public int CurrentSelectedItem = 1;

    public override void _Ready()
    {
        for (int i = 0; i < 10; i++)
        {
            slots[i] = GetNode<HotBarSlot>($"GridContainer/HotBarSlot{i}");
        }
    }

    // 设置指定快捷键
    public void UpdateHotBar(int index, InventoryItem item)
    {
        for (int i = 0; i < 10; i++)
        {
            if (i == index)
                slots[i].UpdateSlot(item);
        }
    }

    // 更新快捷键显示
    public void UpdateHotBar()
    {
        Inventory inventory = (Inventory)GetTree().GetFirstNodeInGroup("inventory");
        Array<Node> HotBarSlots = GetTree().GetNodesInGroup("hotbar_slot");
        foreach (HotBarSlot hotBarSlot in HotBarSlots.Cast<HotBarSlot>())
        {
            if (hotBarSlot.item == null) return;
            List<Item> items = inventory.All(hotBarSlot.item.ItemName);
            foreach (InventoryItem item1 in items.Cast<InventoryItem>())
            {
                if (item1 == hotBarSlot.item)
                {
                    hotBarSlot.item.Amount = item1.Amount;
                }
            }
            hotBarSlot.UpdateSlot(hotBarSlot.item);
        }
    }

    // 选择快捷键
    public InventoryItem HotBarSelect(int index)
    {
        CurrentSelectedItem = index;

        for (int i = 0; i < 10; i++)
        {
            slots[i].SelectSlot(false);
        }
        slots[index].SelectSlot(true);
        return slots[index].item;
    }
}
