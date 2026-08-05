using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LoadingStepByStep
{
    public class LoadingManager : MonoBehaviour
    {
        public static LoadingManager Instance;

        public GameObject loadingUI;
        public Slider progressBar;
        public TMP_Text statusText;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async Task LoadSceneAsync(string sceneName, List<LoadingStep> steps)
        {
            var loadingSceneStep = new LoadingStep("Loading Scene...", async () => await LoadScene(sceneName));
            steps.Add(loadingSceneStep);

            var stepFraction = 1f / steps.Count;
            var totalProgress = 0f;

            progressBar.value = totalProgress;
            loadingUI.SetActive(true);

            foreach (var step in steps)
            {
                statusText.text = step.Description;
                await step.ActionAsync();
                totalProgress += stepFraction;
                progressBar.value = totalProgress;
            }

            loadingUI.SetActive(false);
        }

        private static async Task LoadScene(string sceneName)
        {
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;

            while (asyncOperation.progress < 0.9f)
            {
                await Task.Yield();
            }

            await Task.Delay(500); // Simulate some loading time

            asyncOperation.allowSceneActivation = true;
        }
    }
}