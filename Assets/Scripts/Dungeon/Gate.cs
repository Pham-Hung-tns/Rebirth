using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gate : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private CanvasGroup fade;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Settings.PLAYER_TAG))
        {
            animator.SetTrigger(Settings.GATE_OPEN);
            var pc = collision.GetComponent<PlayerController>();
            if (pc != null)
                pc.enabled = false;
            AudioManager.Instance.PlaySFX(SFXClip.DoorOpen);
            StartCoroutine(IELoadDungeon());
        }
    }

    public IEnumerator IELoadDungeon()
    {
        fade.gameObject.SetActive(true);
        StartCoroutine(Helper.IEFade(fade, 1f, 2f));
        yield return new WaitForSeconds(2f);
        StartCoroutine(Helper.IEFade(fade, 0f, 1f));
        SceneManager.LoadScene(Settings.GAME_SCENE);
        
    }
}
