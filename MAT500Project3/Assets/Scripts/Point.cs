using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Point : MonoBehaviour
{
    public Text pointText;

    public void DisplayIndex(int index)
    {
        pointText.text = "P" + index.ToString();
    }

}
