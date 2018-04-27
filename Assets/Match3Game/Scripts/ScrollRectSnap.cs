using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectSnap : MonoBehaviour {

    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform center;
    private List<Button> btns;

    private float[] distance;
    private bool dragging = false;
    private int btnDistance;
    private int minButtonNum;
    private float btnWidth;

    public void SetButtons(List<Button> buttons)
    {
        btns = buttons;
        int btnLenght = btns.Count;
        distance = new float[btnLenght];
        btnWidth = btns[0].gameObject.GetComponent<RectTransform>().rect.width;
        //Get distance between buttons
        btnDistance = (int)Mathf.Abs(btns[1].GetComponent<RectTransform>().anchoredPosition.x - btns[0].GetComponent<RectTransform>().anchoredPosition.x);
    }

    private void Update()
    {
        if (btns.Count > 0)
        {
            for (int i = 0; i < btns.Count; i++)
            {
                distance[i] = Mathf.Abs(center.transform.position.x - btns[i].transform.position.x);
            }

            float minimumDistance = Mathf.Min(distance);

            for (int a = 0; a < btns.Count; a++)
            {
                if (minimumDistance == distance[a])
                {
                    minButtonNum = a;
                }
            }

            if (!dragging)
            {
                LerpToBtn(minButtonNum * -btnDistance);
                LerpScale(btns[minButtonNum]);
            }
        }
    }

    void LerpToBtn(int position)
    {
        float newX = Mathf.Lerp(panel.anchoredPosition.x, position, Time.deltaTime * 80f);
        Vector2 newPosition = new Vector2(newX, panel.anchoredPosition.y);

        panel.anchoredPosition = newPosition;
    }

    void LerpScale(Button button)
    {
        for (int a = 0; a < btns.Count; a++)
        {
            btns[a].gameObject.GetComponent<RectTransform>().localScale = new Vector3(0.75f, 0.75f, 1.0f);
        }

        button.GetComponent<RectTransform>().localScale = new Vector3(1.25f, 1.25f, 1.0f);
    }

    public void StartDrag()
    {
        dragging = true;
    }

    public void EndDrag()
    {
        dragging = false;
    }

    public void RightButtonClick()
    {
        dragging = true;
        panel.anchoredPosition += new Vector2(-btnWidth,0.0f);
    }

    public void LeftButtonClick()
    {
        dragging = true;
        panel.anchoredPosition += new Vector2(btnWidth, 0.0f);
    }

    public void RightButtonClickUp()
    {
        dragging = false;
    }

    public void LeftButtonClickUp()
    {
        dragging = false;
    }
}
