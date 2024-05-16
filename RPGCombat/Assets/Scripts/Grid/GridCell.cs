using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField] private Vector2 gridPosition = Vector2.zero;

    private bool _isOccupied = false;
    private Character _occupyingCharacter;

    public Vector2 GetGridPosition()
    {
        return gridPosition;
    }

    public bool IsOccupied()
    {
        Debug.Log($"{name}: _isOccupied has value {_isOccupied}, position ({gridPosition})");
        return _isOccupied;
    }

    public void SetIsOccupied(bool isOccupied)
    {
        _isOccupied = isOccupied;
    }

    public Character GetOccupyingCharacter()
    {
        return _occupyingCharacter;
    }

    public void SetOccupyingCharacter(Character occupyingCharacter)
    {
        _occupyingCharacter = occupyingCharacter;
    }
}
