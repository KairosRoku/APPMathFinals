using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour, IPointerDownHandler
{
    public AudioClip ClickSFX;
    public float Volume = 0.5f;

    private Button _button;

    private void Start()
    {
        _button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_button != null && _button.interactable && AudioManager.Instance != null)
        {
            if (ClickSFX != null)
            {
                AudioManager.Instance.PlaySFX(ClickSFX, Volume);
            }
            else
            {
                AudioManager.Instance.PlayClickSFX();
            }
        }
    }
}
