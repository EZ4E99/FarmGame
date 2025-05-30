using Godot;
using System;

public partial class CompostBox : Area3D
{
    public enum CompostBoxState
    {
        Empty,        // 堆肥箱空
        FirstCowpie,  // 第一层牛粪
        FirstLeaf,    // 第一层树叶
        SecondCowpie, // 第二层牛粪
        Composting,   // 第二层树叶（正在堆肥）
        CompostFinished,   // 堆肥完成
    }

    private CompostBoxState compostBoxState;
    private bool _isColliding = false;
    private Label3D _label3D;
    private Label3D _statusPrompt;
    private Player _player;
    private AnimationPlayer _animationPlayer;
    private Item Fertilizer;
    private int CompostingDaysCount = 0;
    private int CompostingDaysNeeded = 7;

    public override void _Ready()
    {
        _label3D = GetNode<Label3D>("InteractPrompt");
        _statusPrompt = GetNode<Label3D>("StatusPrompt");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        Callable bodyEnteredCallable = new(this, MethodName.OnBodyEntered);
        Connect("body_entered", bodyEnteredCallable, 0);
        Callable bodyExitedCallable = new(this, MethodName.OnBodyExited);
        Connect("body_exited", bodyExitedCallable, 0);

        _player = (Player)GetTree().GetFirstNodeInGroup("player");
        Fertilizer = (Item)GD.Load<PackedScene>("res://Assets/Scenes/Items/item_fertilizer.tscn").Instantiate();

        compostBoxState = CompostBoxState.Empty;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("action_use") && _isColliding)
        {
            if (compostBoxState == CompostBoxState.Empty && _player.HasShovel && _player.IsUsingWheelbarrow && _player.CheckWheelbarrowContent("WheelbarrowCurrentCowpie"))
            {
                compostBoxState = CompostBoxState.FirstCowpie;
                _animationPlayer.Play("fill");
                _label3D.Text = "[E] 堆肥（1/4） \n需要 铲子、盛满树叶的小推车";
                _player.FillCompostBox();
            }

            if (compostBoxState == CompostBoxState.FirstCowpie && _player.HasShovel && _player.IsUsingWheelbarrow && _player.CheckWheelbarrowContent("WheelbarrowCurrentLeaf"))
            {
                compostBoxState = CompostBoxState.FirstLeaf;
                _animationPlayer.Play("fill");
                _label3D.Text = "[E] 堆肥（2/4） \n需要 铲子、盛满牛粪的小推车";
                _player.RemoveWheelbarrowLeaf();
            }

            if (compostBoxState == CompostBoxState.FirstLeaf && _player.HasShovel && _player.IsUsingWheelbarrow && _player.CheckWheelbarrowContent("WheelbarrowCurrentCowpie"))
            {
                compostBoxState = CompostBoxState.SecondCowpie;
                _animationPlayer.Play("fill");
                _label3D.Text = "[E] 堆肥（3/4） \n需要 铲子、盛满树叶的小推车";
                _player.FillCompostBox();
            }

            if (compostBoxState == CompostBoxState.SecondCowpie && _player.HasShovel && _player.IsUsingWheelbarrow && _player.CheckWheelbarrowContent("WheelbarrowCurrentLeaf"))
            {
                compostBoxState = CompostBoxState.Composting;
                _animationPlayer.Play("fill");
                _label3D.Text = "堆肥中......";
                UpdateStatusPrompt(true, CompostingDaysNeeded - CompostingDaysCount);
                _player.RemoveWheelbarrowLeaf();
            }

            if (compostBoxState == CompostBoxState.CompostFinished)
            {
                CollectFertilizer();
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
            if (compostBoxState == CompostBoxState.Composting)
                UpdateStatusPrompt(true, CompostingDaysNeeded - CompostingDaysCount);
        }
    }

    public void OnBodyExited(Node3D body)
    {
        if (!body.IsInGroup("player")) return;
        else
        {
            _label3D.Hide();
            _isColliding = false;
            if (compostBoxState == CompostBoxState.Composting)
                UpdateStatusPrompt(false, CompostingDaysNeeded - CompostingDaysCount);
        }
    }

    public void UpdateStatusPrompt(bool visible, int days)
    {
        _statusPrompt.Visible = visible;
        _statusPrompt.Text = $"堆肥完成还需{days}天";
    }

    public void Compost()
    {
        if (compostBoxState != CompostBoxState.Composting) return;

        CompostingDaysCount++;
        if (_isColliding) UpdateStatusPrompt(true, CompostingDaysNeeded - CompostingDaysCount);

        if (CompostingDaysCount >= CompostingDaysNeeded)
        {
            compostBoxState = CompostBoxState.CompostFinished;
            _label3D.Text = "[E] 收集肥料";
            UpdateStatusPrompt(false, 0);
        }
    }

    public void CollectFertilizer()
    {
        if (compostBoxState != CompostBoxState.CompostFinished) return;

        Inventory inventory = (Inventory)GetTree().GetFirstNodeInGroup("inventory");
        inventory.AddItem(Fertilizer, 10);

        compostBoxState = CompostBoxState.Empty;
        CompostingDaysCount = 0;

        UpdateStatusPrompt(false, 0);
        _label3D.Text = "[E] 堆肥\n需要 铲子、盛满牛粪的小推车";
    }
}
