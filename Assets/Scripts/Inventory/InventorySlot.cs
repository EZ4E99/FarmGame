using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class InventorySlot : Control
{
    // 导出字段
    [Export] public PackedScene InventoryItemScene { get; set; }
    [Export] public InventoryItem Item { get; set; }
    [Export] public InventoryItem HintItem { get; set; }
    private TextureButton TextureButton;
    private Label _hotKey;
    private HotBar _hotBar;

    public bool HasHotKey;
    public int AlignedHotKey = -1;

    // 枚举类型
    public enum InventorySlotAction
    {
        Select,
        Split
    }

    // 信号定义
    [Signal] public delegate void SlotInputEventHandler(InventorySlot which, InventorySlotAction action);
    [Signal] public delegate void SlotHoveredEventHandler(InventorySlot which, bool isHovering);

    public override void _Ready()
    {
        AddToGroup("inventory_slots");
        UpdateSlot();

        TextureButton = GetNode<TextureButton>("TextureButton");
        _hotKey = GetNode<Label>("TextureButton/Label");
        _hotBar = (HotBar)GetTree().GetFirstNodeInGroup("hotbar");

        Callable OnTextureButtonMouseEnteredCallable = new(this, MethodName.OnTextureButtonMouseEntered);
        TextureButton.Connect("mouse_entered", OnTextureButtonMouseEnteredCallable, 0); 
        Callable OnTextureButtonMouseExitedCallable = new(this, MethodName.OnTextureButtonMouseExited);
        TextureButton.Connect("mouse_exited", OnTextureButtonMouseExitedCallable, 0); 
        Callable OnTextureButtonGUIInputCallable = new(this, MethodName.OnTextureButtonGuiInput);
        TextureButton.Connect("gui_input", OnTextureButtonGUIInputCallable, 0); 
    }

    // GUI输入处理
    private void OnTextureButtonGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.DoubleClick)
        {
            UnalignHotKey();
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.Left:
                    EmitSignal(SignalName.SlotInput, this, (int)InventorySlotAction.Select);
                    break;
                // 暂时禁用分割功能 —— 有BUG没有解决
                // case MouseButton.Right:
                //     EmitSignal(SignalName.SlotInput, this, (int)InventorySlotAction.Split);
                //     break;
            }
        }

        for (int i = 0; i <= 9; i++)
        {
            if (@event is InputEventKey && Input.IsActionJustPressed($"action_hotbar_{i}"))
            {
                Array<Node> slots = GetTree().GetNodesInGroup("inventory_slots");
                foreach (InventorySlot slot in slots.Cast<InventorySlot>())
                {
                    if (slot.AlignedHotKey == i)
                    {
                        GD.Print(slot.Item.ItemName);
                        slot.UnalignHotKey();
                    }
                }

                if (!HasHotKey && !IsEmpty())
                {
                    // 绑定新快捷键
                    HasHotKey = true;
                    _hotKey.Text = i.ToString();
                    _hotKey.Show();
                    AlignedHotKey = i;

                    // 更新 HotBar 槽位
                    _hotBar.UpdateHotBar(i, Item);
                }
                else if (HasHotKey && AlignedHotKey != i && !IsEmpty())
                {
                    // 清除其他槽位对快捷键i的绑定
                    _hotBar.UpdateHotBar(AlignedHotKey, null);
                    
                    // 更新新索引的绑定
                    AlignedHotKey = i;
                    _hotKey.Text = i.ToString();
                    _hotBar.UpdateHotBar(i, Item);
                }
                else if (HasHotKey && AlignedHotKey == i && !IsEmpty())
                {
                    // 取消当前绑定
                    _hotBar.UpdateHotBar(i, null);
                    UnalignHotKey();
                }

                return; // 确保只处理一次按键
            }
        }
    }

    public void TryAutoAssignToHotKey(InventorySlot pickedSlot)
    {
        // 获取所有 inventory slots
        Array<Node> slots = pickedSlot.GetTree().GetNodesInGroup("inventory_slots");
        HashSet<int> usedHotKeys = [];

        // 检查是否已有相同物品绑定（避免重复）
        foreach (InventorySlot slot in slots.Cast<InventorySlot>())
        {
            if (slot.HasHotKey)
            {
                usedHotKeys.Add(slot.AlignedHotKey);

                // 如果已有相同物品绑定，直接退出
                if (slot.Item == pickedSlot.Item)
                    return;
            }
        }

        // 寻找第一个未占用的快捷键位
        for (int i = 1; i <= 10; i++)
        {
            if (i == 10)
            {
                if (!usedHotKeys.Contains(0))
                {
                    // 绑定快捷键
                    pickedSlot.HasHotKey = true;
                    pickedSlot.AlignedHotKey = 0;
                    pickedSlot._hotKey.Text = "0";
                    pickedSlot._hotKey.Show();
                    
                    _hotBar.UpdateHotBar(0, pickedSlot.Item);
                    return;
                }
            }
            else if (!usedHotKeys.Contains(i))
            {
                // 绑定快捷键
                pickedSlot.HasHotKey = true;
                pickedSlot.AlignedHotKey = i;
                pickedSlot._hotKey.Text = i.ToString();
                pickedSlot._hotKey.Show();

                _hotBar.UpdateHotBar(i, pickedSlot.Item);
                return;
            }
        }

        // 所有快捷键已占用，或没有可用的
    }


    // 鼠标事件处理
    private void OnTextureButtonMouseEntered()
    {
        EmitSignal(SignalName.SlotHovered, this, true);
    }

    private void OnTextureButtonMouseExited()
    {
        EmitSignal(SignalName.SlotHovered, this, false);
    }

    // 验证提示物品
    public bool IsRespectingHint(InventoryItem newItem, bool checkAmount = true)
    {
        if (HintItem == null) return true;
        
        return checkAmount 
            ? newItem.ItemName == HintItem.ItemName && newItem.Amount >= HintItem.Amount
            : newItem.ItemName == HintItem.ItemName;
    }

    // 设置提示物品
    public void SetItemHint(InventoryItem newHint)
    {
        HintItem?.QueueFree();
        HintItem = newHint;
        AddChild(newHint);
        UpdateSlot();
    }

    // 清除提示物品
    public void ClearItemHint()
    {
        HintItem?.QueueFree();
        HintItem = null;
        UpdateSlot();
    }

    // 移除物品
    public void RemoveItem()
    {
        if (Item != null)
        {
            RemoveChild(Item);
            Item.QueueFree();
            Item = null;
        }
        UnalignHotKey();
        UpdateSlot();
    }

    // 选择物品
    public InventoryItem SelectItem()
    {   
        if (Item == null) return null;

        var inventory = GetParent();
        if (inventory == null) return null;

        var tmpItem = Item;
        tmpItem.Reparent(inventory);
        tmpItem.ZIndex = 128;
        Item = null;
        return tmpItem;
    }

    // 放置物品
    public InventoryItem DeselectItem(InventoryItem newItem)
    {
        if (!IsRespectingHint(newItem)) return newItem;

        var inventory = GetParent();
        if (inventory == null) return null;

        if (IsEmpty())
        {
            newItem.Reparent(this);
            Item = newItem;
            newItem.ZIndex = 64;
            newItem.Call("SetSpritePosition", GlobalPosition + new Vector2(24, 24));
            return null;
        }

        if (HasSameItem(newItem))
        {
            Item.Amount += newItem.Amount;
            newItem.QueueFree();
            return null;
        }

        newItem.Reparent(this);
        Item.Reparent(inventory);
        var oldItem = Item;
        Item = newItem;
        newItem.ZIndex = 64;
        oldItem.ZIndex = 128;
        return oldItem;
    }

    // 分裂物品
    public InventoryItem SplitItem()
    {
        if (IsEmpty() || Item == null) return null;

        var inventory = GetParent();
        if (inventory == null) return null;

        if (Item.Amount > 1)
        {
            var newItem = InventoryItemScene.Instantiate<InventoryItem>();
            newItem.Initialize(Item.ItemName, Item.Icon, Item.IsStackable, Item.Amount / 2, Item.ItemType);
            Item.Amount -= newItem.Amount;
            inventory.AddChild(newItem);
            newItem.ZIndex = 128;
            return newItem;
        }

        return Item.Amount == 1 ? SelectItem() : null;
    }

    // 辅助方法
    public bool IsEmpty() => Item == null;
    public bool HasSameItem(InventoryItem item) => item?.ItemName == Item?.ItemName;

    // 更新槽位显示
    public void UpdateSlot()
    {
        if (Item != null)
        {
            if (!GetChildren().Contains(Item))
                AddChild(Item);
                
            if (Item.Amount < 1)
                Item.Fade();
        }

        if (HintItem != null)
        {
            if (!GetChildren().Contains(HintItem))
            {
                AddChild(HintItem);
            }
            HintItem.Fade();
        }
    }

    // 取消绑定快捷键
    public void UnalignHotKey()
    {
        HotBar hotBar = (HotBar)GetTree().GetFirstNodeInGroup("hotbar");
        if (hotBar != null && AlignedHotKey >= 0)
        {
            hotBar.UpdateHotBar(AlignedHotKey, null); // 仅需调用一次
        }

        HasHotKey = false;
        AlignedHotKey = -1;
        _hotKey.Hide();
    }
}