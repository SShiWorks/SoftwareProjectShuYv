using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.IO;
#if PLATFORM_STANDALONE_WIN //如果是Windows平台
using System.Runtime.InteropServices;
#endif

public class LeaveSys : MonoBehaviour
{
    private readonly string _apiUrl = "https://api.oauth.sus.xyz/shiyu/leave/";
    public TextMeshProUGUI title;
    public TextMeshProUGUI content;
    public TextMeshProUGUI needTime;


    private Leave _leave;

    public void PostLeave()
    {
        _leave = new Leave(title.text, content.text, System.DateTime.Now.ToString(),needTime.text, null);
        StartCoroutine(PostLeave(_leave));
    }
#if PLATFORM_STANDALONE_WIN


#endif

    private IEnumerator PostLeave(Leave leave)
    {
        WWWForm form = new();
        form.AddField("Title", leave.Title);
        form.AddField("Content", leave.Content);
        form.AddField("Time", leave.Time);
        form.AddBinaryData("Image", ImageToByte(leave.Image));

        UnityWebRequest www = UnityWebRequest.Post(_apiUrl, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Leave request sent successfully");
        }
    }

    private byte[] ImageToByte(Texture2D image)
    {
        return image.EncodeToPNG();
    }
}

public class Leave
{
    public string Title;
    public string Content;
    public string Time;
    public string NeedTime;
    public Texture2D Image;

    public Leave(string title, string content, string time,string needTime, Texture2D image)
    {
        Title = title;
        Content = content;
        Time = time;
        NeedTime= needTime;
        Image = image;
    }
}