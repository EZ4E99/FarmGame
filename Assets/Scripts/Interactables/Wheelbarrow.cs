using Godot;
using System;

public partial class Wheelbarrow : Area3D
{
    private bool _isColliding = false;
    private bool HasWheelbarrow = true;
    private Label3D _label3D;
    private Player _player;
    private MeshInstance3D meshInstance3D;
    private StaticBody3D staticBody3D;

    public override void _Ready()
    {
        _label3D = GetNode<Label3D>("InteractPrompt");
        meshInstance3D = GetNode<MeshInstance3D>("Wheelbarrow");
        staticBody3D = GetNode<StaticBody3D>("StaticBody3D");

        Callable bodyEnteredCallable = new(this, MethodName.OnBodyEntered);
    	Connect("body_entered", bodyEnteredCallable, 0); 
		Callable bodyExitedCallable = new(this, MethodName.OnBodyExited);
    	Connect("body_exited", bodyExitedCallable, 0); 

        _player = (Player)GetTree().GetFirstNodeInGroup("player");
    }

    public async override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("action_use") && _isColliding)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            if (HasWheelbarrow)
                ChangeWheelbarrowVisibility(true);
            else
                ChangeWheelbarrowVisibility(false);
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

    public void ChangeWheelbarrowVisibility(bool visible)
    {
        Player player = (Player)GetTree().GetFirstNodeInGroup("player");
        player.UpdateWheelbarrow(visible);

        if (visible)
        {
            meshInstance3D.Hide();
            staticBody3D.SetCollisionLayerValue(1, false);
        }
        else
        {
            meshInstance3D.Show();
            staticBody3D.SetCollisionLayerValue(1, true);
        }
        
        HasWheelbarrow = !visible;
    }
}
