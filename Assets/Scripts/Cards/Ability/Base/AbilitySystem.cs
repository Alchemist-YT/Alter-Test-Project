
public abstract class Ability
{
    public abstract void Execute(GameManager gm, bool isHostPlayer, int value);
}

public static class AbilityFactory
{
    public static Ability GetAbility(string type)
    {
        switch (type)
        {
            case GameKeys.AbilityKeys.GainPoints: return new GainPointsAbility();
            case GameKeys.AbilityKeys.StealPoints: return new StealPointsAbility();
            default:
                return new GainPointsAbility();
        }
    }
}