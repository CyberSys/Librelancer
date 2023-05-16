using System;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using ImGuiNET;

namespace LibreLancer.ImUI;

public class Toolbar : IDisposable
{
    private bool isOverflow = false;
    private bool isOverflowOpen = false;

    private Toolbar()
    {
    }

    public static Toolbar Begin(string id, bool sameLine)
    {
        ImGui.PushID(id);
        if (!sameLine) ImGui.Dummy(Vector2.Zero);
        return new Toolbar();
    }

    bool DoOverflow(string text, float margin)
    {
        if (isOverflow) return true;
        ImGui.SameLine();
        var textSize = ImGui.CalcTextSize(text);
        var cpos = ImGui.GetCursorPosX();
        var currentWidth = ImGui.GetWindowWidth();
        if (cpos + textSize.X + (margin * ImGuiHelper.Scale) > currentWidth) {
            isOverflow = true;
            if (ImGui.Button(">")) ImGui.OpenPopup("#overflow");
            isOverflowOpen = ImGui.BeginPopup("#overflow");
            return true;
        }
        return false;
    }

    public bool ButtonItem(string name)
    {
        if (DoOverflow(name, 15))
        {
            if (isOverflowOpen)
                return ImGui.MenuItem(name);
            return false;
        }
        return ImGui.Button(name);
    }

    public void ToggleButtonItem(string name, ref bool isSelected)
    {
        if (DoOverflow(name, 15))
        {
            if (isOverflowOpen) ImGui.MenuItem(name, "", ref isSelected);
        }
        else
        {
            if (ImGuiExt.ToggleButton(name, isSelected)) isSelected = !isSelected;
        }
    }

    public void CheckItem(string name, ref bool isSelected)
    {
        if (DoOverflow(name, 50))
        {
            if (isOverflowOpen) ImGui.MenuItem(name, "", ref isSelected);
        }
        else
        {
            ImGui.Checkbox(name, ref isSelected);
        }
    }

    public void TextItem(string text)
    {
        if (DoOverflow(text, 2))
        {
            if (isOverflowOpen) ImGui.MenuItem(text, false);
        }
        else
        {
            ImGui.TextUnformatted(text);
        }
    }

    public void Dispose()
    {
        if (isOverflow && isOverflowOpen)
        {
            ImGui.EndPopup();
        }
        ImGui.PopID();
    }
}