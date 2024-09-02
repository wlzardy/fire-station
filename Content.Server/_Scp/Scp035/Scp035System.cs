using System.Numerics;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared._Scp.Scp035;
using Content.Shared.Pointing;
using Robust.Shared.Map;

namespace Content.Server._Scp.Scp035;

public sealed class Scp035System : SharedScp035System
{
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly NPCSystem _npc = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Scp035MaskUserComponent, MaskRaiseArmyActionEvent>(OnRaiseArmy);
        SubscribeLocalEvent<Scp035MaskUserComponent, AfterPointedAtEvent>(OnPointedAt);
    }

    private void OnRaiseArmy(Entity<Scp035MaskUserComponent> ent, ref MaskRaiseArmyActionEvent args)
    {
        if (args.Handled)
            return;

        var servant = Spawn("MobServant035", Transform(ent).Coordinates);
        var comp = EnsureComp<Scp035ServantComponent>(servant);
        comp.User = ent;
        Dirty(servant, comp);

        ent.Comp.Servants.Add(servant);
        _npc.SetBlackboard(servant, NPCBlackboard.FollowTarget, new EntityCoordinates(ent, Vector2.Zero));
        UpdateServantNpc(servant, ent.Comp.CurrentOrder);

        args.Handled = true;
    }

    private void OnPointedAt(Entity<Scp035MaskUserComponent> ent, ref AfterPointedAtEvent args)
    {
        if (ent.Comp.CurrentOrder != MaskOrderType.Kill)
            return;

        foreach (var servant in ent.Comp.Servants)
        {
            _npc.SetBlackboard(servant, NPCBlackboard.CurrentOrderedTarget, args.Pointed);
        }
    }

    public override void UpdateServantNpc(EntityUid uid, MaskOrderType orderType)
    {
        if (!TryComp<HTNComponent>(uid, out var htn))
            return;

        if (htn.Plan != null)
            _htn.ShutdownPlan(htn);

        _npc.SetBlackboard(uid, NPCBlackboard.CurrentOrders, orderType);
        _htn.Replan(htn);
    }
}
