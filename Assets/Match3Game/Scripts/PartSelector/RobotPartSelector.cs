using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotPartSelector : MonoBehaviour {

    [SerializeField] private GameObject content;
    [SerializeField] private GameObject robotPartPrefab;

    //The lists of sprites for the robot parts
    [SerializeField] private List<Sprite> robotAntennas;
    [SerializeField] private List<Sprite> robotHeads;
    [SerializeField] private List<Sprite> robotEyes;
    [SerializeField] private List<Sprite> robotBodies;
    [SerializeField] private List<Sprite> robotRightArms;
    [SerializeField] private List<Sprite> robotLeftArms;
    [SerializeField] private List<Sprite> robotLegs;

    //The current robot parts buttons
    [SerializeField] private Button currentRobotAntenna;
    [SerializeField] private Button currentRobotHead;
    [SerializeField] private Button currentRobotEyes;
    [SerializeField] private Button currentRobotBody;
    [SerializeField] private Button currentRobotRightArm;
    [SerializeField] private Button currentRobotLeftArm;
    [SerializeField] private Button currentRobotLegs;

    //The current robot parts Images
    [SerializeField] private Image currentRobotAntennaImg;
    [SerializeField] private Image currentRobotHeadImg;
    [SerializeField] private Image currentRobotEyesImg;
    [SerializeField] private Image currentRobotBodyImg;
    [SerializeField] private Image currentRobotRightArmImg;
    [SerializeField] private Image currentRobotLeftArmImg;
    [SerializeField] private Image currentRobotLegsImg;

    public int robotParts = 5;
    private Image lastSelectedPart;

    private enum RobotParts
    {
        Antenna,
        Head,
        Eyes,
        Body,
        ArmRight,
        ArmLeft,
        Legs
    };

    private RobotParts currentRobotPart = RobotParts.Antenna;

    private void CreateRobotParts(RobotParts robotPartType)
    {
        //Clear the child elements before adding the new ones
        if (content.transform.childCount > 0)
        {
            for (int i = 0; i < content.transform.childCount; i++)
            {
                Destroy(content.transform.GetChild(i).gameObject);
            }
        }

        //Get the correct parts and display them
        List<Sprite> parts = GetParts(robotPartType);

        for (int i = 0; i < robotParts; i++)
        {
            GameObject uiSelectorRobotPart = Instantiate(robotPartPrefab);
            uiSelectorRobotPart.GetComponent<Image>().sprite = parts[i];
            uiSelectorRobotPart.GetComponent<Button>().onClick.AddListener(delegate { SetBodyPart(uiSelectorRobotPart); });
            uiSelectorRobotPart.transform.parent = content.transform;
        }
    }

    private void SetBodyPart(GameObject robotPart)
    {
        if(lastSelectedPart != null)
            lastSelectedPart.sprite = robotPart.GetComponent<Image>().sprite;
    }

    private List<Sprite> GetParts(RobotParts robotPartType)
    {
        if (robotPartType == RobotParts.Antenna)
        {
            return robotAntennas;
        }
        else if (robotPartType == RobotParts.Head)
        {
            return robotHeads;
        }
        else if (robotPartType == RobotParts.Eyes)
        {
            return robotEyes;
        }
        else if (robotPartType == RobotParts.Body)
        {
            return robotBodies;
        }
        else if (robotPartType == RobotParts.ArmRight)
        {
            return robotRightArms;
        }
        else if (robotPartType == RobotParts.ArmLeft)
        {
            return robotLeftArms;
        }
        else
        {
            return robotLegs;
        }
    }

	// Use this for initialization
	void Start () {
        CreateRobotParts(currentRobotPart);
        lastSelectedPart = currentRobotAntennaImg;

        //Add the event listeners for the buttons
        currentRobotAntenna.onClick.AddListener(delegate { CreateRobotParts(RobotParts.Antenna); lastSelectedPart = currentRobotAntennaImg; });
        currentRobotHead.onClick.AddListener(delegate { CreateRobotParts(RobotParts.Head);
            lastSelectedPart = currentRobotHeadImg;
        });
        currentRobotEyes.onClick.AddListener(delegate { CreateRobotParts(RobotParts.Eyes);
            lastSelectedPart = currentRobotEyesImg;
        });
        currentRobotBody.onClick.AddListener(delegate { CreateRobotParts(RobotParts.Body);
            lastSelectedPart = currentRobotBodyImg;
            Debug.Log(lastSelectedPart);
        });
        currentRobotRightArm.onClick.AddListener(delegate { CreateRobotParts(RobotParts.ArmRight);
            lastSelectedPart = currentRobotRightArmImg; });
        currentRobotLeftArm.onClick.AddListener(delegate { CreateRobotParts(RobotParts.ArmLeft);
            lastSelectedPart = currentRobotLeftArmImg;
        });
        currentRobotLegs.onClick.AddListener(delegate { CreateRobotParts(RobotParts.Legs);
            lastSelectedPart = currentRobotLegsImg;
        });
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
