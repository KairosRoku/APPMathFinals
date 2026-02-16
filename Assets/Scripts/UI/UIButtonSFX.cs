using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour, IPointerDownHandler
{
    public AudioClip ClickSFX;
    [Range(0f, 1f)]
    public float Volume = 0.5f;

    private Button _button;

    private void Start()
    {
        _button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_button != null && _button.interactable && ClickSFX != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(ClickSFX, Volume);
        }
    }
}
