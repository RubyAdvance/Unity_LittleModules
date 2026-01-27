using UnityEngine;
using System.Collections;

public class HitEffectController : MonoBehaviour
{
    [Header("效果参数")]
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private Color frozenColor = new Color(0.5f, 0.8f, 1f, 1f);
    
    [Space]
    [SerializeField, Range(0.01f, 2f)] private float hitDuration = 0.1f;
    [SerializeField, Range(0.1f, 5f)] private float frozenDuration = 1f;
    [SerializeField, Range(0.5f, 3f)] private float iceBrightness = 1.2f;
    
    [Header("渲染组件")]
    [SerializeField] private Renderer targetRenderer; // 可以是SpriteRenderer或SkeletonRenderer
    private MaterialPropertyBlock materialPropertyBlock;
    private Material originalMaterial;
    private Material hitEffectMaterial;
    
    // Shader属性ID
    private static readonly int HitColorID = Shader.PropertyToID("_HitColor");
    private static readonly int HitAmountID = Shader.PropertyToID("_HitAmount");
    private static readonly int HitFadeID = Shader.PropertyToID("_HitFade");
    private static readonly int FrozenColorID = Shader.PropertyToID("_FrozenColor");
    private static readonly int FrozenAmountID = Shader.PropertyToID("_FrozenAmount");
    private static readonly int IceBrightnessID = Shader.PropertyToID("_IceBrightness");
    private static readonly int FrozenFadeID = Shader.PropertyToID("_FrozenFade");
    
    // 状态
    private Coroutine currentHitCoroutine;
    private Coroutine currentFrozenCoroutine;
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();
        
        if (targetRenderer == null)
        {
            Debug.LogError("找不到Renderer组件！");
            return;
        }
        
        // 保存原始材质
        originalMaterial = targetRenderer.sharedMaterial;
        
        // 创建效果材质实例
        hitEffectMaterial = new Material(Shader.Find("Custom/HitEffect"));
        hitEffectMaterial.CopyPropertiesFromMaterial(originalMaterial);
        
        // 初始化MaterialPropertyBlock
        materialPropertyBlock = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(materialPropertyBlock);
        
        // 设置初始值
        materialPropertyBlock.SetColor(HitColorID, hitColor);
        materialPropertyBlock.SetFloat(HitAmountID, 0f);
        materialPropertyBlock.SetFloat(HitFadeID, 1f / hitDuration);
        
        materialPropertyBlock.SetColor(FrozenColorID, frozenColor);
        materialPropertyBlock.SetFloat(FrozenAmountID, 0f);
        materialPropertyBlock.SetFloat(IceBrightnessID, iceBrightness);
        materialPropertyBlock.SetFloat(FrozenFadeID, 1f / frozenDuration);
        
