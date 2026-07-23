using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostCarRecorder : MonoBehaviour
{
    public Transform carSpriteObject;
    public GameObject ghostCarPlaybackPrefab;

    GhostCarData ghostCarData = new GhostCarData();

    bool isRecording = true;

    Rigidbody2D carRigidBody2D;
    CarInputHandler carInputHandler;

    private void Awake()
    {
        carRigidBody2D = GetComponent<Rigidbody2D>();
        carInputHandler = GetComponent<CarInputHandler>();

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(GameManager obj)
    {
        if (GameManager.Instance.GetGameState() == GameStates.running)
            StartCoroutine(RecordCarPositionCO());

        if (GameManager.Instance.GetGameState() == GameStates.raceOver)
            StartCoroutine(SaveCarPositonCO());
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!CompareTag("Player"))
        {
            Destroy(this);
            return;
        }

        GameObject ghostCar = Instantiate(ghostCarPlaybackPrefab);

        ghostCar.GetComponent<GhostCarPlayback>().LoadData(carInputHandler.playerNumber);

       
    }

    IEnumerator RecordCarPositionCO()
    {
        while (isRecording)
        {
            if (carSpriteObject != null)
                ghostCarData.AddDataItem(new GhostCarDataListItem(carRigidBody2D.position, carRigidBody2D.rotation, carSpriteObject.localScale, Time.timeSinceLevelLoad));  

            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator SaveCarPositonCO()
    {
        yield return new WaitForSeconds(1); 

        SaveData();
    }

    void SaveData()
    {
        string jsonEncodedData = JsonUtility.ToJson(ghostCarData);

        Debug.Log($"Saved ghost data {jsonEncodedData}");

        if(carInputHandler != null)
        {
            PlayerPrefs.SetString($"{SceneManager.GetActiveScene().name}_{carInputHandler.playerNumber}_ghost", jsonEncodedData);
            PlayerPrefs.Save();
        }

        isRecording = false;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
    }
}
