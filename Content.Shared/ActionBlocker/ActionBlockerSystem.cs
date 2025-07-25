﻿using Content.Shared.Body.Events;
using Content.Shared.Emoting;
using Content.Shared.Hands;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Speech;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using JetBrains.Annotations;
using Robust.Shared.Containers;

namespace Content.Shared.ActionBlocker
{
    /// <summary>
    /// Utility methods to check if a specific entity is allowed to perform an action.
    /// </summary>
    [UsedImplicitly]
    public sealed class ActionBlockerSystem : EntitySystem
    {
        [Dependency] private readonly SharedContainerSystem _container = default!;

        private EntityQuery<ComplexInteractionComponent> _complexInteractionQuery;

        public override void Initialize()
        {
            base.Initialize();

            _complexInteractionQuery = GetEntityQuery<ComplexInteractionComponent>();

            SubscribeLocalEvent<InputMoverComponent, ComponentStartup>(OnMoverStartup);
        }

        private void OnMoverStartup(EntityUid uid, InputMoverComponent component, ComponentStartup args)
        {
            UpdateCanMove(uid, component);
        }

        public bool CanMove(EntityUid uid, InputMoverComponent? component = null)
        {
            return Resolve(uid, ref component, false) && component.CanMove;
        }

        public bool UpdateCanMove(EntityUid uid, InputMoverComponent? component = null)
        {
            if (!Resolve(uid, ref component, false))
                return false;

            var ev = new UpdateCanMoveEvent(uid);
            RaiseLocalEvent(uid, ev);

            if (component.CanMove == ev.Cancelled)
                Dirty(uid, component);

            component.CanMove = !ev.Cancelled;
            return !ev.Cancelled;
        }

        /// <summary>
        /// Checks if a given entity is able to do specific complex interactions.
        /// This is used to gate manipulation to general humanoids. If a mouse shouldn't be able to do something, then it's complex.
        /// </summary>
        public bool CanComplexInteract(EntityUid user)
        {
            return _complexInteractionQuery.HasComp(user);
        }

        /// <summary>
        ///     Raises an event directed at both the user and the target entity to check whether a user is capable of
        ///     interacting with this entity.
        /// </summary>
        /// <remarks>
        ///     If this is a generic interaction without a target (e.g., stop-drop-and-roll when burning), the target
        ///     may be null. Note that this is checked by <see cref="SharedInteractionSystem"/>. In the majority of
        ///     cases, systems that provide interactions will not need to check this themselves, though they may need to
        ///     check other blockers like <see cref="CanPickup(EntityUid)"/>
        /// </remarks>
        /// <returns></returns>
        public bool CanInteract(EntityUid user, EntityUid? target)
        {
            if (!CanConsciouslyPerformAction(user))
                return false;

            var ev = new InteractionAttemptEvent(user, target);
            RaiseLocalEvent(user, ref ev);

            if (ev.Cancelled)
                return false;

            if (target == null || target == user)
                return true;

            var targetEv = new GettingInteractedWithAttemptEvent(user, target);
            RaiseLocalEvent(target.Value, ref targetEv);

            return !targetEv.Cancelled;
        }

        //🌟Starlight🌟
        public bool CanInstrumentInteract(EntityUid user, EntityUid used, EntityUid? target)
        {
            var ev = new InteractionAttemptEvent(user, target);
            RaiseLocalEvent(used, ref ev);

            return !ev.Cancelled;
        }

        /// <summary>
        ///     Can a user utilize the entity that they are currently holding in their hands.
        /// </summary>>
        /// <remarks>
        ///     This event is automatically checked by <see cref="SharedInteractionSystem"/> for any interactions that
        ///     involve using a held entity. In the majority of cases, systems that provide interactions will not need
        ///     to check this themselves.
        /// </remarks>
        public bool CanUseHeldEntity(EntityUid user, EntityUid used)
        {
            var useEv = new UseAttemptEvent(user, used);
            RaiseLocalEvent(user, useEv);

            if (useEv.Cancelled)
                return false;

            var usedEv = new GettingUsedAttemptEvent(user);
            RaiseLocalEvent(used, usedEv);

            return !usedEv.Cancelled;
        }


