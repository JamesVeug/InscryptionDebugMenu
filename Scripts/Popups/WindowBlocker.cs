using System;
using DebugMenu;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowBlocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isHovered = false;
    public RectTransform RectTransform;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        if(isHovered)
            OnPointerExit(null);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Plugin.Log.LogInfo(gameObject.name + " Entered");
        isHovered = true;
        // Callback(true, this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Plugin.Log.LogInfo(gameObject.name + " Exited");
        isHovered = false;
        // Callback(false, this);
    }
}