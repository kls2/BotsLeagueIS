using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameState : MonoBehaviour {

    public static GameState control;

    //Robot data
    [HideInInspector]public int antennaIndex;
    [HideInInspector] public int headIndex;
    [HideInInspector] public int eyesIndex;
    [HideInInspector] public int bodyIndex;
    [HideInInspector] public int rightArmIndex;
    [HideInInspector] public int leftArmIndex;
    [HideInInspector] public int legIndex;
    [HideInInspector] public int baseElementIndex;

    //Levels Data
    [HideInInspector] public Level[] levels;

    private Level currentLevel;

    private void Awake()
    {
        if (control == null)
        {
            DontDestroyOnLoad(gameObject);
            control = this;
        }
        else if (control != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        levels = new Level[3];

        for (int i = 0; i < levels.Length; i++)
        {
            levels[i] = new Level();
            levels[i].unlocked = 0;
            levels[i].completed = 0;
        }
    }

    public void SetCurrentLevel(int indexOfLevel)
    {
        currentLevel = levels[indexOfLevel];
    }
    public void SetCurrentLevelState(byte unlocked, byte completed)
    {
        currentLevel.unlocked = unlocked;
        currentLevel.completed = completed;
    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (!File.Exists(Application.persistentDataPath + "/playerGameState.dat"))
        {
            file = File.Create(Application.persistentDataPath + "/playerGameState.dat");
        }
        else
        {
            file = File.Open(Application.persistentDataPath + "/playerGameState.dat", FileMode.Open);
        }

        PlayerData data = new PlayerData();

        data.antennaIndex = antennaIndex;
        data.headIndex = headIndex;
        data.eyesIndex = eyesIndex;
        data.bodyIndex = bodyIndex;
        data.rightArmIndex = rightArmIndex;
        data.leftArmIndex = leftArmIndex;
        data.legIndex = legIndex;
        data.baseElementIndex = baseElementIndex;

        //Save the level data
        data.Level1Unlocked = levels[0].unlocked;
        data.Level1Completed = levels[0].completed;
        data.Level2Unlocked = levels[1].unlocked;
        data.Level2Completed = levels[1].completed;
        data.Level3Unlocked = levels[2].unlocked;
        data.Level3Completed = levels[2].completed;

        bf.Serialize(file, data);
        file.Close();
    }


    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/playerGameState.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerGameState.dat", FileMode.Open);

            if (file.Length > 0)
            {
                PlayerData data = (PlayerData)bf.Deserialize(file);
                file.Close();

                antennaIndex = data.antennaIndex;
                headIndex = data.headIndex;
                eyesIndex = data.eyesIndex;
                bodyIndex = data.bodyIndex;
                rightArmIndex = data.rightArmIndex;
                leftArmIndex = data.leftArmIndex;
                legIndex = data.legIndex;
                baseElementIndex = data.baseElementIndex;

                //Load the level data
                levels[0].unlocked = data.Level1Unlocked;
                levels[0].completed = data.Level1Completed;
                levels[1].unlocked = data.Level2Unlocked;
                levels[1].completed = data.Level2Completed;
                levels[2].unlocked = data.Level3Unlocked;
                levels[2].completed = data.Level3Completed;
            }
        }
        else
        {
            antennaIndex = 0;
            headIndex = 0;
            eyesIndex = 0;
            bodyIndex = 0;
            rightArmIndex = 0;
            leftArmIndex = 0;
            legIndex = 0;
            baseElementIndex = 0;

            //Load the level data
            levels[0].unlocked = 0;
            levels[0].completed = 0;
            levels[1].unlocked = 0;
            levels[1].completed = 0;
            levels[2].unlocked = 0;
            levels[2].completed = 0;
        }
    }
}

[System.Serializable]
class PlayerData
{
    public int antennaIndex;
    public int headIndex;
    public int eyesIndex;
    public int bodyIndex;
    public int rightArmIndex;
    public int leftArmIndex;
    public int legIndex;
    public int baseElementIndex;

    //Levels
    public byte Level1Unlocked;
    public byte Level1Completed;
    public byte Level2Unlocked;
    public byte Level2Completed;
    public byte Level3Unlocked;
    public byte Level3Completed;
}

[System.Serializable]
public class Level
{
    public byte unlocked;
    public byte completed;
}