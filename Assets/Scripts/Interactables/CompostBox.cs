using Godot;
using System;

public partial class CompostBox : Area3D
{
    private bool _isColliding = false;
    private Label3D _label3D;
    private Label3D _statusPrompt;
    private Player _player;
    private AnimationPlayer _animationPlayer;
    private Item Fertilizer;
    private bool IsComposting = false;
    private bool CompostFinished = false;
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
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("action_use") && _isColliding)
        {
            if (!IsComposting && _player.HasShovel && _player.IsUsingWheelbarrow && _player.WheelbarrowCurrentCowpie == 5)
            {
                IsComposting = true;
                _animationPlayer.Play("fill");
                _label3D.Text = "堆肥中";
                UpdateStatusPrompt(true, CompostingDaysNeeded - CompostingDaysCount);
                _player.FillCompostBox();
            }
            else if (IsComposting && CompostFinished)
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
            if (IsComposting) UpdateStatusPrompt(true, CompostingDaysNeeded - CompostingDaysCount);
        }
    }

    public void OnBodyExited(Node3D body)
    {
        if (!body.IsInGroup("player")) return;
        else
        {
            _label3D.Hide();
            _isColliding = false;
            if (IsComposting) UpdateStatusPrompt(false, CompostingDaysNeeded - CompostingDaysCount);
        }
    }

    public void UpdateStatusPrompt(bool visible, int days)
    {
        _statusPrompt.Visible = visible;
        _statusPrompt.Text = $"堆肥完成还需{days}天";
    }

    public void Compost()
    {
        if (!IsComposting) return;

        CompostingDaysCount++;
        if (_isColliding) UpdateStatusPrompt(true, CompostingDaysNeeded - CompostingDaysCount);

        if (CompostingDaysCount >= CompostingDaysNeeded)
        {
            CompostFinished = true;
            _label3D.Text = "[E] 收集肥料";
            UpdateStatusPrompt(false, 0);
        }
    }

    public void CollectFertilizer()
    {
        if (!CompostFinished) return;

        Inventory inventory = (Inventory)GetTree().GetFirstNodeInGroup("inventory");
        inventory.AddItem(Fertilizer, 10);

        CompostFinished = false;
        IsComposting = false;
        CompostingDaysCount = 0;

        UpdateStatusPrompt(false, 0);
        _label3D.Text = "[E] 堆肥\n需要 铲子、盛满牛粪的小推车";
    }
}
