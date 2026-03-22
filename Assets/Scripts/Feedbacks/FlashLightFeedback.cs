using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashLightFeedback : Feedback
{
    [Header("Flash Light Config")]
    [Tooltip("The Light2D to flash. If null, it looks for one on the same GameObject.")]
    [SerializeField] private Light2D targetLight;
    
    [SerializeField] private float flashDuration = 0.05f;
    [SerializeField] private float intensityWhenFlashing = 1f;

    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light2D>();
        }

        // Ensure light is off initially
        if (targetLight != null)
        {
            targetLight.intensity = 0f;
            targetLight.enabled = false;
        }
    }

    public override void CreateFeedback()
    {
        if (targetLight == null) return;
        if (!gameObject.activeInHierarchy) return;
        
        Debug.Log("Create Feedback");

        CompletePreviousFeedback();
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        targetLight.enabled = true;
        targetLight.intensity = intensityWhenFlashing;
        Debug.Log("Flash Light");
        
        yield return new WaitForSeconds(flashDuration);
        
        targetLight.intensity = 0f;
        targetLight.enabled = false;
    }

    public override void CompletePreviousFeedback()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        
        if (targetLight != null)
        {
            targetLight.intensity = 0f;
            targetLight.enabled = false;
        }
    }
}
