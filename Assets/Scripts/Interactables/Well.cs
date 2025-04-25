using Godot;
using System;

public partial class Well : Area3D
{
    private bool _isColliding = false;
    private Label3D _label3D;
    private Player _player;
    private Item Water;

    public override void _Ready()
    {
        _label3D = GetNode<Label3D>("InteractPrompt");

        Callable bodyEnteredCallable = new(this, MethodName.OnBodyEntered);
    	Connect("body_entered", bodyEnteredCallable, 0); 
		Callable bodyExitedCallable = new(this, MethodName.OnBodyExited);
    	Connect("body_exited", bodyExitedCallable, 0); 

        _player = (Player)GetTree().GetFirstNodeInGroup("player");
        Water = (Item)GD.Load<PackedScene>("res://Assets/Scenes/Items/item_water_bucket.tscn").Instantiate();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("action_use") && _isColliding)
        {
            if (_player._currentSelectedItem != null && _player._currentSelectedItem.Name == "Bucket")
                FillWater(_player._currentSelectedItem);
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

    public void FillWater(Item item)
    {
        Inventory inventory = (Inventory)GetTree().GetFirstNodeInGroup("inventory");
        inventory.RetrieveItem(item.ItemName);
        inventory.AddItem(Water, 1);
    }
}
