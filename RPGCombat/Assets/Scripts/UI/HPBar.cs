using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private float sliderSpeed = 50f;
    [SerializeField] private Slider sliderBar;

    private float _maxValue = 0f;
    private float _currentValue = 0f;
    private float _barVelocity = 0f;

    private void Update()
    {
        float sliderValue = Mathf.SmoothDamp(sliderBar.value, _currentValue, ref _barVelocity, sliderSpeed * Time.deltaTime);
        sliderBar.value = sliderValue;
    }

    public void InitialSetup(int maxHP)
    {
        _maxValue = (float) maxHP;
        _currentValue = _maxValue;
        sliderBar.maxValue = _maxValue;
        sliderBar.value = _maxValue;

        Debug.Log($"{name}: initialized with hp value {_currentValue}");
    }

    public void UpdateBarOnDamageReceived(float currentHP)
    {
        Debug.Log($"{name}: Updating HP bar, setting _currentValue to {currentHP}");
        _currentValue = currentHP;
    }
}
