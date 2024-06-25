using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ScoreInquire : MonoBehaviour
{
    private Score score;
    public bool isServer = false;
    public GameObject showScorePanel;
    public string DataBaseUrl = "mysql:3306";
    private readonly string DataBaseName = "grade_manage_system";
    private readonly string DataUser = "stumanage";
    private readonly string DataPassword = "stumanage";
    private string student_id = "21004190211";
    private MySqlConnection conn; //数据库连接器

    // Start is called before the first frame update
    void Start()
    {
        //连接数据库
        var connStr =
            "server=" + DataBaseUrl +
            ";database=" + DataBaseName +
            ";user=" + DataUser +
            ";password=" + DataPassword;
        conn = new MySqlConnection(connStr);
        SetInquiryFuncButton();
        OnInquiryFuncChanged(0);
        SelectYearButton(0);
        SelectTermButton(0);
        OnbutMajor();
        OnbutEffective();
        showScorePanel.SetActive(false);
    }

    #region 查询方式选择：按学年、按学期、全部

    //缓存数据，学年、学期、查询方式
    private string currentYear;
    private string currentTerm;

    private string currentInquiryFunc;

    //绑定下拉菜单
    public TMP_Dropdown yearDropdown;
    public TMP_Dropdown termDropdown;
    public TMP_Dropdown inquiryFunc;

    private void SetInquiryFuncButton()
    {
        inquiryFunc.GetComponent<TMP_Dropdown>().AddOptions(new List<TMP_Dropdown.OptionData>()
        {
            new("按学期"),
            new("按学年"),
            new("入学以来")
        });
    }

    /// <summary>
    /// 查询时段选择
    /// </summary>
    /// <param name="value">时段</param>
    public void OnInquiryFuncChanged(int value)
    {
        switch (value)
        {
            case 0:
                yearDropdown.interactable = true;
                termDropdown.interactable = true;
                currentInquiryFunc = "InquiryFunc_term";
                break;
            case 1:
                yearDropdown.interactable = true;
                termDropdown.interactable = false;
                currentInquiryFunc = "InquiryFunc_year";
                currentTerm = null;
                break;
            case 2:
                yearDropdown.interactable = false;
                termDropdown.interactable = false;
                currentInquiryFunc = "InquiryFunc_all";
                currentTerm = null;
                currentYear = null;
                break;
        }
    }

    /// <summary>
    /// 学年选择
    /// </summary>
    /// <param name="year">
    /// 学年编号<br/>
    /// 2023-2024学年：0
    /// <br/>依此类推
    /// </param>
    public void SelectYearButton(int year)
    {
        currentYear = year switch
        {
            0 => "2023-2024学年",
            1 => "2022-2023学年",
            2 => "2021-2022学年",
            3 => "2020-2021学年",
            4 => "2019-2020学年",
            5 => "2018-2019学年",
            _ => "超过时限",
        };
    }

    /// <summary>
    /// 学期选择
    /// </summary>
    /// <param name="term">学期编号</param>
    private void SelectTermButton(int term)
    {
        currentTerm = term switch
        {
            0 => "第一学期",
            1 => "第二学期",
            2 => "第三学期",
            _ => "你是怎么找到这个选项的？",
        };
    }

    #endregion

    #region 主修辅修、成绩类型原始or有效

    public GameObject butMajor;
    public GameObject butMinor;
    private string currentMajor;

    /// <summary>
    /// 主修查询
    /// </summary>
    public void OnbutMajor() => currentMajor = ButtonSet(butMajor, butMinor, "主修");

    /// <summary>
    /// 辅修查询
    /// </summary>
    public void OnbutMinor() => currentMajor = ButtonSet(butMinor, butMajor, "辅修");

    public GameObject butOriginal;
    public GameObject butEffective;
    private string currentScoreType;

    /// <summary>
    /// 原始成绩
    /// </summary>
    public void OnbutOriginal() => currentScoreType = ButtonSet(butOriginal, butEffective, "原始");

    /// <summary>
    /// 有效成绩
    /// </summary>
    public void OnbutEffective() => currentScoreType = ButtonSet(butEffective, butOriginal, "有效");

    /// <summary>
    /// 设置按钮状态
    /// </summary>
    /// <param name="butTrue"></param>
    /// <param name="butFalse"></param>
    /// <param name="infoTake"></param>
    /// <returns></returns>
    private string ButtonSet(GameObject butTrue, GameObject butFalse, string infoTake)
    {
        butTrue.SetActive(true);
        butFalse.SetActive(false);
        print(infoTake);
        return infoTake;
    }

    #endregion

    /// <summary>
    /// 开始查询
    /// </summary>
    public void OnInquire()
    {
        if (isServer)
        {
            string query = null;
            if (currentInquiryFunc == "InquiryFunc_term")
            {
                query = InquireStuAndCusTerm();
            }
            else if (currentInquiryFunc == "InquiryFunc_year")
            {
                query = InquireStuAndCusYear();
            }
            else if (currentInquiryFunc == "InquiryFunc_all")
            {
                query = InquireStuAndCusAll();
            }
            else
            {
                print("查询方法错误");
            }

            GetScoresFromDatabase(query);
            ReadFromCsv();
        }
        else
        {
            ReadFromCsv();
        }

        showScorePanel.SetActive(true);
    }


    /// <summary>
    /// 学生的所有课程信息和成绩
    /// </summary>
    public string InquireStuAndCusAll()
    {
        string query = $"SELECT grade from student_course where student_id in({student_id})";
        return query;
    }

    /// <summary>
    /// 查询学年成绩
    /// </summary>
    public string InquireStuAndCusYear()
    {
        string query = $"SELECT grade from student_course where student_id in({student_id}) and year in({currentYear})";
        return query;
    }

    /// <summary>
    /// 查询学期成绩
    /// </summary>
    public string InquireStuAndCusTerm()
    {
        string query =
            $"SELECT grade from student_course where student_id in({student_id}) and year in({currentYear}) and term in({currentTerm})";
        return query;
    }

    /// <summary>
    /// 查询数据库
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public void GetScoresFromDatabase(string query)
    {
        try
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Score sco = Score.CreateScore(
                    reader["grade"].ToString(),
                    reader["course_id"].ToString(),
                    reader["course_name"].ToString(),
                    reader["credit"].ToString(),
                    reader["gpa"].ToString(),
                    reader["credit_gpa"].ToString(),
                    reader["year"].ToString(),
                    reader["term"].ToString(),
                    reader["minorORmajor"].ToString(),
                    reader["score_type"].ToString()
                );
                WriteToCsv(sco, isServer);
            }

            reader.Close();
        }
        catch (MySqlException ex)
        {
            Debug.Log(ex.Message);
        }
        finally
        {
            conn.Close();
        }
    }

    #region 显示成绩

    public Transform scoreContent;
    public GameObject scoreItemPrefab;

    /// <summary>
    /// 计算绩点
    /// </summary>
    /// <returns></returns>
    public float CalculateGPA()
    {
        float gpaa = ((float.Parse(score.Grade) - 60) / 10);
        return gpaa;
    }

    public float CalculateCreditGPA()
    {
        float credit_gpaa = float.Parse(score.Credit) * CalculateGPA();
        return credit_gpaa;
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="scor">成绩类</param>
    /// <param name="isServer">是否使用缓存，<br/>false为是</param>
    private static void WriteToCsv(Score scor, bool isServer)
    {
        string path = "Assets/CSV/Score.txt";
        if (isServer)
        {
            //删除文件内容
            File.WriteAllText(path, string.Empty);
        }

        //序列化数据
        var content = $"{scor.ID},{scor.Name},{scor.Grade}," +
                      $"{scor.Credit},{scor.Gpa},{scor.CreditGpa},{scor.Year}," +
                      $"{scor.Term},{scor.MinorOrMajor},{scor.ScoreType}";
        File.AppendAllText(path, content);
    }

    /// <summary>
    /// 读取缓存
    /// </summary>
    private void ReadFromCsv()
    {
        string path = "Assets/CSV/Score.txt";
        string[] content = File.ReadAllLines(path);
        foreach (var item in content)
        {
            string[] data = item.Split(',');
            score = Score.CreateScore(data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9]);
            ShowScore();
        }
    }

    /// <summary>
    /// 展示课程给
    /// </summary>
    public void ShowScore()
    {
        //实例化
        GameObject scoreItem = Instantiate(scoreItemPrefab, scoreContent);
        //获取子物体
        TextMeshProUGUI[] texts = scoreItem.GetComponentsInChildren<TextMeshProUGUI>();
        //赋值
        texts[0].text = score.ID;
        texts[1].text = score.Name;
        texts[2].text = score.Grade;
        texts[3].text = score.Credit;
        texts[4].text = score.Gpa;
        texts[5].text = score.CreditGpa;
        texts[6].text = score.Year;
        texts[7].text = score.Term;
        texts[8].text = score.MinorOrMajor;
        texts[9].text = score.ScoreType;
    }

    #endregion
}

