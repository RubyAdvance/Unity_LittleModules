using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonScaleAnim : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Button btn;
    
    private float pressScale = 0.95f;  // 按下时放大比例
    private float pressDuration = 0.2f; 
    
    private Vector3 originalScale;

    public void Awake()
    {
        originalScale = this.transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        DOTween.Sequence()
            .Append(this.transform.DOScale(originalScale * pressScale, pressDuration))
            .SetEase(Ease.OutElastic);
    }

    // 按钮释放
    public void OnPointerUp(PointerEventData eventData)
    {
        this.transform.DOScale(originalScale, 0.2f); // 恢复原始尺寸

        //预留播放按钮点击音效
        // AudioManager.instance.PlaySound(Const.AudioPath.Click);
    }
    
}
