using Godot;
using System;

public partial class Furnace : Area3D
{
        private bool _isColliding = false;
    private Label3D _label3D;
    private Player _player;
    private AnimationPlayer _animationPlayer;
    private Inventory inventory;
    private Node3D _woods;
    private Node3D _bread;
    private Item Bread;

    public override void _Ready()
    {
        _woods = GetNode<Node3D>("Meshes/Woods");
        _bread = GetNode<Node3D>("Meshes/Dough");
        _label3D = GetNode<Label3D>("InteractPrompt");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        Callable bodyEnteredCallable = new(this, MethodName.OnBodyEntered);
    	Connect("body_entered", bodyEnteredCallable, 0); 
		Callable bodyExitedCallable = new(this, MethodName.OnBodyExited);
    	Connect("body_exited", bodyExitedCallable, 0); 

        _player = (Player)GetTree().GetFirstNodeInGroup("player");
        inventory = (Inventory)GetTree().GetFirstNodeInGroup("inventory");
        Bread = (Item)GD.Load<PackedScene>("res://Assets/Scenes/Items/item_bread.tscn").Instantiate();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("action_use") && _isColliding)
        {
            if (!_player.IsUsingWheelbarrow)
            {
                if (_player.CurrentHoldWood > 0 && !_woods.Visible)
                {
                    _woods.Show();
                    _player.RemoveHoldingWood();
                }
            }
            else
            {
                if (_player.GetWheelbarrowCurrentWood() > 0 && !_woods.Visible)
                {
                    _woods.Show();
                    _player.RemoveWheelbarrowWood();
                }
            }

            if (_player._currentSelectedItem != null && _player._currentSelectedItem.Name == "Dough" && !_bread.Visible)
            {
                _bread.Show();
                inventory.RetrieveItem(_player._currentSelectedItem.Name);
            }

            if (_woods.Visible && _bread.Visible)
            {
                MakeBread();
            }
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

    public void MakeBread()
    {
        _animationPlayer.Play("bake");
        inventory.AddItem(Bread, 1);
    }
}
