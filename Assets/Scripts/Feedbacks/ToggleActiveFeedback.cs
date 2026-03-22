using System.Collections;
using UnityEngine;

public class ToggleActiveFeedback : Feedback
{
    [Header("Toggle Config")]
    [Tooltip("The GameObject to toggle active/inactive.")]
    [SerializeField] private GameObject targetObject;

    [Tooltip("If > 0, object will auto-deactivate after this many seconds. If 0, it stays on until CompletePreviousFeedback is called.")]
    [SerializeField] private float activeDuration = 0f;

    private Coroutine toggleCoroutine;

    public override void CreateFeedback()
    {
        if (targetObject == null) return;

        CompletePreviousFeedback();
        targetObject.SetActive(true);

        if (activeDuration > 0 && gameObject.activeInHierarchy)
        {
            toggleCoroutine = StartCoroutine(ToggleRoutine());
        }
    }

    private IEnumerator ToggleRoutine()
    {
        yield return new WaitForSeconds(activeDuration);
        targetObject.SetActive(false);
    }

    public override void CompletePreviousFeedback()
    {
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
            toggleCoroutine = null;
        }

        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }
}
