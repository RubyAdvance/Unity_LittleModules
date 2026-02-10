using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class 通用弹窗的Animation引入 : MonoBehaviour
{
    public RectTransform uiParent;
    private SettingUIView settingUIView;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!settingUIView)
            {
                var go = Resources.Load<GameObject>("SettingUIView");
                if (go)
                {
                    var view = Instantiate(go, uiParent);
                    view.transform.SetAsLastSibling();
                    settingUIView = view.GetComponent<SettingUIView>();
                }
            }
            else
            {
                settingUIView.PlayOpenAnimation();
            }

        }
    }
}
