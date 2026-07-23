using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CarLapCounter : MonoBehaviour
{
    public Text carPositionText;

    int passedCheckPointNumber = 0;
    float timeAtLastPassedCheckPoint = 0;

    int numberOfPassedCheckPoints = 0;

    int lapsCompleated = 0;
    const int lapsToComplete = 2;

    private bool isRaceCompleted = false;

    int carPosition = 0;

    bool isHideRoutineRunning = false;
    float hideUIDelayTime;

    LapCounterUIHandler lapCounterUIHandler;

    public event Action<CarLapCounter> OnPassCheckPoint;

    private void Start()
    {
        if (CompareTag("Player"))
        {
            lapCounterUIHandler = FindObjectOfType<LapCounterUIHandler>();
            lapCounterUIHandler.SetLapText($"LAP {lapsCompleated + 1}/{lapsToComplete}");
        }
    }

    public void SetCarPosition(int position)
    {
        carPosition = position;
    }

    public int GetNumberOfCheckPointsPassed()
    {
        return numberOfPassedCheckPoints;
    }

    public float GetTimeAtLastCheckPoint()
    {
        return timeAtLastPassedCheckPoint;
    }

    public bool IsRaceCompleated()
    {
        return isRaceCompleted;
    }

    IEnumerator ShowPositionCO(float delayUntilHidePosition)
    {
        hideUIDelayTime += delayUntilHidePosition;

        carPositionText.text = carPosition.ToString();

        carPositionText.gameObject.SetActive(true);

        if (!isHideRoutineRunning)
        {
            isHideRoutineRunning = true;

            yield return new WaitForSeconds(hideUIDelayTime);
            carPositionText.gameObject.SetActive(false);

            isHideRoutineRunning = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (isRaceCompleted)
            return;

        if (collider2D.CompareTag("CheckPoint"))
        {
            CheckPoint checkPoint = collider2D.GetComponent<CheckPoint>();

            if (passedCheckPointNumber + 1 == checkPoint.checkPointNumber)
            {
                passedCheckPointNumber = checkPoint.checkPointNumber;

                numberOfPassedCheckPoints++;

                timeAtLastPassedCheckPoint = Time.time;

                if (checkPoint.isFinishLine)
                {
                    passedCheckPointNumber = 0;
                    lapsCompleated++;

                    if (lapsCompleated >= lapsToComplete)
                        isRaceCompleted = true;

                    if (!isRaceCompleted && lapCounterUIHandler != null)
                        lapCounterUIHandler.SetLapText($"LAP {lapsCompleated + 1}/{lapsToComplete}");

                }

                OnPassCheckPoint?.Invoke(this);

                if (isRaceCompleted)
                {
                    StartCoroutine(ShowPositionCO(100));

                    if (CompareTag("Player"))
                    {
                        GameManager.Instance.OnRaceCompleated();

                        GetComponent<CarInputHandler>().enabled = false;
                        GetComponent<CarAIHandler>().enabled = true;
                        GetComponent<AStarLite>().enabled = true;
                    }
                }
                else if (checkPoint.isFinishLine)
                {
                    StartCoroutine(ShowPositionCO(1.5f));
                }

            }
        }
    }
}
