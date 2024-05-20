using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float  transitionTime = 1.0f;
    public string[] defaultMusicForLevel;

    private int currentLevelIdx;

    public void Start()
    {
        currentLevelIdx = SceneManager.GetActiveScene().buildIndex;
        if (defaultMusicForLevel.Length > currentLevelIdx)
        {
            AudioManager.instance.CrossfadeMusic(defaultMusicForLevel[currentLevelIdx]);
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name == "Intro Scene")
        {
            Debug.Log("Trying to quit - standalone build only");
            Application.Quit();
        }
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void StartGame()
    {
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

    IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(levelIndex);
    }
}
