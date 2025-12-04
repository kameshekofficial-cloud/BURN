using UnityEngine;
using UnityEngine.UI;

public class TemperatureProgressBar : MonoBehaviour
{
    [Header("UI")]
    public Slider progressSlider;

    private void Update()
    {
        // Update slider based on temperature level (reversed: 0 = max temp, 1 = min temp)
        float temperaturePercentage = TemperatureManager.Instance.CurrentTemperatureLevel / TemperatureManager.Instance.MaxTemperature;
        progressSlider.value = 1f - temperaturePercentage;
    }
}
