using UnityEngine;

/// <summary>
/// 2D摄像机跟随脚本（低滞后丝滑版）
/// 兼顾顺滑和响应速度，停止移动无明显滞后
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标设置")]
    [Tooltip("拖拽你的玩家对象到这里")]
    [SerializeField] private Transform targetPlayer;

    [Header("跟随偏移设置")]
    [Tooltip("摄像机和玩家的相对位置（x/y/z），2D建议Z=-10")]
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 0f, -10f);

    [Header("平滑响应设置（关键！）")]
    [Tooltip("跟随响应速度（建议5-15）：数值越大，跟随越快（滞后越小），5=稍柔，15=几乎无滞后但仍丝滑")]
    [SerializeField, Range(1f, 30f)] private float followResponse = 10f;

    [Tooltip("距离阈值：小于该值时直接吸附，避免停止后微移（建议0.01-0.1）")]
    [SerializeField, Range(0.01f, 0.5f)] private float snapThreshold = 0.05f;

    private void LateUpdate()
    {
        if (targetPlayer == null)
        {
            Debug.LogWarning("未指定摄像机跟随的玩家对象！请在Inspector面板中拖拽玩家到Target Player字段");
            return;
        }

        // 1. 计算目标位置：固定Z轴，只跟随XY轴
        Vector3 targetPosition = new Vector3(
            targetPlayer.position.x + followOffset.x,
            targetPlayer.position.y + followOffset.y,
            followOffset.z
        );

        // 2. 计算当前相机与目标位置的距离
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // 3. 距离小于阈值时直接吸附，避免停止后微移
        if (distanceToTarget < snapThreshold)
        {
            transform.position = targetPosition;
            return;
        }

        // 4. 帧率无关的平滑跟随：用Lerp + Time.deltaTime，响应快且丝滑
        // followResponse * Time.deltaTime 保证不同帧率下响应速度一致
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followResponse * Time.deltaTime
        );
    }

    // 编辑器校验刚体插值设置
    private void OnValidate()
    {
        if (targetPlayer != null && targetPlayer.TryGetComponent(out Rigidbody2D rb))
        {
            if (rb.interpolation == RigidbodyInterpolation2D.None)
            {
                Debug.LogWarning($"玩家{targetPlayer.name}的Rigidbody2D未开启插值，建议设置为Interpolation", targetPlayer);
            }
        }
    }

    // 场景视图辅助绘制
    private void OnDrawGizmos()
    {
        if (targetPlayer != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(targetPlayer.position, targetPlayer.position + followOffset);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPlayer.position + followOffset, 0.2f);
        }
    }
}