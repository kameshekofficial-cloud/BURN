using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FurnitureManager : MonoBehaviour
{
    private static List<FurnitureObject> furnitureList = new List<FurnitureObject>();

    [Header("Global Settings")]
    public float requestInterval = 15f;
    public float minX = -7f;
    public float maxX = 7f;

    private static bool requestInProgress = false;
    public static bool RequestInProgress => requestInProgress;

    private void Start()
    {
        StartCoroutine(RequestRoutine());
    }

    public static void Register(FurnitureObject furniture)
    {
        if (!furnitureList.Contains(furniture))
            furnitureList.Add(furniture);
    }

    public static void Unregister(FurnitureObject furniture)
    {
        furnitureList.Remove(furniture);
    }

    private IEnumerator RequestRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(requestInterval);
            TriggerRandomFurnitureRequest();
        }
    }

    private void TriggerRandomFurnitureRequest()
    {
        if (requestInProgress)
            return;

        var candidates = furnitureList
            .Where(f => f != null && f.CanBeChosen)
            .ToList();

        if (candidates.Count == 0)
            return;

        int index = Random.Range(0, candidates.Count);
        FurnitureObject chosen = candidates[index];

        requestInProgress = true;
        chosen.StartRelocationRequest(minX, maxX);
    }

    public static void NotifyRequestFinished()
    {
        requestInProgress = false;
    }

    /// <summary>
    /// Gets the furniture object that is currently awaiting player move or has an active request.
    /// Returns null if no furniture has an active request.
    /// </summary>
    public static FurnitureObject GetActiveFurnitureRequest()
    {
        return furnitureList
            .FirstOrDefault(f => f != null && f.IsBusy);
    }
}
