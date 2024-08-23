using Content.Server.Actions;
using Content.Shared._Scp.Scp049;
using Content.Shared._Scp.Scp999;
using Content.Shared.Inventory;
using Content.Shared.Zombies;
using Robust.Shared.Map;

namespace Content.Server._Scp.Scp049;

public sealed partial class Scp049System : SharedScp049System
{
    [Dependency] private readonly ActionsSystem _actionsSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Scp049Component, ComponentStartup>(OnStartup);
        InitializeActions();
    }

    private void OnStartup(Entity<Scp049Component> ent, ref ComponentStartup args)
    {
        foreach (var action in ent.Comp.Scp049Actions)
        {
            _actionsSystem.AddAction(ent, action);
        }

        var backPack = Spawn("ClothingBackpackScp049", MapCoordinates.Nullspace);
        _inventorySystem.TryEquip(ent, backPack, "back", true, true);
    }

    private ZombieComponent BuildZombieComponent()
    {
        var zombieComponent = new ZombieComponent
        {
            SkinColor = Color.WhiteSmoke,
            EyeColor = Color.Red,
            StatusIcon = Scp049MinionComponent.Icon
        };

        return zombieComponent;
    }
}
