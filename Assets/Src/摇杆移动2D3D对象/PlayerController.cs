using UnityEngine;

/// <summary>
/// 通用角色移动控制器 - 挂载到【2D精灵】或【3D胶囊体/物体】上
/// 一键切换2D/3D模式，自动适配刚体组件，摇杆控制移动
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("===== 通用移动配置 =====")]
    [Tooltip("移动速度，2D/3D通用，数值越大越快")]
    public float moveSpeed = 8f;

    [Header("===== 模式切换【一键切换，不用改代码】 =====")]
    [Tooltip("勾选 = 2D模式(精灵移动) | 取消勾选 = 3D模式(胶囊体移动)")]
    public bool is2DMode = false;

    // 组件缓存
    private Rigidbody rb3D;       // 3D刚体组件
    private Rigidbody2D rb2D;     // 2D刚体组件

    private void Awake()
    {
        // 自动识别并获取对应刚体组件，无需手动赋值
        if (is2DMode)
        {
            rb2D = GetComponent<Rigidbody2D>();
        }
        else
        {
            rb3D = GetComponent<Rigidbody>();
        }
    }

    // 物理移动固定写在FixedUpdate，帧率稳定无抖动，2D/3D通用
    private void FixedUpdate()
    {
        if (is2DMode)
        {
            MovePlayer2D(); // 2D精灵移动逻辑
        }
        else
        {
            MovePlayer3D(); // 3D物体移动逻辑
        }
    }

    /// <summary>
    /// 3D物体移动逻辑 - 胶囊体/立方体等，X-Z平面水平移动，Y轴高度不变
    /// </summary>
    private void MovePlayer3D()
    {
        if (rb3D == null) return;
        
        // 摇杆方向转3D世界坐标：X→X轴，Y→Z轴，Y轴=0 不浮空不下陷
        Vector3 moveVec = new Vector3(UIController.moveDirection.x, 0, UIController.moveDirection.y);
        // 平滑移动，归一化保证斜向速度一致
        Vector3 targetPos = transform.position + moveVec.normalized * moveSpeed * Time.fixedDeltaTime;
        rb3D.MovePosition(targetPos);
    }

    /// <summary>
    /// 2D精灵移动逻辑 - Sprite，X-Y平面移动，2D游戏标准逻辑
    /// </summary>
    private void MovePlayer2D()
    {
        if (rb2D == null) return;
        
        // 摇杆方向直接转2D世界坐标：X→X轴，Y→Y轴，完美适配2D平面
        Vector2 moveVec = UIController.moveDirection;
        // 平滑移动，归一化保证斜向速度一致
        Vector2 targetPos = (Vector2)transform.position + moveVec.normalized * moveSpeed * Time.fixedDeltaTime;
        rb2D.MovePosition(targetPos);
    }
}