using UnityEngine;
using UnityEngine.UI;

public class TemperatureOverlay : MonoBehaviour
{
    [Header("Overlay Settings")]
    public Image overlayImage;
    public Color overlayColor = new Color(1f, 1f, 0f, 0.5f); // Yellow with 50% transparency
    public float maxOpacity = 0.3f;
    public float minTemperatureToShow = 0.1f; // Show overlay when temperature is above this
    public float maxTemperatureForFullOpacity = 1f; // Full opacity at this temperature

    private void Start()
    {
        // Initialize overlay as hidden
        if (overlayImage != null)
        {
            overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
            overlayImage.enabled = false;
        }
    }

    private void Update()
    {
        if (overlayImage == null)
            return;

        float CurentTempeparutePercentage = TemperatureManager.Instance.CurrentTemperatureLevel / TemperatureManager.Instance.MaxTemperature;
        
        // Show overlay when temperature is above minimum threshold
        if (CurentTempeparutePercentage > minTemperatureToShow)
        {
            // Calculate opacity based on temperature
            // Linear interpolation from minTemperatureToShow (0 opacity) to maxTemperatureForFullOpacity (full opacity)
            float temperatureRange = maxTemperatureForFullOpacity - minTemperatureToShow;
            float normalizedTemp = Mathf.Clamp01((CurentTempeparutePercentage - minTemperatureToShow) / temperatureRange);
            
            // Set overlay color with calculated opacity
            float opacity = maxOpacity * normalizedTemp;
            overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, opacity);
            overlayImage.enabled = true;
        }
        else
        {
            // Hide overlay when temperature is low
            overlayImage.enabled = false;
            overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
        }
    }
}

