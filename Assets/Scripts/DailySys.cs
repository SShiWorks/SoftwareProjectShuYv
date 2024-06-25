using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;


public class DailySys : MonoBehaviour
{
    private readonly string _apiUrl = "https://api.oauth.sus.xyz/shiyu/daily/";
    private List<Daily> _dailies = new();
    private List<Lesson> _lessons = new();

    //定义面板和各种输入框
    public Transform lessonGroup;
    public GameObject addDailyButton;
    public GameObject addDailyPanel;
    public TMP_Text dailyName;
    public TMP_Text dailyContent;
    public TMP_Text dailyTime;
    public TMP_Text dailyWeek;
    private string _dailyId;

    private void Start()
    {
        EnterScene();
        addDailyPanel.SetActive(false);
    }

    /// <summary>
    /// 进入场景
    /// </summary>
    private void EnterScene()
    {
        ShowLessons();
        ShowDailies();
        StartCoroutine(GetLessons());
        StartCoroutine(GetDaily());
    }

    public void OpenAddDailyPanel()
    {
        addDailyButton.SetActive(false);
        addDailyPanel.SetActive(true);
    }

    public void AddDaily()
    {
        _dailyId = Guid.NewGuid().ToString();
        List<Daily> ariaDaily = new()
        {
            Daily.CreateDaily(_dailyId, dailyName.text, dailyContent.text, dailyTime.text, dailyWeek.text)
        };
        StartCoroutine(PostDaily(ariaDaily));
    }

    private IEnumerator PostDaily(List<Daily> creatDailies)
    {
        UnityWebRequest www = new(_apiUrl + "daily", "POST");
        WWWForm form = new();
        foreach (var daily in creatDailies)
        {
            //添加表单数据
            form.AddField("DailyId", daily.DailyId);
            form.AddField("DailyName", daily.DailyName);
            form.AddField("Content", daily.Content);
            form.AddField("Time", daily.Time);
            form.AddField("Week", daily.Week);
        }

        www.uploadHandler = new UploadHandlerRaw(form.data);
        yield return www.SendWebRequest();
    }

    /// <summary>
    /// 展示课程
    /// </summary>
    private void ShowLessons()
    {
        var rootPath = "";
        foreach (var lesson in _lessons)
        {
            //遍历课程
            switch (int.Parse(lesson.Week))
            {
                //根据星期几设置面板路径
                case 1:
                    rootPath = "day1";
                    break;
                case 2:
                    rootPath = "day2";
                    break;
                case 3:
                    rootPath = "day3";
                    break;
                case 4:
                    rootPath = "day4";
                    break;
                case 5:
                    rootPath = "day5";
                    break;
                case 6:
                    rootPath = "day6";
                    break;
                case 7:
                    rootPath = "day7";
                    break;
                default: break;
            }

            if (int.Parse(lesson.Length) == 2)
            {
                //判断课程长度，如果是长课程
                var content = lesson.LessonName + "\n" + lesson.Teacher + "\n" + lesson.Classroom;
                ShowData(rootPath, MorningOrAfternoon(lesson.StartTime), content);
            }

            if (int.Parse(lesson.Length) == 1)
            {
                //判断课程长度，如果是短课程
                var content = lesson.LessonName + "\n" + lesson.Teacher + "\n" + lesson.Classroom;
                ChiPanelSet(rootPath, MorningOrAfternoon(lesson.StartTime),
                    int.Parse(lesson.StartTime) % 2 == 0 ? "down" : "up", content);
            }
        }
    }

    /// <summary>
    /// 最里层的panel设置
    /// </summary>
    /// <param name="day">日期</param>
    /// <param name="moa">上下午</param>
    /// <param name="pos">1/2还是3/4节</param>
    /// <param name="content">内容</param>
    private void ChiPanelSet(string day, string moa, string pos, string content)
    {
        //设置最里层的panel::短课程展示位置

        var insetPanel = new GameObject("InsetPanel"); //创建一个新的GameObject
        var fatherPanel = lessonGroup.Find(day + "/" + moa); //找到父物体
        insetPanel.transform.SetParent(fatherPanel); //设置父物体
        var rect = insetPanel.AddComponent<RectTransform>(); //添加RectTransform组件
        // 设置锚点为 stretch
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        // 根据位置信息设置子物体的位置
        switch (pos)
        {
            case "down":
                // 覆盖在父物体的下半部分
                rect.offsetMin = new Vector2(0, 0);
                rect.offsetMax = new Vector2(0, -rect.parent.GetComponent<RectTransform>().rect.height / 2);
                break;
            case "up":
                // 覆盖在父物体的上半部分
                rect.offsetMin = new Vector2(0, rect.parent.GetComponent<RectTransform>().rect.height / 2);
                rect.offsetMax = new Vector2(0, 0);
                break;
        }

        insetPanel.AddComponent<TMP_Text>().text = content;
    }

