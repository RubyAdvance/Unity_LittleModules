using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUIView : MonoBehaviour
{
    public Animator anim;//直接引用Animator组件
    public Button closeBtn;

    // Start is called before the first frame update
    void Awake()
    {
        closeBtn.onClick.AddListener(() =>
        {
            PlayCloseAnimation();
        });

        //打开时播放打开动画，这个一般放在UIBase中，即所有的UI面板都应该是这样，但当前是测试，所以就直接这样了
        PlayOpenAnimation();
    }


    public void PlayOpenAnimation()
    {
        if (anim)
        {
            anim.Play("open");
        }
    }
    public void PlayCloseAnimation()
    {
        if (anim)
        {
            anim.Play("close");
        }
    }
}
