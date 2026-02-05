using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("摇杆对象 → 拖拽赋值即可（和之前一致）")]
    [SerializeField] private GameObject joystickRoot;
    [SerializeField] private RectTransform fixedBg;
    [SerializeField] private RectTransform moveBtn;

    private float joystickMaxRadius;
    private bool isTouching = false;
    private RectTransform canvasRect;

    public static Vector2 moveDirection;

    private void Start()
    {
        if (joystickRoot != null) joystickRoot.SetActive(false);
        if (fixedBg != null) joystickMaxRadius = fixedBg.sizeDelta.x / 2;
        canvasRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // 电脑鼠标 + 手机触摸 双兼容
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            HandleTouch(touch.position, touch.phase);
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) HandleTouch(Input.mousePosition, TouchPhase.Began);
            else if (Input.GetMouseButton(0)) HandleTouch(Input.mousePosition, TouchPhase.Moved);
            else if (Input.GetMouseButtonUp(0)) HandleTouch(Input.mousePosition, TouchPhase.Ended);
        }
    }

    private void HandleTouch(Vector2 screenPos, TouchPhase phase)
    {
        if (joystickRoot == null || fixedBg == null || moveBtn == null || canvasRect == null) return;

        switch (phase)
        {
            case TouchPhase.Began:
                isTouching = true;
                joystickRoot.SetActive(true);
                // 零误差坐标转换，摇杆精准贴合点击位置
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 joystickPos);
                joystickRoot.GetComponent<RectTransform>().anchoredPosition = joystickPos;
                moveBtn.anchoredPosition = Vector2.zero;
                moveDirection = Vector2.zero;
                break;

            case TouchPhase.Moved:
                if (isTouching)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(fixedBg, screenPos, null, out Vector2 offsetPos);
                    offsetPos = LimitJoystickMove(offsetPos);
                    moveBtn.anchoredPosition = offsetPos;
                    moveDirection = offsetPos.normalized;
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isTouching = false;
                joystickRoot.SetActive(false);
                moveBtn.anchoredPosition = Vector2.zero;
                moveDirection = Vector2.zero;
                break;
        }
    }

    // 限制内圆不超外圆边界
    private Vector2 LimitJoystickMove(Vector2 targetPos)
    {
        float distance = Vector2.Distance(targetPos, Vector2.zero);
        if (distance > joystickMaxRadius)
        {
            targetPos = targetPos.normalized * joystickMaxRadius;
        }
        return targetPos;
    }
}