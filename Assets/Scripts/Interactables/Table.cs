using Godot;
using System;

public partial class Table : Area3D
{
    private bool _isColliding = false;
    private Label3D _label3D;
    private Player _player;
    private Sprite3D _water;
    private Sprite3D _yeast;
    private Sprite3D _salt;
    private Sprite3D _flour;
    private AnimationPlayer _animationPlayer;
    private Item Dough;
    private Inventory inventory;

    public override void _Ready()
    {
        _water = GetNode<Sprite3D>("Items/Water");
        _yeast = GetNode<Sprite3D>("Items/Yeast");
        _salt = GetNode<Sprite3D>("Items/Salt");
        _flour = GetNode<Sprite3D>("Items/Flour");
        _label3D = GetNode<Label3D>("InteractPrompt");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        Callable bodyEnteredCallable = new(this, MethodName.OnBodyEntered);
    	Connect("body_entered", bodyEnteredCallable, 0); 
		Callable bodyExitedCallable = new(this, MethodName.OnBodyExited);
    	Connect("body_exited", bodyExitedCallable, 0); 

        _player = (Player)GetTree().GetFirstNodeInGroup("player");
        inventory = (Inventory)GetTree().GetFirstNodeInGroup("inventory");
        Dough = (Item)GD.Load<PackedScene>("res://Assets/Scenes/Items/item_dough.tscn").Instantiate();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("action_use") && _isColliding)
        {
            if (_player._currentSelectedItem != null && _player._currentSelectedItem.Name == "WaterBucket" && !_water.Visible)
            {
                _water.Show();
                inventory.RetrieveItem(_player._currentSelectedItem.Name);

                Item Bucket = (Item)GD.Load<PackedScene>("res://Assets/Scenes/Items/item_bucket.tscn").Instantiate();
                inventory.AddItem(Bucket, 1);
            }
            if (_player._currentSelectedItem != null && _player._currentSelectedItem.Name == "Flour" && !_flour.Visible)
            {
                _flour.Show();
                inventory.RetrieveItem(_player._currentSelectedItem.Name);
            }
            if (_player._currentSelectedItem != null && _player._currentSelectedItem.Name == "Yeast" && !_yeast.Visible)
            {
                _yeast.Show();
                inventory.RetrieveItem(_player._currentSelectedItem.Name);
            }
            if (_player._currentSelectedItem != null && _player._currentSelectedItem.Name == "Salt" && !_salt.Visible)
            {
                _salt.Show();
                inventory.RetrieveItem(_player._currentSelectedItem.Name);
            }

            if (_salt.Visible && _yeast.Visible && _water.Visible && _flour.Visible)
            {
                MakeDough();
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

    public void MakeDough()
    {
        _water.Hide();
        _flour.Hide();
        _yeast.Hide();
        _salt.Hide();

        _animationPlayer.Play("work");
        inventory.AddItem(Dough, 1);
    }
}
