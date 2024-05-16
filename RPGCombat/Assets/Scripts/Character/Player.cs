using UnityEngine;

public class Player : Character
{
    [SerializeField] private PlayerUIController playerUIController;

    public override int GetHealRange()
    {
        return healRange;
    }

    public override void HealTarget(Player target)
    {
        target._currentHP += healAmount;
    }

    public override void UpdateIcons()
    {
        playerUIController.UpdateIcons();
    }

    public override void ResetUI()
    {
        playerUIController.ResetIcons();
    }
}
