using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private List<Character> characters;
    [SerializeField] private EndgameScreen endgameScreen;
    [SerializeField] private GridCell[] cells;
    [SerializeField] private int gridWidth = 6;
    [SerializeField] private int gridHeight = 4;
    [SerializeField] private Transform cemetery;

    bool _isGameOver = false;
    private int _turnCounter = 0;
    private int _pveEnemiesLeft;
    private Character _activeCharacter;
    private Dictionary<Vector2, GridCell> _cellsByPosition = new ();
    private Character _characterToRemove;

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
        // We cap FPS at 60 
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

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
            _turnCounter++;

            for (int i = 0; i < characters.Count; i++) {
                Debug.Log($"Status: characters.Count is {characters.Count}, index is {i}");
                if (characters.Count < i)
                {
                    Debug.Log($"{name}: Character is dead and will be removed. Turn skipped.");
                    continue;
                }

                // FYI: An error might be thrown here due to an Editor bug if the characters array
                // is open in the editor. Minimizing the array in the Editor UI will fix it.
                _activeCharacter = characters[i];
                Debug.Log($"{name}: Starting {_activeCharacter.name}'s turn.");

                // Wait until character turn is over
                yield return CharacterPlayTurn();
                Debug.Log($"{name}: Finished {_activeCharacter.name}'s turn.");

                // If a character died, we need to remove it and rearrange the loop index
                RemoveDeadCharacter(i, out i);

                // Break loop if game over was triggered
                if (_isGameOver)
                    break;
            }
        }

        if (_pveEnemiesLeft > 0)
            endgameScreen.ShowEndgameScreen(false, _turnCounter, "");
        else
            endgameScreen.ShowEndgameScreen(true, _turnCounter, characters[0].name);

        yield return null;
    }

    private void RemoveDeadCharacter(int index, out int updatedIndex)
    {
        if (_characterToRemove != null)
        {
            CheckIfEndgameWasTriggered();
            int deadCharacterIndex = characters.IndexOf(_characterToRemove);

            // Should never happen but it's a security check in case IndexOf returns not found
            if(deadCharacterIndex >= 0)
            {
                // Move to cemetery
                _characterToRemove.transform.SetParent(cemetery);
                _characterToRemove.transform.localPosition = Vector2.zero;

                // Hide
                _characterToRemove.HideDeadCharacter();

                Debug.Log($"Remove character at index {deadCharacterIndex}");
                characters.RemoveAt(deadCharacterIndex);
            }

            // Reset _characterToRemove and update loop index
            _characterToRemove = null;
            //index--;
        }

        updatedIndex = index;
    }

    private IEnumerator CharacterPlayTurn()
    {
        // Turn on UI and reset character's speed
        _activeCharacter.ToggleIsCharacterActive(true);

        // Wait for character to play its turn
        if (_activeCharacter.IsPlayer())
            yield return StartCoroutine(PlayerTurn());
        else
            yield return StartCoroutine(EnemyTurn());

        // Turn off UI before moving on to the next character
        _activeCharacter.ToggleIsCharacterActive(false);
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
        _activeCharacter.UpdateIcons();

        // If at least one attack array has targets
        if(_activeCharacter.HasTargets())
        {
            // Wait for action event (to heal or attack)
            while (!_activeCharacter.HasCharacterAttackedThisTurn())
                yield return new WaitForEndOfFrame();
        }

        // Refresh UI
        _activeCharacter.ResetUI();

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

        // Wait so player can see the movement
        yield return new WaitForSeconds(1);

        // Check attack areas
        CheckActionAreas();

        // If at least one attack array has targets
        if (_activeCharacter.HasTargets())
        {
            // Choose random and attack
            ActionTarget target = _activeCharacter.GetRandomTarget();
            ExecuteActionOnTarget(target);
            _activeCharacter.SetHasCharacterAttacked(true);
        }

        // Wait so player can see the attack
        yield return new WaitForSeconds(1);

        yield return null;
    }

    public void ReceiveInputAndMove(Vector2 position)
    {
        if (_activeCharacter.IsPlayer())
            MoveCharacterToPosition(position);
    }

    // Move
    public void MoveCharacterToPosition(Vector2 position)
    {
        Debug.Log($"{name}: input received for {_activeCharacter.name}, value is {position}, current position is {_activeCharacter.GetCurrentPosition()}.");
        // Early exit if it's AI or if the character has no movements left
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
        Debug.Log($"CheckRangeAndAddTarget called for {_activeCharacter.name}. isHealAction: {isHealTarget}, isMeleeTarget: {isMeleeTarget}");

        // If it's a self healing character checking for heal targets, add themself and early exit
        if (isHealTarget && _activeCharacter.IsPlayer() && _activeCharacter.GetHealRange() == 0)
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
        Debug.Log($"CheckIfTargetIsValidForAction called. Position: {cell.GetGridPosition()}, isHealAction: {isHealAction}");
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

    public void ExecuteActionOnTarget(ActionTarget target)
    {
        Character characterTarget = target.GetTarget();
        Debug.Log($"{_activeCharacter.name} is executing action on {characterTarget.name}");

        if (target.IsHealTarget() && characterTarget.IsPlayer())
            characterTarget.ReceiveHeal(_activeCharacter.GetHealAmount());
        else if (target.IsMeleeAttackTarget())
            characterTarget.TakeDamage(_activeCharacter.GetMeleeDamage());
        else
            characterTarget.TakeDamage(_activeCharacter.GetRangedDamage());

        _activeCharacter.SetHasCharacterAttacked(true);

        Debug.Log($"{characterTarget.name}'s hp is {characterTarget.GetCurrentHP()}");

        // If target died, flag it to be removed from the active characters list
        if (!characterTarget.IsAlive())
        {
            Debug.Log($"{name}: {characterTarget.name} was killed.");
            _characterToRemove = characterTarget;

            if(!characterTarget.IsPlayer())
                _pveEnemiesLeft--;
        }
    }

    /*
     * If attacked target was killed we check if endgame was triggered.
     * Scenarios where endgame is triggered:
     *  - If a player was killed and we're still in PVE stage, or 
     *  - We're in PVP stage and we have only one player standing
     *    (We check characters.Count == 2 because the dead player hasn't been removed yet)
     */
    private void CheckIfEndgameWasTriggered()
    {
        if ((_characterToRemove.IsPlayer() && _pveEnemiesLeft > 0)
                || characters.Count == 2)
            _isGameOver = true;
    }
}
