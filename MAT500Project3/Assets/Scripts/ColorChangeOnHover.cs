using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangeOnHover : MonoBehaviour
{
    public Material overMat;
    public Material notOverMat;

    MeshRenderer objMeshRenderer;

    bool isPressed;

    private void Start()
    {
        objMeshRenderer = this.GetComponent<MeshRenderer>();
    }
    private void OnMouseOver()
    {
        objMeshRenderer.material = overMat;
    }

    private void OnMouseExit()
    {
        if (!isPressed)
        {
            objMeshRenderer.material = notOverMat;
        }
    }

    private void OnMouseDown()
    {
        isPressed = true;
    }
    private void OnMouseUp()
    {
        isPressed = false;
        objMeshRenderer.material = notOverMat;
    }
}
