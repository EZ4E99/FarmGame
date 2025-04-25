using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Control
{
    [Export] public int Rows = 3; // 行数
    [Export] public int Cols = 6; // 列数

    [Export] public GridContainer InventoryGrid; // 背包网格
    [Export] public PackedScene InventorySlotScene; // 背包槽的场景
    [Export] public Tooltip Tooltip; // 提示框，必须在所有实例之间共享

    private List<InventorySlot> Slots = new List<InventorySlot>(); // 背包槽列表
    private static Item SelectedItem = null; // 选中的物品（静态）

    private PackedScene _inventoryItemScene = GD.Load<PackedScene>("res://Assets/Scenes/UI/inventory_item.tscn"); // 物品实例化场景

    public override void _Ready()
    {
        // 设置网格的列数
        InventoryGrid.Columns = Cols;

        // 初始化背包槽
        for (int i = 0; i < Rows * Cols; i++)
        {
            var slot = (InventorySlot)InventorySlotScene.Instantiate();
            Slots.Add(slot);
            InventoryGrid.AddChild(slot);

            // 连接信号
            slot.SlotInput += _on_slot_input;
            slot.SlotHovered += _on_slot_hovered;
        }

        // 默认隐藏提示框
        Tooltip.Visible = false;

        AddToGroup("inventory");
    }

    public override void _Process(double delta)
    {
        // 更新提示框的位置，跟随鼠标
        Tooltip.GlobalPosition = GetGlobalMousePosition() + new Vector2(8, 8);

        if (SelectedItem != null)
        {
            Tooltip.Visible = false;
            SelectedItem.GlobalPosition = GetGlobalMousePosition();
        }
    }

    // 背包槽点击事件处理
    private void _on_slot_input(InventorySlot which, InventorySlot.InventorySlotAction action)
    {
        GD.Print(action);
        GD.Print(SelectedItem);
        // 如果没有选中物品，则选择或拆分物品
        if (SelectedItem == null)
        {
            if (action == InventorySlot.InventorySlotAction.Select)
            {
                SelectedItem = which.SelectItem();
            }
        }
        else
        {
            SelectedItem = which.DeselectItem((InventoryItem)SelectedItem); // 取消选择
        }
    }

    // 背包槽悬停事件处理
    private void _on_slot_hovered(InventorySlot which, bool isHovering)
    {
        if (which.Item != null)
        {
            Tooltip.SetText(which.Item.ItemName);
            Tooltip.Visible = isHovering;
        }
        else if (which.HintItem != null)
        {
            Tooltip.SetText(which.HintItem.ItemName);
            Tooltip.Visible = isHovering;
        }
    }

    // API：

    // 销毁性（从世界中移除物品并将其副本添加到背包中）
    // 调用此函数意味着该物品还不在背包中
    public void AddItem(Item item, int amount)
    {
        var _item = (InventoryItem)_inventoryItemScene.Instantiate(); // 创建物品副本
        _item.Initialize(item.ItemName, item.Icon, item.IsStackable, amount, item.ItemType);

        // 如果物品是堆叠型的，寻找合适的槽位进行堆叠
        if (item.IsStackable)
        {
            foreach (var slot in Slots)
            {
                if (slot.Item != null && slot.Item.ItemName == _item.ItemName) // 找到相同物品
                {
                    slot.Item.Amount += _item.Amount;
                    return;
                }
            }
        }

        // 如果找不到堆叠的槽位，则寻找空槽进行放置
        foreach (var slot in Slots)
        {
            if (slot.Item == null && slot.IsRespectingHint(_item))
            {
                slot.Item = _item;
                slot.UpdateSlot(); // 更新槽位
                return;
            }
        }
    }

    // 销毁性（从背包中取出物品并返回该物品）
    public Item RetrieveItem(string itemName)
    {
        foreach (var slot in Slots)
        {
            if (slot.Item != null && slot.Item.ItemName == itemName)
            {
                var copyItem = new Item
                {
                    ItemName = slot.Item.ItemName,
                    Name = slot.Item.ItemName,
                    Icon = slot.Item.Icon,
                    IsStackable = slot.Item.IsStackable
                };

                // 如果物品数量大于 1，则减少数量，否则移除物品
                if (slot.Item.Amount > 1)
                {
                    slot.Item.Amount -= 1;
                }
                else
                {
                    slot.RemoveItem(); // 移除物品
                    slot.UnalignHotKey();
                }

                return copyItem;
            }
        }
        return null; // 如果未找到物品，返回 null
    }

    // 非销毁性（只读函数）获取背包中的所有物品
    public List<Item> AllItems()
    {
        var items = new List<Item>();
        foreach (var slot in Slots)
        {
            if (slot.Item != null)
            {
                items.Add(slot.Item);
            }
        }
        return items;
    }

    // 非销毁性（只读函数）获取指定类型的所有物品
    public List<Item> All(string name)
    {
        var items = new List<Item>();
        foreach (var slot in Slots)
        {
            if (slot.Item != null && slot.Item.ItemName == name)
            {
                items.Add(slot.Item);
            }
        }
        return items;
    }

    // 销毁性（移除所有指定类型的物品）
    public void RemoveAll(string name)
    {
        foreach (var slot in Slots)
        {
            if (slot.Item != null && slot.Item.ItemName == name)
            {
                slot.RemoveItem();
            }
        }
    }

    // 销毁性（清空背包中的所有物品）
    public void ClearInventory()
    {
        foreach (var slot in Slots)
        {
            slot.RemoveItem();
        }
    }
}
