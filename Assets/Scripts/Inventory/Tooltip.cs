using Godot;
using System;

public partial class Tooltip : ColorRect
{
    private MarginContainer _marginContainer;
    private Label _label;

    public override void _Ready()
    {
        _marginContainer = GetNode<MarginContainer>("MarginContainer");
        _label = GetNode<Label>("MarginContainer/Label");
    }

    public void SetText(string text)
    {
        _label.Text = text;
        _marginContainer.Size = Vector2.Zero;
        Size = _marginContainer.Size;
    }
}
