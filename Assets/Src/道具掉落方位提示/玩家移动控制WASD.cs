using UnityEngine;

/// <summary>
/// 2D角色移动控制器
/// 支持WASD控制移动，移动速度可在编辑器调整
/// </summary>
[RequireComponent(typeof(Rigidbody2D))] // 自动添加Rigidbody2D组件，避免遗漏
public class 玩家移动控制WASD : MonoBehaviour
{
    // 移动速度，[SerializeField]让私有变量暴露在编辑器中
    [Header("移动设置")] // 编辑器分组，更易读
    [SerializeField] private float moveSpeed = 5f; 
    [Tooltip("是否限制斜向移动速度（防止斜着走比直着快）")]
    [SerializeField] private bool limitDiagonalSpeed = true;

    // 缓存Rigidbody2D组件，避免重复获取
    private Rigidbody2D rb;

    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void Awake()
    {
        // 获取自身的Rigidbody2D组件
        rb = GetComponent<Rigidbody2D>();
        
        // 可选：设置刚体参数，避免角色受重力影响下落
        rb.gravityScale = 0f;
        rb.freezeRotation = true; // 防止角色旋转
    }

    /// <summary>
    /// 物理相关的更新（固定时间步长，更稳定）
    /// </summary>
    private void FixedUpdate()
    {
        // 读取输入：Horizontal对应A/D键，Vertical对应W/S键（Unity默认输入轴）
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 构建移动向量
        Vector2 moveDirection = new Vector2(horizontalInput, verticalInput);

        // 如果开启斜向速度限制，归一化向量（长度变为1）
        if (limitDiagonalSpeed && moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // 设置刚体速度，实现移动
        rb.velocity = moveDirection * moveSpeed;
    }
}