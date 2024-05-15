using UnityEngine;
using UnityEngine.UI;

public class CharacterUIController : MonoBehaviour
{
    [SerializeField] protected Character character;
    [SerializeField] private Image characterMenuIcon;
    [SerializeField] private HPBar hpBar;

    private Sprite _characterIcon;

    private void OnEnable()
    {
        hpBar.InitialSetup(character.GetCurrentHP());
        _characterIcon = character.GetCharacterIcon();
        characterMenuIcon.sprite = _characterIcon;

        character.OnDamageTaken += UpdateBarOnDamageReceived;
    }

    private void OnDisable()
    {
        character.OnDamageTaken -= UpdateBarOnDamageReceived;
    }

    public void UpdateBarOnDamageReceived(int damageReceived)
    {
        hpBar.UpdateBarOnDamageReceived((float)damageReceived);
    }

    // Handle UI input

    // Show heal menu

    // Receive Heal input

    // Show attack menus

    // Receive melee attack input

    // Receive ranged attack input
}
