using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]

public class Line : MonoBehaviour
{
    public Color baseColor;
    public Material material;

    public Transform origin;
    public Vector3[] points;

    void OnRenderObject()
    {
        RenderLines(points, baseColor);
    }
    virtual public void RenderLines(Vector3[] points, Color color)
    {
        GL.Begin(GL.LINES);
        material.SetPass(0);
        for (int i = 0; i < points.Length - 1; i++)
        {
            GL.Color(color);
            GL.Vertex(points[i]);
            GL.Color(color);
            GL.Vertex(points[i + 1]);
        }
        GL.End();
    }
    public void SetPoints(Vector3[] _points, Color color)
    {
        points = _points;
        baseColor = color;
    }
}