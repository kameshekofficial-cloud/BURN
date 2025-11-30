using UnityEngine;

public class TrashObject : MonoBehaviour
{
    private void OnEnable()
    {
        TrashManager.RegisterTrash(this);
    }

    private void OnDestroy()
    {
        TrashManager.UnregisterTrash(this);
    }
}
