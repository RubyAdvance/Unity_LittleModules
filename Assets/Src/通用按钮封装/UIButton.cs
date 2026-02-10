using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : Button, IDragHandler
{
    public Action<PointerEventData> DragAction;
    public Action<PointerEventData> BeginDrag;
    public Action<PointerEventData> EndDrag;

    public bool canDrag { get; set; } = true;

    private bool isDrag = false;

    private float clickTime = 0;
    
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDrag)
        {
            return;
        }

        if (Input.touchCount > 1 || !canDrag)
        {
            OnPointerUp(eventData);
            return;
        }

        if (isDrag && Input.touchCount > 2)
        {
            isDrag = false;
            EndDrag?.Invoke(eventData);
            return;
        }
        
        //拖拽中
        DragAction?.Invoke(eventData);
    }

    
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (clickTime != 0 && Time.unscaledTime - clickTime < 0.5f)
        {
            return;
        }

        clickTime = Time.unscaledTime;
        
        if (Input.touchCount > 1)
        {
            OnPointerUp(eventData);
            return;
        }
        
        isDrag = true;
        BeginDrag?.Invoke(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (!isDrag)
        {
            return;
        }
        
        isDrag = false;
        EndDrag?.Invoke(eventData);
    }
}
