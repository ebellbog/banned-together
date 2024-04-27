using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace StarterAssets
{
    public class LevelLoader : MonoBehaviour
    {

        public GameObject player;
        private StarterAssetsInputs _input;
        public Animator transition;
        public float transitionTime = 1f;

        public string levelToLoad;

        void Start() {
            _input = player.GetComponent<StarterAssetsInputs>();
        }

        // Update is called once per frame
        void Update()
        {
        if (_input.focus) {
            LoadFocus();
            }   
        }

        public void LoadFocus() {
        //SceneManager.LoadScene("Level Loader Destination");
        StartCoroutine(LoadLevel(levelToLoad));
        }

        IEnumerator LoadLevel (string levelName) {

            //Play animation
            transition.SetTrigger("Start");

            //Wait
            yield return new WaitForSeconds(transitionTime);

            //Load scene
            SceneManager.LoadScene(levelName);

        }
    }
}