using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class PointPlacer : MonoBehaviour
{
    public GameObject pointPrefab;
    public GameObject linePrefab;
    private GameObject curve;
    private Point[] pointPool;
    private int degree = 1;
    private Vector3[] linePoints;
    private int mode = 0;
    private int[,] pascalsTri = new int[21, 21];
    private float[] coeffs = new float[21];

    [SerializeField] private Dropdown m_degreeDropdown;
    [SerializeField] private Dropdown m_modeDropdown;


    //Creates Control Points in the Pool
    void Start()
    {
        pointPool = new Point[21];
        for (int i = 0; i <= 20; i++) {
            pointPool[i] = Instantiate(pointPrefab, this.transform).GetComponent<Point>();
            pointPool[i].gameObject.SetActive(false);
        }

        // Initializing Pascal's Triangle till d = 20
        for (int i = 0; i < 21; ++i) {
            for (int j = 0; j <= i; ++j) {
                if (j == 0 || j == i) {
                    pascalsTri[i, j] = 1;
                }
                else {
                    pascalsTri[i, j] = pascalsTri[i - 1, j - 1] + pascalsTri[i - 1, j];
                }
            }
        }

        curve = Instantiate(linePrefab, this.transform);
        curve.SetActive(true);

        linePoints = new Vector3[101];

        ActivePoint();
    }

    public void changeMode() {
        mode = m_modeDropdown.value;
    }

    public void ActivePoint()
    {
        degree = m_degreeDropdown.value + 1;

        for (int i = 0; i <= 20; i++)
        {
            if (degree == 0) {
                pointPool[i].gameObject.SetActive(false);
            }
            else {
                if (i <= degree)
                {
                    pointPool[i].gameObject.SetActive(true);
                    if (i == 0)
                    {
                        pointPool[i].transform.position = WorldCoords(new Vector3(0, 1, 10));
                    }
                    if (i > 0 && i < degree)
                    {
                        pointPool[i].transform.position = WorldCoords(new Vector3((1.0f / (float)(degree)) * i, 1, 10));
                    }
                    if (i == degree)
                    {
                        pointPool[i].transform.position = WorldCoords(new Vector3(1, 1, 10));
                    }
                }
                else
                {
                    pointPool[i].gameObject.SetActive(false);
                }
            }
        }

    }
    
    private Vector3 WorldCoords(Vector3 graphCoords) {
        Vector3 worldCoords;
        worldCoords.x = (graphCoords.x * 12) - 2;
        worldCoords.y = (((graphCoords.y + 3) * 12) / 6) - 6;
        worldCoords.z = graphCoords.z;
        return worldCoords;
    }

    private float GraphCoordY(Vector3 worldCoords) {
        return (((worldCoords.y + 6) * 6) / 12) - 3;
    }

    private float Bernstein(float t, float[] coeffs) {
        float res = 0;
        for (int i = 0; i <= degree; ++i) {
            // Using Pascal's Triangle
            res += coeffs[i] * pascalsTri[degree, i] * Mathf.Pow((1 - t), (degree - i)) * Mathf.Pow(t, i);
        }
        return res;
    }

    private float NLI(float t, float[] coeffs) {
        int curLen = degree;
        float[] newCoeffs = (float[])coeffs.Clone();
        while (curLen > 0) {
            for (int j = 0; j < curLen; ++j) {
                newCoeffs[j] = ((1 - t) * newCoeffs[j]) + (t * newCoeffs[j + 1]);
            }
            --curLen;
        }
        return newCoeffs[0];
    }

    void Update() {
        for (int i = 0; i <= degree; ++i) {
            coeffs[i] = GraphCoordY(pointPool[i].transform.position);
            pointPool[i].GetComponent<Point>().DisplayCoeff(coeffs[i]);
        }
        for (int i = 0; i <= 100; ++i) {
            float x = i / 100.0f;
            float y = 0;
            if (mode == 0) {
                y = Bernstein(x, coeffs);
            }
            else {
                y = NLI(x, coeffs);
            }
            linePoints[i] = WorldCoords(new Vector3(x, y, 10));
        }
        curve.GetComponent<Line>().SetPoints(linePoints);
    }
}