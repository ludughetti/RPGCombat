using UnityEngine;

public class Player : Character
{
    [SerializeField] private PlayerUIController playerUIController;

    public override int GetHealRange()
    {
        return healRange;
    }

    public override void ReceiveHeal(int healAmount)
    {
        int finalHPAmount = _currentHP + healAmount;
        if (finalHPAmount >= hp)
            _currentHP = hp;
        else
            _currentHP = finalHPAmount;

        OnHealReceived.Invoke();
    }

    public override int GetHealAmount()
    {
        return healAmount;
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
