using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 引导的参数合集，包括所有的引导的UI位置和回调等等，外部打开引导面板时，实例化传参即可
/// </summary>
public class GuideUIParam
{
    public int guideId;
    public RectTransform testBtn;
    public Action vAction;
    public RectTransform gameStart;
    public RectTransform battlePart;
    public RectTransform battleTime;
    public RectTransform battleGold;
    public Transform battleEquip;
    public Transform rogueTrans;
    public RectTransform returnRect;
    public Transform callBackupRectTrans;
    public Transform cellRectTrans;
    public Transform upLevelTrans;
    public Transform randomTrans;
    public Transform equipBtnTrans;
    public Transform mutationTrans;
    public Transform equipItemTrans;
    public Transform paramTrans1;
}
public class GuideUIView : MonoBehaviour
{
    public RectTransform hollowOutRect;//镂空洞1
    public RectTransform hollowOutRect2;//镂空洞2
    public RectTransform dailogueRect;//引导对话框
    public Text dailogueText;//引导提示文本
    public RectTransform finger;//引导手指
    public Button maskBtn;//检测玩家点击的按钮，有时候我们引导的时候即使玩家没有点击到特定的镂空区域，点到别的位置也推进引导步骤，这个就是检测玩家输入的
    public RectTransform Trans;//提示框和提示文本的父节点
    public GameObject mask;//接受射线的背景地板，防止穿过这个面板点击到下面的UI内容
    public Button hollowOutBtn;//镂空洞1的按钮组件


    private GuideUIParam mParam;//外部传参时获得
    private int guideId = 0;//这个在外部传参时获得当前引导的id

    // Start is called before the first frame update
    void Start()
    {
        hollowOutBtn.onClick.AddListener(OnClickHollowOutBtn);//镂空区域点击事件注册
        maskBtn.onClick.AddListener(OnClickMaskBtn);

        //以下是一个测试，实际是外部打开这个面板时传参进来的
        ShowUI(new GuideUIParam()
        {
            guideId = 0,
            testBtn = GameObject.Find("Canvas/StartGameBtn").GetComponent<RectTransform>(),
        });
    }
    /// <summary>
    /// 这个是UIBase中的方法，打开面板的时候会自动调用，当然自己也可以重写
    /// </summary>
    /// <param name="param"></param>
    public void ShowUI(GuideUIParam param)
    {
        mParam = param as GuideUIParam;
        int vGuideId = mParam.guideId;
        ResetGuide(vGuideId);
    }

    public void ResetGuide(int vGuideId)
    {
        hollowOutRect2.gameObject.SetActive(false);
        mask.gameObject.SetActive(true);
        guideId = vGuideId;
        finger.transform.localScale = Vector3.one;

        //不同的引导步骤进行不同的处理即可，包括空洞的位置、大小，提示文本
        switch (guideId)
        {
            case 0:
                hollowOutRect.transform.position = mParam.testBtn.position;
                hollowOutRect.sizeDelta = mParam.testBtn.sizeDelta;

                Trans.transform.position = hollowOutRect.transform.position;
                Trans.sizeDelta = hollowOutRect.sizeDelta;

                dailogueRect.anchoredPosition = new Vector3(116, 153);
                dailogueRect.localScale = new Vector3(-1, -1, 1);
                dailogueText.GetComponent<RectTransform>().localScale = new Vector3(-1, -1, 1);
                dailogueText.rectTransform.anchoredPosition = new Vector3(5, -15);
                dailogueText.text = "测试按钮";
                finger.anchoredPosition = new Vector3(-10, -10);
                break;
            case 1:
                hollowOutRect.transform.position = mParam.gameStart.position;
                hollowOutRect.sizeDelta = mParam.gameStart.sizeDelta;

                Trans.transform.position = hollowOutRect.transform.position;
                Trans.sizeDelta = hollowOutRect.sizeDelta;

                dailogueRect.anchoredPosition = new Vector3(116, 153);
                dailogueRect.localScale = new Vector3(-1, -1, 1);
                dailogueText.rectTransform.anchoredPosition = new Vector3(116, 169);
                dailogueText.text = "点击开始游戏";
                finger.anchoredPosition = new Vector3(-10, -10);
                break;
            case 2:
                hollowOutRect.transform.position = mParam.battlePart.position;
                hollowOutRect.sizeDelta = new Vector2(272, 100);

                Trans.transform.position = hollowOutRect.transform.position;
                Trans.sizeDelta = hollowOutRect.sizeDelta;

                dailogueRect.anchoredPosition = new Vector3(55, -132);
                dailogueRect.localScale = new Vector3(-1, 1, 1);
                dailogueText.rectTransform.anchoredPosition = new Vector3(55, -150);
                dailogueText.text = "寻找零件是我们这趟旅程的主要目的";
                finger.anchoredPosition = new Vector3(131, -174);

                hollowOutRect2.gameObject.SetActive(true);
                hollowOutRect2.anchoredPosition = new Vector2(-144, -252);
                hollowOutRect2.sizeDelta = new Vector2(167, 158);
                break;
        }
    }
    public void OnClickHollowOutBtn()
    {
        //根据当前引导的id，调用对应模块的对应方法，点击在镂空按钮上但是间接调用引导实际模块的方法
        switch (guideId)
        {
            case 0:
                Debug.Log("TODO 调用真正的进入游戏方法或者推进引导到下一步");
                break;
            case 1://比如这个是商店模块的引导奖励领取按钮，那么点击之后就调用商店模块的领取按钮的方法，当然在传参的时候传入一个action也是可以的

                break;
            case 2://如果这个是主界面的开始游戏按钮的引导，那么点击镂空按钮的时候调用开始新游戏的方法即可

                break;
            case 3:

                break;
            case 4:

                break;
            case 5:
                break;
        }
    }
    private float clickTime;
    private int clickCount;
    private void Update()
    {
        if (clickCount <= 0)
            return;

        if (Time.unscaledTime - clickTime > 1f)
        {
            clickCount = 0;
        }
    }
    //双击mask区域也推进引导
    public void OnClickMaskBtn()
    {
        clickTime = Time.unscaledTime;
        clickCount += 1;

        if (clickCount > 2)
        {
            OnClickHollowOutBtn();
        }
    }




}
