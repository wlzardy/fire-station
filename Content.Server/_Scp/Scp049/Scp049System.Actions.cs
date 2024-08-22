using Content.Server.Administration.Systems;
using Content.Server.IdentityManagement;
using Content.Shared._Scp.Scp049;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Server._Scp.Scp049;

public sealed partial class Scp049System
{
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IdentitySystem _identitySystem = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenateSystem = default!;



    public void InitializeActions()
    {
        SubscribeLocalEvent<Scp049Component, Scp049KillLivingBeingAction>(OnKillLeavingBeing);
        SubscribeLocalEvent<Scp049Component, Scp049KillResurrectedAction>(OnKillResurrected);
        SubscribeLocalEvent<Scp049Component, Scp049ResurrectAction>(OnResurrect);
        SubscribeLocalEvent<Scp049Component, Scp049SelfHealAction>(OnSelfHeal);
    }

    private void OnSelfHeal(Entity<Scp049Component> ent, ref Scp049SelfHealAction args)
    {
        _rejuvenateSystem.PerformRejuvenate(args.Performer);

        var entityName = Identity.Name(args.Performer, EntityManager);

        var localeMessage = Loc.GetString("scp096-self-heal", ("performer", entityName));
        _popupSystem.PopupEntity(localeMessage, args.Performer, PopupType.Medium);
    }

    private void OnResurrect(Entity<Scp049Component> ent, ref Scp049ResurrectAction args)
    {
        EnsureComp<ScpShow049HudComponent>(args.Target);
        var minionComponent = EnsureComp<Scp049MinionComponent>(args.Target);

        minionComponent.Scp049Owner = ent;

        ent.Comp.Minions.Add(ent);

        Dirty(ent);

        _rejuvenateSystem.PerformRejuvenate(args.Target);

        var targetName = Identity.Name(args.Target, EntityManager);
        var performerName = Identity.Name(ent, EntityManager);

        var localeMessage = Loc.GetString("scp096-touch-action-success",
            ("target", targetName),
            ("performer", performerName));

        _popupSystem.PopupEntity(localeMessage, ent, PopupType.MediumCaution);
    }

    private void OnKillResurrected(Entity<Scp049Component> ent, ref Scp049KillResurrectedAction args)
    {
        _mobStateSystem.ChangeMobState(args.Target, MobState.Dead);

        var targetName = Identity.Name(args.Target, EntityManager);
        var performerName = Identity.Name(args.Target, EntityManager);

        var localeMessage = Loc.GetString("scp096-touch-action-success",
            ("target", targetName),
            ("performer", performerName));

        _popupSystem.PopupEntity(localeMessage, ent, PopupType.MediumCaution);
    }

    private void OnKillLeavingBeing(Entity<Scp049Component> ent, ref Scp049KillLivingBeingAction args)
    {
        if (_mobStateSystem.IsIncapacitated(args.Target))
        {
            _popupSystem.PopupEntity(Loc.GetString("scp096-kill-action-already-dead"), args.Target, ent, PopupType.MediumCaution);
            _actionsSystem.SetCooldown(args.Action, TimeSpan.Zero);
            return;
        }

        if (!_mobStateSystem.HasState(args.Target, MobState.Dead))
        {
            _popupSystem.PopupEntity(Loc.GetString("scp096-kill-action-cant-kill"), args.Target, ent, PopupType.MediumCaution);
            _actionsSystem.SetCooldown(args.Action, TimeSpan.Zero);
            return;
        }

        _mobStateSystem.ChangeMobState(args.Target, MobState.Dead);

        var targetName = Identity.Name(args.Target, EntityManager);
        var performerName = Identity.Name(ent, EntityManager);


        var localeMessage = Loc.GetString("scp096-touch-action-success",
            ("target", targetName),
            ("performer", performerName));

        _popupSystem.PopupEntity(localeMessage, ent, PopupType.MediumCaution);
    }
}
