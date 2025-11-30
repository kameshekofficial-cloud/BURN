using UnityEngine;
using System.Collections;

public class RearrangeAnimation : MonoBehaviour
{
    [SerializeField] private GameObject animationObject;
    [SerializeField] private Animator animator;
    [SerializeField] private int chanceToShow = 5;

    bool isAnimating = false;
    bool hasFlipped = false;
    
    private void Awake()
    {
        // If no animation object is assigned, use this GameObject
        if (animationObject == null)
        {
            animationObject = gameObject;
        }
        
        // Try to get the Animator component
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null && animationObject != null)
            {
                animator = animationObject.GetComponent<Animator>();
            }
        }
        
        // Start with the animation object disabled
        if (animationObject != null)
        {
            animationObject.SetActive(false);
        }
    }

    private void Update()
    {
        if(isAnimating)
        {
            return;
        }

        var activeFurniture = FurnitureManager.GetActiveFurnitureRequest();
        
        if (activeFurniture == null)
        {
            hasFlipped = false;
            return;
        }
        Debug.Log("Progress to ghost: " + activeFurniture.GetProgressToGhost());
        if(activeFurniture.GetProgressToGhost() < 1 && !hasFlipped)
        {
            hasFlipped = true;
            if(Random.Range(1,chanceToShow) == 1)
            {

                Debug.Log("Showing rearrange");
                ShowRearrange();
            }
        }
    }
    
    public void ShowRearrange()
    {
        if (animationObject != null && animator != null)
        {
            StartCoroutine(ShowRearrangeCoroutine());
        }
        else
        {
            Debug.LogWarning("RearrangeAnimation: Missing animator or animation object!");
        }
    }
    
    private IEnumerator ShowRearrangeCoroutine()
    {
        isAnimating = true;
        // Enable the GameObject
        animationObject.SetActive(true);
        
        // Play the animation from the beginning
        animator.Play("rearange", 0, 0f);
        
        // Wait one frame for the animation to start
        yield return null;
        
        // Wait for the animation to finish playing
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.normalizedTime < 1.0f || animator.IsInTransition(0))
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
        
        // Disable the GameObject after animation completes
        animationObject.SetActive(false);
        isAnimating = false;
    }
}

