using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropItem : MonoBehaviour
{
    public 玩家移动控制WASD playerController; // 直接引用玩家控制脚本
    public Camera camera;//主摄像机，也可以直接使用Camer.Main

    public Transform tips;//圆圈提示，修改RotationZ即可指示当前道具在玩家的哪个位置，但是注意必须要显示在屏幕范围内，且当道具进入玩家的视野active=false;
    public Transform tipsArrow;//提示箭头
    // Update is called once per frame
    void Update()
    {
        //调用刷新
        UpdateTips();
    }
    private Vector3 edgeOffset = new Vector3(1, 1);
    /// <summary>
    /// 刷新提示位置和显示状态以及箭头的朝向
    /// </summary>
    public void UpdateTips()
    {
        //先判断道具是否在玩家的视野内
        if (IsCameraVisible(camera, transform.position, 0.05f))
        {
            //可见
            tips.transform.position = Vector3.one * 9999;//放到足够远的位置
        }
        else if (null != playerController)
        {
            // 不可见，提示坐标
            var edgePos = LimitPosInScreen(camera, transform.position, edgeOffset);
            //不可见显示提示和箭头指引
            tips.transform.position = edgePos;
            if (null != playerController.transform)
            {
                tipsArrow.up = transform.position - tips.transform.position;
            }
        }
    }

    /// <summary>
    /// 判断某个点是否在相机视野内
    /// </summary>
    public static bool IsCameraVisible(Camera cam, Vector3 pos, float offset = 0.1f)
    {
        Vector3 viewPos = cam.WorldToViewportPoint(pos);
        if (viewPos.x > -offset && viewPos.x < 1f + offset)
        {
            if (viewPos.y > -offset && viewPos.y < 1f + offset)
            {
                // if (viewPos.z >= cam.nearClipPlane && viewPos.z <= cam.farClipPlane)
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 限制提示在屏幕可见范围内
    /// </summary>
    /// <param name="cam"></param>
    /// <param name="pos"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static Vector3 LimitPosInScreen(Camera cam, Vector3 pos, Vector3 offset)
    {
        Vector3 viewPos = cam.WorldToViewportPoint(pos);



        if (viewPos.x > 1)
        {
            viewPos.x = 1;
            offset.x *= -1;
        }
        else if (viewPos.x < 0)
        {
            viewPos.x = 0;
        }
        else
        {
            offset.x = Mathf.Lerp(offset.x, -offset.x, viewPos.x);
        }

        if (viewPos.y > 1)
        {
            viewPos.y = 1;
            offset.y *= -1;
        }
        else if (viewPos.y < 0)
        {
            viewPos.y = 0;
        }
        else
        {
            offset.y = Mathf.Lerp(offset.y, -offset.y, viewPos.y);
        }

        return cam.ViewportToWorldPoint(viewPos) + offset;
    }

}
