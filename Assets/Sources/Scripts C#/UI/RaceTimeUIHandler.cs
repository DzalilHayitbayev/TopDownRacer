using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RaceTimeUIHandler : MonoBehaviour
{
    Text timeText;

    float lastRaceTimeUpdated = 0;

    private void Awake()
    {
        timeText = GetComponent<Text>();
    }

    void Start()
    {
        StartCoroutine(UpdateTimeCO());
    }

    IEnumerator UpdateTimeCO()
    {
        

        while (true)
        {
            float raceTime = GameManager.Instance.GetRaceTime();

            if (lastRaceTimeUpdated != raceTime)
            {
                int raceTimeMinutes = (int)Mathf.Floor(raceTime / 60);
                int raceTimeSeconds = (int)Mathf.Floor(raceTime % 60);

                timeText.text = $"{raceTimeMinutes.ToString("00")}:{raceTimeSeconds.ToString("00")}";

                lastRaceTimeUpdated = raceTime;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