/// <summary>
/// 成绩类
/// </summary>
public class Score
{
    public string Grade; //成绩
    public string Name; //课程名
    public string ID; //课程号
    public string Credit; //学分
    public string Gpa; //绩点
    public string CreditGpa; //学分绩点
    public string Year; //学年
    public string Term; //学期
    public string MinorOrMajor; //主修或辅修
    public string ScoreType; //成绩类型

    /// <summary>
    /// 构造函数
    /// </summary>
    private Score(string grade, string course_name, string course_id, string credit, string gpa,
        string course_creditGpa,
        string year, string term, string minorOrMajor, string course_scoreType)
    {
        Gpa = gpa;
        Credit = credit;
        ID = course_id;
        Name = course_name;
        CreditGpa = course_creditGpa;
        Year = year;
        Term = term;
        MinorOrMajor = minorOrMajor;
        ScoreType = course_scoreType;
        Grade = grade;
        Name = course_name;
    }

    /// <summary>
    /// 创建成绩
    /// </summary>
    public static Score CreateScore(string grade, string course_name, string course_id, string credit, string gpa,
        string course_creditGpa, string year, string term, string minorOrMajor, string course_scoreType)
    {
        return new Score(grade, course_name, course_id, credit, gpa, course_creditGpa, year, term, minorOrMajor,
            course_scoreType);
    }
}