using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LogViewer : MonoBehaviour
{
    [SerializeField] int logLimit = 100;
    [SerializeField] GameObject logPrefab;
    VerticalLayoutGroup scrollContent;
    readonly Queue<GameObject> logs = new();
    void Awake()
    {
        Application.logMessageReceived += LogReceived;
    }
    void Start()
    {
        scrollContent = GetComponentInChildren<VerticalLayoutGroup>();
    }

    void LogReceived(string logString, string stackTrace, LogType type)
    {
        GameObject theLog = Instantiate(logPrefab, scrollContent.transform);
        TextMeshProUGUI tmp = theLog.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = type != LogType.Log ? $"{logString}\n{stackTrace}" : logString;
        tmp.color = type switch { LogType.Assert => Color.cyan, LogType.Error => Color.red, LogType.Exception => Color.red, LogType.Log => Color.white, LogType.Warning => Color.yellow, _ => Color.gray };
        theLog.transform.SetAsFirstSibling();
        logs.Enqueue(theLog);
        if (logs.Count > logLimit)
            Destroy(logs.Dequeue());
    }

    public void ToggleVisibility() => gameObject.SetActive(!gameObject.activeInHierarchy);
}
