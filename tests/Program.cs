using PlayerCastBars.Core;

var passed = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    passed++;
}

Check(!CastPolicy.ShouldShowPlayer(true, true, true, false), "Self must be excluded by default.");
Check(CastPolicy.ShouldShowPlayer(false, true, true, false), "Other party members must be included.");
Check(!CastPolicy.ShouldShowPlayer(false, false, true, false), "Non-party players must be excluded.");
Check(CastPolicy.ShouldShowPlayer(false, false, false, false), "Nearby-player mode must include non-party players.");
Check(!CastPolicy.ShouldShowPlayer(true, true, false, false), "Nearby-player mode must still exclude self.");
Check(CastPolicy.ShouldShowPlayer(true, false, true, true), "Explicit self toggle must also work while solo.");
Check(CastPolicy.TryGetProgress(true, 120, 1.5f, 3, out var progress) && progress == 0.5f, "Active cast must show 50% progress.");
Check(!CastPolicy.TryGetProgress(false, 120, 1.5f, 3, out _), "Interrupted cast must disappear immediately.");
Check(!CastPolicy.TryGetProgress(true, 120, 0, 0, out _), "Instant skills must not display.");
Check(!CastPolicy.TryGetProgress(true, 120, 3, 3, out _), "Finished cast must disappear.");
Check(!CastPolicy.TryGetProgress(true, 0, 1, 3, out _), "Missing action must not display.");
Check(!CastPolicy.TryGetProgress(true, 120, float.NaN, 3, out _), "Invalid elapsed time must not draw.");
Check(!CastPolicy.TryGetProgress(true, 120, 1, float.PositiveInfinity, out _), "Invalid total duration must not draw.");
Check(CastPolicy.TryGetProgress(true, 120, -0.1f, 3, out progress) && progress == 0, "Negative elapsed time must clamp safely.");
Console.WriteLine($"Passed {passed} cast visibility and progress checks.");
