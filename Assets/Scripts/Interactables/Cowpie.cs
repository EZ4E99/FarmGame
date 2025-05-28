using Godot;
using System;

public partial class Cowpie : Area3D
{
    private bool _isColliding = false;
    private Label3D _label3D;
    private Player _player;

    public override void _Ready()
    {
        _label3D = GetNode<Label3D>("InteractPrompt");

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
            if (_player.HasShovel && _player.IsUsingWheelbarrow && _player.GetWheelbarrowCurrentCowpie() < 5 && _player.GetWheelbarrowCurrentWood() == 0)
            {
                _player.AddWheelbarrowCowpie();

                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                QueueFree(); // 销毁物品（在本帧结束时）
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
}
