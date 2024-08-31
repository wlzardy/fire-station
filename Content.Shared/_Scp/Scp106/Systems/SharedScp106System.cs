using System.Linq;
using Content.Shared._Scp.Scp106.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Scp.Scp106.Systems;

public abstract class SharedScp106System : EntitySystem
{
    /*
     106
Имеет возможность перемещаться в рандомные части карты благодаря Экшен с КД в 10 минут. (В представлении дед делает телепорт в какое - то место на станции при этом он должен визуально появляться выходя из - под земли совместно со спрайтом).
При движении издает специфический саунд своего перемещения. (если останавливается, звук тоже заканчивается)
     */

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Scp106Component, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<Scp106Component> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || !args.HitEntities.Any())
            return;

        foreach (var entity in args.HitEntities)
        {
            if (entity == ent.Owner)
                return;

            SendToBackrooms(entity);
        }
    }

    public virtual async void SendToBackrooms(EntityUid target) {}
}
