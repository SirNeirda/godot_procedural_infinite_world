using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class DebugUI : PanelContainer
{
    public static DebugUI Instance;
    [Export] Label DebugItem;
    [Export] VBoxContainer DebugItemContainer;

    public Dictionary<string, Label> DebugItems = new Dictionary<string, Label>();

    public override void _Ready()
    {
        Instance = this;
    }

    public void AddChunkData(string key, string text)
    {
        if (DebugItems.ContainsKey(key))
        {
            DebugItems[key].Text = text;
        }
        else
        {
            Label newItem = DebugItem.Duplicate() as Label;
            DebugItemContainer.CallDeferred("add_child", newItem);
            newItem.Visible = true;
            SetText(key, text);
            DebugItems.Add(key, newItem);
        }
    }

    void SetText(string key, string text)
    {
        CallDeferred("setText", key, text);
    }

    void setText(string key, string text)
    {
        if (DebugItems.ContainsKey(key))
        {
            DebugItems[key].Text = text;
        }
    }

    public void RemoveChunkData(string key)
    {
        if (DebugItems.ContainsKey(key))
        {
            DebugItems[key].QueueFree();
            DebugItems.Remove(key);
        }
    }
}
