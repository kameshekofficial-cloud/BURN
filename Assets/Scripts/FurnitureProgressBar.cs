using UnityEngine;
using UnityEngine.UI;

public class FurnitureProgressBar : MonoBehaviour
{
    [Header("UI")]
    public Slider progressSlider;

    private void Update()
    {
        // Find the furniture that has an active request
        var activeFurniture = FurnitureManager.GetActiveFurnitureRequest();
        
        if (activeFurniture == null)
        {
            // No active request (no ghost), set bar to maximum
            progressSlider.value = 1f;
            return;
        }

        // Get progress from the active furniture
        // Progress starts at 1.0 (maximum) when request begins
        // Stays at 1.0 until ghost appears
        // Decreases as ghost moves to target (1.0 -> 0.5)
        // Continues decreasing as furniture moves to ghost (0.5 -> 0.0)
        // Reaches 0.0 when furniture reaches ghost
        float progress = activeFurniture.GetProgressToGhost();
        
        // Reverse the progress: 0.0 becomes 1.0, 1.0 becomes 0.0
        // So bar starts at minimum and increases to maximum as progress is made
        progressSlider.value = Mathf.Clamp01(1f - progress);
    }
}

