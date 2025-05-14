using Godot;
using System;

public partial class WoodsStack : Area3D
{
    private bool _isColliding = false;
    private Label3D _label3D;
    private Player _player;
    private Item Wood;

    public override void _Ready()
    {
        _label3D = GetNode<Label3D>("InteractPrompt");

        Callable bodyEnteredCallable = new(this, MethodName.OnBodyEntered);
    	Connect("body_entered", bodyEnteredCallable, 0); 
		Callable bodyExitedCallable = new(this, MethodName.OnBodyExited);
    	Connect("body_exited", bodyExitedCallable, 0); 

        _player = (Player)GetTree().GetFirstNodeInGroup("player");
        Wood = (Item)GD.Load<PackedScene>("res://Assets/Scenes/Items/item_wood.tscn").Instantiate();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("action_use") && _isColliding && !_player.IsUsingWheelbarrow)
        {
            Inventory inventory = (Inventory)GetTree().GetFirstNodeInGroup("inventory");
            inventory.AddItem(Wood, 1);
        }
        base._PhysicsProcess(delta);
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
}
