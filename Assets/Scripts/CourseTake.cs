using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 课程信息
/// </summary>
public class Course
{
    public string course_id;
    public string course_name;
    public string course_credit;
    public string course_teacher;
    public string course_time;
    public string course_place;
    public string course_week;
    public string course_type;
    public string course_year;
    public string course_term;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="course_id">课程id</param>
    /// <param name="course_name">课程名字</param>
    /// <param name="course_credit">学分</param>
    /// <param name="course_teacher">教授教师</param>
    /// <param name="course_time">上课时间</param>
    /// <param name="course_place">授课地点</param>
    /// <param name="course_week">课程周期</param>
    /// <param name="course_type">课程类型</param>
    /// <param name="course_year">学年</param>
    /// <param name="course_term">学期</param>
    private Course(
        string course_id, string course_name, string course_credit, string course_teacher, string course_time,
        string course_place, string course_week, string course_type, string course_year, string course_term)
    {
        this.course_id = course_id;
        this.course_name = course_name;
        this.course_credit = course_credit;
        this.course_teacher = course_teacher;
        this.course_time = course_time;
        this.course_place = course_place;
        this.course_week = course_week;
        this.course_type = course_type;
        this.course_year = course_year;
        this.course_term = course_term;
    }
    /// <summary>
    /// 添加课程信息
    /// </summary>
    public static Course CreateCourse(
        string course_id, string course_name, string course_credit, string course_teacher, string course_time,
        string course_place, string course_week, string course_type, string course_year, string course_term)
    {
        return new Course(
            course_id, course_name, course_credit, course_teacher, course_time,
            course_place, course_week, course_type, course_year, course_term);
    }
}

public class CourseTake : MonoBehaviour
{
    private MySqlConnection conn;
    public string DataBaseUrl = "mysql:3306";
    private readonly string DataBaseName = "grade_manage_system";
    private readonly string DataUser = "stumanage";
    private readonly string DataPassword = "stumanage";

