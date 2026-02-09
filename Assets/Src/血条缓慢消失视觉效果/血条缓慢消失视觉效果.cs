using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class 血条缓慢消失视觉效果 : MonoBehaviour
{
    [Header("血条组件")]
    public Image hpFront; // 前层（即时显示当前血量）
    public Image hpBack;  // 后层（缓慢消退残影）
    [Header("操作按钮")]
    public Button addHpBtn;
    public Button reduceHpBtn;
    [Header("血条参数")]
    public float maxHp = 100f;
    public float currentHp = 100f;
    [Header("动画参数（项目常用值）")]
    public float frontAnimDuration = 0.1f;  // 前层动画时长（接近即时）
    public float backAnimDuration = 0.6f;   // 后层消退时长
    public float backDelay = 0.2f;          // 后层延迟启动时间

    // 动画唯一标识（避免多次触发冲突）
    private readonly int _frontTweenId = Animator.StringToHash("HpFront");
    private readonly int _backTweenId = Animator.StringToHash("HpBack");

    void Start()
    {
        addHpBtn.onClick.AddListener(() => ChangeHp(20));
        reduceHpBtn.onClick.AddListener(() => ChangeHp(-20));
        ResetHp();
    }

    /// <summary>
    /// 重置血条到满值
    /// </summary>
    public void ResetHp()
    {
        currentHp = maxHp;
        // 终止所有残留动画，保证初始化一致
        DOTween.Kill(_frontTweenId);
        DOTween.Kill(_backTweenId);
        
        hpFront.fillAmount = 1f;
        hpBack.fillAmount = 1f;
    }

    /// <summary>
    /// 核心：修改血量（项目级封装，兼容多次快速触发）
    /// </summary>
    public void ChangeHp(float delta)
    {
        // 1. 边界校验（避免血量异常）
        float newHp = Mathf.Clamp(currentHp + delta, 0f, maxHp);
        if (Mathf.Approximately(newHp, currentHp)) return; // 血量无变化直接返回
        
        float targetFill = newHp / maxHp;
        currentHp = newHp;

        // 2. 区分加血/扣血逻辑（项目中加血无需延迟，扣血才需要消退效果）
        if (delta > 0)
        {
            // 加血：前后层同步动画（无延迟，符合玩家直觉）
            PlayAddHpAnim(targetFill);
        }
        else
        {
            // 扣血：前层快变，后层延迟缓跟（核心消退效果）
            PlayReduceHpAnim(targetFill);
        }
    }

    /// <summary>
    /// 加血动画（前后层同步）
    /// </summary>
    private void PlayAddHpAnim(float targetFill)
    {
        DOTween.Kill(_frontTweenId);
        DOTween.Kill(_backTweenId);

        // 缓出曲线（项目中常用，动画更自然）
        hpFront.DOFillAmount(targetFill, frontAnimDuration)
            .SetId(_frontTweenId)
            .SetEase(Ease.OutQuad);
        
        hpBack.DOFillAmount(targetFill, frontAnimDuration)
            .SetId(_backTweenId)
            .SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// 扣血动画（后层延迟跟进）
    /// </summary>
    private void PlayReduceHpAnim(float targetFill)
    {
        DOTween.Kill(_frontTweenId);
        DOTween.Kill(_backTweenId);

        // 前层：极短时间变化（视觉上≈即时）
        hpFront.DOFillAmount(targetFill, frontAnimDuration)
            .SetId(_frontTweenId)
            .SetEase(Ease.OutQuad);

        // 后层：延迟 + 缓动消退（项目核心视觉效果）
        hpBack.DOFillAmount(targetFill, backAnimDuration)
            .SetId(_backTweenId)
            .SetDelay(backDelay)  // DOTween 延迟（替代 Invoke/协程）
            .SetEase(Ease.InOutQuad); // 缓入缓出，消退更丝滑
    }

    // 防止场景销毁时动画残留（项目必加）
    private void OnDestroy()
    {
        DOTween.Kill(_frontTweenId);
        DOTween.Kill(_backTweenId);
    }
}