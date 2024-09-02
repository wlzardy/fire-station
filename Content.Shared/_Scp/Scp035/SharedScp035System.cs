using Content.Shared.Clothing;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

namespace Content.Shared._Scp.Scp035;

public abstract class SharedScp035System : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Mask
        SubscribeLocalEvent<Scp035MaskComponent, ClothingGotEquippedEvent>(OnMaskEquipped);
        SubscribeLocalEvent<Scp035MaskComponent, ClothingGotUnequippedEvent>(OnMaskUnequipped);
        SubscribeLocalEvent<Scp035MaskComponent, BeingEquippedAttemptEvent>(OnEquippeAttempt);

        // User
        SubscribeLocalEvent<Scp035MaskUserComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    #region Mask

    private void OnMaskEquipped(Entity<Scp035MaskComponent> ent, ref ClothingGotEquippedEvent args)
    {
        EnsureComp<UnremoveableComponent>(ent);

        ent.Comp.User = args.Wearer;
        Dirty(ent);

        var maskUserComponent = EnsureComp<Scp035MaskUserComponent>(args.Wearer);
        maskUserComponent.Mask = ent;
        Dirty(args.Wearer, maskUserComponent);
    }

    private void OnMaskUnequipped(Entity<Scp035MaskComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        ent.Comp.User = null;
        Dirty(ent);

        RemComp<Scp035MaskUserComponent>(args.Wearer);
    }

    private void OnEquippeAttempt(Entity<Scp035MaskComponent> ent, ref BeingEquippedAttemptEvent args)
    {
        if (!HasComp<HumanoidAppearanceComponent>(args.Equipee))
        {
            args.Cancel();

            _stun.TryParalyze(args.Equipee, TimeSpan.FromSeconds(10), true);

            if (_net.IsServer)
            {
                _popup.PopupEntity("Маска отвергает вас!", args.Equipee, args.Equipee, PopupType.LargeCaution);

                var impulse = _random.NextVector2() * 10000;
                _physics.ApplyLinearImpulse(args.Equipee, impulse);
            }
        }
    }

    #endregion


    #region User

    private void OnMobStateChanged(Entity<Scp035MaskUserComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (!ent.Comp.Mask.HasValue)
            return;

        var maskEntity = ent.Comp.Mask.Value;

        RemComp<UnremoveableComponent>(maskEntity);
        _containerSystem.TryRemoveFromContainer(maskEntity, true);
        _transform.AttachToGridOrMap(maskEntity);

        var ash = Spawn("Ash", Transform(maskEntity).Coordinates);
        _transform.AttachToGridOrMap(ash);

        QueueDel(ent);
    }

    #endregion
}
