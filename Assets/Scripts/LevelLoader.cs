using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1.0f;
    public string[] defaultMusicForLevel;
    public TextMeshProUGUI skipText;
    public ScriptableRendererFeature ambientOcclusion;
    public static LevelLoader current {get; private set;}

    void Awake()
    {
        current = this;
    }

    void Start()
    {
        if (!GS.isReady)
        {
            GS.ResetAll();
            GS.isReady = true;
        }
        if (defaultMusicForLevel.Length > GS.currentSceneIdx)
        {
            AudioManager.instance.CrossfadeMusic(defaultMusicForLevel[GS.currentSceneIdx]);
        }
        else
        {
            Debug.Log("No default music for level");
        }
        if (skipText) skipText.enabled = false;

        ambientOcclusion?.SetActive(SceneManager.GetActiveScene().name != "Intro Scene");
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
            // if (Input.GetKeyDown(KeyCode.Escape))
            // {
            //     Quit();
            // }
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
        UI.UnlockCursor();
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

    public void StartIntroCutscene()
    {
        GS.ResetAll();
        LoadNextLevel();
        StartCoroutine(_StartIntroCutscene());
    }

    public void StartGame()
    {
        //GS.ResetAll();
        LoadNextLevel();
        StartCoroutine(_StartGame());
    }

    public void EnterLibrary()
    {
        LoadNextLevel();
        StartCoroutine(_EnterLibrary());
    }

    public void StartNextDay()
    {
        StartCoroutine(LoadLevel(GS.currentSceneIdx));
        GS.ResetDaily();
        GS.currentDay++;
        JournalManager.Main.AddDayBreak();
        // AudioManager.instance.ResetMusicEffects();
    }

    IEnumerator _StartIntroCutscene()
    {
        AudioManager.instance.StopMusic();
        yield return new WaitForSeconds(.7f);
        //YarnDispatcher.StartInternalMonologue("IntroTitles");
        //AudioManager.instance.PlaySFX("Typewriter");
        GS.interactionMode = InteractionType.Default;
        yield return null;
    }

    IEnumerator _StartGame()
    {
        AudioManager.instance.StopMusic();
        yield return new WaitForSeconds(.7f);
        //YarnDispatcher.StartInternalMonologue("IntroTitles");
        //AudioManager.instance.PlaySFX("Typewriter");
        GS.interactionMode = InteractionType.Default;
        yield return null;
    }

    IEnumerator _EnterLibrary()
    {
        yield return new WaitForSeconds(.7f);
        GS.RemoveAllStickers();
        // YarnDispatcher.StartInternalMonologue("EnterLibrary");
        yield return null;
    }

    IEnumerator LoadLevel(int levelIndex, bool loadAdditively = false)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);

        // Support independently managed Yarn triggers and journal entries for each scene
        GS.yarnStateTriggers = null;
        GS.journalEntryByKey = null;
        JournalManager.Main = null;

        SceneManager.LoadScene(levelIndex, loadAdditively ? LoadSceneMode.Additive : LoadSceneMode.Single);
        GS.currentSceneIdx = levelIndex;
    }
}
