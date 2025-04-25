using Godot;
using System;

public partial class InventoryItem : Item
{   
    private int amount;
    [Export]
    public int Amount
    {
        get => amount;
        set
        {
            amount = value;
            // 通知 HotBar 更新显示
            if (GetParent() is InventorySlot slot && slot.HasHotKey)
            {
                HotBar hotBar = (HotBar)GetTree().GetFirstNodeInGroup("hotbar");
                hotBar?.UpdateHotBar(slot.AlignedHotKey, this);
            }
        }
    }
    
    [Export] public Sprite2D Sprite { get; set; }
    [Export] public Label Label { get; set; }

    // 基础属性
    // public string ItemName { get; private set; }
    // public Texture2D Icon { get; private set; }
    // public bool IsStackable { get; private set; }

    public void Initialize(string name, Texture2D icon, bool isStackable, int amount, ItemTypes type)
    {
        ItemName = name;
        Name = name; // Node的Name属性
        Icon = icon;
        IsStackable = isStackable;
        Amount = amount;
        ItemType = type;

        // 确保Sprite和Label已经赋值
        if (Sprite == null)
        {
            GD.PrintErr("Sprite is not assigned.");
            return;
        }
        if (Label == null)
        {
            GD.PrintErr("Label is not assigned.");
            return;
        }
    }

    public override void _Process(double delta)
    {
        Sprite.Texture = Icon;
        SetSpriteSize(Sprite, new Vector2(42, 42));
        
        if (IsStackable)
        {
            Label.Text = Amount.ToString();
            Label.Visible = true;
        }
        else
        {
            Label.Visible = false;
        }
    }

    private void SetSpriteSize(Sprite2D sprite, Vector2 targetSize)
    {
        if (sprite.Texture == null) return;
        
        Vector2 textureSize = sprite.Texture.GetSize();
        Vector2 scaleFactor = new Vector2(
            targetSize.X / textureSize.X,
            targetSize.Y / textureSize.Y
        );
        sprite.Scale = scaleFactor;
    }

    private void SetSpritePosition(Vector2 targetPos)
    {
        Sprite.GlobalPosition = targetPos;
        Label.GlobalPosition = new Vector2(targetPos.X + 8, targetPos.Y + 8);
    }

    public void Fade()
    {
        Color fadedColor = Colors.White with { A = 0.4f };
        Sprite.Modulate = fadedColor;
        Label.Modulate = fadedColor;
    }

    public static explicit operator Control(InventoryItem v)
    {
        throw new NotImplementedException();
    }

    public static explicit operator InventoryItem(Control v)
    {
        throw new NotImplementedException();
    }
}