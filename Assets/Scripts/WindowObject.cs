using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class WindowObject : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Opening Settings")]
    public float openHoldDuration = 5f;  // seconds to hold T key
    public float temperatureDecreaseDuration = 5f;  // seconds to decrease temperature
    public float temperatureMinDuration = 5f;  // seconds to stay at minimum

    [Header("Animations")]
    public Animator windowAnimator;
    private const string SPEED_MULTIPLIER_PARAM = "SpeedMultiplier";

    private SpriteRenderer sr;
    private bool isOpen = false;
    private bool isOpening = false;
    private float holdTimer = 0f;
    private Coroutine temperatureCycleCoroutine;

    private float _openSpeedMultiplier = 1;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();
        
        sr.sprite = closedSprite;

    // Calculate _openSpeedMultiplier to match openHoldDuration to the length of the opening animation (if available)
    if (windowAnimator != null && windowAnimator.runtimeAnimatorController != null)
    {
        try
        {
            // Assume opening animation is at layer 0, state 0
            AnimatorStateInfo stateInfo = windowAnimator.GetCurrentAnimatorStateInfo(0);
            // However, at Awake the animator might not have proper state info yet, so prefer clip length
            if (windowAnimator.runtimeAnimatorController.animationClips.Length > 0)
            {
                AnimationClip openClip = windowAnimator.runtimeAnimatorController.animationClips[0];
                float animationLength = openClip.length;
                if (animationLength > 0f)
                {
                    _openSpeedMultiplier = animationLength / openHoldDuration;
                }
            }
        }
        catch
        {
            // Fallback to default if anything fails
            _openSpeedMultiplier = 1f;
        }
    }

    }

    private void Start()
    {
        // Temperature is already initialized by TemperatureManager with starting temperature
        // Start temperature cycle
        StartTemperatureCycle();
        
        // Initialize SpeedMultiplier to 0 (paused)
        SetSpeedMultiplier(0f);
        
        // Debug: Check initial state
        Debug.Log($"WindowObject started. isOpen: {isOpen}, GameObject active: {gameObject.activeSelf}, enabled: {enabled}");
    }

    private void Update()
    {
        // Check if window is already open
        if (isOpen)
        {
            // Window is open, T key doesn't do anything
            // Check if opening animation has completed
            CheckAnimationCompletion();
            return;
        }

        // Check for T key input - support both old and new Input System
        bool tKeyDown = Input.GetKeyDown(KeyCode.T) || (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame);
        bool tKeyHeld = Input.GetKey(KeyCode.T) || (Keyboard.current != null && Keyboard.current.tKey.isPressed);
        bool tKeyUp = Input.GetKeyUp(KeyCode.T) || (Keyboard.current != null && Keyboard.current.tKey.wasReleasedThisFrame);

        if (isOpening)
        {
            // Currently in opening process, check if still holding T
            if (tKeyHeld)
            {
                // Keep SpeedMultiplier at 1 (opening)
                SetSpeedMultiplier(_openSpeedMultiplier);
                holdTimer += Time.deltaTime;

                // Check if opening animation has completed
                CheckAnimationCompletion();

                // If we've held for the required duration and animation completed, mark as open
                if (holdTimer >= openHoldDuration)
                {
                    CompleteWindowOpening();
                }
            }
            else if (tKeyUp)
            {
                // Key was released before reaching the duration
                if (holdTimer < openHoldDuration)
                {
                    // Start closing animation (SpeedMultiplier = -5)
                    SetSpeedMultiplier(-5f);
                    isOpening = false;
                }
                holdTimer = 0f;
            }
            
            // Only return early if we're still in the opening process
            // If isOpening became false (T released early), allow code to continue to check for new T presses
            if (isOpening)
            {
                return;
            }
        }
        
        // Check if closing animation has completed (when T was released early)
        if (!isOpening && !isOpen)
        {
            if (CheckClosingAnimationCompletion())
            {
                // Closing completed, reset state
                holdTimer = 0f;
            }
        }

        // Window is closed and not opening - check for T key press
        if (tKeyDown)
        {
            Debug.Log("T key pressed - starting window opening");
            // Start opening animation (SpeedMultiplier = 1)
            SetSpeedMultiplier(_openSpeedMultiplier);
            isOpening = true;
            holdTimer = 0f;
        }
    }

    private void SetSpeedMultiplier(float value)
    {
        if (windowAnimator == null)
            return;

        // Check if the Animator's GameObject is active
        if (!windowAnimator.gameObject.activeInHierarchy)
            return;

        // Check if controller exists
        if (windowAnimator.runtimeAnimatorController == null)
            return;

        // Set the SpeedMultiplier parameter
        try
        {
            windowAnimator.SetFloat(SPEED_MULTIPLIER_PARAM, value);
        }
        catch
        {
            // Parameter doesn't exist or controller is invalid
            // Silently skip - sprite changes will still work
        }
    }

    private void CheckAnimationCompletion()
    {
        // This method is called when window is open to check if opening completed
        // Opening completion is handled in CompleteWindowOpening()
    }

    private bool CheckClosingAnimationCompletion()
    {
        if (windowAnimator == null)
        {
            // No animator, consider it closed
            sr.sprite = closedSprite;
            return true;
        }

        // Check if the Animator's GameObject is active
        if (!windowAnimator.gameObject.activeInHierarchy)
        {
            sr.sprite = closedSprite;
            return true;
        }

        // Check if controller exists
        if (windowAnimator.runtimeAnimatorController == null)
        {
            sr.sprite = closedSprite;
            return true;
        }

        try
        {
            AnimatorStateInfo stateInfo = windowAnimator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.normalizedTime;

            // If closing animation completed (normalizedTime <= 0)
            if (normalizedTime <= 0f)
            {
                // Closing animation completed
                SetSpeedMultiplier(0f);
                sr.sprite = closedSprite;
                return true;
            }
        }
        catch
        {
            // Controller is invalid or not properly initialized
            sr.sprite = closedSprite;
            return true;
        }

        return false;
    }

    private void CompleteWindowOpening()
    {
        if (isOpen)
            return;

        // Check if opening animation has completed
        if (windowAnimator != null && windowAnimator.gameObject.activeInHierarchy && windowAnimator.runtimeAnimatorController != null)
        {
            try
            {
                AnimatorStateInfo stateInfo = windowAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime < 1f)
                {
                    // Animation hasn't completed yet, wait
                    return;
                }
            }
            catch
            {
                // If we can't check, proceed anyway
            }
        }

        // Opening animation completed and hold duration reached
        SetSpeedMultiplier(0f); // Pause animation at open state
        isOpen = true;
        isOpening = false;
        holdTimer = 0f;

        // Change sprite to open (backup in case animation doesn't handle it)
        sr.sprite = openSprite;

        // Restart temperature cycle to handle open state
        StartTemperatureCycle();
    }


    private void StartTemperatureCycle()
    {
        if (temperatureCycleCoroutine != null)
        {
            StopCoroutine(temperatureCycleCoroutine);
        }
        temperatureCycleCoroutine = StartCoroutine(TemperatureCycleRoutine());
    }

    private IEnumerator TemperatureCycleRoutine()
    {
        while (true)
        {
            if (isOpen)
            {
                // Window is open: decrease temperature to minimum over 5 seconds
                float elapsed = 0f;
                float startTemp = TemperatureManager.Instance.CurrentTemperatureLevel;

                while (elapsed < temperatureDecreaseDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / temperatureDecreaseDuration;
                    TemperatureManager.Instance.SetTemperature(Mathf.Lerp(startTemp, 0f, t));
                    Debug.Log("Temperature: " + TemperatureManager.Instance.CurrentTemperatureLevel);
                    yield return null;
                }

                TemperatureManager.Instance.SetTemperature(0f);

                // Stay at minimum for 5 seconds
                yield return new WaitForSeconds(temperatureMinDuration);

                // Close window and cycle repeats
                CloseWindow();
            }
            else
            {
                // Window is closed: temperature increases continuously
                // This runs every frame while closed
                TemperatureManager.Instance.IncreaseTemperature(Time.deltaTime);
                yield return null;
            }
        }
    }

    private void CloseWindow()
    {
        if (!isOpen)
            return;

        // Start closing animation (SpeedMultiplier = -5)
        SetSpeedMultiplier(-5f);
        isOpen = false;
        
        // Start coroutine to monitor closing animation completion
        StartCoroutine(WaitForClosingAnimation());
    }

    private IEnumerator WaitForClosingAnimation()
    {
        // Wait until closing animation completes (normalizedTime <= 0)
        while (true)
        {
            if (windowAnimator != null && windowAnimator.gameObject.activeInHierarchy && windowAnimator.runtimeAnimatorController != null)
            {
                try
                {
                    AnimatorStateInfo stateInfo = windowAnimator.GetCurrentAnimatorStateInfo(0);
                    if (stateInfo.normalizedTime <= 0f)
                    {
                        // Closing animation completed
                        SetSpeedMultiplier(0f); // Pause animation at closed state
                        // Change sprite to closed (backup in case animation doesn't handle it)
                        sr.sprite = closedSprite;
                        yield break;
                    }
                }
                catch
                {
                    // If we can't check, use sprite change as fallback
                    SetSpeedMultiplier(0f);
                    sr.sprite = closedSprite;
                    yield break;
                }
            }
            else
            {
                // No animator, just change sprite
                sr.sprite = closedSprite;
                yield break;
            }
            
            yield return null;
        }
    }

    public bool IsOpen => isOpen;
}