        targetRenderer.SetPropertyBlock(materialPropertyBlock);
        targetRenderer.material = hitEffectMaterial;
    }
    
    // 触发受击效果（闪白/闪红）
    public void TriggerHitEffect(Color? customColor = null)
    {
        if (targetRenderer == null) return;
        
        Color effectColor = customColor ?? hitColor;
        
        // 更新颜色
        materialPropertyBlock.SetColor(HitColorID, effectColor);
        materialPropertyBlock.SetFloat(HitFadeID, 1f / hitDuration);
        targetRenderer.SetPropertyBlock(materialPropertyBlock);
        
        // 停止之前的受击协程
        if (currentHitCoroutine != null)
            StopCoroutine(currentHitCoroutine);
        
        // 开始新的受击效果
        currentHitCoroutine = StartCoroutine(HitEffectRoutine());
    }
    
    private IEnumerator HitEffectRoutine()
    {
        float timer = 0f;
        
        // 瞬间达到最大强度
        materialPropertyBlock.SetFloat(HitAmountID, 1f);
        targetRenderer.SetPropertyBlock(materialPropertyBlock);
        
        // 逐渐消退
        while (timer < hitDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / hitDuration;
            float hitAmount = 1f - normalizedTime;
            
            materialPropertyBlock.SetFloat(HitAmountID, hitAmount);
            targetRenderer.SetPropertyBlock(materialPropertyBlock);
            
            yield return null;
        }
        
        // 确保完全消退
        materialPropertyBlock.SetFloat(HitAmountID, 0f);
        targetRenderer.SetPropertyBlock(materialPropertyBlock);
        
        currentHitCoroutine = null;
    }
    
    // 触发冰冻效果
    public void TriggerFrozenEffect(Color? customColor = null)
    {
        if (targetRenderer == null) return;
        
        Color effectColor = customColor ?? frozenColor;
        
        // 更新参数
        materialPropertyBlock.SetColor(FrozenColorID, effectColor);
        materialPropertyBlock.SetFloat(FrozenFadeID, 1f / frozenDuration);
        materialPropertyBlock.SetFloat(IceBrightnessID, iceBrightness);
        targetRenderer.SetPropertyBlock(materialPropertyBlock);
        
        // 停止之前的冰冻协程
        if (currentFrozenCoroutine != null)
            StopCoroutine(currentFrozenCoroutine);
        
        // 开始新的冰冻效果
        currentFrozenCoroutine = StartCoroutine(FrozenEffectRoutine());
    }
    
    private IEnumerator FrozenEffectRoutine()
    {
        float timer = 0f;
        
        // 渐变进入
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / 0.1f;
            
            materialPropertyBlock.SetFloat(FrozenAmountID, normalizedTime);
            targetRenderer.SetPropertyBlock(materialPropertyBlock);
            
            yield return null;
        }
        
        // 保持最大强度
        materialPropertyBlock.SetFloat(FrozenAmountID, 1f);
        targetRenderer.SetPropertyBlock(materialPropertyBlock);
        
        // 保持冰冻状态
        yield return new WaitForSeconds(frozenDuration - 0.2f);
        
        timer = 0f;
        
        // 渐变退出
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / 0.1f;
            
            materialPropertyBlock.SetFloat(FrozenAmountID, 1f - normalizedTime);
            targetRenderer.SetPropertyBlock(materialPropertyBlock);
            
            yield return null;
        }
        
        // 确保完全消退
        materialPropertyBlock.SetFloat(FrozenAmountID, 0f);
        targetRenderer.SetPropertyBlock(materialPropertyBlock);
        
        currentFrozenCoroutine = null;
    }
    
    // 停止所有效果
    public void StopAllEffects()
    {
        if (currentHitCoroutine != null)
        {
            StopCoroutine(currentHitCoroutine);
            currentHitCoroutine = null;
        }
        
        if (currentFrozenCoroutine != null)
        {
            StopCoroutine(currentFrozenCoroutine);
            currentFrozenCoroutine = null;
        }
        
        if (targetRenderer != null && materialPropertyBlock != null)
        {
            materialPropertyBlock.SetFloat(HitAmountID, 0f);
            materialPropertyBlock.SetFloat(FrozenAmountID, 0f);
            targetRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
    
    // 参数设置方法（可以在编辑器或运行时调整）
    public void SetHitDuration(float duration) => hitDuration = Mathf.Max(0.01f, duration);
    public void SetFrozenDuration(float duration) => frozenDuration = Mathf.Max(0.1f, duration);
    public void SetHitColor(Color color) => hitColor = color;
    public void SetFrozenColor(Color color) => frozenColor = color;
    public void SetIceBrightness(float brightness) => iceBrightness = Mathf.Clamp(brightness, 0.5f, 3f);
    
    void OnDestroy()
    {
        StopAllEffects();
        
        // 恢复原始材质
        if (targetRenderer != null && originalMaterial != null)
        {
            targetRenderer.material = originalMaterial;
        }
        
        // 销毁创建的材质
        if (hitEffectMaterial != null)
        {
            Destroy(hitEffectMaterial);
        }
    }
}