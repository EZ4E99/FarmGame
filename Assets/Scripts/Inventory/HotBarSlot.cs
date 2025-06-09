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

    public async void UpdateSlot(InventoryItem itemin)
    {
        if (itemin == null)
        {
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

            label.Show();

            if (itemin.Amount > 1)
                label.Text = itemin.Amount.ToString();
            else if (itemin.Amount == 1)
                label.Text = "1";

            this.item = itemin;

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            sprite2D.Texture = itemin.Sprite.Texture;
            sprite2D.Show();
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
