using Godot;
using System;

public partial class Wheelbarrow : Area3D
{
    private bool _isColliding = false;
    private Label3D _label3D;
    private Player _player;
    private MeshInstance3D meshInstance3D;
    private StaticBody3D staticBody3D;
    private Node3D _shovel;
    private Label3D _currentWheelbarrowCount;
    private AnimationPlayer _itemAnimationPlayer;

    public int WheelbarrowCurrentCowpie = 0;
    public int WheelbarrowCurrentWood = 0;

    public override void _Ready()
    {
        _label3D = GetNode<Label3D>("InteractPrompt");
        meshInstance3D = GetNode<MeshInstance3D>("Wheelbarrow");
        staticBody3D = GetNode<StaticBody3D>("StaticBody3D");
        _shovel = GetNode<Node3D>("Wheelbarrow/shovel");
        _currentWheelbarrowCount = GetNode<Label3D>("Wheelbarrow/Label3D");
        _itemAnimationPlayer = GetNode<AnimationPlayer>("ItemAnimationPlayer");

        Callable bodyEnteredCallable = new(this, MethodName.OnBodyEntered);
        Connect("body_entered", bodyEnteredCallable, 0);
        Callable bodyExitedCallable = new(this, MethodName.OnBodyExited);
        Connect("body_exited", bodyExitedCallable, 0);

        _player = (Player)GetTree().GetFirstNodeInGroup("player");
    }

    public async override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("action_use") && _isColliding && !(_player.CurrentPlayerState == Player.PlayerState.Idle_Holding || _player.CurrentPlayerState == Player.PlayerState.Run_Holding))
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            ChangeWheelbarrowVisibility(true);
        }

        if (Input.IsActionJustPressed("action_use_alt") && _isColliding && !(_player.CurrentPlayerState == Player.PlayerState.Idle || _player.CurrentPlayerState == Player.PlayerState.Run))
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
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
        _player.UpdateWheelbarrow(this, visible);

        if (visible)
        {
            _label3D.Text = "[R] 放置 小推车";
            UpdateShovel();
            _currentWheelbarrowCount.Show();
            staticBody3D.SetCollisionLayerValue(1, false);
        }
        else
        {
            _label3D.Text = "[E] 使用 小推车";
            _shovel.Hide();
            _currentWheelbarrowCount.Hide();
            staticBody3D.SetCollisionLayerValue(1, true);
        }
    }

    public void AddWheelbarrowCowpie()
    {
        if (!_player.IsUsingWheelbarrow || WheelbarrowCurrentCowpie >= 5 || WheelbarrowCurrentWood > 0) return;

        WheelbarrowCurrentCowpie++;
        _currentWheelbarrowCount.Text = WheelbarrowCurrentCowpie + "/5";
        _itemAnimationPlayer.Play("collect_cowpie_" + WheelbarrowCurrentCowpie);
    }

    public void AddWheelbarrowWood()
    {
        if (!_player.IsUsingWheelbarrow || WheelbarrowCurrentWood >= 5 || WheelbarrowCurrentCowpie > 0) return;

        WheelbarrowCurrentWood++;
        _currentWheelbarrowCount.Text = WheelbarrowCurrentWood + "/5";
        _itemAnimationPlayer.Play("collect_wood_" + WheelbarrowCurrentWood);
    }

    public void UpdateShovel()
    {
        _shovel.Visible = _player.HasShovel;
    }

    public void FillCompostBox()
    {
        WheelbarrowCurrentCowpie = 0;
        _currentWheelbarrowCount.Text = WheelbarrowCurrentCowpie + "/5";
        _itemAnimationPlayer.Play("fill_compost_box");
    }

    public void RemoveWheelbarrowWood()
    {
        if (!_player.IsUsingWheelbarrow || WheelbarrowCurrentWood > 5 || WheelbarrowCurrentWood <= 0) return;

        _itemAnimationPlayer.Play("collect_wood_" + WheelbarrowCurrentWood, -1, -1, true);
        WheelbarrowCurrentWood--;
        _currentWheelbarrowCount.Text = WheelbarrowCurrentWood + "/5";
    }
}
