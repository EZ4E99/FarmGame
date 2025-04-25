using Godot;
using System;

public partial class HotBarSlot : CenterContainer
{
    public InventoryItem item;
    public Sprite2D sprite2D;
    public Label label;
    public TextureButton textureButton;
    public Player player;

    public override void _Ready()
    {
        sprite2D = GetNode<Sprite2D>("TextureButton/Sprite2D");
        label = GetNode<Label>("TextureButton/Label");
        textureButton = GetNode<TextureButton>("TextureButton");
        player = (Player)GetTree().GetFirstNodeInGroup("player");
    }

    public void UpdateSlot(InventoryItem item)
    {
        if (item == null)
        {
            sprite2D.Texture = null;
            // 隐藏图标和文本
            sprite2D.Hide();
            label.Hide();
            label.Text = "";
            this.item = null;

            player._currentSelectedItem = null;
        }
        else
        {
            // 确保其他情况下图标和文本可见
            sprite2D.Show();
            label.Show();

            if (item.Amount > 1)
                label.Text = item.Amount.ToString();
            else if (item.Amount == 1)
                label.Text = "1";

            sprite2D.Texture = item.Sprite.Texture;
            this.item = item;
        }

        // 更新位置
        sprite2D.GlobalPosition = new Vector2(GlobalPosition.X + 24, GlobalPosition.Y + 24);
        label.GlobalPosition = new Vector2(GlobalPosition.X + 8, GlobalPosition.Y + 32);
    }

    public InventoryItem SelectSlot(bool boolean)
    {
        if (boolean)
        {
            textureButton.Disabled = false;
            return item;
        }
        else
        {
            textureButton.Disabled = true;
            return null;
        }
    }
}
