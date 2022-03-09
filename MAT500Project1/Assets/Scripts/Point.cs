using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Point : MonoBehaviour
{
    public Text coeffText;

    public void DisplayCoeff(float coeff) {
        coeffText.text = coeff.ToString("n2");
    }

}
