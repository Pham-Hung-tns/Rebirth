using Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public CinemachineVirtualCamera cmVC;
    private float timer;
    private float shakeTimeTotal;
    private float startingIntensity;

    protected override void Awake()
    {
        base.Awake();
        if (cmVC == null)
        {
            cmVC = Object.FindAnyObjectByType<CinemachineVirtualCamera>();
        }
    }

    private void Start()
    {
        // Tự động follow player nếu đang ở DungeonScene (có LevelManager)
        if (LevelManager.Instance != null && LevelManager.Instance.SelectedPlayer != null)
        {
            SetFollowTarget(LevelManager.Instance.SelectedPlayer.transform);
        }
    }

    public void SetFollowTarget(Transform target)
    {
        if (cmVC != null)
        {
            cmVC.Follow = target;
        }
    }
    public void ShakeCM(float intensity, float shakeTimer)
    {
        CinemachineBasicMultiChannelPerlin obj = cmVC.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        obj.m_AmplitudeGain = intensity;
        startingIntensity = intensity;
        timer = shakeTimer;
        shakeTimeTotal = shakeTimer;
    }

    public void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            CinemachineBasicMultiChannelPerlin obj = cmVC.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            obj.m_AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, 1- (timer / shakeTimeTotal));

        }
    }

}
