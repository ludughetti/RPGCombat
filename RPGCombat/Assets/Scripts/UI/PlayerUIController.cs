using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerUIController : CharacterUIController
{
    [SerializeField] private GameObject healMenu;
    [SerializeField] private GameObject meleeAttackMenu;
    [SerializeField] private GameObject rangedAttackMenu;
    [SerializeField] private List<ActionIcon> healTargetIcons;
    [SerializeField] private List<ActionIcon> meleeTargetIcons;
    [SerializeField] private List<ActionIcon> rangedTargetIcons;

    private int _healTargets = 0;
    private int _meleeTargets = 0;
    private int _rangedTargets = 0;

    public void UpdateIcons()
    {
        List<ActionTarget> actionTargets = character.GetActionTargets();
        foreach (ActionTarget target in actionTargets)
        {
            if (target.IsHealTarget())
            {
                Debug.Log($"{name}: Heal target {target.GetTarget().name} processed");
                _healTargets++;
                AssignTargets(target, healTargetIcons);
            }
            else if (target.IsMeleeAttackTarget())
            {
                Debug.Log($"{name}: Melee attack target {target.GetTarget().name} processed");
                _meleeTargets++;
                AssignTargets(target, meleeTargetIcons);
            }   
            else
            {
                Debug.Log($"{name}: Ranged attack target {target.GetTarget().name} processed");
                _rangedTargets++;
                AssignTargets(target, rangedTargetIcons);
            }
        }

        // Show or hide target icons depending on whether each array has targets
        Debug.Log($"{name}: _healTargets value is {_healTargets}");
        ShowHideTargetMenu(_healTargets, healMenu);
        Debug.Log($"{name}: _meleeTargets value is {_meleeTargets}");
        ShowHideTargetMenu(_meleeTargets, meleeAttackMenu);
        Debug.Log($"{name}: _rangedTargets value is {_rangedTargets}");
        ShowHideTargetMenu(_rangedTargets, rangedAttackMenu);
    }

    private void AssignTargets(ActionTarget target, List<ActionIcon> actionIconsArray)
    {
        foreach (var actionIcon in actionIconsArray)
        {
            if(!actionIcon.HasTarget())
            {
                actionIcon.SetTarget(target);
                break;
            }
        }
    }

    private void ShowHideTargetMenu(int count, GameObject menu)
    {
        if (count > 0)
            menu.SetActive(true);
        else
            menu.SetActive(false);
    }

    // Reset all lists and hide them in the UI
    public void ResetIcons()
    {
        ResetOptions(healTargetIcons);
        ResetOptions(meleeTargetIcons);
        ResetOptions(rangedTargetIcons);

        _healTargets = 0;
        _meleeTargets = 0;
        _rangedTargets = 0;

        ShowHideTargetMenu(_healTargets, healMenu);
        ShowHideTargetMenu(_meleeTargets, meleeAttackMenu);
        ShowHideTargetMenu(_rangedTargets, rangedAttackMenu);
    }

    private void ResetOptions(List<ActionIcon> icons)
    {
        foreach (var icon in icons)
        {
            icon.ResetActionIcon();
        }
    }
}
