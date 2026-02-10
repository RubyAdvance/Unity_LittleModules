using TMPro;
using UnityEngine;

public class ToastUIView : MonoBehaviour
{
    public Animator mAnimator;
    public TextMeshProUGUI Label;

    public float StayTime = 1.2f;

    public float MoveSpeed = 100;

    public float LifeTime = 3;



    private float _stayTime;
    private float _lifeTime;
    private bool hasInit = false;

    public void SetToast(string text, float duration = 0f) {
        Label.text = text;
        if (duration > 0)
        {
            StayTime = duration;
            LifeTime = duration + 2f;
        }
        mAnimator.Play("ani_ToastUIView", -1, 0);
    }

    void Update() {
        LifeTime -= Time.deltaTime;
        if (LifeTime <= 0) Destroy(gameObject);

        if (StayTime > 0)
        {
            StayTime -= Time.deltaTime;
            if (StayTime <= 0) {
                mAnimator.Play("ani_ToastUIView_close", -1, 0);
                // transform.position = new Vector3(transform.position.x, transform.position.y + MoveSpeed * Time.deltaTime, transform.position.z);
            }
        }
    }
}
