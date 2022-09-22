using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogToScreen : MonoBehaviour
{
    private uint myMaxLogLines;
    private Queue myLogQueue;

    private void Start()
    {
        if (CLU.GetIsPrintToScreenEnabled())
        {
            Shared.Log("[HOOD][LOG] - Started up logging.");
        }
    }

    private void OnEnable()
    {
        if (CLU.GetIsPrintToScreenEnabled()) 
        {
            myMaxLogLines = 15;
            myLogQueue = new Queue();
            Application.logMessageReceived += HandleLog;
        }
    }

    private void OnDisable()
    {
        if (CLU.GetIsPrintToScreenEnabled())
        {
            Application.logMessageReceived -= HandleLog;
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > myMaxLogLines)
            myLogQueue.Dequeue();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height));
        GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()));
        GUILayout.EndArea();
    }
}
