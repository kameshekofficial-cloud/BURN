using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    [Header("Trash Settings")]
    public GameObject[] trashPrefabs;   // <- multiple prefabs

    [Header("Spawn Line Settings")]
    public float minX = -5f;
    public float maxX = 5f;
    public float yPos = 0f;

    [Header("Spawn Timing")]
    public float minSpawnTime = 3f;
    public float maxSpawnTime = 7f;

    [Header("Trash Limit")]
    public int maxTrashCount = 20; // Maximum number of trash objects allowed in the scene

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private System.Collections.IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            // Only spawn if we haven't reached the maximum
            if (TrashManager.TrashCount < maxTrashCount)
            {
                SpawnTrash();
            }
        }
    }

    private void SpawnTrash()
    {
        float x = Random.Range(minX, maxX);
        Vector3 spawnPos = new Vector3(x, yPos, 0f);

        // pick a random trash type
        int index = Random.Range(0, trashPrefabs.Length);
        GameObject prefabToSpawn = trashPrefabs[index];

        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }
}
