using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class Player : CharacterBody3D
{
    public enum PlayerState
    {
        Idle,        // 静止
        Run,      // 奔跑
        Hold_Idle,  // 手持静止
        Hold_Run,     // 手持奔跑
    }

    // 移动参数
    [Export] public float Speed = 5.0f;
    [Export] public float JumpForce = 4.5f;
    [Export] public float MouseSensitivity = 0.002f;
    [Export] public float CameraMinVertical = -30.0f;
    [Export] public float CameraMaxVertical = 60.0f;
    [Export] public float RotationSpeed = 10.0f; // 旋转平滑速度

    // 节点引用
    private Node3D _playerModel;
    private AnimationPlayer _playerModelAnimationPlayer;
    private Node3D _cameraPivot;
    private Camera3D _camera;
    private CanvasLayer _inventoryUI;
    private Button _closeButton;
    private Panel _menu;
    private Button _menuCloseButton;
    private Button _returnButton;
    private HotBar _hotBar;
    private Label _daysCount;

    private float _gravity;
    private Vector2 _cameraRotation = Vector2.Zero;
    private int _currentDay = 1;
    public InventoryItem _currentSelectedItem;
    

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        
        _playerModel = GetNode<Node3D>("Character");
        _playerModelAnimationPlayer = GetNode<AnimationPlayer>("Character/AnimationPlayer");
        _cameraPivot = GetNode<Node3D>("CameraPivot");
        _camera = GetNode<Camera3D>("CameraPivot/Camera3D");
        _inventoryUI = GetNode<CanvasLayer>("HUD/InventoryUI");
        _closeButton = GetNode<Button>("HUD/InventoryUI/Panel/CloseButton");
        _menu = GetNode<Panel>("HUD/Menu");
        _menuCloseButton = GetNode<Button>("HUD/Menu/VBoxContainer/HBoxContainer/Button2");
        _returnButton = GetNode<Button>("HUD/Menu/VBoxContainer/HBoxContainer/Button");
        _hotBar = GetNode<HotBar>("HUD/HotBar");
        _daysCount = GetNode<Label>("HUD/Label");

        Callable OnButtonPressedCallable = new(this, MethodName.ToggleInventory);
    	_closeButton.Connect("pressed", OnButtonPressedCallable, 0); 
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
            _cameraRotation.Y = Mathf.Clamp(
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

        if (Velocity.Length() != 0)
            _playerModelAnimationPlayer.Play("Run");
        else
            _playerModelAnimationPlayer.Play("Idle");
    }

    public void UpdateHotBar()
    {
        _hotBar.UpdateHotBar();
    }

    public void ToggleInventory()
    {
        _inventoryUI.Visible = !_inventoryUI.Visible;

        if (_inventoryUI.Visible) Input.MouseMode = Input.MouseModeEnum.Visible;
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
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
    }

    public void ToggleMenu()
    {
        _menu.Visible = !_menu.Visible;

        if (_menu.Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _inventoryUI.Hide();
            _hotBar.Hide();
            _daysCount.Hide();
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            _hotBar.Show();
            _daysCount.Show();
            _hotBar.HotBarSelect(_hotBar.CurrentSelectedItem);
            if (_hotBar.HotBarSelect(_hotBar.CurrentSelectedItem) != null)
            {
                _currentSelectedItem = _hotBar.HotBarSelect(_hotBar.CurrentSelectedItem);
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