   private void Start()
    {
        var connStr =
            "server=" + DataBaseUrl +
            ";database=" + DataBaseName +
            ";user=" + DataUser +
            ";password=" + DataPassword;
        conn = new MySqlConnection(connStr);
        CheckedCourse();
        SetCourseType();
        GetCoursesFromServer();
    }
    private void GetCoursesFromServer()
    {
        courses.Clear();
        try
        {
            conn.Open();
            const string sql = "select * from course";
            var cmd = new MySqlCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var course = Course.CreateCourse(
                    reader.GetString("course_id"),
                    reader.GetString("course_name"),
                    reader.GetString("course_credit"),
                    reader.GetString("course_teacher"),
                    reader.GetString("course_time"),
                    reader.GetString("course_place"),
                    reader.GetString("course_week"),
                    reader.GetString("course_type"),
                    reader.GetString("course_year"),
                    reader.GetString("course_term")
                );
                courses.Add(course);
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
    //测试数据
    public List<Course> courses = new()
    {
            Course.CreateCourse("1", "计算机网络", "3", "张三", "周一1-2节", "教1-101", "1-16周", "必修", "2021", "1"),
            Course.CreateCourse("2", "计算机组成原理", "3", "李四", "周二3-4节", "教1-102", "1-16周", "必修", "2021", "1"),
            Course.CreateCourse("3", "数据结构", "3", "王五", "周三5-6节", "教1-103", "1-16周", "必修", "2021", "1"),
            Course.CreateCourse("4", "操作系统", "3", "赵六", "周四7-8节", "教1-104", "1-16周", "必修", "2021", "1"),
            Course.CreateCourse("5", "数据库原理", "3", "钱七", "周五9-10节", "教1-105", "1-16周", "必修", "2021", "1"),
            Course.CreateCourse("6", "软件工程", "3", "孙八", "周六11-12节", "教1-106", "1-16周", "必修", "2021", "1"),
            Course.CreateCourse("7", "编译原理", "3", "周九", "周日13-14节", "教1-107", "1-16周", "必修", "2021", "1"),
            Course.CreateCourse("8", "计算机网络", "3", "张三", "周一1-2节", "教1-101", "1-16周", "必修", "2021", "2"),
            Course.CreateCourse("9", "计算机组成原理", "3", "李四", "周二3-4节", "教1-102", "1-16周", "必修", "2021", "2"),
            Course.CreateCourse("10", "数据结构", "3", "王五", "周三5-6节", "教1-103", "1-16周", "必修", "2021", "2"),

        };

    private Course currentCourse;



    public GameObject checkInfoPanel;
    /// <summary>
    /// 确认课程信息
    /// </summary>

    #region 文件操作
    public string StudentInfo = "Asstes/CSV/StudentInfo.txt";
    /// <summary>
    /// 确认选课,写入文件
    /// </summary>
    public void CheckingCouurse()
    {
        if (Login.LoginAPI.isDebug)
        {
            string content = $"{currentCourse.course_id}\t" +
                $"{currentCourse.course_name}\t" +
                $"{currentCourse.course_credit}\t" +
                $"{currentCourse.course_teacher}\t" +
                $"{currentCourse.course_time}\t" +
                $"{currentCourse.course_place}\t" +
                $"{currentCourse.course_week}\t" +
                $"{currentCourse.course_type}\t" +
                $"{currentCourse.course_year}\t" +
                $"{currentCourse.course_term}";
            File.AppendAllText(StudentInfo, content + "\n");
        }
        else
        {
            try
            {
                conn.Open();
                string sql = "insert into student_course values(@student_id,@course_id)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("student_id", Login.LoginAPI.account);
                cmd.Parameters.AddWithValue("course_id", currentCourse.course_id);
                cmd.ExecuteNonQuery();
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
    }
    public readonly List<Course> checkedCourse = new();//已选课程
    /// <summary>
    /// 已选课程
    /// </summary>
    public void CheckedCourse()
    {
        string[] content = File.ReadAllLines(StudentInfo);
        foreach (var item in content)
        {
            string[] data = item.Split('\t');
            string course_id = data[0];
            for (int i = 0; i < courses.Count; i++)
            {
                if (courses[i].course_id == course_id)
                {
                    checkedCourse.Add(courses[i]);//添加已选课程
                }
            }
        }
    }
    #endregion

    #region 界面显示
    public GameObject coursePanel;
    public GameObject showCoursePrefab;

    public void ShowCourse(Course cours)
    {
        GameObject showCourse = Instantiate(showCoursePrefab, coursePanel.transform);
        TextMeshProUGUI[] textsLine0 = showCourse.transform.Find("textLine0")
            .GetComponentsInChildren<TextMeshProUGUI>();
        TextMeshProUGUI[] textsLine1 = showCourse.transform.Find("textLine1")
            .GetComponentsInChildren<TextMeshProUGUI>();
        textsLine0[0].text = cours.course_id;
        textsLine0[1].text = cours.course_name;
        textsLine0[2].text = cours.course_credit;
        textsLine0[3].text = cours.course_teacher;
        textsLine1[0].text = cours.course_time;
        textsLine1[1].text = cours.course_place;
        textsLine1[2].text = cours.course_week;
        textsLine1[3].text = cours.course_type;
        textsLine1[4].text = cours.course_year;
        textsLine1[5].text = cours.course_term;
        showCourse.AddComponent<Button>();
        //添加点击事件
        showCourse.GetComponent<Button>().onClick.AddListener(
            () =>
            {
                currentCourse = cours;
                checkInfoPanel.SetActive(true);
            });
    }

    #region 展示选择
    public TMP_Dropdown selectCourse;
    /// <summary>
    /// 当前可以选择的课程
    /// </summary>
    public List<Course> currentCourseList = new();
    public TMP_Dropdown courseDropdown;
    /// <summary>
    /// 判断当前可选的课程有哪些
    /// </summary>
    public void CourseRightNow()
    {
        currentCourseList.Clear();
        foreach (var curcourse in courses)
        {
            if (!checkedCourse.Contains(curcourse))//如果已选课程不包含当前课程
            {
                currentCourseList.Add(curcourse);//添加当前展示课程
            }
        }
    }
    string currentType;
    /// <summary>
    /// 设置展示类型
    /// </summary> 
    public void SetCourseType()
    {
        string[] courseType = { "全部课程", "可选课程" };
        courseDropdown.AddOptions(new List<string> { "已选课程" });
        foreach (var course in courseType)
        {
            courseDropdown.options.Add(new TMP_Dropdown.OptionData(course));
        }
    }
    /// <summary>
    /// 选择课程展示类型
    /// </summary>
    /// <param name="index">展示类型</param>
    public void SelectCourse(int index)
    {
        if (currentType == null) { return; };
        currentType = courseDropdown.options[index].text;
        ShowCoursePanel();
    }
    #endregion
    /// <summary>
    /// 设置展示界面
    /// </summary>
    public void ShowCoursePanel()
    {
        if (currentType == "已选课程")
        {
            foreach (var course in checkedCourse)
            {
                ShowCourse(course);
            }
        }
        else if (currentType == "可选课程")
        {
            CourseRightNow();
            foreach (var course in currentCourseList)
            {
                ShowCourse(course);
            }
        }
        else
        {//全部课程
            foreach (var course in courses)
            {
                ShowCourse(course);
            }
        }
    }

    #endregion
}
