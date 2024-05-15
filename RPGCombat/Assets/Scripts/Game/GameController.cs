using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private Character[] characters;
    [SerializeField] private GridCell[] cells;
    [SerializeField] private int gridWidth = 6;
    [SerializeField] private int gridHeight = 4;

    bool _isGameOver = false;
    private int _turnCounter = 0;
    private int _pveEnemiesLeft;
    private Character _activeCharacter;
    private Dictionary<Vector2, GridCell> _cellsByPosition = new Dictionary<Vector2, GridCell>();

    private void OnEnable()
    {
        InitialSetup();
    }

    private void OnDisable()
    {
        ActionIcon.IconClicked -= ExecuteActionOnTarget;
    }

    private void Start()
    {
        StartCoroutine(PlayGame());
    }

    private void InitialSetup()
    {
        InitializeGrid();
        RandomizeCharacterPositions();
        ActionIcon.IconClicked += ExecuteActionOnTarget;
    }

    private void InitializeGrid()
    {
        foreach (var cell in cells)
        {
            _cellsByPosition.Add(cell.GetGridPosition(), cell);
            Debug.Log($"{name}: cell in position {cell.GetGridPosition()} was initialized.");
        }
    }

    // Assign random positions
    private void RandomizeCharacterPositions()
    {
        foreach (var character in characters)
        {
            if (!character.IsPlayer())
                _pveEnemiesLeft++;

            MoveCharacterToRandomPosition(character);
        }
    }

    private void MoveCharacterToRandomPosition(Character character)
    {
        bool wasCharacterAssigned = false;
        int xCoord;
        int yCoord;

        while (!wasCharacterAssigned)
        {
            xCoord = Random.Range(0, gridWidth);
            yCoord = Random.Range(0, gridHeight);

            Debug.Log($"{name}: Random position obtained ({xCoord},{yCoord}) for character {character.name}");

            Vector2 position = new(xCoord, yCoord);
            wasCharacterAssigned = WasCharacterAssignedToCell(character, position);
        }
    }

    // This method attempt to assign the character to the random grid cell
    private bool WasCharacterAssignedToCell(Character character, Vector2 position)
    {
        if (_cellsByPosition.TryGetValue(position, out var cell) && !cell.IsOccupied())
        {
            cell.SetIsOccupied(true);
            cell.SetOccupyingCharacter(character);
            character.MoveToGridCell(cell);

            Debug.Log($"{name}: Character {character.name} was moved to cell ({position.x},{position.y}).");

            return true;
        }

        return false;
    }

    // Play game
    private IEnumerator PlayGame()
    {
        Debug.Log($"{name}: Starting game.");

        while (!_isGameOver)
        {
            foreach (var character in characters)
            {
                Debug.Log($"{name}: Starting {character.name}'s turn.");
                // Wait until player turn is over
                yield return CharacterPlayTurn(character);
                Debug.Log($"{name}: Finished {character.name}'s turn.");
            }

            _turnCounter++;
        }

        yield return null;
    }

    private IEnumerator CharacterPlayTurn(Character character)
    {
        _activeCharacter = character;
        // Turn on UI and reset character's speed
        character.ToggleIsCharacterActive(true);

        // Wait for character to play its turn
        if (character.IsPlayer())
            yield return StartCoroutine(PlayerTurn());
        else
            yield return StartCoroutine(EnemyTurn());

        // Turn off UI before moving on to the next character
        character.ToggleIsCharacterActive(false);
    }

    private IEnumerator PlayerTurn()
    {
        Debug.Log($"{name}: Player detected, wait for move.");
        // Wait for player to move 
        while (_activeCharacter.CanCharacterMove())
            yield return new WaitForEndOfFrame();

        // After moving, check range to attack
        CheckActionAreas();

        // Get player and update UI
        /* 
         * Player activePlayer = (Player) _activeCharacter;
        * activePlayer.UpdateIcons(); 
        */
        _activeCharacter.UpdateIcons();

        // If at least one attack array has targets
        if(_activeCharacter.HasTargets())
        {
            // Wait for action event (to heal or attack)
            while (!_activeCharacter.HasCharacterAttackedThisTurn())
                yield return new WaitForEndOfFrame();
        }


        Debug.Log($"{_activeCharacter.name}: Turn finished.");

        yield return null;
    }

    private IEnumerator EnemyTurn()
    {
        Debug.Log($"{name}: Enemy detected, will move automatically.");
        int xCoord;
        int yCoord;

        while (_activeCharacter.CanCharacterMove())
        {
            int enemySpeed = _activeCharacter.GetSpeed();
            xCoord = Random.Range(-enemySpeed, enemySpeed + 1);
            yCoord = Random.Range(-enemySpeed, enemySpeed + 1);

            if (xCoord == 0 && yCoord == 0)
                continue;

            Debug.Log($"{_activeCharacter.name}: try moving ({xCoord},{yCoord})");

            MoveCharacterToPosition(new Vector2(xCoord, yCoord));
        }

        // Check attack areas
        CheckActionAreas();

        // If at least one attack array has targets
        if (_activeCharacter.HasTargets())
        {
            // Choose random and attack
            ActionTarget target = _activeCharacter.GetRandomTarget();
            ExecuteActionOnTarget(target);
            _activeCharacter.SetHasCharacterAttacked(true);

            // Check if player dead
            if (!target.GetTarget().IsAlive())
            {
                _isGameOver = true;
            }
        }

        yield return null;
    }

    // Receive input and move
    public void MoveCharacterToPosition(Vector2 position)
    {
        Debug.Log($"{name}: input received for {_activeCharacter.name}, value is {position}.");
        // Early exit if character has no movements left
        if (_activeCharacter.GetCurrentMovementsLeft() <= 0)
            return;

        Vector2 nextPosition = _activeCharacter.GetCurrentPosition() + position;

        // If cell exists and isn't occupied, move. Else, consume move attempt.
        if (_cellsByPosition.TryGetValue(nextPosition, out var cell)
                && !cell.IsOccupied())
            _activeCharacter.MoveToGridCell(cell);
        else
            _activeCharacter.ConsumeMoveAttempt();
    }

    // Check attack areas after movement
    private void CheckActionAreas()
    {
        // If isPlayer, check heal range
        if (_activeCharacter.IsPlayer())
            CheckRangeAndAddTarget(_activeCharacter.GetHealRange(), true, false);

        // Check melee range
        CheckRangeAndAddTarget(_activeCharacter.GetMeleeRange(), false, true);

        // If character has ranged attack
        if (_activeCharacter.HasRangedAttack())
            CheckRangeAndAddTarget(_activeCharacter.GetRangedRange(), false, false);
    }

    private void CheckRangeAndAddTarget(int range, bool isHealTarget, bool isMeleeTarget)
    {
        // If it's a self healing character checking for heal targets, add themself and early exit
        if(isHealTarget && _activeCharacter.IsPlayer() && _activeCharacter.GetHealRange() == 0)
        {
            ActionTarget target = new(_activeCharacter);
            _activeCharacter.AddTarget(target);
            return;
        } 

        // Loop through the grids using the player's position as center
        Vector2 initialPosition = _activeCharacter.GetCurrentPosition();
        int characterX = (int) initialPosition.x;
        int characterY = (int) initialPosition.y;

        for (int i = (characterX - range); i <= (characterX + range); i++)
        {
            for (int j = (characterY - range); j <= (characterY + range); j++)
            {
                if (_cellsByPosition.TryGetValue(new(i, j), out var cell)
                        && cell.IsOccupied()
                        && CheckIfTargetIsValidForAction(cell, isHealTarget, out var targetCharacter))
                {
                    ActionTarget target = new(targetCharacter, isHealTarget, isMeleeTarget);
                    _activeCharacter.AddTarget(target);
                }
            }
        }
    }

    /*
     * This method checks if we found a valid target for the given action.
     * Scenarios:
     *  - If it's a healing action, check the target is a player. 
     *    Active character is a player and has healRange > 0 for this scenario.
     *  - If it's a character attacking themself, we don't want to allow this.
     *  - If we're in PVE stage (_pveEnemiesLeft > 0), we only want attacks between a player and an enemy.
     *  - If there are no enemies left then we're in PVP stage, we don't need to check for character types.
     */
    private bool CheckIfTargetIsValidForAction(GridCell cell, bool isHealAction, out Character character)
    {
        // Get the character from the cell and assign the out
        Character target = cell.GetOccupyingCharacter();
        character = target;

        if (isHealAction)
        {
            return target.IsPlayer();
        } else if(_activeCharacter == target)
        {
            // A character can heal themself but not attack
            return false;
        }
        else if (_pveEnemiesLeft > 0)
        {
            // If it's still PVE stage, attacks can only happen between a player and an enemy
            return _activeCharacter.IsPlayer() != target.IsPlayer();
        }

        return true;
    }

    // Attack
    public void ExecuteActionOnTarget(ActionTarget target)
    {
        Character characterTarget = target.GetTarget();

        if (target.IsHealTarget())
            _activeCharacter.HealTarget((Player) characterTarget);
        else if (target.IsMeleeAttackTarget())
            _activeCharacter.MeleeAttack(characterTarget);
        else
            _activeCharacter.RangedAttack(characterTarget);
    }

    // Check win

    // Show endgame message


}
