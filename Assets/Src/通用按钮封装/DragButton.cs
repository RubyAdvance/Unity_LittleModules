using System;

using UnityEngine;
using UnityEngine.EventSystems;

public class DragButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{

    public Camera UICamera { get; set; } = null;
    public bool canClick { get; set; } = false;
    public Action<Transform, PointerEventData> pointerDownCallback { get; set; } = null;
    public Action<Transform, PointerEventData> pointerUpCallback { get; set; } = null;
    public Action<Transform, PointerEventData> dragStartCallback { get; set; } = null;
    public Action<Transform, PointerEventData> dragCallback { get; set; } = null;
    public Action dragEndCallback { get; set; } = null;
    public bool clickHasDrag { get; set; } = false;
    public int dragButtonId { get; private set; } = 0;



    // private bool _hasBeginDrag = false;
    private Vector2 _pointerDownPos;




    public void Start()
    {
        dragButtonId = DragButtonManager.instance.dragButtonId++;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        // Debug.Log("OnPointerDown 111: " + dragButtonId + " " + canClick + " " + DragButtonManager.instance.clickCount + gameObject.name);
        if (!canClick || DragButtonManager.instance.clickCount > 0)
        {
            // Debug.Log("OnPointerDown false OnPointerUp" + dragButtonId);
            OnPointerUp(eventData);
            return;
        }

        // Debug.Log("OnPointerDown true11: " + dragButtonId);
        // Debug.Log("OnPointerDown true22" + transform.name);
        DragButtonManager.instance.curPointerId = dragButtonId;
        DragButtonManager.instance.clickCount++;
        _pointerDownPos = eventData.position;
        // _hasBeginDrag = false;
        pointerDownCallback?.Invoke(transform, eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Debug.Log("OnPointerUp 111: " + dragButtonId);
        if (!canClick || dragButtonId != DragButtonManager.instance.curPointerId)
        {
            // Debug.Log("OnPointerUp return" + dragButtonId);
            return;
        }

        // if (_hasBeginDrag)
        // {
        //     // Debug.Log("OnPointerUp OnEndDrag" + dragButtonId);
        //     OnEndDrag(eventData);
        // }

        // Debug.Log("OnPointerUp true:" + dragButtonId);
        // _hasBeginDrag = false;
        DragButtonManager.instance.clickCount--;
        DragButtonManager.instance.curPointerId = -1;
        pointerUpCallback?.Invoke(transform, eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Debug.Log("OnBeginDrag 111: " + dragButtonId);
        if (!canClick || DragButtonManager.instance.curPointerId != dragButtonId)
        {
            OnEndDrag(eventData);
            return;
        }

        clickHasDrag = true;
        dragStartCallback?.Invoke(transform, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Debug.Log("OnDrag 111: " + dragButtonId);
        if (!canClick || !clickHasDrag || DragButtonManager.instance.curPointerId != dragButtonId)
        {
            OnEndDrag(eventData);
            return;
        }

        dragCallback?.Invoke(transform, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Debug.Log("OnEndDrag 111: " + dragButtonId);
        // Debug.Log("End Drag");
        if (!canClick || !clickHasDrag)
        {
            // Debug.Log("OnEndDrag return" + dragButtonId);
            return;
        }
        // Debug.Log("OnEndDrag true" + dragButtonId);
        clickHasDrag = false;
        dragEndCallback?.Invoke();
    }
}
