using System;

namespace PlayerCastBars.Core;

public static class CastPolicy
{
    public static bool ShouldShowPlayer(bool isSelf, bool isPartyMember, bool partyOnly, bool showSelf)
        => isSelf ? showSelf : !partyOnly || isPartyMember;

    public static bool TryGetProgress(bool isCasting, uint actionId, float elapsed, float total, out float progress)
    {
        progress = 0;
        if (!isCasting || actionId == 0 || !float.IsFinite(elapsed) || !float.IsFinite(total) || total <= 0 || elapsed >= total)
            return false;
        progress = Math.Clamp(elapsed / total, 0, 1);
        return true;
    }
}
