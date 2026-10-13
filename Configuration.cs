using System;
using Dalamud.Configuration;

namespace PlayerCastBars;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool Enabled = true;
    public bool PartyOnly = true;
    public bool ShowSelf = false;
    public bool ShowRemaining = true;
    public float Width = 150;
    public float Height = 12;
    public float HeadOffset = 0.3f;
    public float ScreenOffset = -12;
    public float MaxDistance = 50;

    public void Sanitize()
    {
        Width = Clamp(Width, 80, 320, 150);
        Height = Clamp(Height, 4, 30, 12);
        HeadOffset = Clamp(HeadOffset, 0, 3, 0.3f);
        ScreenOffset = Clamp(ScreenOffset, -150, 150, -12);
        MaxDistance = Clamp(MaxDistance, 5, 100, 50);
    }

    private static float Clamp(float value, float min, float max, float fallback)
        => float.IsFinite(value) ? Math.Clamp(value, min, max) : fallback;
}
