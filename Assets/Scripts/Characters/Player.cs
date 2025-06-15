using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class Player : CharacterBody3D
{
    public enum PlayerState
    {
        Idle,     // 静止
        Run,      // 奔跑
        Idle_Holding,  // 手持静止
        Run_Holding,   // 手持奔跑
    }

    // 移动参数
    [Export] public float Speed = 5.0f;
    [Export] public float JumpForce = 4.5f;
    [Export] public float MouseSensitivity = 0.002f;
    [Export] public float CameraMinVertical = -30.0f;
    [Export] public float CameraMaxVertical = 60.0f;
    [Export] public float RotationSpeed = 10.0f; // 旋转平滑速度
    public PlayerState CurrentPlayerState = PlayerState.Idle;
    public bool IsUsingWheelbarrow = false;
    public bool HasShovel = false;
    public int CurrentHoldWood = 0;

    // 节点引用
    private Node3D _playerModel;
    private AnimationPlayer _playerModelAnimationPlayer;
    private Sprite3D _currentItem; //当前角色手持的物品（缺乏模型暂时用Sprite3D作为占位符）
    private Node3D _cameraPivot;
    private Camera3D _camera;
    private RayCast3D _crosshairRaycast3D;
    private Node3D Raycast3DCollider = null;
    private CanvasLayer _inventoryUI;
    private Phone _phoneUI;
    private TextureRect _crosshair;
    private Button _inventoryCloseButton;
    private Button _phoneCloseButton;
    private Panel _menu;
    private Button _menuCloseButton;
    private Button _returnButton;
    private HotBar _hotBar;
    private Label _daysCount;
    private VBoxContainer _missionSlots;

    private float _gravity;
    private Vector2 _cameraRotation = Vector2.Zero;
    public int _currentDay = 1;
    public int _currentMoney = 1000;
    public InventoryItem _currentSelectedItem;

    public Node3D raycastCollider;


    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        _playerModel = GetNode<Node3D>("Character");
        _playerModelAnimationPlayer = GetNode<AnimationPlayer>("Character/AnimationPlayer");
        _currentItem = GetNode<Sprite3D>("Character/Items/CurrentItem/Sprite3D");
        _cameraPivot = GetNode<Node3D>("CameraPivot");
        _camera = GetNode<Camera3D>("CameraPivot/Camera3D");
        _crosshairRaycast3D = GetNode<RayCast3D>("CameraPivot/Camera3D/RayCast3D");
        _inventoryUI = GetNode<CanvasLayer>("HUD/InventoryUI");
        _phoneUI = GetNode<Phone>("HUD/PhoneUI/Phone");
        _crosshair = GetNode<TextureRect>("HUD/Crosshair");
        _inventoryCloseButton = GetNode<Button>("HUD/InventoryUI/Panel/CloseButton");
        _phoneCloseButton = GetNode<Button>("HUD/PhoneUI/Phone/CloseButton");
        _menu = GetNode<Panel>("HUD/Menu");
        _menuCloseButton = GetNode<Button>("HUD/Menu/VBoxContainer/HBoxContainer/Button2");
        _returnButton = GetNode<Button>("HUD/Menu/VBoxContainer/HBoxContainer/Button");
        _hotBar = GetNode<HotBar>("HUD/HotBar");
        _daysCount = GetNode<Label>("HUD/DaysCount");
        _missionSlots = GetNode<VBoxContainer>("HUD/MissionContainer");

        Callable OnInventoryCloseButtonPressedCallable = new(this, MethodName.ToggleInventory);
        _inventoryCloseButton.Connect("pressed", OnInventoryCloseButtonPressedCallable, 0);
        Callable OnPhoneCloseButtonPressedCallable = new(this, MethodName.TogglePhone);
        _phoneCloseButton.Connect("pressed", OnPhoneCloseButtonPressedCallable, 0);
        Callable OnMenuCloseButtonPressedCallable = new(this, MethodName.OnMenuCloseButtonPressed);
        _menuCloseButton.Connect("pressed", OnMenuCloseButtonPressedCallable, 0);
        Callable OnReturnButtonPressedCallable = new(this, MethodName.OnReturnButtonPressed);
        _returnButton.Connect("pressed", OnReturnButtonPressedCallable, 0);

        _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && !_inventoryUI.Visible)
        {
            // 水平旋转（摄像机）
            _cameraPivot.RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            
            // 垂直旋转（摄像机）
            _cameraRotation.Y -= mouseMotion.Relative.Y * MouseSensitivity;
            _cameraRotation.Y = Mathf.Clamp
            (
                _cameraRotation.Y,
                Mathf.DegToRad(CameraMinVertical),
                Mathf.DegToRad(CameraMaxVertical)
            );
            
            _camera.Rotation = new Vector3(_cameraRotation.Y, _camera.Rotation.Y, _camera.Rotation.Z);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float deltaF = (float)delta;
        Vector3 velocity = Velocity;

        // 重力
        if (!IsOnFloor())
            velocity.Y -= _gravity * deltaF;

        if (Input.IsActionJustPressed("move_jump") && IsOnFloor())
            velocity.Y = JumpForce;

        if (Input.IsActionJustPressed("action_exit"))
            ToggleMenu();

        if (Input.IsActionJustPressed("action_inventory_toggle"))
            ToggleInventory();

        if (Input.IsActionJustPressed("action_phone_toggle"))
            TogglePhone();

        if (Input.IsActionJustPressed("debug_add_day"))
            Sleep();

        // 快捷栏选择
        if (!_inventoryUI.Visible)
        {
            // 遍历 0-9 检测对应的 action
            for (int i = 0; i <= 9; i++)
            {
                if (Input.IsActionJustPressed($"action_hotbar_{i}"))
                {
                    _currentSelectedItem = _hotBar.HotBarSelect(i);
                }
            }

            if (Input.IsActionJustPressed("action_hotbar_left"))
            {
                if (_hotBar.CurrentSelectedItem == 0)
                    _currentSelectedItem = _hotBar.HotBarSelect(9);
                else
                    _currentSelectedItem = _hotBar.HotBarSelect(_hotBar.CurrentSelectedItem - 1);
            }

            if (Input.IsActionJustPressed("action_hotbar_right"))
            {
                if (_hotBar.CurrentSelectedItem == 9)
                    _currentSelectedItem = _hotBar.HotBarSelect(0);
                else
                    _currentSelectedItem = _hotBar.HotBarSelect(_hotBar.CurrentSelectedItem + 1);
            }

                    if (_currentSelectedItem == null)
                        _currentItem.Texture = null;
                    else
                        _currentItem.Texture = _currentSelectedItem.Sprite.Texture;
        }


        // 移动输入
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        
        // 基于摄像机方向计算移动方向
        Vector3 cameraForward = _cameraPivot.GlobalTransform.Basis.Z.Normalized();
        Vector3 cameraRight = _cameraPivot.GlobalTransform.Basis.X.Normalized();
        
        Vector3 direction = (cameraForward * inputDir.Y + cameraRight * inputDir.X).Normalized();
        direction.Y = 0; // 确保水平移动

        if (direction != Vector3.Zero)
        {
            // 角色模型朝向移动方向
            float targetAngle = -(Rotation.Y - Mathf.Atan2(velocity.X, velocity.Z));
            float currentAngle = _playerModel.Rotation.Y;
            float newAngle = Mathf.LerpAngle(
                currentAngle, 
                targetAngle, 
                RotationSpeed * deltaF
            );
            _playerModel.Rotation = new Vector3(0, newAngle ,0);

            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();

        if (Velocity.Length() == 0)
        {
            if (IsUsingWheelbarrow || CurrentHoldWood > 0)
                CurrentPlayerState = PlayerState.Idle_Holding;
            else
                CurrentPlayerState = PlayerState.Idle;
        }
        else
        {
            if (IsUsingWheelbarrow || CurrentHoldWood > 0)
                CurrentPlayerState = PlayerState.Run_Holding;
            else
                CurrentPlayerState = PlayerState.Run;
        }

        _playerModelAnimationPlayer.Play(CurrentPlayerState.ToString());

        if (_crosshairRaycast3D.IsColliding())
        {
            var newCollider = (Node3D)_crosshairRaycast3D.GetCollider();

            if (IsInstanceValid(Raycast3DCollider) && Raycast3DCollider != newCollider)
                Raycast3DCollider.Call("OnBodyExited", this);

            Raycast3DCollider = newCollider;
            raycastCollider = newCollider;

            if (IsInstanceValid(Raycast3DCollider))
                Raycast3DCollider.Call("OnBodyEntered", this);
        }
        else if (IsInstanceValid(Raycast3DCollider))
        {
            Raycast3DCollider.Call("OnBodyExited", this);
            Raycast3DCollider = null;
            raycastCollider = null;
        }
    }

    public void UpdateHotBar()
    {
        _hotBar.UpdateHotBar();
    }

    public void ToggleInventory()
    {
        _inventoryUI.Visible = !_inventoryUI.Visible;

        if (_inventoryUI.Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _crosshair.Hide();
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            _crosshair.Show();
            _hotBar.HotBarSelect(_hotBar.CurrentSelectedItem);
            if (_hotBar.HotBarSelect(_hotBar.CurrentSelectedItem) != null)
            {
                _currentSelectedItem = _hotBar.HotBarSelect(_hotBar.CurrentSelectedItem);
            }
        }
    }

    public void TogglePhone()
    {
        _phoneUI.Visible = !_phoneUI.Visible;

        if (_phoneUI.Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _inventoryUI.Hide();
            _hotBar.Hide();
            _crosshair.Hide();
            _daysCount.Hide();

            _phoneUI.UpdateDayCount();
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            _hotBar.Show();
            _crosshair.Show();
            _daysCount.Show();
            _hotBar.HotBarSelect(_hotBar.CurrentSelectedItem);
            if (_hotBar.HotBarSelect(_hotBar.CurrentSelectedItem) != null)
            {
                _currentSelectedItem = _hotBar.HotBarSelect(_hotBar.CurrentSelectedItem);
            }
        }
    }

    public void Sleep()
    {
        _currentDay++;
        _daysCount.Text = "DAY - " + _currentDay;

        Array<Node> Muds = GetTree().GetNodesInGroup("mud");
        foreach (Mud mud in Muds.Cast<Mud>())
        {
            mud.Grow();
        }

        CompostBox compostBox = (CompostBox)GetTree().GetFirstNodeInGroup("compost_box");
        compostBox.Compost();

        PackageDeliveryBox packageDeliveryBox = (PackageDeliveryBox)GetTree().GetFirstNodeInGroup("package_delivery_box");
        packageDeliveryBox.Deliver();
    }

    public void ToggleMenu()
    {
        _menu.Visible = !_menu.Visible;

        if (_menu.Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _inventoryUI.Hide();
            _hotBar.Hide();
            _crosshair.Hide();
            _daysCount.Hide();
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            _hotBar.Show();
            _crosshair.Show();
            _daysCount.Show();
            _hotBar.HotBarSelect(_hotBar.CurrentSelectedItem);
            if (_hotBar.HotBarSelect(_hotBar.CurrentSelectedItem) != null)
            {
                _currentSelectedItem = _hotBar.HotBarSelect(_hotBar.CurrentSelectedItem);
            }
        }
    }

    public void AddHoldingWood()
    {
        if (IsUsingWheelbarrow || CurrentHoldWood >= 2) return;
        CurrentHoldWood++;
        Node3D Wood = GetNode<Node3D>("Character/Items/Woods/Wood" + CurrentHoldWood);
        Wood.Show();

        _currentItem.Hide();
    }

    public void RemoveHoldingWood()
    {
        if (IsUsingWheelbarrow || CurrentHoldWood <= 0) return;
        Node3D Wood = GetNode<Node3D>("Character/Items/Woods/Wood" + CurrentHoldWood);
        Wood.Hide();
        CurrentHoldWood--;

        if (CurrentHoldWood == 0)
            _currentItem.Show();
    }

    public void UpdateWheelbarrow(Wheelbarrow wheelbarrow, bool visible)
    {
        if ((CurrentPlayerState == PlayerState.Idle_Holding || CurrentPlayerState == PlayerState.Run_Holding) && CurrentHoldWood > 0) return;

        IsUsingWheelbarrow = visible;
        _currentItem.Visible = !visible;

        if (visible)
        {
            Node3D items = GetNode<Node3D>("Character/Items");
            wheelbarrow.Reparent(items);
            wheelbarrow.GlobalRotation = _playerModel.GlobalRotation;
            wheelbarrow.GlobalPosition = _playerModel.GlobalPosition;
            wheelbarrow.Position += new Vector3(0, 0, -1);
            wheelbarrow.AddToGroup("wheelbarrow");
        }
        else
        {
            Node3D environment = (Node3D)GetTree().GetFirstNodeInGroup("environment");
            wheelbarrow.Reparent(environment);
            wheelbarrow.RemoveFromGroup("wheelbarrow");
        }
    }

    public Wheelbarrow GetWheelbarrow()
    {
        if (!IsUsingWheelbarrow) return null;

        return (Wheelbarrow)GetTree().GetFirstNodeInGroup("wheelbarrow");
    }

    public void AddWheelbarrowWood()
    {
        if (GetWheelbarrow() == null) return;

        Wheelbarrow wheelbarrow = GetWheelbarrow();
        wheelbarrow.AddWheelbarrowWood();
    }

    public void RemoveWheelbarrowWood()
    {
        if (GetWheelbarrow() == null) return;

        Wheelbarrow wheelbarrow = GetWheelbarrow();
        wheelbarrow.RemoveWheelbarrowWood();
    }

    public void AddWheelbarrowCowpie()
    {
        if (GetWheelbarrow() == null) return;

        Wheelbarrow wheelbarrow = GetWheelbarrow();
        wheelbarrow.AddWheelbarrowCowpie();
    }

    public void FillCompostBox()
    {
        if (GetWheelbarrow() == null) return;

        Wheelbarrow wheelbarrow = GetWheelbarrow();
        wheelbarrow.FillCompostBox();
    }

    public void AddWheelbarrowLeaf()
    {
        if (GetWheelbarrow() == null) return;

        Wheelbarrow wheelbarrow = GetWheelbarrow();
        wheelbarrow.AddWheelbarrowLeaf();
    }

    public void RemoveWheelbarrowLeaf()
    {
        if (GetWheelbarrow() == null) return;

        Wheelbarrow wheelbarrow = GetWheelbarrow();
        wheelbarrow.RemoveWheelbarrowLeaf();
    }

    public bool CheckWheelbarrowIsEmpty()
    {
        Wheelbarrow wheelbarrow = GetWheelbarrow();
        return wheelbarrow.IsWheelbarrowEmpty;
    }

    public bool CheckWheelbarrowContent(string name)
    {
        if (GetWheelbarrow() != null)
        {
            Wheelbarrow wheelbarrow = GetWheelbarrow();

            switch (name)
            {
                case "WheelbarrowCurrentCowpie":
                    if (wheelbarrow.WheelbarrowCurrentCowpie > 0)
                        return true;
                    else
                        return false;
                case "WheelbarrowCurrentWood":
                    if (wheelbarrow.WheelbarrowCurrentWood > 0)
                        return true;
                    else
                        return false;
                case "WheelbarrowCurrentLeaf":
                    if (wheelbarrow.WheelbarrowCurrentLeaf > 0)
                        return true;
                    else
                        return false;
                default:
                    return false;
            }
        }
        else
            return false;
    }

    public void UpdateShovel()
    {
        HasShovel = true;
    }

    public void UpdateMissionSlots(MissionSlot mission, bool activated)
    {
        if (activated)
        {
            Label newLabel = new() { Text = mission.Text };
            LabelSettings labelSettings = new(){FontSize = 24, OutlineSize = 10, OutlineColor = new Color(0,0,0,1)};
            newLabel.LabelSettings = labelSettings;

            _missionSlots.AddChild(newLabel);
        }
        else
        {
            Array<Node> labels = _missionSlots.GetChildren();
            if (labels.Count == 0) return;

            foreach (Label label in labels.Cast<Label>())
            {
                if (label.Text == mission.Text)
                {
                    label.QueueFree();
                    return;
                }
            }
        }
    }

    public void OnMenuCloseButtonPressed()
    {
        GetTree().Quit();
    }

    public void OnReturnButtonPressed()
    {
        ToggleMenu();
    }
}