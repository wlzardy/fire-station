using Content.Shared.Actions;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Scp.Scp049;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class Scp049Component : Component
{
    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> Minions = new();

    [DataField("actions")]
    public List<ProtoId<EntityPrototype>> Scp049Actions =
    [
        "ActionScp096Resurrect", "ActionScp096KillResurrected", "ActionScp096KillLeavingBeing", "ActionScp096SelfHeal",
    ];

    [DataField]
    public static ProtoId<FactionIconPrototype> Icon = "Scp049StatusIcon";
}
