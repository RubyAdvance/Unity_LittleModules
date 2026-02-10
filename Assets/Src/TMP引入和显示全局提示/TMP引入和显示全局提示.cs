using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMP引入和显示全局提示 : MonoBehaviour
{
    public RectTransform uiParent;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowToast("测试提示信息");
        }
    }


    public void ShowToast(string msg, float duration = 0)
    {
        if (string.IsNullOrEmpty(msg)) return;
        //这里使用的是Resource资源加载，在实际的项目中会封装ResourceMgr来管理资源加载，一般是Addressales方式
        var go = Resources.Load("ToastUIView") as GameObject;
        if (go != null)
        {
            var toast = Instantiate(go, uiParent);
            toast.transform.SetAsLastSibling();
            toast.GetComponent<ToastUIView>().SetToast(msg, duration);
        }
    }
}
