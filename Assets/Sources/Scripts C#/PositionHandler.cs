using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PositionHandler : MonoBehaviour
{
    LeaderBoardUIHandler leaderboardUIHandler;


    public List<CarLapCounter> carLapCounters = new List<CarLapCounter>();

    private void Start()
    {
        CarLapCounter[] carLapCountersArray = Object.FindObjectsByType<CarLapCounter>();

        carLapCounters = carLapCountersArray.ToList<CarLapCounter>();

        foreach (CarLapCounter carLapCounter in carLapCounters)
            carLapCounter.OnPassCheckPoint += OnPassCheckPoint;

        leaderboardUIHandler = FindObjectOfType<LeaderBoardUIHandler>();

        if (leaderboardUIHandler != null)
            leaderboardUIHandler.UpdateList(carLapCounters);
    }

    void OnPassCheckPoint(CarLapCounter carLapCounter)
    {
        carLapCounters = carLapCounters.OrderByDescending(s => s.GetNumberOfCheckPointsPassed()).ThenBy(s => s.GetTimeAtLastCheckPoint()).ToList();

        int carPosition = carLapCounters.IndexOf(carLapCounter) + 1;

        carLapCounter.SetCarPosition(carPosition);

        if (carLapCounter.IsRaceCompleated())
        {
            int playerNumber = carLapCounter.GetComponent<CarInputHandler>().playerNumber;
            GameManager.Instance.SetDriversLastRacePosition(playerNumber, carPosition);

            int ChampionshipPointsAwarded = FindObjectOfType<SpawnCars>().GetNumberOfCarsSpawned() - carPosition;
            GameManager.Instance.AddPointsToChampionship(playerNumber, ChampionshipPointsAwarded);

        }

        if (leaderboardUIHandler != null)
            leaderboardUIHandler.UpdateList(carLapCounters);
    }
}
