using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class scrollButton : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    public VerticalLayoutGroup buttonContainer;

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            int selectedIndex = getSelectedIndex();
            Scroll(selectedIndex);
        }
        
    }

    private int getSelectedIndex()
    {
        if (CompareTag("Button_1")) return 1;
        if (CompareTag("Button_2")) return 2;
        if (CompareTag("button_5")) return 5;
        if (CompareTag("button_6")) return 6;
        return 0;
    }

    private void Scroll(int selectedIndex)
    {
        scrollRect.verticalNormalizedPosition = (float)(5-(selectedIndex - 1) )/ 5;
    }

}
