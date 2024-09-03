using Content.Shared._Scp.Scp939;

namespace Content.Server._Scp.Scp939;

public sealed partial class Scp939System
{
    private void InitializeActions()
    {
        SubscribeLocalEvent<Scp939Component, Scp939SleepAction>(OnSleepAction);
        SubscribeLocalEvent<Scp939Component, Scp939GasAction>(OnGasAction);
    }

    private void OnSleepAction(Entity<Scp939Component> ent, ref Scp939SleepAction args)
    {
        args.Handled = _sleepingSystem.TrySleeping(ent.Owner);
    }

    private void OnGasAction(Entity<Scp939Component> ent, ref Scp939GasAction args)
    {
        _smokeSystem.StartSmoke(ent, ent.Comp.SmokeSolution!, ent.Comp.SmokeDuration, ent.Comp.SmokeSpread);
        args.Handled = true;
    }

    
}