    /// <summary>
    /// 判断上下午
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private static string MorningOrAfternoon(string time)
    {
        var childPath = "";
        switch (int.Parse(time))
        {
            case 1:
            case 2:
                childPath = "classup";
                break;
            case 3:
            case 4:
                childPath = "classdown";
                break;
        }

        return childPath;
    }

    /// <summary>
    /// 展示大课
    /// </summary>
    /// <param name="day">周几</param>
    /// <param name="moa">上下午</param>
    /// <param name="content">内容</param>
    private void ShowData(string day, string moa, string content)
    {
        lessonGroup.Find(day + "/" + moa).AddComponent<TMP_Text>().text = content;
    }

    /// <summary>
    /// 展示日常
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">抛出异常</exception>
    private void ShowDailies()
    {
        foreach (var ariaDaily in _dailies)
        {
            var rootPath = "";
            var day = int.Parse(ariaDaily.Week) switch
            {
                //根据星期几设置面板路径
                1 => rootPath = "day1",
                2 => rootPath = "day2",
                3 => rootPath = "day3",
                4 => rootPath = "day4",
                5 => rootPath = "day5",
                6 => rootPath = "day6",
                7 => rootPath = "day7",
                _ => throw new ArgumentOutOfRangeException()
            };
            var content = ariaDaily.DailyName + "\n" + ariaDaily.Content;
            if (_lessons.Exists(lesson => lesson.Week == ariaDaily.Week && lesson.StartTime == ariaDaily.Time))
            {
                return;
            }

            //
            ChiPanelSet(rootPath, MorningOrAfternoon(ariaDaily.Time),
                int.Parse(ariaDaily.Time) % 2 == 0 ? "down" : "up", content);
        }
    }

    /// <summary>
    /// 获取课程表
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private IEnumerator GetLessons()
    {
        UnityWebRequest www = new(_apiUrl + "lessons", "GET");
        yield return www.SendWebRequest();
        switch (www.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError(www.error);
                break;
            case UnityWebRequest.Result.Success:
                var cont = www.downloadHandler.text;
                _lessons = JsonConvert.DeserializeObject<List<Lesson>>(cont);
                break;
            case UnityWebRequest.Result.InProgress:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// 获取日常
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private IEnumerator GetDaily()
    {
        UnityWebRequest www = new(_apiUrl + "daily", "GET");
        yield return www.SendWebRequest();
        switch (www.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError(www.error);
                break;
            case UnityWebRequest.Result.Success:
                if (www.downloadHandler.text == "[daily],[none]")
                {
                    break;
                }

                var cont = www.downloadHandler.text;
                _dailies = JsonConvert.DeserializeObject<List<Daily>>(cont);
                break;
            case UnityWebRequest.Result.InProgress:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

/// <summary>
/// 日程类
/// </summary>
internal class Daily
{
    public string DailyId;
    public string DailyName;
    public string Content;
    public string Time;
    public string Week;

    private Daily()
    {
    }

    public static Daily CreateDaily(string dailyId, string dailyName, string content, string time, string week)
    {
        return new Daily
        {
            DailyId = dailyId,
            DailyName = dailyName,
            Content = content,
            Time = time,
            Week = week
        };
    }
}

/// <summary>
/// 课程类
/// </summary>
internal class Lesson
{
    public string LessonId;
    public string LessonName;
    public string Teacher;
    public string Classroom;
    public string StartTime;
    public string Week;
    public string Length;

    public static Lesson CreateLesson(string lessonId, string lessonName, string teacher, string classroom, string time,
        string week, string length)
    {
        return new Lesson
        {
            LessonId = lessonId,
            LessonName = lessonName,
            Teacher = teacher,
            Classroom = classroom,
            StartTime = time,
            Week = week,
            Length = length,
        };
    }
}