        /// <summary>
        /// Whether a user conscious to perform an action.
        /// </summary>
        /// <remarks>
        /// This should be used when you want a much more permissive check than <see cref="CanInteract"/>
        /// </remarks>
        public bool CanConsciouslyPerformAction(EntityUid user)
        {
            var ev = new ConsciousAttemptEvent(user);
            RaiseLocalEvent(user, ref ev);

            return !ev.Cancelled;
        }

        public bool CanThrow(EntityUid user, EntityUid itemUid)
        {
            var ev = new ThrowAttemptEvent(user, itemUid);
            RaiseLocalEvent(user, ev);

            if (ev.Cancelled)
                return false;

            var itemEv = new ThrowItemAttemptEvent(user);
            RaiseLocalEvent(itemUid, ref itemEv);

            return !itemEv.Cancelled;
        }

        public bool CanSpeak(EntityUid uid)
        {
            // This one is used as broadcast
            var ev = new SpeakAttemptEvent(uid);
            RaiseLocalEvent(uid, ev, true);

            return !ev.Cancelled;
        }

        public bool CanDrop(EntityUid uid)
        {
            var ev = new DropAttemptEvent();
            RaiseLocalEvent(uid, ev);

            return !ev.Cancelled;
        }

        public bool CanPickup(EntityUid user, EntityUid item)
        {
            var userEv = new PickupAttemptEvent(user, item);
            RaiseLocalEvent(user, userEv);

            if (userEv.Cancelled)
                return false;

            var itemEv = new GettingPickedUpAttemptEvent(user, item);
            RaiseLocalEvent(item, itemEv);

            return !itemEv.Cancelled;
        }

        public bool CanEmote(EntityUid uid)
        {
            // This one is used as broadcast
            var ev = new EmoteAttemptEvent(uid);
            RaiseLocalEvent(uid, ev, true);

            return !ev.Cancelled;
        }

        public bool CanAttack(EntityUid uid, EntityUid? target = null, Entity<MeleeWeaponComponent>? weapon = null, bool disarm = false)
        {
            // If target is in a container can we attack
            if (target != null && _container.IsEntityInContainer(target.Value))
            {
                return false;
            }

            _container.TryGetOuterContainer(uid, Transform(uid), out var outerContainer);

            // If we're in a container can we attack the target.
            if (target != null && target != outerContainer?.Owner && _container.IsEntityInContainer(uid))
            {
                var containerEv = new CanAttackFromContainerEvent(uid, target);
                RaiseLocalEvent(uid, containerEv);
                if (!containerEv.CanAttack)
                    return false;
            }

            var ev = new AttackAttemptEvent(uid, target, weapon, disarm);
            RaiseLocalEvent(uid, ev);

            if (ev.Cancelled)
                return false;

            if (target == null)
                return true;

            var tev = new GettingAttackedAttemptEvent(uid, weapon, disarm);
            RaiseLocalEvent(target.Value, ref tev);
            return !tev.Cancelled;
        }

        public bool CanChangeDirection(EntityUid uid)
        {
            var ev = new ChangeDirectionAttemptEvent(uid);
            RaiseLocalEvent(uid, ev);

            return !ev.Cancelled;
        }

        public bool CanShiver(EntityUid uid)
        {
            var ev = new ShiverAttemptEvent(uid);
            RaiseLocalEvent(uid, ref ev);

            return !ev.Cancelled;
        }

        public bool CanSweat(EntityUid uid)
        {
            var ev = new SweatAttemptEvent(uid);
            RaiseLocalEvent(uid, ref ev);

            return !ev.Cancelled;
        }
    }
}
