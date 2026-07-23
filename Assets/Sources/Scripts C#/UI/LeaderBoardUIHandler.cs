using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardUIHandler : MonoBehaviour
{
    public GameObject leaderboardItemPrefab;

    SetLeaderboardItemInfo[] setLeaderBoardItemInfo;

    bool isInitialized = false;

    Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.enabled = false;

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(GameManager obj)
    {
        if (GameManager.Instance.GetGameState() == GameStates.raceOver) 
        {
            canvas.enabled = true;
        }
    }

    private void Start()
    {
        VerticalLayoutGroup leaderboardLayoutGroup = GetComponentInChildren<VerticalLayoutGroup>();

        CarLapCounter[] carLapCounterArray = FindObjectsOfType<CarLapCounter>();

        setLeaderBoardItemInfo = new SetLeaderboardItemInfo[carLapCounterArray.Length];

        for (int i = 0; i < carLapCounterArray.Length; i++)
        {
            GameObject leaderboardInfoGameObject = Instantiate(leaderboardItemPrefab, leaderboardLayoutGroup.transform);

            setLeaderBoardItemInfo[i] = leaderboardInfoGameObject.GetComponent<SetLeaderboardItemInfo>();

            setLeaderBoardItemInfo[i].SetPositionText($"{i + 1}.");
        }

        Canvas.ForceUpdateCanvases();

        isInitialized = true;
    }

    public void UpdateList(List<CarLapCounter> lapCounters)
    {
        if(!isInitialized)
            return;

        for (int i = 0; i < lapCounters.Count; i++)
        {
            setLeaderBoardItemInfo[i].SetDriverName(lapCounters[i].gameObject.name);
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
    }
}
