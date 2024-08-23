using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JournalEntry
{
    public string key;
    [Multiline(3)]
    public string content;
    private bool _alreadyAdded = false;
    public bool alreadyAdded {
        get {
            return _alreadyAdded;
        }
        set {
            _alreadyAdded = value;
        }
    }
}

public class JournalManager : MonoBehaviour
{
    private Dictionary<string, JournalEntry> entryDict = new Dictionary<string, JournalEntry>();
    public List<JournalEntry> journalEntries = new List<JournalEntry>();
    public Animator notificationAnimator;
    public string defaultJournalEntry = "firstItem";

    private bool unreadNotifications = false;
    private bool isShowingNotification = false;

    public static JournalManager Main {get; private set;}

    void Awake()
    {
        if (!Main) Main = this;
    }

    void OnValidate()
    {
        foreach(JournalEntry data in journalEntries)
        {
            entryDict.TryAdd(data.key, data);
        }
    }

    void Update()
    {
        if (unreadNotifications && !isShowingNotification && GS.interactionMode == InteractionType.Default)
        {
            notificationAnimator.SetTrigger("Show");
            isShowingNotification = true;
        }
        else if (isShowingNotification && (!unreadNotifications || GS.interactionMode != InteractionType.Default))
        {
            notificationAnimator.SetTrigger("Hide");
            isShowingNotification = false;
        }
    }

    public void AddToJournal(string key)
    {
        if (key == null || key.Length == 0) key = defaultJournalEntry;
        Debug.Log($"Trying to add {key} to journal");

        JournalEntry data = entryDict[key];
        if (data == null)
        {
            Debug.Log("No matching journal entry found");
            return;
        }
        if (data.alreadyAdded) 
        {
            Debug.Log("Journal entry already added.");
            return;
        }

        GS.journalContent += $"{(GS.journalContent.Length > 0 ? "\n\n" : "")}{data.content}";
        data.alreadyAdded = true;

        unreadNotifications = true;
    }

    public void AddDayBreak()
    {
        GS.journalContent += $"\n\n10/{22+GS.currentDay}/23";
    }

    public void MarkAsRead()
    {
        unreadNotifications = false;
    }

    public void Reset()
    {
        entryDict.Clear();
        foreach(JournalEntry data in journalEntries)
        {
            data.alreadyAdded = false;
            entryDict.TryAdd(data.key, data);
        }
    }
}