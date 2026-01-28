using System;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 通用敌人材质效果控制器（可迁移复用）
/// 核心功能：普通敌人受击闪红、Boss受击闪白、冰冻/自动解冻（Update内统一计时）
/// </summary>
public class EnemyMaterialController : IDisposable
{
    #region 可配置参数（按需调整，无需改代码）
    [Serializable]
    public class MaterialEffectConfig
    {
        [Header("普通敌人-受击闪红")]
        public Color hitColor_Normal = new Color(1f, 0.25f, 0.25f); // 闪红基色（#FF4040）
        public float hitTextureFadeValue_Normal = 0.6f;            // 普通敌人闪红强度
        public float hitDuration_Normal = 0.4f;                    // 普通敌人闪红持续时间

        [Header("Boss-受击闪白")]
        public Color hitColor_Boss = Color.white;                  // Boss闪白基色
        public float hitTextureFadeValue_Boss = 0.5f;              // Boss闪白强度（可自定义）
        public float hitDuration_Boss = 0.6f;                      // Boss闪白持续时间（可自定义）

        [Header("通用受击配置")]
        public float hitCooldown = 0.7f;                           // Boss防重复受击冷却

        [Header("冰冻效果（核心：计时移到Update）")]
        public float iceValue = 0.3f;                              // 冰冻强度
        public float iceResetValue = 0f;                           // 解冻值
        public float freezeDuration = 1f;                          // 冰冻持续时长（默认1秒，编辑器可配）

        [Header("通用设置")]
        public bool useCustomDeltaTime = true;                     // 是否使用自定义时间源
        public Color defaultColor = new Color(1f, 0.25f, 0.25f);   // 初始默认颜色（普通敌人）
        public Color defaultColor_Boss = Color.white;              // Boss初始默认颜色
    }

    // 公开配置，外部可自定义
    public MaterialEffectConfig EffectConfig = new MaterialEffectConfig();
    #endregion

    #region 核心字段
    private readonly Renderer _targetRenderer;       // 目标渲染器（兼容MeshRenderer/SpriteRenderer）
    private readonly Func<float> _getDeltaTime;      // 时间源委托
    private readonly Func<bool> _isBossCheck;        // Boss判断委托（外部传入，解耦）

    // Shader属性ID（缓存）
    private readonly int _idIce = Shader.PropertyToID("_Ice");
    private readonly int _idTextureFade = Shader.PropertyToID("_TextureFade");
    private readonly int _idColor = Shader.PropertyToID("_Color"); // 新增：颜色属性ID

    // 状态变量（新增冰冻计时器）
    private MaterialPropertyBlock _mpb;              // 材质属性块
    private float _hitTimer = -1f;                   // 受击效果计时器
    private float _hitCooldownTimer = -1f;           // Boss防重复受击计时器
    private float _freezeTimer = -1f;                // 冰冻计时器（核心：移到这里）
    private bool _isDisposed = false;                // 释放标记
    private bool _isBoss;                            // 缓存Boss状态，避免重复调用委托
    #endregion

    #region 构造函数（灵活初始化）
    /// <summary>
    /// 通用构造函数（推荐）
    /// </summary>
    /// <param name="targetRenderer">目标渲染器</param>
    /// <param name="isBossCheck">Boss判断逻辑</param>
    /// <param name="deltaTimeProvider">自定义时间源（可选）</param>
    public EnemyMaterialController(Renderer targetRenderer, Func<bool> isBossCheck, Func<float> deltaTimeProvider = null)
    {
        _targetRenderer = targetRenderer ?? throw new ArgumentNullException(nameof(targetRenderer), "目标渲染器不能为空！");
        _isBossCheck = isBossCheck ?? (() => false);
        _getDeltaTime = deltaTimeProvider ?? (() => Time.deltaTime);

        // 初始化材质属性块
        _mpb = new MaterialPropertyBlock();

        // 缓存Boss状态，初始化默认颜色
        _isBoss = _isBossCheck.Invoke();
        InitDefaultState();
    }

    /// <summary>
    /// 简化构造函数（适配原有逻辑）
    /// </summary>
    /// <param name="enemyRootObj">敌人根物体</param>
    /// <param name="isBossCheck">Boss判断逻辑</param>
    /// <param name="deltaTimeProvider">自定义时间源</param>
    public EnemyMaterialController(GameObject enemyRootObj, Func<bool> isBossCheck, Func<float> deltaTimeProvider = null)
    {
        if (enemyRootObj == null)
            throw new ArgumentNullException(nameof(enemyRootObj), "敌人根物体不能为空！");

        _targetRenderer = enemyRootObj.GetComponentInChildren<Renderer>();
        if (_targetRenderer == null)
            throw new InvalidOperationException("敌人物体下未找到Renderer组件！");

        _isBossCheck = isBossCheck ?? (() => false);
        _getDeltaTime = deltaTimeProvider ?? (() => Time.deltaTime);
        _mpb = new MaterialPropertyBlock();

        // 缓存Boss状态，初始化默认颜色
        _isBoss = _isBossCheck.Invoke();
        InitDefaultState();
    }
    #endregion

