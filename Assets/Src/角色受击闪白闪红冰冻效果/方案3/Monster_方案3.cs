using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster_方案3 : MonoBehaviour
{
    private EnemyMaterialController _matController;
    public GameObject selfGo;
    
    [Tooltip("Boss受击会闪白，普通受击会闪红")]
    public bool isBoss;

    [Header("冰冻效果配置")]
    [Tooltip("冰冻持续时长（秒），默认1秒")]
    public float freezeDuration = 1f; // 暴露到编辑器，传给控制器

    void Start()
    {
        // 初始化控制器，传入Boss判断逻辑
        _matController = new EnemyMaterialController(
            selfGo.GetComponentInChildren<Renderer>(),
            isBossCheck: () => isBoss 
        );

        // 1. 自定义受击参数
        _matController.EffectConfig.hitTextureFadeValue_Boss = 0.5f;
        _matController.EffectConfig.hitDuration_Normal = 0.3f;

        // 2. 将编辑器配置的冰冻时长传给控制器（核心：统一由控制器管理）
        _matController.EffectConfig.freezeDuration = freezeDuration;
    }

    void Update()
    {
        // 仅调用控制器的Update，所有计时逻辑都在控制器内
        _matController?.Update();
    }

    // 受击调用
    public void TakeDamage()
    {
        _matController?.OnHit(); // 自动区分Boss闪白/普通闪红
    }

    // 冰冻调用（仅触发，计时由控制器处理）
    public void OnIce()
    {
        _matController?.OnIce();
    }

    // 手动解冻（可选）
    public void OutIce()
    {
        _matController?.OutIce();
    }

    private void OnDestroy()
    {
        _matController?.Dispose();
    }
}