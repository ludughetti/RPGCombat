using System;
using UnityEngine;
using UnityEngine.UI;

public class ActionIcon : MonoBehaviour
{
    [SerializeField] private Image icon;

    public static event Action<ActionTarget> IconClicked;

    private bool _hasTarget = false;
    private ActionTarget _target;

    public bool HasTarget()
    {
        return _hasTarget;
    }

    public void SetTarget(ActionTarget target)
    {
        _target = target;
        _hasTarget = true;
        icon.sprite = _target.GetTarget().GetCharacterIcon();
        gameObject.SetActive(true);
    }

    public void ResetActionIcon()
    {
        icon = null;
        _target = null;
        _hasTarget = false;
    }

    public ActionTarget GetTarget()
    {
        return _target;
    }

    public void OnIconClicked()
    {
        Debug.Log("OnIconClicked received");
        IconClicked?.Invoke(_target);
    }
}
