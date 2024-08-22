using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1.0f;
    public string[] defaultMusicForLevel;
    public TextMeshProUGUI skipText;

    public void Start()
    {
        if (GS.currentLevelIdx < 0)
        {
            GS.Reset();
            GS.currentLevelIdx = SceneManager.GetActiveScene().buildIndex;
        }
        if (defaultMusicForLevel.Length > GS.currentLevelIdx)
        {
            AudioManager.instance.CrossfadeMusic(defaultMusicForLevel[GS.currentLevelIdx]);
        }
        if (skipText) skipText.enabled = false;
    }

    public void Quit()
    {
        Debug.Log("Trying to quit - standalone build only");
        EventSystem.current.SetSelectedGameObject(null);
        Application.Quit();
    }

    public void Update()
    {
        if (SceneManager.GetActiveScene().name == "Intro Scene")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Quit();
            }
            if (Input.anyKeyDown && YarnDispatcher.YarnSpinnerIsActive())
            {
                if ((bool)skipText?.enabled)
                {
                    YarnDispatcher.Stop(false);
                    AudioManager.instance.StopSFX();

                    StopAllCoroutines();

                    transitionTime = 0.5f;
                    LoadNextLevel();
                } else if (skipText)
                {
                    skipText.enabled = true;
                }
            }
        }
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void LoadMainMenu()
    {
        StartCoroutine(LoadLevel(0));
        AudioManager.instance.ResetMusicEffects();
    }

    public void ReturnToGame()
    {
        if (!(GS.prevLevelIdx > 0))
        {
            Debug.LogWarning("No active game to return to: " + GS.prevLevelIdx);
            return;
        }
        StartCoroutine(LoadLevel(GS.prevLevelIdx));
    }

    public void StartGame()
    {
        GS.Reset();
        LoadNextLevel();
        StartCoroutine(_StartGame());
    }

    IEnumerator _StartGame()
    {
        AudioManager.instance.CrossfadeMusic("Intrigue", .7f);
        yield return new WaitForSeconds(.7f);
        YarnDispatcher.StartInternalMonologue("Intro");
        AudioManager.instance.PlaySFX("Intro");
        yield return null;
    }

    IEnumerator LoadLevel(int levelIndex, bool loadAdditively = false)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(levelIndex, loadAdditively ? LoadSceneMode.Additive : LoadSceneMode.Single);
        GS.currentLevelIdx = levelIndex;
    }
}
