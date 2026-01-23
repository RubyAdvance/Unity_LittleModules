using System;
using UnityEngine;
using Newtonsoft.Json;
/// <summary>
/// 签到数据本地化存储工具（基于PlayerPrefs）
/// </summary>
public static class SignDataPersistence
{
    /// <summary>
    /// PlayerPrefs存储的Key（唯一标识签到数据）
    /// </summary>
    private const string SignDataKey = "Player_Sign_System_Data";

    #region 存储签到数据
    /// <summary>
    /// 将玩家签到数据存储到PlayerPrefs（JSON序列化）
    /// </summary>
    /// <param name="signData">要存储的签到数据</param>
    public static void SaveSignData(PlayerSignData signData)
    {
        if (signData == null)
        {
            Debug.LogError("签到数据为空，无法存储");
            return;
        }

        try
        {
            // 方案1：使用Newtonsoft.Json（推荐，支持复杂类型/空值）
            string jsonData = JsonConvert.SerializeObject(signData);
            
            // 方案2：使用Unity内置JsonUtility（无需额外包，注意：需PlayerSignData加[Serializable]）
            // string jsonData = JsonUtility.ToJson(signData);

            // 存储到PlayerPrefs
            PlayerPrefs.SetString(SignDataKey, jsonData);
            PlayerPrefs.Save(); // 立即保存，避免数据丢失
            Debug.Log("签到数据存储成功");
        }
        catch (Exception e)
        {
            Debug.LogError($"签到数据存储失败：{e.Message}");
        }
    }
    #endregion

    #region 读取签到数据
    /// <summary>
    /// 从PlayerPrefs读取玩家签到数据（JSON反序列化）
    /// </summary>
    /// <returns>读取的签到数据；无数据则返回新的空数据</returns>
    public static PlayerSignData LoadSignData()
    {
        try
        {
            // 检查是否有存储的数据
            if (!PlayerPrefs.HasKey(SignDataKey))
            {
                Debug.Log("无已存储的签到数据，返回新数据");
                return new PlayerSignData(); // 返回空数据，避免空指针
            }

            // 读取JSON字符串
            string jsonData = PlayerPrefs.GetString(SignDataKey);
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.Log("签到数据为空，返回新数据");
                return new PlayerSignData();
            }

            // 方案1：Newtonsoft.Json反序列化
            PlayerSignData signData = JsonConvert.DeserializeObject<PlayerSignData>(jsonData);
            
            // 方案2：Unity内置JsonUtility反序列化
            // PlayerSignData signData = JsonUtility.FromJson<PlayerSignData>(jsonData);

            // 安全校验：避免反序列化失败返回null
            return signData ?? new PlayerSignData();
        }
        catch (Exception e)
        {
            Debug.LogError($"签到数据读取失败：{e.Message}，返回新数据");
            return new PlayerSignData();
        }
    }
    #endregion

    #region 清空签到数据（测试/重置用）
    /// <summary>
    /// 清空PlayerPrefs中的签到数据（测试时重置用）
    /// </summary>
    public static void ClearSignData()
    {
        if (PlayerPrefs.HasKey(SignDataKey))
        {
            PlayerPrefs.DeleteKey(SignDataKey);
            PlayerPrefs.Save();
            Debug.Log("签到数据已清空");
        }
        else
        {
            Debug.Log("无签到数据可清空");
        }
    }
    #endregion
}