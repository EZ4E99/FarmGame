using Godot;
using System;

public enum ItemTypes
{
    Seed,
    Crop,
    Food,
    Tool,
}

public partial class Item : Node2D
{
    [Export] public string ItemName;
    [Export] public Texture2D Icon;
    [Export] public bool IsStackable = false;
    [Export] public ItemTypes ItemType;
    public string SeedCropName;

    public override void _Ready()
    {
        if (ItemType == ItemTypes.Seed)
            SeedCropName = "wheat"; // 因为Demo中只有一种作物，暂时用这个

        AddToGroup("items");
    }
}