using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostCarPlayback : MonoBehaviour
{
    GhostCarData ghostCarData = new GhostCarData();
    List<GhostCarDataListItem> ghostCarDataList = new List<GhostCarDataListItem>();

    int currentPlaybackIndex = 0;

    float lastStoredTime = 0.1f;
    Vector2 lastStoredPosition = Vector2.zero;
    float lastStoredRotation = 0;
    Vector3 lastStoredLocalScale = Vector3.zero;

    float duration = 0.1f;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (ghostCarDataList.Count == 0)
            return;

        if (Time.timeSinceLevelLoad >= ghostCarDataList[currentPlaybackIndex].timeSinceLevelLoaded)
        {
            lastStoredTime = ghostCarDataList[currentPlaybackIndex].timeSinceLevelLoaded;
            lastStoredPosition = ghostCarDataList[currentPlaybackIndex].position;
            lastStoredRotation = ghostCarDataList[currentPlaybackIndex].rotationZ;
            lastStoredLocalScale = ghostCarDataList[currentPlaybackIndex].localScale;

            if (currentPlaybackIndex < ghostCarDataList.Count - 1)
                currentPlaybackIndex++;

            duration = ghostCarDataList[currentPlaybackIndex].timeSinceLevelLoaded - lastStoredTime;
        }

        float timePassed = Time.timeSinceLevelLoad - lastStoredTime;
        float lerpPercentage = timePassed / duration;

        transform.position = Vector2.Lerp(lastStoredPosition, ghostCarDataList[currentPlaybackIndex].position, lerpPercentage);
        transform.rotation = Quaternion.Lerp(Quaternion.Euler(0, 0, lastStoredRotation), Quaternion.Euler(0, 0, ghostCarDataList[currentPlaybackIndex].rotationZ), lerpPercentage);
        transform.localScale = Vector3.Lerp(lastStoredLocalScale, ghostCarDataList[currentPlaybackIndex].localScale, lerpPercentage);

    }

    public void LoadData(int playerNumber)
    {
        if (!PlayerPrefs.HasKey($"{SceneManager.GetActiveScene().name}_{playerNumber}_ghost"))
            Destroy(gameObject);
        else
        {
            string jsonEncodedData = PlayerPrefs.GetString($"{SceneManager.GetActiveScene().name}_{playerNumber}_ghost");

            ghostCarData = JsonUtility.FromJson<GhostCarData>(jsonEncodedData);

            ghostCarDataList = ghostCarData.GetDataList();
        }

    }
}
