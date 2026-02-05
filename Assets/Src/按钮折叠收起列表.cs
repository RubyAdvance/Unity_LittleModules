// ========================================================
// 描述：
// 功能：
// 作者：吴超 
// 创建时间：2023-11-18 09:44:20
// 版本：1.0
// 变更时间：
// 变更版本：#CreateTime2#
// 脚本路径：Assets/Scripts/按钮折叠收起列表.cs
// ========================================================


/**
1.使用布局显示依然。在收起的时候显示三个，展开的时候全部显示。
2.结合使用mask,在收起的时候mask的大小就等于两个半按钮的大小，展开的时候mask的大小就等于全部的大小。
3.背景和箭头不可被遮挡，取消maskable勾选.
4.箭头的位置






**/
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class 按钮折叠收起列表 : MonoBehaviour
{
    public GameObject[] allBtn;
    //public Button btn;
    public bool isShow;
    public Transform parent;
    public GameObject labelArrow;
    public GameObject realArrow;
    public Button arrowBtn;
    public RectMask2D mask;
    public Transform parentLabArrow;
    public Transform parentRealArrow;
    CancellationTokenSource tokenSource;

    //public RectMask2D mask;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Hide());
        //按钮注册
        arrowBtn.onClick.AddListener(() =>
        {
            if (isShow)
            {
                StartCoroutine(Hide());
            }
            else
            {
                StartCoroutine(Show());
            }
        });
    }

    IEnumerator Show()
    {
        Debug.Log("打开");
        for (int i = 3; i < allBtn.Length; i++)
        {
            allBtn[i].SetActive(true);
        }
        isShow = true;
        //禁用遮罩
        mask.enabled = false;
        //箭头翻转
        realArrow.transform.GetChild(0).localScale = new Vector3(1, 1, 1);
        yield return 0;//使用协程延迟一帧，防止位置更新不及时
        //labelArrow处于布局下的箭头  realArrow实际触发箭头 realArrow的位置等于布局下的箭头位置即可
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRealArrow.gameObject.GetComponent<RectTransform>(), labelArrow.transform.position, null, out Vector2 point);
        realArrow.GetComponent<RectTransform>().localPosition = point;
    }

    IEnumerator Hide()
    {
        Debug.Log("收起");
        for (int i = allBtn.Length - 1; i > 2; i--)
        {
            allBtn[i].SetActive(false);
        }
        isShow = false;
        mask.enabled = true;
        realArrow.transform.GetChild(0).localScale = new Vector3(1, -1, 1);
        yield return 0;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRealArrow.gameObject.GetComponent<RectTransform>(), labelArrow.transform.position, null, out Vector2 point);
        realArrow.GetComponent<RectTransform>().localPosition = point;
    }
}
