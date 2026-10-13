using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using Dalamud.Interface.Utility;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using PlayerCastBars.Core;
using ActionRow = Lumina.Excel.Sheets.Action;
using NativeGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace PlayerCastBars;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager Commands { get; private set; } = null!;
    [PluginService] internal static IObjectTable Objects { get; private set; } = null!;
    [PluginService] internal static IPartyList Party { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IDataManager Data { get; private set; } = null!;
    [PluginService] internal static ICondition Conditions { get; private set; } = null!;

    private readonly Configuration configuration;
    private readonly HashSet<uint> partyIds = [];
    private readonly Dictionary<uint, string> actionNames = [];
    private bool settingsOpen;

    public Plugin()
    {
        configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        configuration.Sanitize();
        Commands.AddHandler("/pcbar", new CommandInfo(OnCommand) { HelpMessage = "打开玩家咏唱条设置。" });
        PluginInterface.UiBuilder.Draw += Draw;
        PluginInterface.UiBuilder.OpenConfigUi += OpenSettings;
        PluginInterface.UiBuilder.OpenMainUi += OpenSettings;
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenSettings;
        PluginInterface.UiBuilder.OpenMainUi -= OpenSettings;
        Commands.RemoveHandler("/pcbar");
    }

    private void OnCommand(string command, string args) => settingsOpen = !settingsOpen;
    private void OpenSettings() => settingsOpen = true;

    private void Draw()
    {
        DrawSettings();
        var local = Objects.LocalPlayer;
        if (!configuration.Enabled || local == null ||
            Conditions[ConditionFlag.BetweenAreas] || Conditions[ConditionFlag.BetweenAreas51] ||
            Conditions[ConditionFlag.WatchingCutscene] || Conditions[ConditionFlag.WatchingCutscene78] ||
            Conditions[ConditionFlag.OccupiedInCutSceneEvent])
            return;

        partyIds.Clear();
        foreach (var member in Party)
            partyIds.Add(member.EntityId);

        foreach (var actor in Objects)
        {
            if (actor is not IPlayerCharacter player ||
                !CastPolicy.ShouldShowPlayer(player.EntityId == local.EntityId,
                    partyIds.Contains(player.EntityId), configuration.PartyOnly, configuration.ShowSelf) ||
                Vector3.DistanceSquared(local.Position, player.Position) > configuration.MaxDistance * configuration.MaxDistance ||
                player.CastActionType != 1 ||
                !CastPolicy.TryGetProgress(player.IsCasting, player.CastActionId, player.CurrentCastTime, player.TotalCastTime, out var progress))
                continue;

            DrawCastBar(player, progress);
        }
    }

    private unsafe void DrawCastBar(IPlayerCharacter player, float progress)
    {
        // Use the model's height rather than assuming all races are the same size.
        var modelHeight = ((NativeGameObject*)player.Address)->GetHeight();
        if (!float.IsFinite(modelHeight) || modelHeight <= 0)
            return;
        var anchor = player.Position + new Vector3(0, modelHeight + configuration.HeadOffset, 0);
        if (!GameGui.WorldToScreen(anchor, out var screen, out var inView) || !inView)
            return;

        var scale = ImGuiHelpers.GlobalScale;
        var width = configuration.Width * scale;
        var height = configuration.Height * scale;
        var viewport = ImGui.GetMainViewport();
        // WorldToScreen already includes the game window's viewport position.
        var center = screen + new Vector2(0, configuration.ScreenOffset * scale);
        var topLeft = center - new Vector2(width / 2, 0);
        var bottomRight = topLeft + new Vector2(width, height);
        var draw = ImGui.GetBackgroundDrawList();
        draw.PushClipRect(viewport.Pos, viewport.Pos + viewport.Size, true);
        draw.AddRectFilled(topLeft, bottomRight, 0xD9202020, 3 * scale);
        draw.AddRectFilled(topLeft, topLeft + new Vector2(width * progress, height), 0xFF50C9F0, 3 * scale);
        draw.AddRect(topLeft, bottomRight, 0xE6FFFFFF, 3 * scale);

        var actionId = player.CastActionId;
        if (!actionNames.TryGetValue(actionId, out var name))
        {
            var row = Data.GetExcelSheet<ActionRow>().GetRowOrDefault(actionId);
            name = row?.Name.ExtractText() ?? $"技能 #{actionId}";
            if (string.IsNullOrWhiteSpace(name)) name = $"技能 #{actionId}";
            actionNames[actionId] = name;
        }
        var remaining = Math.Max(0, player.TotalCastTime - player.CurrentCastTime);
        var label = configuration.ShowRemaining ? $"{name}  {remaining:0.0}s" : name;
        var textSize = ImGui.CalcTextSize(label);
        var textPosition = new Vector2(center.X - Math.Min(textSize.X, width) / 2, topLeft.Y - textSize.Y - 3 * scale);
        draw.PushClipRect(new Vector2(topLeft.X, textPosition.Y - scale), new Vector2(bottomRight.X, topLeft.Y), true);
        draw.AddText(textPosition + new Vector2(scale), 0xFF000000, label);
        draw.AddText(textPosition, 0xFFFFFFFF, label);
        draw.PopClipRect();
        draw.PopClipRect();
    }

    private void DrawSettings()
    {
        if (!settingsOpen) return;
        ImGui.SetNextWindowSize(new Vector2(430, 350), ImGuiCond.FirstUseEver);
        var visible = ImGui.Begin("玩家咏唱条###PlayerCastBarsSettings", ref settingsOpen, ImGuiWindowFlags.AlwaysAutoResize);
        try
        {
            if (!visible) return;
            var changed = ImGui.Checkbox("启用咏唱条", ref configuration.Enabled);
            changed |= ImGui.Checkbox("只显示队伍成员", ref configuration.PartyOnly);
            changed |= ImGui.Checkbox("显示自己（独立开关）", ref configuration.ShowSelf);
            changed |= ImGui.Checkbox("显示剩余秒数", ref configuration.ShowRemaining);
            changed |= ImGui.SliderFloat("宽度", ref configuration.Width, 80, 320, "%.0f px");
            changed |= ImGui.SliderFloat("高度", ref configuration.Height, 4, 30, "%.0f px");
            changed |= ImGui.SliderFloat("头顶高度偏移", ref configuration.HeadOffset, 0, 3, "%.2f");
            changed |= ImGui.SliderFloat("屏幕垂直偏移", ref configuration.ScreenOffset, -150, 150, "%.0f px");
            changed |= ImGui.SliderFloat("最大距离", ref configuration.MaxDistance, 5, 100, "%.0f");
            ImGui.TextWrapped("默认仅显示其他队友。瞬发、咏唱结束或中断时隐藏。关闭队伍限制可显示附近其他玩家。仅显示当前屏幕内角色；不会透过遮挡判断可见性。");
            if (changed) PluginInterface.SavePluginConfig(configuration);
        }
        finally { ImGui.End(); }
    }
}
