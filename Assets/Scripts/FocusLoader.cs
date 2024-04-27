using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.SceneManagement;

namespace StarterAssets
{
    public class FocusLoader : MonoBehaviour
    {

        public GameObject player;
        private StarterAssetsInputs _input;
        public Animator transition;
        public float transitionTime = 1f;

        public string levelToLoad;

        private float holdDuration = 0f;

        public Image radialBlur;

        public Light directionalLight;

        public GameObject playerCamera;

        private CinemachineVirtualCamera camera;

        private float shakeTimer;

        public float shakeIntensity;

        public float shakeTime;

        private bool isFocused;

        private Color radialAlpha;


        void Start() {
            _input = player.GetComponent<StarterAssetsInputs>();
            camera = playerCamera.GetComponent<CinemachineVirtualCamera>();
            isFocused = false;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateOverlay();

            if (shakeTimer > 0) {
                shakeTimer -= Time.deltaTime;
                if (shakeTimer <= 0f){
                    CinemachineBasicMultiChannelPerlin perlin = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            perlin.m_AmplitudeGain = 0f;
                }
            }

            Debug.Log(isFocused);

        }

        void UpdateOverlay() {
            
            radialAlpha = radialBlur.color;

            if (_input.focus) {
                
                radialAlpha.a += Time.deltaTime * 0.6f;

                if (radialAlpha.a >= 1) {
                    TriggerFocus();
                    radialAlpha.a = 0;
                } 
            } 

            else {
                if (radialAlpha.a > 0) {
                    radialAlpha.a -= Time.deltaTime * 0.6f;
                }
            }

            radialBlur.color = radialAlpha;

            //Debug.Log(radialAlpha.a);
        }

        public void TriggerFocus() {

            if (isFocused == false) {
                directionalLight.intensity = 3;
                CameraShake(shakeIntensity, shakeTime);
                isFocused = true;
                radialAlpha = Color.black;
            } else {
                directionalLight.intensity = 1;
                CameraShake(shakeIntensity, shakeTime);
                isFocused = false;
                radialAlpha = Color.white;
            }
        }

        private void CameraShake(float intensity, float time) {

           CinemachineBasicMultiChannelPerlin perlin = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            perlin.m_AmplitudeGain = intensity;

            shakeTimer = time;
        }

        //Previous approach

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

        IEnumerator Wait () {
            yield return new WaitForSeconds(1);
        }
    }
}