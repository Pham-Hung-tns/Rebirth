using UnityEngine;

public abstract class Feedback : MonoBehaviour
{
    /// <summary>
    /// Executes the visual/audio feedback logic.
    /// </summary>
    public abstract void CreateFeedback();

    /// <summary>
    /// Resets or instantly completes any ongoing feedback (e.g. stopping coroutines).
    /// </summary>
    public abstract void CompletePreviousFeedback();

    protected virtual void OnDestroy()
    {
        CompletePreviousFeedback();
    }
}
