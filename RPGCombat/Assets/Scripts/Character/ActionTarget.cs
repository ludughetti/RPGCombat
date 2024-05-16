using UnityEngine;

public class ActionTarget
{
    private Character _target;
    private bool _isHealTarget;
    private bool _isMeleeAttackTarget;

    // Exclusive constructor for characters with self healing only
    public ActionTarget(Character target)
    {
        this._target = target;
        this._isHealTarget = true;
        this._isMeleeAttackTarget = false;
    }

    /*
     * Scenarios:
     *  - If it's a heal target -> isHealTarget = true. 
     *    We set isMeleeAttackTarget to false but it has no effect.
     *  - If it's a melee target -> isHealTarget = false && isMeleeAttackTarget = true.
     *  - If it's a ranged target -> isHealTarget = false && isMeleeAttackTarget = false.
     */
    public ActionTarget(Character target, bool isHealTarget, bool isMeleeAttackTarget)
    {
        this._target = target;
        this._isHealTarget = isHealTarget;
        this._isMeleeAttackTarget = isMeleeAttackTarget;
    }

    public bool IsHealTarget()
    {
        return _isHealTarget;
    }

    public bool IsMeleeAttackTarget()
    {
        return !_isHealTarget && _isMeleeAttackTarget;
    }

    public Character GetTarget()
    {
        return _target;
    }
}
