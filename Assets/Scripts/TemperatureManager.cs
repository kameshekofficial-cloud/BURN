using UnityEngine;

public class TemperatureManager : MonoBehaviour
{
    private static TemperatureManager _instance;
    public static TemperatureManager Instance => _instance;
    public float MaxTemperature => maxTemperature;
    [SerializeField] private float maxTemperature = 100f;
    [SerializeField] private float timeToMaxTemperature = 3f;
    [SerializeField] private float startingTemperature = 0f; // Starting temperature level (0 to maxTemperature)
    private float increaseRate => maxTemperature/timeToMaxTemperature; // temperature increase per second when window is closed
    public float CurrentTemperatureLevel { get; private set; } = 0f;

    private void Awake()
    {
        _instance = this;

        // Initialize with starting temperature (clamped to valid range)
        CurrentTemperatureLevel = Mathf.Clamp(startingTemperature, 0f, maxTemperature);
    }

    public void SetTemperature(float value)
    {
        CurrentTemperatureLevel = Mathf.Clamp(value, 0f, maxTemperature);
    }

    public void IncreaseTemperature(float deltaTime)
    {
        CurrentTemperatureLevel = Mathf.Clamp(CurrentTemperatureLevel + (increaseRate * deltaTime), 0f, maxTemperature);
    }

    public void DecreaseTemperature(float amount)
    {
        CurrentTemperatureLevel = Mathf.Clamp(CurrentTemperatureLevel - amount, 0f, maxTemperature);
    }
}

