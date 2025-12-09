public class StealPointsAbility : Ability
{
    public override void Execute(GameManager gm, bool isHostPlayer, int value)
    {
        gm.ModifyScore(!isHostPlayer, -value);
        gm.ModifyScore(isHostPlayer, value);
    }
}
