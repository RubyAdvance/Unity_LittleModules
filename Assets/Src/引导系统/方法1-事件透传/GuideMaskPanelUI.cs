using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class GuideMaskPanelUI : MonoBehaviour, IPointerClickHandler
{
    [Header("遮罩")]
    public Image mask;
    public Transform hand;
    // 事件穿透对象(在实际项目中这个对象是在打开GuideMaskPanelUI时传参的，目前测试我直接拖拽了)
    public GameObject target;
    public TextMeshProUGUI tipsText;
    public Transform tipsTrans;
    private Action callback;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = target.transform.position;
        var mat = mask.material;
        mat.SetFloat("_Width", target.GetComponent<RectTransform>().rect.width);
        mat.SetFloat("_Height", target.GetComponent<RectTransform>().rect.height);
        mat.SetFloat("_Scale", 10);
        mat.DOFloat(1, "_Scale", 0.5f);


        //测试
        target.GetComponent<Button>().onClick.AddListener(() =>
        {
            Debug.Log("点击了目标按钮");
        });
    }

    // Update is called once per frame
    void Update()
    {

    }


    /*
    注意点：由于用到了ShaderGrapha，所以必须在Package Manager中安装ShaderGraph包和URP包然后Asset目录中新建URP Asset，且项目需要为URP或HDRP模版，基础的不支持ShaderGrapha
    并且在Project Settings->Graphics中把对应的Render Pipeline Asset拖拽到Scriptable Render Pipeline Settings中，否则运行时会报错找不到Shader 

    尽管镂空了，但是玩家的点击操作依然会点击在UI面板上，所以需要将点击事件透下去，才能实际触发点击按钮！！！！
    */

    // 把事件透下去
    public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function)
        where T : IEventSystemHandler
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        GameObject current = data.pointerCurrentRaycast.gameObject;
        for (int i = 0; i < results.Count; i++)
        {
            if (target == results[i].gameObject)
            {
                // ClosePanel();
                callback?.Invoke();

                // 如果是目标物体，则把事件透传下去，然后break
                ExecuteEvents.Execute(results[i].gameObject, data, function);


                break;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.pointerClickHandler);
    }



    //以下是实际项目打开GuidePanael时调用Init方法传参的代码，centerOffset是相对于target中心点的偏移位置

    // public void Init(GameObject target, float width, float height, Vector3 centerOffset, Vector3 handPos,
    // Vector3 tipsPos, string text, Action callback = null)
    // {
    //     var targetPos = Vector3.zero;
    //     if (centerOffset == Vector3.zero)
    //     {
    //         targetPos = target.transform.position;
    //     }
    //     else
    //     {
    //         targetPos = target.transform.TransformPoint(centerOffset);
    //     }
    //     transform.position = targetPos;
    //     this.target = target;
    //     var mat = mask.material;
    //     mat.SetFloat("_Width", width);
    //     mat.SetFloat("_Height", height);
    //     mat.SetFloat("_Scale", 10);
    //     mat.DOFloat(1, "_Scale", 0.5f);

    //     hand.localPosition = handPos;
    //     if (handPos.x < 0)
    //     {
    //         hand.localScale = new Vector3(-1, 1, 1);
    //     }

    //     if (string.IsNullOrEmpty(text))
    //     {
    //         tipsTrans.gameObject.SetActive(false);
    //     }
    //     else
    //     {
    //         tipsTrans.localScale = Vector3.zero;
    //         tipsTrans.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetDelay(0.5f);

    //         tipsTrans.localPosition = tipsPos;
    //         tipsText.text = text;
    //     }



    //     this.callback = callback;
    // }

    //例如
        //     Ctrls.uiPanelCtrl.ShowPanel<GuideMaskPanelUI>((panel) =>
        // {
        //     panel.Init(guideTarget, 135, 128, Vector3.zero, Vector3.zero, new Vector3(83, 301, 0),
        //                 "点击\"力量属性\"按钮");
        // });
}
