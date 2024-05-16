using System;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private int hp;
    [SerializeField] private int speed;
    [SerializeField] private int moveAttemptsPerTurn = 5;
    [SerializeField] protected int healAmount = 0;
    [SerializeField] protected int healRange = 0;
    [SerializeField] private int meleeDamage;
    [SerializeField] private int meleeRange = 1;
    [SerializeField] private int rangedDamage;
    [SerializeField] private int rangedRange;
    [SerializeField] private bool hasRangedAttack = false;
    [SerializeField] private bool isPlayer = false;
    [SerializeField] protected SpriteRenderer iconRenderer;
    [SerializeField] private GameObject activeCharacterIcon;

    public Action<int> OnDamageTaken = delegate { };

    protected int _currentHP;
    private bool _hasCharacterAttacked = false;
    private GridCell _currentCell;
    private int _currentMovements;
    private int _currentMoveAttempts;
    private List<ActionTarget> _targets = new();

    private void Awake()
    {
        _currentHP = hp;
        Debug.Log($"{name}: awakened, _currentHP is {_currentHP}");
    }

    private void OnEnable()
    {
        OnDamageTaken += TakeDamage;
    }

    private void OnDisable()
    {
        OnDamageTaken -= TakeDamage;
    }

    public Sprite GetCharacterIcon()
    {
        return iconRenderer.sprite;
    }

    public int GetCurrentHP()
    {
        return _currentHP;
    }

    public int GetSpeed()
    {
        return speed;
    }

    public int GetCurrentMovementsLeft()
    {
        return _currentMovements;
    }

    public int GetCurrentMoveAttempts()
    {
        return _currentMoveAttempts;
    }

    /*
     * Since characters can be landlocked we add move attempts. 
     * If the character runs out of _currentMovements (based on speed)
     * or all _currentMoveAttempts (based on moveAttemptsPerTurn) are consumed, 
     * the character will enter combat stage.
    */
    public bool CanCharacterMove()
    {
        return GetCurrentMovementsLeft() > 0 && GetCurrentMoveAttempts() > 0;
    }

    public void ConsumeMoveAttempt()
    {
        _currentMoveAttempts--;
    }

    public virtual int GetHealRange()
    {
        Debug.Log("Not implemented for characters");
        return 0;
    }

    public virtual void HealTarget(Player target)
    {
        Debug.Log("Not implemented for characters");
    }

    public Vector2 GetCurrentPosition()
    {
        return _currentCell.GetGridPosition();
    }

    public bool IsPlayer()
    {
        return isPlayer;
    }

    public bool IsAlive()
    {
        return _currentHP >= 0;
    }

    public int GetMeleeRange()
    {
        return meleeRange;
    }

    public void MeleeAttack(Character target)
    {
        target.OnDamageTaken.Invoke(meleeDamage);
    }

    public bool HasRangedAttack()
    {
        return hasRangedAttack;
    }

    public int GetRangedRange()
    {
        return rangedRange;
    }

    public void RangedAttack(Character target)
    {
        if (hasRangedAttack)
            target.OnDamageTaken.Invoke(rangedDamage);
    }

    public void TakeDamage(int damage)
    {
        _currentHP -= damage;
    }

    [ContextMenu("InvokeHPDelegate")]
    private void TestDelegateHP()
    {
        OnDamageTaken.Invoke(5);
    }

    public void ToggleIsCharacterActive(bool isActive)
    {
        ShowActiveIcon(isActive);

        if (isActive)
        {
            _currentMovements = speed;
            _currentMoveAttempts = moveAttemptsPerTurn;
            _hasCharacterAttacked = false;
        }
    }

    public void ShowActiveIcon(bool isCharacterActive)
    {
        activeCharacterIcon.SetActive(isCharacterActive);
    }

    public void MoveToGridCell(GridCell cell)
    {
        // Reset the previous cell's state
        if (_currentCell != null)
            _currentCell.SetIsOccupied(false);

        // Then move the character to the new cell
        transform.SetParent(cell.GetComponent<Transform>());
        transform.localPosition = Vector2.zero;
        _currentMovements--;
        cell.SetIsOccupied(true);
        cell.SetOccupyingCharacter(this);
        _currentCell = cell;
    }

    public void AddTarget(ActionTarget target)
    {
        _targets.Add(target);
    }

    public bool HasTargets()
    {
        return _targets.Count > 0;
    }

    public ActionTarget GetRandomTarget()
    {
        int index = UnityEngine.Random.Range(0, _targets.Count);
        return _targets[index];
    }

    public List<ActionTarget> GetActionTargets()
    {
        return _targets;
    }

    public bool HasCharacterAttackedThisTurn()
    {
        return _hasCharacterAttacked;
    }

    public void SetHasCharacterAttacked(bool hasCharacterAttacked)
    {
        _hasCharacterAttacked = hasCharacterAttacked;
    }

    public virtual void UpdateIcons()
    {
        Debug.Log("Not implemented for characters");
    }

    public virtual void ResetUI()
    {
        Debug.Log("Not implemented for characters");
    }
}
