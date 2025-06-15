using Godot;
using System;

public partial class StoreItem : Button
{
    [Export] public Item Item;
    [Export] public int ItemAmount;
    [Export] public int Price;

    public override void _Ready()
    {
        Icon = Item.Icon;
        Text = Tr(Item.ItemName) + " x " + ItemAmount + " - ￥ " + Price;
        Callable OnButtonPressedCallable = new(this, MethodName.OnButtonPressed);
        Connect("pressed", OnButtonPressedCallable, 0);
    }

    public void OnButtonPressed()
    {
        Player player = (Player)GetTree().GetFirstNodeInGroup("player");
        if (player._currentMoney < Price) return;
        player._currentMoney -= Price;

        Phone phone = (Phone)GetTree().GetFirstNodeInGroup("phone");
        phone.UpdateMoneyCount();

        PackageDeliveryBox box = (PackageDeliveryBox)GetTree().GetFirstNodeInGroup("package_delivery_box");
        box.AddPackageDeliveryBoxItem(Item, ItemAmount);

        Disabled = true;
        Text = "已售罄！";
    }
}
