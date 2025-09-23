using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


[RequireComponent(typeof(InteractableItem))]
public class ReadableBook : MonoBehaviour
{
    [Dropdown("GetBookTitles")]
    public string bookTitle;
    public bool assignRandomBook = true;

    [NonSerialized]
    public int currentPage = 0;

    private InteractableItem _interactableItem;
    private bool _wasExaminable;

    void Start() {
        OnValidate();
    }

    void OnValidate()
    {
        ConfigureInteractableItem();
        if (assignRandomBook) AssignRandomBook();
    }

    void OnDestroy()
    {
        ResetInteractableItem();
    }

    private List<string> GetBookTitles()
    {
        List<string> titles = new List<string>();
 
        if (LibraryManager.Main != null && LibraryManager.Main.novels != null)
        {
            foreach (NovelData novel in LibraryManager.Main.novels)
            {
                if (!string.IsNullOrEmpty(novel.novelTitle))
                {
                    titles.Add(novel.novelTitle);
                }
            }
        }
        
        // Add a default empty option
        if (titles.Count == 0)
        {
            titles.Add("No books available");
        }
        
        return titles;
    }

    private void ConfigureInteractableItem()
    {
        if (!_interactableItem) {
            _interactableItem = GetComponent<InteractableItem>();
            _wasExaminable = _interactableItem.isExaminable;
        }

        _interactableItem.isExaminable = false;

        // Clear existing component actions to avoid duplicates
        _interactableItem.componentActions.Clear();

        // Add click action to read the book
        ComponentAction readBookAction = new ComponentAction
        {
            timing = ActionTiming.onClick,
            targetComponent = this,
            callFunctionByName = "ReadBook",
            callOnlyOnce = false
        };

        _interactableItem.componentActions.Add(readBookAction);

        // Set hover effects
        if (LibraryManager.Main != null)
        {
            _interactableItem.cursorOverride = LibraryManager.Main.hoverCursor;
            _interactableItem.outlineColorOverride = LibraryManager.Main.hoverOutlineColor;
        }
    }

    public void ResetInteractableItem()
    {
        if (!_interactableItem) {
            _interactableItem = GetComponent<InteractableItem>();
        }
        _interactableItem.isExaminable = true;//_wasExaminable;
        _interactableItem.componentActions.Clear();
    }

    private void AssignRandomBook()
    {
        // if (bookTitle != null) return;
        List<string> titles = GetBookTitles();
        int randomIndex = UnityEngine.Random.Range(0, titles.Count);
        bookTitle = titles[randomIndex];
    }

    public void ReadBook()
    {
        LibraryManager.Main.OpenBook(this);
    }

    public Material GetBookMaterial()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && meshRenderer.sharedMaterials.Length > 0)
        {
            return meshRenderer.sharedMaterials[0];
        }
        return null;
    }
}