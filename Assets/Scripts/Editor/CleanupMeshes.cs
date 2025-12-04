using UnityEngine;
using UnityEditor;
using System.Linq;

public class CleanupMeshes : EditorWindow
{
    [MenuItem("Tools/Cleanup Unused Meshes")]
    public static void CleanupUnusedMeshes()
    {
        int removedCount = 0;
        
        // Find all TextMesh components in the scene
        TextMesh[] textMeshes = FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (TextMesh tm in textMeshes)
        {
            if (tm.font == null)
            {
                Debug.Log($"Removing TextMesh component from {tm.gameObject.name} (no font assigned)");
                DestroyImmediate(tm);
                removedCount++;
            }
        }
        
        // Find all MeshRenderer components without valid meshes
        MeshRenderer[] meshRenderers = FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (MeshRenderer mr in meshRenderers)
        {
            MeshFilter mf = mr.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh == null)
            {
                // Check if this is a TextMesh (TextMesh creates its own mesh)
                if (mr.GetComponent<TextMesh>() == null)
                {
                    Debug.Log($"Removing MeshRenderer from {mr.gameObject.name} (no mesh)");
                    DestroyImmediate(mr);
                    if (mf != null)
                    {
                        DestroyImmediate(mf);
                    }
                    removedCount++;
                }
            }
        }
        
        // Find orphaned GameObjects with TextMesh in name but no valid components
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("TrashLabel") || obj.name.Contains("TextMesh"))
            {
                // Check if it has any meaningful components
                if (obj.GetComponents<Component>().Length <= 1) // Only Transform
                {
                    Debug.Log($"Removing orphaned GameObject: {obj.name}");
                    DestroyImmediate(obj);
                    removedCount++;
                }
                else if (obj.GetComponent<TextMesh>() != null && obj.GetComponent<TextMesh>().font == null)
                {
                    Debug.Log($"Removing GameObject with invalid TextMesh: {obj.name}");
                    DestroyImmediate(obj);
                    removedCount++;
                }
            }
        }
        
        Debug.Log($"Cleanup complete. Removed {removedCount} unused mesh components/objects.");
    }
}

