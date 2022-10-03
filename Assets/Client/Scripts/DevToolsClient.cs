using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;


public class DevToolsClient : MonoBehaviour
{
    private uint myMaxLogLines;
    private Queue myLogQueue;
    private bool myIsLogToScreenEnabled;


    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, string lpString);
    
    [DllImport("user32.dll", EntryPoint = "GetActiveWindow")]
    public static extern System.IntPtr GetActiveWindow();

    private void Awake()
    {
        // Cache logging to screen state to a local variable
        myIsLogToScreenEnabled = CLU.GetIsPrintToScreenEnabled();

#if !UNITY_EDITOR
        SetWindowTitle();
#endif
    }

    private void Start()
    {
        if (myIsLogToScreenEnabled)
        {
            Shared.Log("[HOOD][LOG] - Started logging to screen.");
        }
    }

    private void OnEnable()
    {
        if (myIsLogToScreenEnabled) 
        {
            myMaxLogLines = 15;
            myLogQueue = new Queue();
            Application.logMessageReceived += HandleLog;
        }
    }

    private void OnDisable()
    {
        if (myIsLogToScreenEnabled)
        {
            Application.logMessageReceived -= HandleLog;
        }
    }

    private void HandleLog(string aStringToLog, string aStackTrace, LogType aLogType)
    {
        myLogQueue.Enqueue("[" + aLogType + "] : " + aStringToLog);
        if (aLogType == LogType.Exception) 
        {
            myLogQueue.Enqueue(aStackTrace);
        }
        while (myLogQueue.Count > myMaxLogLines)
        {
            myLogQueue.Dequeue();
        }
    }

    private void OnGUI()
    {
        if (myIsLogToScreenEnabled)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height));
            GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()));
            GUILayout.EndArea();
        }
    }

    // Changes the title of the compiled client window
    // Based on https://answers.unity.com/questions/148723/how-can-i-change-the-title-of-the-standalone-playe.html,  
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getactivewindow
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowtexta
    private void SetWindowTitle() 
    {
        string sessionCacheFilename = CLU.GetSessionCacheFilename();
        if (!string.IsNullOrEmpty(sessionCacheFilename))
        {
            // Get the current window handle.
            System.IntPtr windowPtr = GetActiveWindow();

            // Set the title text using the window handle.
            SetWindowText(windowPtr, "hoodies_client - " + sessionCacheFilename);
        }
    }
}
