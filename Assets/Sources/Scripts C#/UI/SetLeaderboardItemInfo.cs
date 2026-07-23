using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetLeaderboardItemInfo : MonoBehaviour
{
    public Text positionText;
    public Text driverNameText;


    void Start()
    {

    }

    public void SetPositionText(string newPosition)
    {
        positionText.text = newPosition;
    }

    public void SetDriverName(string newDriverName)
    {
        driverNameText.text = newDriverName;
    }


}
