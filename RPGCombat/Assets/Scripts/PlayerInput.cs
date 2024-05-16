using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private GameController controller;
    [SerializeField] private float cooldown = 0.25f;

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
            _cooldown = cooldown;
            Vector2 position = context.ReadValue<Vector2>();
            controller.ReceiveInputAndMove(new Vector2(-position.y, position.x).normalized);
        }
    }
}
