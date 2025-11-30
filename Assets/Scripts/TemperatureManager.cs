using UnityEngine;

public class TemperatureManager : MonoBehaviour
{
    private static float temperatureLevel = 0f;
    private static float maxTemperature = 100f;
    private static float increaseRate = 10f; // temperature increase per second when window is closed

    public static float TemperatureLevel => temperatureLevel;
    public static float MaxTemperature => maxTemperature;
    public static float TemperaturePercentage => temperatureLevel / maxTemperature;

    [Header("Temperature Settings")]
    public float maxTemp = 100f;
    public float increaseRatePerSecond = 10f;

    private void Awake()
    {
        maxTemperature = maxTemp;
        increaseRate = increaseRatePerSecond;
        temperatureLevel = 0f;
    }

    public static void SetTemperature(float value)
    {
        temperatureLevel = Mathf.Clamp(value, 0f, maxTemperature);
    }

    public static void IncreaseTemperature(float deltaTime)
    {
        temperatureLevel = Mathf.Clamp(temperatureLevel + (increaseRate * deltaTime), 0f, maxTemperature);
    }

    public static void DecreaseTemperature(float amount)
    {
        temperatureLevel = Mathf.Clamp(temperatureLevel - amount, 0f, maxTemperature);
    }
}