    #region 核心功能（冰冻计时移到Update，和受击逻辑统一）
    /// <summary>
    /// 初始化默认状态（区分Boss/普通敌人初始颜色）
    /// </summary>
    public void InitDefaultState()
    {
        if (_isDisposed || _targetRenderer == null) return;

        // 区分Boss/普通敌人的初始颜色
        Color initColor = _isBoss ? EffectConfig.defaultColor_Boss : EffectConfig.defaultColor;
        
        _targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(_idColor, initColor);       // 设置初始颜色
        _mpb.SetFloat(_idTextureFade, 0f);        // 重置闪效强度
        _mpb.SetFloat(_idIce, EffectConfig.iceResetValue); // 重置冰冻
        _targetRenderer.SetPropertyBlock(_mpb);

        // 重置所有计时器（包括冰冻）
        _hitTimer = -1f;
        _hitCooldownTimer = -1f;
        _freezeTimer = -1f; // 重置冰冻计时器
    }

    /// <summary>
    /// 启用时重置状态
    /// </summary>
    public void OnEnable()
    {
        if (_isDisposed) return;

        _hitTimer = 0f;
        _hitCooldownTimer = 0f;
        _freezeTimer = -1f; // 启用时默认未冰冻
        SetTextureFade(0f);
        OutIce();
    }

    /// <summary>
    /// 受击触发（核心：普通敌人闪红，Boss闪白）
    /// </summary>
    public void OnHit()
    {
        if (_isDisposed || _targetRenderer == null) return;

        // Boss防重复受击冷却逻辑（保留）
        if (_isBoss && _hitCooldownTimer > 0)
        {
            return;
        }

        // ========== 区分Boss/普通敌人的闪效 ==========
        if (_isBoss)
        {
            // Boss：闪白
            _hitTimer = EffectConfig.hitDuration_Boss; // Boss专属持续时间
            SetHitColor(EffectConfig.hitColor_Boss);    // 设置闪白颜色
            SetTextureFade(EffectConfig.hitTextureFadeValue_Boss); // Boss专属强度
        }
        else
        {
            // 普通敌人：闪红
            _hitTimer = EffectConfig.hitDuration_Normal; // 普通敌人持续时间
            SetHitColor(EffectConfig.hitColor_Normal);    // 设置闪红颜色
            SetTextureFade(EffectConfig.hitTextureFadeValue_Normal); // 普通敌人强度
        }

        // Boss防重复受击冷却（保留）
        _hitCooldownTimer = _hitTimer + EffectConfig.hitCooldown;
    }

    /// <summary>
    /// 触发冰冻（初始化冰冻计时器）
    /// </summary>
    public void OnIce()
    {
        if (_isDisposed || _targetRenderer == null) return;

        // 1. 设置冰冻材质参数
        _targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(_idIce, EffectConfig.iceValue);
        _targetRenderer.SetPropertyBlock(_mpb);

        // 2. 初始化冰冻计时器（重新计时，覆盖原有计时）
        _freezeTimer = EffectConfig.freezeDuration;
    }

    /// <summary>
    /// 手动解冻（外部可调用）
    /// </summary>
    public void OutIce()
    {
        if (_isDisposed || _targetRenderer == null) return;

        // 1. 重置冰冻材质参数
        _targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(_idIce, EffectConfig.iceResetValue);
        _targetRenderer.SetPropertyBlock(_mpb);

        // 2. 重置冰冻计时器
        _freezeTimer = -1f;
    }

    /// <summary>
    /// 帧更新（核心：统一处理受击+冰冻计时）
    /// </summary>
    public void Update()
    {
        if (_isDisposed) return;

        float deltaTime = _getDeltaTime.Invoke();

        // ========== 1. 处理受击闪效计时（原有逻辑） ==========
        if (_hitTimer >= 0)
        {
            _hitTimer -= deltaTime;
            if (_hitTimer < 0)
            {
                // 恢复初始颜色+重置闪效强度
                Color initColor = _isBoss ? EffectConfig.defaultColor_Boss : EffectConfig.defaultColor;
                SetHitColor(initColor);
                SetTextureFade(0f);
            }
        }

        // Boss防重复受击冷却倒计时
        if (_hitCooldownTimer >= 0)
        {
            _hitCooldownTimer -= deltaTime;
        }

        // ========== 2. 处理冰冻计时（新增：和受击逻辑统一） ==========
        if (_freezeTimer >= 0)
        {
            _freezeTimer -= deltaTime;
            // 计时结束，自动解冻
            if (_freezeTimer < 0)
            {
                OutIce(); // 调用解冻逻辑
            }
        }
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 设置受击颜色（封装）
    /// </summary>
    private void SetHitColor(Color color)
    {
        if (_isDisposed || _targetRenderer == null) return;
        _targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(_idColor, color);
        _targetRenderer.SetPropertyBlock(_mpb);
    }

    /// <summary>
    /// 设置闪效强度（封装）
    /// </summary>
    private void SetTextureFade(float value)
    {
        if (_isDisposed || _targetRenderer == null) return;
        _targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(_idTextureFade, value);
        _targetRenderer.SetPropertyBlock(_mpb);
    }

    /// <summary>
    /// 资源清理
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        InitDefaultState();
        _mpb = null;
        _isDisposed = true;
    }
    #endregion
}