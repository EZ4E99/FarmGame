using Godot;
using System;
using System.Collections.Generic;

public partial class PackageDeliveryBox : Area3D
{
    public List<PackageDeliveryBoxItem> Items;
    private bool _isColliding = false;
    private Label3D _label3D;
    private Player _player;
    private Inventory _inventory;

    public override void _Ready()
    {
        _label3D = GetNode<Label3D>("InteractPrompt");
        Callable bodyEnteredCallable = new(this, MethodName.OnBodyEntered);
        Connect("body_entered", bodyEnteredCallable, 0);
        Callable bodyExitedCallable = new(this, MethodName.OnBodyExited);
        Connect("body_exited", bodyExitedCallable, 0);

        Items = [];
        _player = (Player)GetTree().GetFirstNodeInGroup("player");
        _inventory = (Inventory)GetTree().GetFirstNodeInGroup("inventory");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("action_use") && _isColliding)
        {
            List<PackageDeliveryBoxItem> toRemove = new();
            foreach (PackageDeliveryBoxItem item in Items)
            {
                if (item.IsArrived)
                {
                    _inventory.AddItem(item.Item, item.Amount);
                    toRemove.Add(item);
                }
            }

            foreach (var item in toRemove)
            {
                Items.Remove(item);
            }

            UpdateInteractPrompt();
        }
    }

    public void AddPackageDeliveryBoxItem(Item item, int amount)
    {
        PackageDeliveryBoxItem newItem = new(item, amount);
        Items.Add(newItem);

        UpdateInteractPrompt();
        GD.Print(_label3D.Text);
    }

    public void UpdateInteractPrompt()
    {
        _label3D.Text = "[E] 获取物品";

        if (Items.Count == 0) return;
        string prompt = "";
        foreach (PackageDeliveryBoxItem item in Items)
        {
            if (item.IsArrived)
            {
                prompt = prompt + "\n" + Tr(item.Item.ItemName) + " x " + item.Amount;
            }
            else
            {
                prompt = prompt + "\n" + Tr(item.Item.ItemName) + " x " + item.Amount + " - 运输中 - " + (item.ArrivedDaysNeeded - item.ArrivedDaysCount) + "天后到达";
            }
        }

        _label3D.Text += prompt;
    }

    public void Deliver()
    {
        foreach (PackageDeliveryBoxItem item in Items)
        {
            if (item.IsArrived) return;

            item.ArrivedDaysCount++;
            if (item.ArrivedDaysCount >= item.ArrivedDaysNeeded)
                item.IsArrived = true;
        }

        UpdateInteractPrompt();
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
}

// 快递箱物品类型
public class PackageDeliveryBoxItem(Item item, int value)
{
    public Item Item { get; set; } = item;
    public int Amount { get; set; } = value;
    public int ArrivedDaysCount = 0;
    public int ArrivedDaysNeeded = 3;
    public bool IsArrived = false;
}
