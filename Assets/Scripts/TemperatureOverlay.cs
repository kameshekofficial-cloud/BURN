using UnityEngine;
using UnityEngine.UI;

public class TemperatureOverlay : MonoBehaviour
{
    [Header("Overlay Settings")]
    public Image overlayImage;
    public Color overlayColor = new Color(1f, 1f, 0f, 0.5f); // Yellow with 50% transparency
    public float minTemperatureToShow = 10f; // Show overlay when temperature is above this
    public float maxTemperatureForFullOpacity = 100f; // Full opacity at this temperature

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

        float currentTemperature = TemperatureManager.TemperatureLevel;
        
        // Show overlay when temperature is above minimum threshold
        if (currentTemperature > minTemperatureToShow)
        {
            // Calculate opacity based on temperature
            // Linear interpolation from minTemperatureToShow (0 opacity) to maxTemperatureForFullOpacity (full opacity)
            float temperatureRange = maxTemperatureForFullOpacity - minTemperatureToShow;
            float normalizedTemp = Mathf.Clamp01((currentTemperature - minTemperatureToShow) / temperatureRange);
            
            // Set overlay color with calculated opacity
            float opacity = overlayColor.a * normalizedTemp;
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

