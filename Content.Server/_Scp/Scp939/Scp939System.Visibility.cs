using Content.Server.Chat.Systems;
using Content.Shared._Scp.Scp939;
using Content.Shared.Mobs.Components;

namespace Content.Server._Scp.Scp939;

public sealed partial class Scp939System
{
    private void InitializeVisibility()
    {
        SubscribeLocalEvent<MobStateComponent, ComponentInit>(OnMobInit);
        SubscribeLocalEvent<Scp939VisibilityComponent, EntitySpokeEvent>(OnTargetSpoke);
        SubscribeLocalEvent<Scp939VisibilityComponent, EmoteEvent>(OnTargetEmote);
    }

    private void OnTargetEmote(Entity<Scp939VisibilityComponent> ent, ref EmoteEvent args)
    {
        MobDidSomething(ent);
    }

    private void OnMobInit(Entity<MobStateComponent> ent, ref ComponentInit args)
    {
        if (HasComp<Scp939Component>(ent))
        {
            return;
        }

        var visibilityComponent = EnsureComp<Scp939VisibilityComponent>(ent);
        visibilityComponent.VisibilityAcc = 0;
    }


    private void OnTargetSpoke(Entity<Scp939VisibilityComponent> ent, ref EntitySpokeEvent args)
    {
        MobDidSomething(ent);
    }

    private void MobDidSomething(Entity<Scp939VisibilityComponent> ent)
    {
        ent.Comp.VisibilityAcc = 0;
        Dirty(ent);
    }
}
