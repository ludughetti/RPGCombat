using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private GameController controller;

    private float _cooldown = 0f;

    private void Update()
    {
        if(_cooldown > 0f)
            _cooldown -= Time.deltaTime;
    }

    public void MovePlayer(InputAction.CallbackContext context)
    {
        if(context.performed && _cooldown <= 0f)
        {
            _cooldown = 1f;
            Vector2 position = context.ReadValue<Vector2>();
            controller.MoveCharacterToPosition(new Vector2(-position.y, position.x).normalized);
        }
    }
}
