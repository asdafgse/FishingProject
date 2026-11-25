using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!button.IsActive() ||
            !button.IsInteractable() ||
            clickSound == null)
        {
            return;
        }

        GameObject soundObject =
            new GameObject("ButtonClickAudio");

        DontDestroyOnLoad(soundObject);

        AudioSource source =
            soundObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = volume;
        source.clip = clickSound;

        source.Play();

        Destroy(soundObject, clickSound.length + 0.1f);
    }
}