using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitGlitter : MonoBehaviour
{
    public Animator hitAnimator;
    
    [Header("渲染组件配置")]
    public List<MeshRenderer> monsterIconList = new List<MeshRenderer>();
    public List<SpriteRenderer> spriteList = new List<SpriteRenderer>();

    [Header("效果参数")]
    public float defaultFillPhase = 0f;
    
    // 关键修复：为每个Renderer创建独立的MPB
    private Dictionary<Renderer, MaterialPropertyBlock> rendererMPBDict = new Dictionary<Renderer, MaterialPropertyBlock>();
    
    private float _innerGlitterValue = 0f;
    public float anim_GlitterValue = 0f;
    private Coroutine hitEffectCoroutine;
    
    void Awake()
    {
        InitializeAllRenderers();
    }
    
    void OnEnable()
    {
        // 确保启用时重置状态
        ResetToDefault();
    }
    
    // 初始化所有渲染器的MPB
    void InitializeAllRenderers()
    {
        rendererMPBDict.Clear();
        
        // 处理所有MeshRenderer
        foreach (var renderer in monsterIconList)
        {
            if (renderer != null) 
                CreateMPBForRenderer(renderer);
        }
        
        // 处理所有SpriteRenderer
        foreach (var renderer in spriteList)
        {
            if (renderer != null) 
                CreateMPBForRenderer(renderer);
        }
        
        ResetToDefault();
    }
    
    // 为Renderer创建MPB
    void CreateMPBForRenderer(Renderer renderer)
    {
        if (renderer == null || rendererMPBDict.ContainsKey(renderer)) 
            return;
        
        var mpb = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(mpb);
        rendererMPBDict[renderer] = mpb;
        
        // 设置默认值
        UpdateRendererFillPhase(renderer, defaultFillPhase);
    }
    
    // 更新单个Renderer的FillPhase
    void UpdateRendererFillPhase(Renderer renderer, float fillPhase)
    {
        if (renderer == null || !rendererMPBDict.ContainsKey(renderer))
            return;
            
        var mpb = rendererMPBDict[renderer];
        mpb.SetFloat("_FillPhase", fillPhase);
        renderer.SetPropertyBlock(mpb);
    }
    
    // 重置到默认状态
    public void ResetToDefault()
    {
        anim_GlitterValue = 0f;
        _innerGlitterValue = 0f;
        
        foreach (var kvp in rendererMPBDict)
        {
            UpdateRendererFillPhase(kvp.Key, defaultFillPhase);
        }
    }
    
    // 初始化方法（可由外部调用）
    public void Init()
    {
        if (hitAnimator != null)
        {
            hitAnimator.Play("ani_monster_null", -1, 0);
            hitAnimator.transform.localScale = Vector3.one;
        }
        
        InitializeAllRenderers();
        ResetToDefault();
    }
    
    // 触发闪白效果
    public void PlayHitGlitter(bool isWall = false, bool isBoss = false, bool isPlay = false)
    {
        // 停止正在运行的协程
        if (hitEffectCoroutine != null)
            StopCoroutine(hitEffectCoroutine);
        
        // 开始新的闪白效果
        hitEffectCoroutine = StartCoroutine(HitEffectRoutine(isWall, isBoss, isPlay));
    }
    
    // 闪白效果协程
    IEnumerator HitEffectRoutine(bool isWall, bool isBoss, bool isPlay)
    {
        // 播放动画
        if (hitAnimator != null)
        {
            string stateName = isBoss ? "boss" : (isPlay ? "build" : "ani_monster_hit");
            hitAnimator.Play(stateName, -1, 0);
        }
        
        // 从0到1的渐变
        float upDuration = 0.08f;
        float downDuration = 0.15f;
        float holdDuration = 0.05f;
        
        // 上升阶段：0 -> 1
        float timer = 0f;
        while (timer < upDuration)
        {
            timer += Time.deltaTime;
            anim_GlitterValue = Mathf.Lerp(0f, 1f, timer / upDuration);
            yield return null;
        }
        anim_GlitterValue = 1f;
        
        // 保持阶段
        yield return new WaitForSeconds(holdDuration);
        
        // 下降阶段：1 -> 0
        timer = 0f;
        while (timer < downDuration)
        {
            timer += Time.deltaTime;
            anim_GlitterValue = Mathf.Lerp(1f, 0f, timer / downDuration);
            yield return null;
        }
        anim_GlitterValue = 0f;
        
        hitEffectCoroutine = null;
    }
    
    void LateUpdate()
    {
        // 仅当值变化时更新所有Renderer
        if (Mathf.Abs(_innerGlitterValue - anim_GlitterValue) > 0.001f)
        {
            _innerGlitterValue = anim_GlitterValue;
            
            foreach (var kvp in rendererMPBDict)
            {
                UpdateRendererFillPhase(kvp.Key, _innerGlitterValue);
            }
        }
    }
    
    // 添加动态渲染器的方法
    public void AddRenderer(Renderer renderer)
    {
        if (renderer != null && !rendererMPBDict.ContainsKey(renderer))
        {
            CreateMPBForRenderer(renderer);
        }
    }
    
    // 移除渲染器
    public void RemoveRenderer(Renderer renderer)
    {
        if (renderer != null && rendererMPBDict.ContainsKey(renderer))
        {
            rendererMPBDict.Remove(renderer);
        }
    }
}