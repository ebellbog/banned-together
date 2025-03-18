using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*
Journal-related classes:
- ShowJournal.cs
- JournalManager.cs
- ManageBook.cs
- BookLive.cs
- BookPage.cs
*/

[System.Serializable]
public class JournalEntry
{
    public string key;
    [Multiline(3)]
    public string content;
    public string focusSummary;
    [Tooltip("Separate terms by commas or spaces")]
    public string focusWords;
    [NonSerialized]
    public List<string> focusList;
    public bool addByDefault = false;

    JournalEntry(string key, string content, string focusWords)
    {
        this.key = key;
        this.content = content;
        this.focusWords = focusWords;
    }

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

/// <summary>
///  Manages adding content to journal and showing notifications for new content
/// </summary>
public class JournalManager : MonoBehaviour
{
    public List<JournalEntry> journalEntries = new List<JournalEntry>();
    public Animator notificationAnimator;
    public Animator activeFocusAnimator;
    public TextMeshProUGUI activeFocusText;
    public string defaultJournalEntry = "firstItem";

    private bool unreadNotifications = false;
    private bool isShowingNotification = false;
    private bool isShowingStickerSummary = false;

    public static JournalManager Main;

    void Awake()
    { 
        if (!Main) Main = this;
    }

    void OnValidate()
    {
        if (GS.journalEntryByKey == null) {
            GS.journalEntryByKey = new Dictionary<string, JournalEntry>();
            GS.journalEntryByContent = new Dictionary<string, JournalEntry>();
        }
        foreach(JournalEntry data in journalEntries)
        {
            GS.journalEntryByKey.TryAdd(data.key, data);

            if (data.focusWords?.Length > 0)
            {
                string[] focusWords = data.focusWords?.Split(new[] {",", " "}, StringSplitOptions.RemoveEmptyEntries);
                data.focusList = new List<string>(focusWords);
                GS.journalEntryByContent.TryAdd(data.content.Trim(), data);
            }

            if (data.addByDefault) AddToJournal(data.key, false);
        }
        AddDayBreak();
    }

    void Update()
    {
        if (GS.journalEntryByKey == null)
        {
            OnValidate();
        }
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

        if (GS.redStickerPlacement.associatedJournalEntry != null)
        {
            if (!isShowingStickerSummary && GS.interactionMode == InteractionType.Default)
            {
                activeFocusAnimator.SetTrigger("Show");
                isShowingStickerSummary = true;
            }
            activeFocusText.text = $"\"{GS.redStickerPlacement.associatedJournalEntry.focusSummary}\"";
        }
        if (isShowingStickerSummary && (GS.redStickerPlacement.associatedJournalEntry == null || GS.interactionMode != InteractionType.Default))
        {
            activeFocusAnimator.SetTrigger("Hide");
            isShowingStickerSummary = false;
        }
    }

    public void AddToJournal(string key, bool markUnread = true)
    {
        if (key == null || key.Length == 0) key = defaultJournalEntry;

        JournalEntry data;
        if (GS.journalEntryByKey.TryGetValue(key, out data))
        {
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

            GS.journalContent += $"{(GS.journalContent.Length > 0 ? " \n\n" : "")}{data.content}"
                .Replace("\r\n", "\n").Replace("\r", "\n");

            data.alreadyAdded = true;
            if (markUnread && GS.journalEnabled > 0) unreadNotifications = true;
        }
    }

    public void AddDayBreak()
    {
        GS.journalContent += $" \n\n10/{22+GS.currentDay}/23";
    }

    public void MarkAsRead()
    {
        unreadNotifications = false;
    }
}