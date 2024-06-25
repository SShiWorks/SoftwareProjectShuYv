using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


internal enum NoticeType
{
    LevelInfo,
    LevelImport
}

/// <summary>
/// 通知系统
/// </summary>
public class NoticeSystem : MonoBehaviour
{
    private readonly string _apiUrl = "https://api.oauth.sus.xyz/shiyu/event/";

    private enum Identity
    {
        Student,
        Teacher,
        Admin
    }


    private readonly List<Notice> _notices = new();
    private Identity _identity;
    public GameObject noticePanel;
    public GameObject importantNoticePanel;
    public GameObject noticeContent;
    public GameObject noticeInfoPanel;

    //发布
    public GameObject publishButton;
    public GameObject publishSettingPanel;
    public GameObject publish;


   private void Start()
    {
        CheckIdentity();
        StartCoroutine(GetNotice());
        EnterScene();
    }

    private IEnumerator PublishNotice(Notice json)
    {
        UnityWebRequest www = new($"{_apiUrl}notice", "POST")
        {
            uploadHandler =
            new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(json))) //上传数据
        }; //构建请求
        www.SetRequestHeader("Content-Type", "application/json"); //设置请求头
        yield return www.SendWebRequest(); //发送请求
    }

    private void EnterScene()
    {
        switch (_identity)
        {
            case Identity.Teacher:
                publishButton.SetActive(true);
                publishButton.AddComponent<Button>().onClick.AddListener(() =>
                {
                    publishSettingPanel.SetActive(true);
                    publishSettingPanel.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                    {
                        //点击打开发布也米娜
                        publish.SetActive(true);
                        publish.transform.Find("TypeSet").transform.GetComponent<TMP_Dropdown>().AddOptions(
                            new List<string>
                            {
                                NoticeType.LevelInfo.ToString(),
                                NoticeType.LevelImport.ToString()
                            });
                        publish.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                        {
                            //点击发布
                            var title = publish.transform.GetChild(1).GetComponent<TMP_InputField>().text;
                            var content = publish.transform.GetChild(2).GetComponent<TMP_InputField>().text;
                            WWWForm form = new(); //构建表单
                            form.AddField("title", title);
                            form.AddField("Content", content);
                            form.AddField("Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            form.AddField("Author", File.ReadAllLines("Assets/CSV/Account")[1]);
                            form.AddField("Type",
                                publish.transform.Find("TypeSet").transform.GetComponent<TMP_Dropdown>().value
                                    .ToString());
                            form.AddField("Identity", _identity.ToString());
                            form.AddField("Id", Guid.NewGuid().ToString());
                            form.AddField("Status", "1");

                            var notice = Notice.FromJson(form.ToString());
                            StartCoroutine(PublishNotice(notice));
                        });
                    });
                });
                break;
            case Identity.Student:
                break;
            case Identity.Admin:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ShowNotice();
    }

    /// <summary>
    /// 展示消息
    /// </summary>
    private void ShowNotice()
    {
        noticePanel.SetActive(true);
        importantNoticePanel.SetActive(true);
        foreach (var noty in _notices)
        {
            switch (noty.Type)
            {
                case NoticeType.LevelInfo:
                {
                    var notice = Instantiate(noticeInfoPanel, noticePanel.transform);
                    notice.transform.GetChild(0).GetComponent<Text>().text = noty.Title;
                    notice.transform.GetChild(1).GetComponent<Text>().text = noty.Time;
                    notice.transform.GetChild(2).GetComponent<Text>().text = noty.Author;
                    notice.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
                    {
                        noticeContent.SetActive(true);
                        noticeContent.transform.GetChild(0).GetComponent<Text>().text = noty.Title;
                        noticeContent.transform.GetChild(1).GetComponent<Text>().text = noty.Content;
                    });
                    break;
                }
                case NoticeType.LevelImport:
                {
                    var notice = Instantiate(noticeInfoPanel, importantNoticePanel.transform);
                    notice.transform.GetChild(0).GetComponent<Text>().text = noty.Title;
                    notice.transform.GetChild(1).GetComponent<Text>().text = noty.Time;
                    notice.transform.GetChild(2).GetComponent<Text>().text = noty.Author;
                    notice.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
                    {
                        noticeContent.SetActive(true);
                        noticeContent.transform.GetChild(0).GetComponent<Text>().text = noty.Title;
                        noticeContent.transform.GetChild(1).GetComponent<Text>().text = noty.Content;
                    });
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(); //抛出异常
            }
        }
    }

    /// <summary>
    /// 判断身份
    /// </summary>
    private void CheckIdentity()
    {
        var account = File.ReadAllLines("Assets/CSV/Account")[1];
        _identity = account.Length switch
        {
            //账号长度大于10为学生
            11 => Identity.Student,
            //账号长度5-10为教师
            > 5 and < 10 => Identity.Teacher,
            _ => Identity.Admin
        };
    }

    /// <summary>
    /// 获取消息
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetNotice()
    {
        var request = UnityWebRequest.Get($"{_apiUrl}notice?identity=" + _identity);
        yield return request.SendWebRequest();
        switch (request.result)
        {
            case UnityWebRequest.Result.ConnectionError:
                Debug.Log(request.error);
                break;
            default:
                try
                {
                    var notices = JsonConvert.DeserializeObject<List<Notice>>(request.downloadHandler.text);
                    _notices.AddRange(notices);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }

                break;
        }
    }
}

/// <summary>
/// 
/// </summary>
internal class Notice
{
    public string Title;
    public string Content;
    public string Time;
    public string Author;
    public NoticeType Type;
    public string Identity;
    public string Id;
    public string Status;

    internal Notice(string json)
    {
        var notice = JsonUtility.FromJson<Notice>(json);
        Title = notice.Title;
        Content = notice.Content;
        Time = notice.Time;
        Author = notice.Author;
        Type = notice.Type;
        Identity = notice.Identity;
        Id = notice.Id;
        Status = notice.Status;
    }


    public static Notice FromJson(string json)
    {
        return JsonUtility.FromJson<Notice>(json);
    }
}