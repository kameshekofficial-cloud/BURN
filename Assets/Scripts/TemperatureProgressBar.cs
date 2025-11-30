using UnityEngine;
using UnityEngine.UI;

public class TemperatureProgressBar : MonoBehaviour
{
    [Header("UI")]
    public Slider progressSlider;

    private void Update()
    {
        // Update slider based on temperature level (0 = min temp, 1 = max temp)
        float temperaturePercentage = TemperatureManager.TemperaturePercentage;
        progressSlider.value = temperaturePercentage;
    }
}

