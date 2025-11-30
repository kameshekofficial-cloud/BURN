using UnityEngine;
using System.Collections;

public class FurnitureObject : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite wantsMoveSprite;

    [Header("Ghost / Target Settings")]
    public GameObject ghostPrefab;
    public float ghostMoveDuration = 2f;   // how long ghost moves to new place
    public float moveSpeedToGhost = 2f;    // speed when holding Y
    public float moveBackSpeed = 2f;       // speed when sliding back
    public float snapDistance = 0.05f;     // how close is "good enough" to snap
    public float minMoveDistance = 1.5f;   // minimum distance ghost must move

    private SpriteRenderer sr;
    private bool isBusy = false;
    private bool awaitingPlayerMove = false;

    private Transform ghostTransform;
    private Vector3 originalPosition;      // where the furniture started from
    private Vector3 ghostTargetPosition;  // where the ghost is moving to
    private Vector3 ghostStartPosition;   // where the ghost spawned (same as original position)

    public bool IsAwaitingMove => awaitingPlayerMove;
    public bool IsBusy => isBusy;  // true when request has started (bar should be max)
    public Vector3 OriginalPosition => originalPosition;
    public Vector3 GhostPosition => ghostTransform != null ? ghostTransform.position : Vector3.zero;
    public Vector3 GhostTargetPosition => ghostTargetPosition;
    public bool HasGhost => ghostTransform != null;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = normalSprite;
    }

    private void OnEnable()
    {
        FurnitureManager.Register(this);
    }

    private void OnDisable()
    {
        FurnitureManager.Unregister(this);
    }

    public bool CanBeChosen => !isBusy;

    public void StartRelocationRequest(float minX, float maxX)
    {
        if (!isBusy)
        {
            StartCoroutine(RelocationRoutine(minX, maxX));
        }
    }

    private IEnumerator RelocationRoutine(float minX, float maxX)
    {
        isBusy = true;

        // remember where we started (for sliding back)
        originalPosition = transform.position;

        // change appearance to "wants to move"
        sr.sprite = wantsMoveSprite;

        // pick target position on same Y, enforcing minimum distance
        Vector3 startPos = transform.position;

        float targetX;
        int safetyCounter = 0;
        do
        {
            targetX = Random.Range(minX, maxX);
            safetyCounter++;
            if (safetyCounter > 20) break; // avoid infinite loop if range is too small
        }
        while (Mathf.Abs(targetX - startPos.x) < minMoveDistance);

        Vector3 targetPos = new Vector3(targetX, startPos.y, startPos.z);
        ghostTargetPosition = targetPos;
        ghostStartPosition = startPos;

        // spawn ghost at current position
        GameObject ghost = Instantiate(ghostPrefab, startPos, Quaternion.identity);
        ghostTransform = ghost.transform;

        // move ghost to target position over time
        float t = 0f;
        while (t < ghostMoveDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / ghostMoveDuration);
            ghostTransform.position = Vector3.Lerp(startPos, targetPos, lerp);
            yield return null;
        }

        // make sure it's exactly at target
        ghostTransform.position = targetPos;

        // now we wait for the player to move the furniture
        awaitingPlayerMove = true;
    }

    private void Update()
    {
        if (!awaitingPlayerMove || ghostTransform == null)
            return;

        Vector3 current = transform.position;

        if (Input.GetKey(KeyCode.Y))
        {
            // ↗️ Move towards the ghost while Y is held
            Vector3 target = new Vector3(
                ghostTransform.position.x,
                ghostTransform.position.y,
                current.z
            );

            transform.position = Vector3.MoveTowards(
                current,
                target,
                moveSpeedToGhost * Time.deltaTime
            );

            float distance = Vector3.Distance(transform.position, target);
            if (distance <= snapDistance)
            {
                OnReachedGhost();
            }
        }
        else
        {
            // ↩️ Move back towards original position while Y is NOT held
            Vector3 backTarget = new Vector3(
                originalPosition.x,
                originalPosition.y,
                current.z
            );

            transform.position = Vector3.MoveTowards(
                current,
                backTarget,
                moveBackSpeed * Time.deltaTime
            );
        }
    }

    private void OnReachedGhost()
    {
        // snap exactly to ghost
        transform.position = new Vector3(
            ghostTransform.position.x,
            ghostTransform.position.y,
            transform.position.z
        );

        // restore normal sprite
        sr.sprite = normalSprite;

        // delete ghost
        if (ghostTransform != null)
        {
            Destroy(ghostTransform.gameObject);
            ghostTransform = null;
        }

        awaitingPlayerMove = false;
        isBusy = false;

        // tell manager we're done so another furniture can be chosen
        FurnitureManager.NotifyRequestFinished();
    }

    /// <summary>
    /// Calculates progress bar value.
    /// Returns 1.0 (maximum) when request starts but ghost hasn't spawned yet.
    /// Returns 1.0 (maximum) when ghost has spawned and furniture is at initial position (minimum after reversal).
    /// Decreases as ghost moves to target, then decreases as furniture moves to ghost.
    /// Returns 0.0 (minimum) when furniture reaches ghost.
    /// </summary>
    public float GetProgressToGhost()
    {
        // If request hasn't started, return 0 (will be reversed to 1.0 = maximum when no ghost)
        if (!isBusy)
            return 0f;

        // If request started but ghost hasn't spawned yet, return 1.0 (will be reversed to 0.0 = minimum)
        if (ghostTransform == null)
            return 1f;

        // Calculate total distance from original position to ghost target
        float totalDistance = Vector3.Distance(originalPosition, ghostTargetPosition);
        
        if (totalDistance < 0.001f)
            return 1f; // Same position, return max (will be reversed to min)

        // Calculate how far ghost has moved from its start position
        float ghostDistance = Vector3.Distance(ghostStartPosition, ghostTargetPosition);
        float ghostCurrentDistance = Vector3.Distance(ghostStartPosition, ghostTransform.position);
        float ghostProgress = ghostDistance > 0.001f ? Mathf.Clamp01(ghostCurrentDistance / ghostDistance) : 1f;

        // Calculate how far furniture has moved toward ghost target
        Vector3 currentPos = transform.position;
        Vector3 targetDirection = ghostTargetPosition - originalPosition;
        Vector3 currentDirection = currentPos - originalPosition;
        float dotProduct = Vector3.Dot(currentDirection, targetDirection.normalized);
        float furnitureProgress = Mathf.Clamp01(dotProduct / totalDistance);

        // Combine progress: ghost movement + furniture movement
        // When ghost just spawned (0% progress) and furniture at initial (0% progress): combined = 0, return 1.0 (reversed to 0.0 = minimum) ✓
        // When ghost at target (100% progress) and furniture at initial (0% progress): combined = 0.5, return 0.5 (reversed to 0.5)
        // When ghost at target (100% progress) and furniture at ghost (100% progress): combined = 1.0, return 0.0 (reversed to 1.0 = maximum) ✓
        
        // Weight: 50% for ghost movement, 50% for furniture movement
        float combinedProgress = (ghostProgress * 0.5f) + (furnitureProgress * 0.5f);
        return 1f - combinedProgress;
    }
}
