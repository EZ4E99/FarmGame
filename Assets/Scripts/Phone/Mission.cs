using Godot;
using System;

public partial class Mission : Node
{
    // 任务种类
    public enum MissionType
    {
        Collect,
    }
    [Export] public MissionType CurrentMissionType;

    // 任务状态
    public enum MissionStatus
    {
        Inactivated,
        Activated,
        Finished,
    }
    public MissionStatus CurrentMissionStatus = MissionStatus.Inactivated;
    [Export] public string MissionID; // 任务ID
    [Export] public string MissionName; // 任务名
    [Export] public Item CollectMissionItem; // 收集类任务物品
    [Export] private int CollectMissionItemNeedCount; // 收集类任务物品需要数量
    private int CollectMissionItemCurrentCount = 0;

    private Inventory inventory;

    public override void _Ready()
    {
        inventory = (Inventory)GetTree().GetFirstNodeInGroup("inventory");
    }

    public void ToggleMissionStatus()
    {
        if (CurrentMissionStatus == MissionStatus.Finished) return;

        if (CurrentMissionStatus == MissionStatus.Inactivated)
        {
            CurrentMissionStatus = MissionStatus.Activated;
            if (CurrentMissionType == MissionType.Collect)
                CheckCollectMissionFinished();
        }
        else if (CurrentMissionStatus == MissionStatus.Activated)
        {
            CurrentMissionStatus = MissionStatus.Inactivated;
        }
    }

    public void CheckCollectMissionFinished()
    {
        if (CurrentMissionType != MissionType.Collect) return;

        CollectMissionItemCurrentCount = inventory.CountAll(CollectMissionItem.ItemName);

        if (CollectMissionItemCurrentCount >= CollectMissionItemNeedCount)
            CurrentMissionStatus = MissionStatus.Finished;
    }
}
