using UnityEngine;

public class InstantiateFeedback : Feedback
{
    [Header("Instantiate Config")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform spawnTransform;
    [Tooltip("If true, the spawned object will be parented to the spawnTransform.")]
    [SerializeField] private bool parentToTransform = true;

    private GameObject spawnedObject;

    public override void CreateFeedback()
    {
        if (prefab == null || spawnTransform == null) return;
        
        CompletePreviousFeedback();

        if (ObjPoolManager.Instance != null)
        {
            spawnedObject = ObjPoolManager.Instance.GetFromPool(
                prefab, 
                spawnTransform.position, 
                Quaternion.identity, 
                parentToTransform ? spawnTransform : null
            );
            spawnedObject.SetActive(true);
        }
        else
        {
            spawnedObject = Instantiate(
                prefab, 
                spawnTransform.position, 
                Quaternion.identity, 
                parentToTransform ? spawnTransform : null
            );
        }
    }

    public override void CompletePreviousFeedback()
    {
        // Only return to pool if it's still active (hasn't been auto-despawned by another script)
        if (spawnedObject != null && spawnedObject.activeSelf)
        {
            if (ObjPoolManager.Instance != null)
            {
                ObjPoolManager.Instance.ReturnToPool(spawnedObject);
            }
            else
            {
                Destroy(spawnedObject);
            }
        }
        spawnedObject = null;
    }
}
