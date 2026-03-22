using System.Collections.Generic;
using UnityEngine;

public class FeedbackPlayer : MonoBehaviour
{
    [Tooltip("List of feedbacks to play. If empty, it will auto-collect all Feedbacks on this GameObject.")]
    [SerializeField] private List<Feedback> feedbacks = new List<Feedback>();

    private void Awake()
    {
        // Auto-populate if empty to make it easier to attach and play
        if (feedbacks.Count == 0)
        {
            feedbacks.AddRange(GetComponentsInChildren<Feedback>());
        }
    }

    public void PlayFeedbacks()
    {
        for (int i = 0; i < feedbacks.Count; i++)
        {
            if (feedbacks[i] != null)
            {
                feedbacks[i].CreateFeedback();
            }
        }
    }

    public void CompleteFeedbacks()
    {
        for (int i = 0; i < feedbacks.Count; i++)
        {
            if (feedbacks[i] != null)
            {
                feedbacks[i].CompletePreviousFeedback();
            }
        }
    }
}
