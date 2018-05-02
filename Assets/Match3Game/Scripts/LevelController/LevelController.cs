using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class LevelUI
{
    public Image levelCogImg;
    public Image levelNumTxt;
}

public class LevelController : MonoBehaviour {

    [SerializeField] private List<LevelUI> levelsUI;
    [SerializeField] private List<Button> levelsButtons;
    [SerializeField] private List<Button> levelsNumButtons;

    public void StartLevel1()
    {
            GameState.control.SetCurrentLevel(0);
            SceneManager.LoadScene("BTGameplay");
    }

    public void StartLevel2()
    {
            GameState.control.SetCurrentLevel(1);
            SceneManager.LoadScene("BTGameplay");
    }

    public void StartLevel3()
    {
            GameState.control.SetCurrentLevel(2);
            SceneManager.LoadScene("BTGameplay");
    }

    private void Start()
    {
        //Load the state of each level
        GameState.control.Load();
        LoadLevelsStatus();
    }

    private void LoadLevelsStatus()
    {
        for (int i = 0; i < GameState.control.levels.Length; i++)
        {
            if (GameState.control.levels[i].unlocked == 0)
            {
                //Apha transparent
                levelsUI[i].levelCogImg.color = new Color(levelsUI[i].levelCogImg.color.r, levelsUI[i].levelCogImg.color.g, levelsUI[i].levelCogImg.color.b, 0.5f);
                levelsUI[i].levelNumTxt.color = new Color(levelsUI[i].levelNumTxt.color.r, levelsUI[i].levelNumTxt.color.g, levelsUI[i].levelNumTxt.color.b, 0.5f);

                    levelsButtons[i].interactable = false;
                    levelsNumButtons[i].interactable = false;
            }
            else if (GameState.control.levels[i].unlocked == 1 && GameState.control.levels[i].completed == 0)
            {
                //Default color
                levelsUI[i].levelCogImg.color = new Color(levelsUI[i].levelCogImg.color.r, levelsUI[i].levelCogImg.color.g, levelsUI[i].levelCogImg.color.b, 1.0f);
                levelsUI[i].levelNumTxt.color = new Color(levelsUI[i].levelNumTxt.color.r, levelsUI[i].levelNumTxt.color.g, levelsUI[i].levelNumTxt.color.b, 1.0f);
                levelsButtons[i].interactable = true;
                levelsNumButtons[i].interactable = true;
            }
            else if (GameState.control.levels[i].unlocked == 1 && GameState.control.levels[i].completed == 1)
            {
                //Number on level is green
                levelsUI[i].levelNumTxt.color = Color.green;
                levelsButtons[i].interactable = true;
                levelsNumButtons[i].interactable = true;
            }
        }
    }

}
