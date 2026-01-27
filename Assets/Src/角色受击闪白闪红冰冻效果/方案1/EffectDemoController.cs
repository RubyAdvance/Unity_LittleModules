using UnityEngine;
using UnityEngine.UI;

public class EffectDemoController : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Button hitButton;
    [SerializeField] private Button frozenButton;
    [SerializeField] private Button flashRedButton;
    
    [Header("目标敌人")]
    [SerializeField] private HitEffectController[] enemies;
    
    [Header("效果参数")]
    [SerializeField] private Color flashRedColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color flashWhiteColor = Color.white;
    [SerializeField] private Color frozenColor = new Color(0.5f, 0.8f, 1f, 1f);
    
    void Start()
    {
        // 按钮事件绑定
        if (hitButton != null)
            hitButton.onClick.AddListener(OnHitButtonClicked);
        
        if (frozenButton != null)
            frozenButton.onClick.AddListener(OnFrozenButtonClicked);
        
        if (flashRedButton != null)
            flashRedButton.onClick.AddListener(OnFlashRedButtonClicked);
        
        // 如果没有手动指定敌人，尝试自动查找
        if (enemies == null || enemies.Length == 0)
        {
            enemies = FindObjectsOfType<HitEffectController>();
        }
    }
    
    void OnHitButtonClicked()
    {
        TriggerAllEnemiesHit(flashWhiteColor);
    }
    
    void OnFrozenButtonClicked()
    {
        TriggerAllEnemiesFrozen(frozenColor);
    }
    
    void OnFlashRedButtonClicked()
    {
        TriggerAllEnemiesHit(flashRedColor);
    }
    
    void TriggerAllEnemiesHit(Color color)
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.TriggerHitEffect(color);
            }
        }
    }
    
    void TriggerAllEnemiesFrozen(Color color)
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.TriggerFrozenEffect(color);
            }
        }
    }
    
    // 为敌人动态添加HitEffectController
    public void SetupEnemy(GameObject enemyObject)
    {
        if (enemyObject == null) return;
        
        var renderer = enemyObject.GetComponent<Renderer>();
        if (renderer == null) return;
        
        var hitEffect = enemyObject.GetComponent<HitEffectController>();
        if (hitEffect == null)
        {
            hitEffect = enemyObject.AddComponent<HitEffectController>();
            hitEffect.SetHitDuration(0.1f);
            hitEffect.SetFrozenDuration(1f);
        }
    }
    
    void OnDestroy()
    {
        // 清理按钮事件
        if (hitButton != null)
            hitButton.onClick.RemoveAllListeners();
        
        if (frozenButton != null)
            frozenButton.onClick.RemoveAllListeners();
        
        if (flashRedButton != null)
            flashRedButton.onClick.RemoveAllListeners();
    }
}