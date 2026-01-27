using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button hitBtn;
    public Monster monster_spine;
    public Monster monster_sprite;
    void Awake()
    {
        hitBtn.onClick.AddListener(() =>
        {
            monster_spine.TakeDamage(10);
            monster_sprite.TakeDamage(10);
        });
    }
}
