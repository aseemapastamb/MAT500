using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointPlacer : MonoBehaviour
{
    public GameObject pointPrefab;
    public GameObject linePrefab;
    private GameObject curve;

    private Point[] pointPool;
    private int points = -1;
    private Vector3[] curvePoints;
    private Vector2[,] DD;

    void Start()
    {
        //Creates Interpolation Points in the Pool
        pointPool = new Point[20];
        for (int i = 0; i < 20; ++i)
        {
            pointPool[i] = Instantiate(pointPrefab, this.transform).GetComponent<Point>();
            pointPool[i].gameObject.SetActive(false);
        }

        DD = new Vector2[20, 20];

        curvePoints = new Vector3[151];
        curve = Instantiate(linePrefab, this.transform);
        curve.SetActive(false);
    }

    //Enables a point on right click
    private void Update()
    {
        if (Input.GetMouseButtonUp(1))
        {
            if (points < 19)
            {
                ++points;
                pointPool[points].transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
                pointPool[points].GetComponent<Point>().DisplayIndex(points);
                ActivePoint();
            }
        }

        if (Input.GetKeyUp("space") || Input.GetMouseButtonUp(2))
        {
            if (points > -1)
            {
                --points;
                ActivePoint();
            }
        }

        if (points > 0)
        {
            UpdateDD();
            curve.gameObject.SetActive(true);
            for (int i = 0; i < 151; ++i)
            {
                float t = (i / 150.0f) * points;
                curvePoints[i] = CalculatePolynomial(t);
            }
            curve.GetComponent<Line>().SetPoints(curvePoints, Color.red);
        }
        else
        {
            curve.gameObject.SetActive(false);
        }
    }

    private void MakeDDTable()
    {
        int curLen = points;
        for (int i = 1; i <= points; ++i)
        {
            for (int j = 0; j < curLen; ++j)
            {
                DD[i, j] = (DD[i - 1, j + 1] - DD[i - 1, j]) / i;
            }
            --curLen;
        }
    }

    private Vector3 CalculatePolynomial(float t)
    {
        Vector2 res = Vector2.zero;
        for (int i = 0; i <= points; ++i)
        {
            float p = 1.0f;
            for (int j = 0; j < i; ++j)
            {
                p *= (t - j);
            }
            res += DD[i, 0] * p;
        }
        return res;
    }

    private void UpdateDD()
    {
        for (int i = 0; i < 20; i++)
        {
            if (i <= points)
            {
                DD[0, i] = pointPool[i].transform.position;
            }
            else
            {
                DD[0, i] = Vector2.zero;
            }
        }
        MakeDDTable();
    }

    private void ActivePoint()
    {
        for (int i = 0; i < 20; i++)
        {
            if (i <= points)
            {
                pointPool[i].gameObject.SetActive(true);
            }
            else
            {
                pointPool[i].gameObject.SetActive(false);
            }
        }
    }
}