using Godot;
using System;

public partial class WoodsStack : Area3D
{
    private bool _isColliding = false;
    private Label3D _label3D;
    private Player _player;

    private int WoodCount = 21;

    public override void _Ready()
    {
        _label3D = GetNode<Label3D>("InteractPrompt");

        Callable bodyEnteredCallable = new(this, MethodName.OnBodyEntered);
        Connect("body_entered", bodyEnteredCallable, 0);
        Callable bodyExitedCallable = new(this, MethodName.OnBodyExited);
        Connect("body_exited", bodyExitedCallable, 0);

        _player = (Player)GetTree().GetFirstNodeInGroup("player");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("action_use") && _isColliding && !_player.IsUsingWheelbarrow)
        {
            RemoveWood();
            _player.AddHoldingWood();
        }

        if (Input.IsActionJustPressed("action_use") && _isColliding && _player.IsUsingWheelbarrow)
        {
            RemoveWood();
            _player.AddWheelbarrowWood();
        }

        if (Input.IsActionJustPressed("action_use_alt") && _isColliding && !_player.IsUsingWheelbarrow)
        {
            AddWood();
            _player.RemoveHoldingWood();
        }

        if (Input.IsActionJustPressed("action_use_alt") && _isColliding && _player.IsUsingWheelbarrow)
        {
            AddWood();
            _player.RemoveWheelbarrowWood();
        }

        base._PhysicsProcess(delta);
    }

    public void AddWood()
    {
        GD.Print(_player.CurrentHoldWood);

        if (_player.IsUsingWheelbarrow)
        {
            if (!_player.CheckWheelbarrowContent("WheelbarrowCurrentWood") && !_player.CheckWheelbarrowIsEmpty())
                return;
        }
        else
        {
            if (_player.CurrentHoldWood == 0)
                return;
        }

        WoodCount++;
        Node3D Wood = GetNode<Node3D>("Meshes/MeshInstance3D" + WoodCount);
        Wood.Show();
    }

    public void RemoveWood()
    {
        if (_player.IsUsingWheelbarrow)
        {
            if (_player.CheckWheelbarrowContent("WheelbarrowCurrentWood") || _player.CheckWheelbarrowIsEmpty())
                return;
        }
        else
        {
            if (_player.CurrentHoldWood >= 2)
                return;
        }

        Node3D Wood = GetNode<Node3D>("Meshes/MeshInstance3D" + WoodCount);
        Wood.Hide();
        WoodCount--;

        GD.Print(WoodCount);
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
