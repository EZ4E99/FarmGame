using Godot;
using System;

public partial class MissionSlot : CheckBox
{
    public Mission Mission;

    public override void _Ready()
    {
        Mission = GetNode<Mission>("Mission");

        Callable OnButtonPressedCallable = new(this, MethodName.ToggleMissionStatus);
        Connect("pressed", OnButtonPressedCallable, 0);
        AddToGroup("mission_slots");

        Text = Mission.MissionName;
        Icon = Mission.CollectMissionItem.Icon;

        UpdateButtonStatus();
    }

    public void ToggleMissionStatus()
    {
        Mission.ToggleMissionStatus();

        Player _player = (Player)GetTree().GetFirstNodeInGroup("player");
        _player.UpdateMissionSlots(this, ButtonPressed);
    }

    public void UpdateButtonStatus()
    {
        if (Mission.CurrentMissionStatus == Mission.MissionStatus.Inactivated)
            ButtonPressed = false;
        else if (Mission.CurrentMissionStatus == Mission.MissionStatus.Activated)
            ButtonPressed = true;
        else if (Mission.CurrentMissionStatus == Mission.MissionStatus.Finished)
        {
            Player _player = (Player)GetTree().GetFirstNodeInGroup("player");
            _player.UpdateMissionSlots(this, false);

            Text = Mission.MissionName + " - 已完成！";
            ButtonPressed = false;
            Disabled = true;
        }
    }
}
