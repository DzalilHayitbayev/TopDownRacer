using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
public class SpawnCars : MonoBehaviour
{
    [SerializeField] CinemachineCamera playerCinemachineCamera;
    [SerializeField] ZombieSpawner zombieSpawner;
    int numberOfCarsSpawned = 0;

    void Start()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        spawnPoints = spawnPoints.ToList().OrderBy(s => s.name).ToArray();

        CarData[] carDatas = Resources.LoadAll<CarData>("CarData/");

        List<DriverInfo> driverInfoList = new List<DriverInfo>(GameManager.Instance.GetDriverList());

        driverInfoList = driverInfoList.OrderBy(s => s.lastRacePosition).ToList();


        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform spawnPoint = spawnPoints[i].transform;

            if (driverInfoList.Count == 0) return;

            DriverInfo driverInfo = driverInfoList[0];

            int selectedCarID = driverInfo.carUniqueID;

            foreach (CarData carData in carDatas)
            {
                if (carData.CarUniqueID == selectedCarID)
                {
                    GameObject car = Instantiate(carData.CarPrefab, spawnPoint.position, spawnPoint.rotation);

                    car.name = driverInfo.playerName;

                    car.GetComponent<CarInputHandler>().playerNumber = driverInfo.playerNumber;


                    if (driverInfo.isAI)
                    {
                        car.GetComponent<CarInputHandler>().enabled = false;
                        car.tag = "AI";

                    }
                    else
                    {
                        car.GetComponent<CarAIHandler>().enabled = false;
                        car.GetComponent<AStarLite>().enabled = false;
                        car.GetComponentInChildren<CarColorApplier>().ApplyColor(carData.CarColorSchemes[driverInfo.carColorIndex].ColorSprite); zombieSpawner.SetPlayerCarLapCounter(car.GetComponent<CarLapCounter>());
                        car.tag = "Player";
                        playerCinemachineCamera.Follow = car.transform;

                    }

                    numberOfCarsSpawned++;

                    break;
                }
            }

            driverInfoList.Remove(driverInfo);
        }
    }

    public int GetNumberOfCarsSpawned() 
    {
        return numberOfCarsSpawned;
    }
        

}
