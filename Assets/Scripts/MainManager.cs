using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    ///<summary>
    ///跳转教务系统页面
    ///</summary>
    public void JumpToEduSystem()
    {
        SceneManager.LoadScene("EducationSystem");
    }
}
