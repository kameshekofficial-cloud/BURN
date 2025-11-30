using UnityEngine;
using UnityEngine.UI;

public class TrashProgressBar : MonoBehaviour
{
    [Header("UI")]
    public Slider progressSlider;

    [Header("Trash Settings")]
    public int maxTrashOnScreen = 20;   // tweak this based on your game

    private void Update()
    {
        int currentTrash = TrashManager.TrashCount;

        // Clamp currentTrash just in case it goes over the expected max
        currentTrash = Mathf.Clamp(currentTrash, 0, maxTrashOnScreen);

        // We want: 0 trash -> 1.0 (full bar)
        //          maxTrashOnScreen trash -> 0.0 (empty bar)
        float fullness = 1f - (currentTrash / (float)maxTrashOnScreen);

        progressSlider.value = fullness;
    }
}
