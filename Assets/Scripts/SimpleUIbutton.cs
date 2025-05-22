using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SimpleUIButton : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onClick;
    public AudioClip clickSound;
    public GameObject popupWindow;
    public float scaleOnClick = 1.1f;
    public float scaleDuration = 0.1f;

    private Vector3 originalScale;
    private AudioSource audioSource;

    void Start()
    {
        originalScale = transform.localScale;
        audioSource = Camera.main.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            GameObject audioObj = new GameObject("AudioSource");
            audioSource = audioObj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ClickAnimation());

        if (clickSound && audioSource)
        {
            audioSource.PlayOneShot(clickSound);
        }

        onClick?.Invoke();

        if (popupWindow != null)
        {
            popupWindow.SetActive(true);
        }
    }

    private System.Collections.IEnumerator ClickAnimation()
    {
        Vector3 targetScale = originalScale * scaleOnClick;

        float time = 0;
        while (time < scaleDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, time / scaleDuration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        time = 0;
        while (time < scaleDuration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, time / scaleDuration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
