using Content.Server.Actions;
using Content.Server.Fluids.EntitySystems;
using Content.Shared._Scp.Scp939;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.StatusEffect;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server._Scp.Scp939;

public sealed partial class Scp939System : SharedScp939System
{
    [Dependency] private readonly SmokeSystem _smokeSystem = default!;
    [Dependency] private readonly SleepingSystem _sleepingSystem = default!;
    [Dependency] private readonly ActionsSystem _actionsSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private static readonly string SleepKey = "Sleep";

    public override void Initialize()
    {
        base.Initialize();
        InitializeActions();

        SubscribeLocalEvent<Scp939Component, ComponentInit>(OnInit);
        SubscribeLocalEvent<Scp939Component, SleepStateChangedEvent>(OnSleepChanged);

        InitializeVisibility();
    }

    private void OnSleepChanged(Entity<Scp939Component> ent, ref SleepStateChangedEvent args)
    {
        _appearanceSystem.SetData(ent, Scp939Visuals.Sleeping, args.FellAsleep);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<Scp939Component, SleepingComponent>();

        while (query.MoveNext(out var uid, out var scp939Component, out _))
        {
            _damageableSystem.TryChangeDamage(uid, scp939Component.HibernationHealingRate * frameTime);
        }
    }

    private void OnInit(Entity<Scp939Component> ent, ref ComponentInit args)
    {
        foreach (var action in ent.Comp.Actions)
        {
            _actionsSystem.AddAction(ent, action);
        }
    }
}
