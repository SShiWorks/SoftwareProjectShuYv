using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static UnityEditor.ShaderData;
using System.IO;
/// <summary>
///登录脚本<br/>
///注册
///登录
///找回密码
///第三方登录
///
/// </summary>

public class Login : MonoBehaviour
{
    public bool isDebug = false;
    public TMP_InputField account;
    public TMP_InputField password;
    public TMP_InputField forgotAccount;
    public TMP_InputField forgotCredentCode;
    public TMP_InputField forgotNewPassword;
    public TMP_Text errorText;
    private readonly string APIURL = "https://api.oauth.sus.xyz/shiyu/oauth2/";
    public static Login LoginAPI;
    private LoginState isCredentialCorrect = LoginState.AccountOrPasswordError;
    public LoginState isCredentialCheck;
    public GameObject forgotPasswordPanel;
    private readonly string tokenPath = "Assets/Token/";
    public enum LoginState
    {
        AccountOrPasswordError,
        LoginSuccess,
        LoginFail,
        CredentialSuccess,
        NoState,
        NoAccessToken,
        NoResult
    }

    // Start is called before the first frame update
    void Start()
    {
        //#if UNITY_EDITOR
        //        isDebug = true;
        //#else
        //        isDebug =false
        //#endif

        //登录API静态实例
        LoginAPI = this;

        SetActivateFalse();
    }
    ///<summary>
    ///登录
    /// </summary>
    public void LogIn()
    {
        if (isDebug)
        {
            print("登录测试111");
            SceneManager.LoadScene("MainPageOne");
            print("登录测试");
        }
        else
        {
            StartCoroutine(LoginWithScrectGetToken(password.text));
            StartCoroutine(CheckCredential());
            StartCoroutine(WaitForCredentialCheck());
        }        
    }

    /// <summary>
    /// 确认登录状态
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForCredentialCheck()
    {
        yield return new WaitUntil(() => isCredentialCorrect != LoginState.AccountOrPasswordError);
        if (isCredentialCorrect != LoginState.LoginFail &&
            isCredentialCorrect != LoginState.LoginSuccess &&
            isCredentialCorrect != LoginState.NoState)
        {
            File.WriteAllText("Assets/CSV/account",null);
            SceneManager.LoadScene("MainPageOne");
            File.AppendAllText("Assets/CSV/account", account.text + "\n");
        }
        else
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "账号或密码错误";
            Invoke(nameof(SetActivateFalse), 2);

        }
    }

    private string Encodepasswd(string passwd)
    {
        // 加密
        SHA256 sha256Sc = SHA256.Create();
        byte[] inputeBytes = Encoding.ASCII.GetBytes(passwd);
        byte[] hashByte = sha256Sc.ComputeHash(inputeBytes);

        // 字节转换为字符串
        StringBuilder sb = new();
        for (int i = 0; i < hashByte.Length; i++)
        {
            sb.Append(hashByte[i].ToString("X2"));
        }
        string hashedPassword = sb.ToString();
        return hashedPassword;
    }

    /// <summary>
    /// 登录获取Token
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoginWithScrectGetToken(string passwd)
    {
        string hashedPassword = Encodepasswd(passwd);

        // 请求access token
        string url = APIURL + "accessToken?password=" + UnityWebRequest.EscapeURL(hashedPassword);
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // 存储Token
            PlayerPrefs.SetString("access_token", www.downloadHandler.text); // 存储
            PlayerPrefs.Save(); // 保存
        }
    }
    public void ForgetPasswordPanel()
    {
        forgotPasswordPanel.SetActive(true);

    }
    public void GetCredentCode()
    {
        if (forgotAccount.text.Length == 0)
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "请输入账号";
            Invoke(nameof(SetActivateFalse), 2);
            return;
        }
        string url = APIURL + "get_code?account=" + UnityWebRequest.EscapeURL(forgotAccount.text);
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SendWebRequest();
    }/// <summary>
     /// 设置找回button
     /// </summary>
    public void ChangePassword()
    {
        string hashedPassword = Encodepasswd(forgotNewPassword.text);

        WWWForm form = new();
        form.AddField("account", UnityWebRequest.EscapeURL(forgotAccount.text));
        form.AddField("code", UnityWebRequest.EscapeURL(forgotCredentCode.text));
        form.AddField("new_password", UnityWebRequest.EscapeURL(hashedPassword));

        UnityWebRequest www = UnityWebRequest.Post(APIURL + "change_password", form);
        www.SendWebRequest();
        forgotPasswordPanel.SetActive(false);
        forgotNewPassword.text = "";
        forgotCredentCode.text = "";
        forgotAccount.text = "";
        errorText.text = "修改成功";
        errorText.gameObject.SetActive(true);
        Invoke(nameof(SetActivateFalse), 2);

    }
    /// <summary>
    /// 确认Token
    /// </summary>
    /// <returns></returns>
    public IEnumerator CheckCredential()
    {
        string storedToken = PlayerPrefs.GetString("access_token");
        if (string.IsNullOrEmpty(storedToken))
        {
            isCredentialCorrect = LoginState.LoginFail;
            isCredentialCheck= LoginState.NoAccessToken;
            StartCoroutine(SetCredentialCheckStateDefault());
            yield return false;
        }

        // 使用token检查
        string url = APIURL + "accesstokencheck?token=" + UnityWebRequest.EscapeURL(storedToken);
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            isCredentialCorrect = LoginState.LoginFail;
            isCredentialCheck = LoginState.NoResult;
            StartCoroutine(SetCredentialCheckStateDefault());
            yield return false;
        }
        else
        {
            isCredentialCorrect = LoginState.LoginSuccess;
            isCredentialCheck = LoginState.CredentialSuccess;
            StartCoroutine(SetCredentialCheckStateDefault());
            yield return true;
        }
    }

    /// <summary>
    /// 弹出错误面板
    /// </summary>
    public void SetActivateFalse()
    {
        errorText.gameObject.SetActive(false);
    }
    /// <summary>
    /// 重置鉴权状态
    /// </summary>
    /// <returns></returns>
    IEnumerator SetCredentialCheckStateDefault()
    {
        yield return new WaitForSeconds(3);
        isCredentialCheck = LoginState.NoState;
    }

}

