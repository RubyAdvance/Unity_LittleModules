using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_方案3 : MonoBehaviour
{
    public Monster_方案3 monster_sprite;
    public Button hitBtn;
    public Button frezzBtn;

    void Awake()
    {
        hitBtn.onClick.AddListener(() =>
        {
            //受伤
            monster_sprite.TakeDamage();
        });
        frezzBtn.onClick.AddListener(() =>
        {
            //冰冻
            monster_sprite.OnIce();
        });

    }
}
