using Godot;
using System;

public partial class Mud : Node3D
{
    private Area3D _area3D;
    private int _daysInCurrentStage;
    private bool _isColliding = false;
    private bool _hasCrop = false;
    private Crop _currentCrop;
    private GrowthStage _currentStage = GrowthStage.种子;
    private Label3D _label3D;
    private Player _player;

    public override void _Ready()
    {
        _area3D = GetNode<Area3D>("Area3D");
        _label3D = GetNode<Label3D>("InteractPrompt");

        Callable bodyEnteredCallable = new(this, MethodName.OnBodyEntered);
    	_area3D.Connect("body_entered", bodyEnteredCallable, 0); 
		Callable bodyExitedCallable = new(this, MethodName.OnBodyExited);
    	_area3D.Connect("body_exited", bodyExitedCallable, 0); 

        _player = (Player)GetTree().GetFirstNodeInGroup("player");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("action_use") && _isColliding && !_hasCrop)
        {
            PlantCrop(_player._currentSelectedItem);
        }
        else if (Input.IsActionJustPressed("action_use") && _isColliding && _hasCrop && _currentStage == GrowthStage.成熟)
        {
            HarvestCrop();
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
            _currentCrop?.UpdateStatusPrompt(true, _currentStage.ToString(), _currentCrop.GetDaysForStage(_currentStage) - _daysInCurrentStage);
        }
    }

    public void OnBodyExited(Node3D body)
    {
        if (!body.IsInGroup("player")) return;
        else
        {
            _label3D.Hide();
            _isColliding = false;
            _currentCrop?.UpdateStatusPrompt(false, null, 0);
        }
    }

    // 种植作物
    public void PlantCrop(InventoryItem item)
    {
        if (item == null || item.ItemType != ItemTypes.Seed) return;

        GD.Print(item.ItemName,item.ItemType);
        // 处理背包物品
        Inventory inventory = (Inventory)GetTree().GetFirstNodeInGroup("inventory");
        inventory.RetrieveItem(item.ItemName);

        // 处理土地
        _hasCrop = true;

        PackedScene newCrop = GD.Load<PackedScene>("res://Assets/Scenes/Crops/" + item.SeedCropName + ".tscn");
        Crop newCropIns = (Crop)newCrop.Instantiate();
        _currentCrop = newCropIns;

        _player.UpdateHotBar();
        AddChild(newCropIns);
    }

    // 收获
    public void HarvestCrop()
    {
        if (_player._currentSelectedItem?.ItemName != "Sickle") return;
        
        Inventory inventory = (Inventory)GetTree().GetFirstNodeInGroup("inventory");
        inventory.AddItem(_currentCrop.CropItem, 3);

        _player.UpdateHotBar();

        _currentCrop.QueueFree();
        _currentCrop = null;
        _hasCrop = false;
        _currentStage = GrowthStage.种子;
    }

    // 生长
    public void Grow()
    {
        if (!_hasCrop && _currentCrop == null) return;

        _daysInCurrentStage++;
        // 检查是否进入下一阶段
        if (_daysInCurrentStage >= _currentCrop.GetDaysForStage(_currentStage))
        {
            AdvanceToNextStage();
            _currentCrop.UpdateAnimatedSprites3D(_currentStage);
            _daysInCurrentStage = 0;
        }
    }

    private void AdvanceToNextStage()
    {
        if (_currentStage == GrowthStage.成熟) return;
        
        _currentStage++;
        GD.Print($"Crop advanced to {_currentStage} stage!");
        
        if (_currentStage == GrowthStage.成熟)
        {
            GD.Print($"{_currentCrop.CropName} is ready to harvest!");
        }
    }
}
