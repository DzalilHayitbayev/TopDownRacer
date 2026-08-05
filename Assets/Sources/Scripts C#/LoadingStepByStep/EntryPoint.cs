using LoadingStepByStep.Services;
using System.Collections.Generic;
using UnityEngine;

namespace LoadingStepByStep
{
    public class EntryPoint : MonoBehaviour
    {
        private readonly List<string> _phrases = new()
        {
            "Loading and Reloading guns...",
            "Killing zombies and reloading guns...",
            "Killing zombies and reloading guns and killing zombies...",
            "Repairing Cars...",
            "Pooping mines on the road...",
            "I have no idea what I'm doing...",
            "Loading 6 or 7...",
            "Tung Tung Tung Sahuuur",
            "Fih"
        };

        private async void Start()
        {
            var adsService = new AdsService();
            var analyticsService = new AnalyticsService();
            var iapService = new IAPService();
            var gameStateProvider = new GameStateProvider();

            var loadingSteps = new List<LoadingStep>
            {
                new LoadingStep(GetRandomPhrase(_phrases), async () => await adsService.InitializeAsync()),
                new LoadingStep(GetRandomPhrase(_phrases), async () => await analyticsService.InitializeAsync()),
                new LoadingStep(GetRandomPhrase(_phrases), async () => await iapService.InitializeAsync()),
                new LoadingStep(GetRandomPhrase(_phrases), async () => await gameStateProvider.LoadStateAsync())
            };

            await LoadingManager.Instance.LoadSceneAsync("Menu", loadingSteps);
        }
        string GetRandomPhrase(List<string> phrases)
        {
            var randomIndex = Random.Range(0, phrases.Count);
            return phrases[randomIndex];
        }
    }

}
