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
        character.OnDamageTaken += UpdateBarOnDamageReceived;
    }

    private void Start()
    {
        hpBar.InitialSetup(character.GetCurrentHP());
        _characterIcon = character.GetCharacterIcon();
        characterMenuIcon.sprite = _characterIcon;

        Debug.Log($"{name}: UI setup for character {character.name}");
    }

    private void OnDisable()
    {
        character.OnDamageTaken -= UpdateBarOnDamageReceived;
    }

    public void UpdateBarOnDamageReceived(int damageReceived)
    {
        hpBar.UpdateBarOnDamageReceived((float)damageReceived);
    }
}
