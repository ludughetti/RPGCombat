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
        character.OnDamageTaken += UpdateBarOnHealthChange;

        if (character.IsPlayer())
            character.OnHealReceived += UpdateBarOnHealthChange;
    }

    private void OnDisable()
    {
        character.OnDamageTaken -= UpdateBarOnHealthChange;

        if (character.IsPlayer())
            character.OnHealReceived -= UpdateBarOnHealthChange;
    }

    private void Start()
    {
        hpBar.InitialSetup(character.GetCurrentHP());
        _characterIcon = character.GetCharacterIcon();
        characterMenuIcon.sprite = _characterIcon;

        Debug.Log($"{name}: UI setup for character {character.name}");
    }

    // damageReceived is not used aside from logging
    public void UpdateBarOnHealthChange()
    {
        Debug.Log($"{name}: A health change was received. HP Bar will be updated.");
        hpBar.UpdateBarOnDamageReceived((float) character.GetCurrentHP());
    }
}
