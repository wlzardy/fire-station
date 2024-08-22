using Content.Server.Actions;
using Content.Shared._Scp.Scp049;
using Content.Shared._Scp.Scp999;

namespace Content.Server._Scp.Scp049;

public sealed partial class Scp049System : SharedScp049System
{
    [Dependency] private readonly ActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Scp049Component, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<Scp049Component> ent, ref ComponentStartup args)
    {
        foreach (var action in ent.Comp.Scp049Actions)
        {
            _actionsSystem.AddAction(ent, action);
        }
    }
}
