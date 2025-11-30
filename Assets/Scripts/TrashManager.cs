using UnityEngine;
using System.Collections.Generic;

public class TrashManager : MonoBehaviour
{
    private static List<TrashObject> trashList = new List<TrashObject>();

public static int TrashCount => trashList.Count;

    public static void RegisterTrash(TrashObject trash)
    {
        trashList.Add(trash);
    }

    public static void UnregisterTrash(TrashObject trash)
    {
        trashList.Remove(trash);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveRandomTrash();
        }
    }

    void RemoveRandomTrash()
    {
        if (trashList.Count == 0)
        {
            Debug.Log("No more trash to remove!");
            return;
        }

        int index = Random.Range(0, trashList.Count);
        TrashObject selected = trashList[index];
        Destroy(selected.gameObject);
    }
}
