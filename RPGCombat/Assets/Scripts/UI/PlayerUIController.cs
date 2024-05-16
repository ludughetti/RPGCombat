using System.Collections.Generic;
using UnityEngine;

public class PlayerUIController : CharacterUIController
{
    [SerializeField] private GameObject healMenu;
    [SerializeField] private GameObject meleeAttackMenu;
    [SerializeField] private GameObject rangedAttackMenu;
    [SerializeField] private List<ActionIcon> healTargetIcons;
    [SerializeField] private List<ActionIcon> meleeTargetIcons;
    [SerializeField] private List<ActionIcon> rangedTargetIcons;

    public void UpdateIcons()
    {
        List<ActionTarget> actionTargets = character.GetActionTargets();
        foreach (ActionTarget target in actionTargets)
        {
            if (target.IsHealTarget())
            {
                Debug.Log($"{name}: Heal target {target.GetTarget().name} processed");
                AssignTargetsToIcons(target, healTargetIcons);
            }
            else if (target.IsMeleeAttackTarget())
            {
                Debug.Log($"{name}: Melee attack target {target.GetTarget().name} processed");
                AssignTargetsToIcons(target, meleeTargetIcons);
            }   
            else
            {
                Debug.Log($"{name}: Ranged attack target {target.GetTarget().name} processed");
                AssignTargetsToIcons(target, rangedTargetIcons);
            }
        }

        // Show or hide target icons depending on whether each array has targets
        ShowHideTargetMenu(healTargetIcons, healMenu);
        ShowHideTargetMenu(meleeTargetIcons, meleeAttackMenu);
        ShowHideTargetMenu(rangedTargetIcons, rangedAttackMenu);
    }

    private void AssignTargetsToIcons(ActionTarget target, List<ActionIcon> actionIconsArray)
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

    private void ShowHideTargetMenu(List<ActionIcon> icons, GameObject menu)
    {
        if (icons.Count > 0)
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

        healTargetIcons.Clear();
        meleeTargetIcons.Clear();
        rangedTargetIcons.Clear();

        ShowHideTargetMenu(healTargetIcons, healMenu);
        ShowHideTargetMenu(meleeTargetIcons, meleeAttackMenu);
        ShowHideTargetMenu(rangedTargetIcons, rangedAttackMenu);
    }

    private void ResetOptions(List<ActionIcon> icons)
    {
        foreach (var icon in icons)
        {
            icon.ResetActionIcon();
        }

        icons.Clear();
    }
}
