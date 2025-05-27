using Godot;
using System;

public partial class ItemPickup : Area3D
{
    [Export] private Item _item;
    [Export] private int _quantity;
    private Player _player;
    private Label3D _label3D;
    private bool _isColliding = false;


    public override void _Ready()
    {
        _label3D = GetNode<Label3D>("InteractPrompt");

        Callable bodyEnteredCallable = new(this, MethodName.OnBodyEntered);
    	Connect("body_entered", bodyEnteredCallable, 0); 
		Callable bodyExitedCallable = new(this, MethodName.OnBodyExited);
    	Connect("body_exited", bodyExitedCallable, 0); 

        _player = (Player)GetTree().GetFirstNodeInGroup("player");
    }

    public void OnBodyEntered(Node3D body)
    {
        if (!body.IsInGroup("player")) return;
        else
        {
            _label3D.Show();
            _isColliding = true;
        }
    }

    public void OnBodyExited(Node3D body)
    {
        if (!body.IsInGroup("player")) return;
        else
        {
            _label3D.Hide();
            _isColliding = false;
        }
    }

    public async override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("action_use") && _isColliding && !(_player.CurrentPlayerState == Player.PlayerState.Idle_Holding || _player.CurrentPlayerState == Player.PlayerState.Run_Holding))
        {
            Inventory inventory = (Inventory)GetTree().GetFirstNodeInGroup("inventory");
            inventory.AddItem(_item, _quantity);

            if (_item.ItemName == "Shovel") _player.UpdateShovel();

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            QueueFree(); // 销毁物品（在本帧结束时）
        }
        else
            return;
    }
}
