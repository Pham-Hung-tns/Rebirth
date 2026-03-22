using System.Collections;
using UnityEngine;

public class BlinkFeedback : Feedback
{
    [Header("Blink Config")]
    [Tooltip("The SpriteRenderer to blink. If null, looks for one on the same GameObject or children.")]
    [SerializeField] private SpriteRenderer targetSprite;
    
    [Tooltip("The material to apply when blinking (e.g., GUI/Text Shader material).")]
    [SerializeField] private Material blinkMaterial;
    [SerializeField] private float blinkDuration = 0.15f;

    private Material originalMaterial;
    private Coroutine blinkCoroutine;
    private bool initialized = false;

    private void Awake()
    {
        if (targetSprite == null)
        {
            targetSprite = GetComponentInChildren<SpriteRenderer>();
        }

        if (targetSprite != null)
        {
            originalMaterial = targetSprite.material;
            initialized = true;
        }
    }

    public override void CreateFeedback()
    {
        if (targetSprite == null || blinkMaterial == null || !initialized) return;
        if (!gameObject.activeInHierarchy) return;
        
        CompletePreviousFeedback();
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        targetSprite.material = blinkMaterial;
        yield return new WaitForSeconds(blinkDuration);
        targetSprite.material = originalMaterial;
    }

    public override void CompletePreviousFeedback()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (targetSprite != null && originalMaterial != null && initialized)
        {
            targetSprite.material = originalMaterial;
        }
    }
}
