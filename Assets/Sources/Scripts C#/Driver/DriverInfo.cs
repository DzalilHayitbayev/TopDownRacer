using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriverInfo 
{
    public int playerNumber = 0;
    public string playerName = "";
    public int carUniqueID = 0;
    public int carColorIndex = 0;
    public bool isAI = false;
    public int lastRacePosition = 0;
    public int championshipPoints = 0;

    public DriverInfo(int playerNumber, string name,int carUniqueID, int carColorIndex, bool isAI)
    {
        this.playerNumber = playerNumber;
        this.playerName = name;
        this.carUniqueID = carUniqueID;
        this.carColorIndex = carColorIndex;
        this.isAI = isAI;
    }

}
