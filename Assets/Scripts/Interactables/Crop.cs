using Godot;
using System;

// 作物生长阶段枚举
public enum GrowthStage
{
    种子,        // 种子阶段
    发芽,      // 发芽
    生长期,  // 生长期
    开花期,     // 开花期
    成熟       // 成熟期
}

public partial class Crop : Node3D
{
    private AnimatedSprite3D _animatedSprite3D;
    private Label3D _statusPrompt;
    [Export] public string CropName { get; set; } = "Unnamed Crop";
    [Export] public int[] GrowthDaysPerStage = [2, 3, 3, 2]; // 四个阶段所需天数
    [Export] public Item CropItem;

    public override void _Ready()
    {
        base._Ready();

        _animatedSprite3D = GetNode<AnimatedSprite3D>("AnimatedSprite3D");
        _statusPrompt = GetNode<Label3D>("StatusPrompt");
    }


    public int GetDaysForStage(GrowthStage stage)
    {
        UpdateAnimatedSprites3D(stage);

        return stage switch
        {
            GrowthStage.种子 => GrowthDaysPerStage[0],
            GrowthStage.发芽 => GrowthDaysPerStage[1],
            GrowthStage.生长期 => GrowthDaysPerStage[2],
            GrowthStage.开花期 => GrowthDaysPerStage[3],
            _ => 0
        };
    }

    public void UpdateAnimatedSprites3D(GrowthStage stage)
    {
        _animatedSprite3D.Play(CropName);

        switch(stage)
        {
            case GrowthStage.种子:
                _animatedSprite3D.Frame = 0;
                break;
            case GrowthStage.发芽:
                _animatedSprite3D.Frame = 1;
                break;
            case GrowthStage.生长期:
                _animatedSprite3D.Frame = 2;
                break;
            case GrowthStage.开花期:
                _animatedSprite3D.Frame = 3;
                break;
            case GrowthStage.成熟:
                _animatedSprite3D.Frame = 4;
                break;
            default:
                return;
        };
    }

    public void UpdateStatusPrompt(bool visible, string text, int days)
    {
        _statusPrompt.Visible = visible;
        _statusPrompt.Text = text + $" 生长到下一阶段还需{days}天";
    }
}
