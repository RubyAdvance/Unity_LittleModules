using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 签到倒计时管理器（挂载到UI文本对象）
/// </summary>
[RequireComponent(typeof(Text))]
public class SignCountdownManager : MonoBehaviour
{
    private Text _countdownText;
    private Coroutine _countdownCoroutine;
    private float _remainingSeconds;
    private bool _isCounting = false;

    private void Awake()
    {
        _countdownText = GetComponent<Text>();
        _countdownText.text = "";
    }

    /// <summary>
    /// 启动倒计时
    /// </summary>
    /// <param name="initialSeconds">初始剩余秒数</param>
    public void StartCountdown(float initialSeconds)
    {
        if (initialSeconds <= 0)
        {
            StopCountdown();
            return;
        }
        
        _remainingSeconds = initialSeconds;
        _isCounting = true;
        
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
        }
        
        _countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    /// <summary>
    /// 停止倒计时
    /// </summary>
    public void StopCountdown()
    {
        _isCounting = false;
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }
        _countdownText.text = "";
    }

    /// <summary>
    /// 倒计时协程
    /// </summary>
    private IEnumerator CountdownCoroutine()
    {
        while (_isCounting && _remainingSeconds > 0)
        {
            // 转换秒数为时:分:秒（01:20:30）
            int hours = (int)(_remainingSeconds / 3600);
            int minutes = (int)((_remainingSeconds % 3600) / 60);
            int seconds = (int)(_remainingSeconds % 60);
            
            _countdownText.text = $"下次签到剩余：{hours:D2}:{minutes:D2}:{seconds:D2}";
            
            yield return new WaitForSeconds(1f);
            _remainingSeconds -= 1;
        }
        
        // 倒计时结束
        _countdownText.text = "";
        _isCounting = false;
        _countdownCoroutine = null;
    }

    private void OnDestroy()
    {
        StopCountdown();
    }
}