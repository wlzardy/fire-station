using Content.Shared._Scp.Scp939;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Client._Scp.Scp939;

public sealed partial class Scp939System : SharedScp939System
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();
    }
}
