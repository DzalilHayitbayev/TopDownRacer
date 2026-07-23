using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuUIHandler : MonoBehaviour
{
    Canvas canvas;
    void Awake()
    {
        canvas = GetComponent<Canvas>();

        canvas.enabled = false;

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(GameManager obj)
    {
        if (GameManager.Instance.GetGameState() == GameStates.raceOver)
        {
            StartCoroutine(ShowMenuCO());
        }
    }

    public void OnRaceAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnExiMenu()
    {
        SceneManager.LoadScene(0);
    }

    IEnumerator ShowMenuCO()
    {
        yield return new WaitForSeconds(1);
        canvas.enabled = true;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
    }
}
