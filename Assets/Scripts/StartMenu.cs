using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    //0: Main
    //1: onlineSelection
    public GameObject[] menus;
    public void startAI()
    {
        //StartCoroutine(LoadScene(aiSetup));
    }

    private void aiSetup()
    {
        SceneManager.Instance.turn.mode = "AI";
        SceneManager.Instance.networkManager.setMode("Null");
    }

    public void startLocal()
    {
       StartCoroutine(LoadScene(localSetup));
    }

    private void localSetup()
    {
        SceneManager.Instance.turn.mode = "twoPlayerLocal";
        SceneManager.Instance.networkManager.setMode("Null");
    }
    public void onlineSelection()
    {
        menus[0].SetActive(false);
        menus[1].SetActive(true);
    }

    public void startOnline(GameObject text)
    {
        startOnline(text.GetComponent<Text>().text);
    }

    public void startOnline(string ip)
    {
        Action<string> action1 = onlineSetup;
        StartCoroutine(LoadScene(null, action1, ip));
    }

    private void onlineSetup(string ip)
    {
        SceneManager.Instance.turn.mode = "Online";
        if (ip == "Host")
        {
            SceneManager.Instance.networkManager.setMode("Host");
            SceneManager.Instance.networkManager.Startup();
        }
        else
        {
            SceneManager.Instance.networkManager.setMode("Client");
            SceneManager.Instance.networkManager.connectSocket(ip);
            string result = SceneManager.Instance.networkManager.sendMessage("Connected</MSG>");
            Debug.Log("Result: " + result);

            //changed for debuging suppose to be set ==
            if (result != "Message Recieved")
            {
                SceneManager.Instance.networkManager.Startup();
            } else
            {
                SceneManager.Instance.networkManager.setMode("Failed");
                SceneManager.Instance.networkManager.disconnectSocket();
            }
        }
    }

    public void back(GameObject currMenu)
    {
        currMenu.SetActive(false);
        menus[0].SetActive(true);
    }

    private IEnumerator LoadScene(Action cb, Action<string> cbs = null, string ip = "")
    {
        // Start loading the scene
        AsyncOperation asyncLoadLevel = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("main", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        
        // Wait until the level finish loading
        while (!asyncLoadLevel.isDone)
            yield return null;
        
        // Wait a frame so every Awake and Start method is called
        yield return new WaitForEndOfFrame();
        if (ip.Length > 0 && cbs != null)
            cbs(ip);
        else if(cb != null)
            cb();
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetSceneByName("Menu").buildIndex);
    }
}
