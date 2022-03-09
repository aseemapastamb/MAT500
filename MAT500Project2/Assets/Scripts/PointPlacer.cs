using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class PointPlacer : MonoBehaviour
{
    public GameObject pointPrefab;
    public GameObject linePrefab;
    public GameObject circlePrefab;

    private Point[] pointPool;
    private Line[] controlPolyLines;
    private Line[] shellLines;
    private GameObject[] bezierPoints;
    private int bCount = 0;
    private int points = -1;
    private Vector3[] curvePoints;
    private int msCount = 0;
    private GameObject curve;
    private int mode = 0;
    private bool slideMode = true;
    float displayT = 0.5f;
    private int[,] pascalsTri = new int[21, 21];

    [SerializeField] private Dropdown m_modeDropdown;
    [SerializeField] private Text m_tValueText;
    [SerializeField] private Toggle m_shellToggle;
    [SerializeField] private Slider m_tValueSlider;

    void Start()
    {
        //Creates Control Points in the Pool
        pointPool = new Point[20];
        for (int i = 0; i < 20; ++i)
        {
            pointPool[i] = Instantiate(pointPrefab, this.transform).GetComponent<Point>();
            pointPool[i].gameObject.SetActive(false);
        }

        // Create Control Lines
        controlPolyLines = new Line[19];
        for (int i = 0; i < 19; ++i)
        {
            controlPolyLines[i] = Instantiate(linePrefab, this.transform).GetComponent<Line>();
            controlPolyLines[i].gameObject.SetActive(false);
        }

        // Create Shell Lines
        shellLines = new Line[18];
        for (int i = 0; i < 18; ++i)
        {
            shellLines[i] = Instantiate(linePrefab, this.transform).GetComponent<Line>();
            shellLines[i].gameObject.SetActive(false);
        }

        // Create Bezier Points
        bezierPoints = new GameObject[190];
        for (int i = 0; i < 190; ++i)
        {
            bezierPoints[i] = Instantiate(circlePrefab, this.transform);
            bezierPoints[i].SetActive(false);
        }

        // Initializing Pascal's Triangle till d = 20
        for (int i = 0; i < 21; ++i)
        {
            for (int j = 0; j <= i; ++j)
            {
                if (j == 0 || j == i)
                {
                    pascalsTri[i, j] = 1;
                }
                else
                {
                    pascalsTri[i, j] = pascalsTri[i - 1, j - 1] + pascalsTri[i - 1, j];
                }
            }
        }

        curvePoints = new Vector3[101];
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

        displayT = m_tValueSlider.value;
        m_tValueText.text = "t-value = " + displayT.ToString("n2");

        if (points > 0)
        {
            curve.SetActive(true);
            Vector3[] controlPos = new Vector3[points + 1];
            for (int i = 0; i <= points; ++i)
            {
                if (i < points)
                {
                    Vector3[] points = new Vector3[2];
                    points[0] = pointPool[i].transform.position;
                    points[1] = pointPool[i + 1].transform.position;
                    controlPolyLines[i].GetComponent<Line>().SetPoints(points, Color.gray);
                }
                controlPos[i] = pointPool[i].gameObject.transform.position;
            }
            if (mode == 2)
            {
                msCount = 0;
                curvePoints = MidpointSubdivision(controlPos);
                int c = 0;
                for (int i = 0; i < curvePoints.Length - 1; ++i)
                {
                    if (Mathf.Abs(curvePoints[i + 1].x - curvePoints[i].x) > 0.001f &&
                        Mathf.Abs(curvePoints[i + 1].y - curvePoints[i].y) > 0.001f)
                    {
                        curvePoints[c] = curvePoints[i];
                        ++c;
                    }
                    else
                    {
                        ++i;
                    }
                }
                curvePoints[c] = curvePoints[curvePoints.Length - 1];
                System.Array.Resize(ref curvePoints, c + 1);
            }
            else
            {
                System.Array.Clear(curvePoints, 0, curvePoints.Length);
                System.Array.Resize(ref curvePoints, 101);
                for (int i = 0; i < 101; ++i)
                {
                    float t = i / 100.0f;
                    if (mode == 0)
                    {
                        curvePoints[i] = NLI(controlPos, t);
                    }
                    else
                    {
                        curvePoints[i] = Bernstein(controlPos, t);
                    }
                }
            }
            curve.GetComponent<Line>().SetPoints(curvePoints, Color.red);
            if (mode == 0 && slideMode)
            {
                ActiveShells(true);
            }
            else
            {
                ActiveShells(false);
            }
        }
        else
        {
            curve.SetActive(false);
        }
    }

    private Vector3[] BezierPoints(Vector3[] pointsArr)
    {
        Vector3[] newPoints = new Vector3[pointsArr.Length * 2];
        newPoints[0] = pointsArr[0];
        newPoints[newPoints.Length - 1] = pointsArr[pointsArr.Length - 1];
        int count = 1;
        Vector3[] pointsClone = (Vector3[])pointsArr.Clone();
        int curLen = pointsArr.Length - 1;
        while (curLen > 0)
        {
            for (int i = 0; i < curLen; ++i)
            {
                pointsClone[i] = (0.5f * pointsClone[i]) + (0.5f * pointsClone[i + 1]);
            }
            newPoints[count] = pointsClone[0];
            newPoints[newPoints.Length - 1 - count] = pointsClone[curLen - 1];
            --curLen;
            ++count;
        }
        return newPoints;
    }

    private Vector3[] MidpointSubdivision(Vector3[] pointsArr)
    {
        if (msCount == 4)
        {
            return pointsArr;
        }
        Vector3[] newPoints = new Vector3[pointsArr.Length * 2];
        int sections = pointsArr.Length / (points + 1);
        for (int i = 0; i < sections; ++i)
        {
            Vector3[] pointsSection = new Vector3[points + 1];
            System.Array.Copy(pointsArr, i * (points + 1), pointsSection, 0, points + 1);
            System.Array.Copy(BezierPoints(pointsSection), 0, newPoints, i * 2 * (points + 1), pointsSection.Length * 2);
        }
        ++msCount;
        return MidpointSubdivision(newPoints);
    }

    private Vector3 Bernstein(Vector3[] pointsArr, float t)
    {
        Vector3 res = new Vector3(0.0f, 0.0f, 0.0f);
        for (int i = 0; i <= points; ++i)
        {
            res += pointsArr[i] * pascalsTri[points, i] * Mathf.Pow((1 - t), (points - i)) * Mathf.Pow(t, i);
        }
        return res;
    }

    private Vector3 NLI(Vector3[] pointsArr, float t)
    {
        int curLen = points;
        int shellCount = 0;
        int bezierCount = 0;
        Vector3[] newVec = (Vector3[])pointsArr.Clone();
        while (curLen > 0)
        {
            for (int i = 0; i < curLen; ++i)
            {
                newVec[i] = ((1 - t) * newVec[i]) +(t * newVec[i + 1]);
            }
            --curLen;
            if (curLen > 0)
            {
                if (t.ToString("n2") == displayT.ToString("n2"))
                {
                    Vector3[] shellPoints = new Vector3[curLen + 1];
                    System.Array.Copy(newVec, shellPoints, curLen + 1);
                    shellLines[shellCount].GetComponent<Line>().SetPoints(shellPoints, Color.blue);
                    ++shellCount;
                    for (int j = 0; j < 190 - bezierCount; ++j)
                    {
                        if (j < shellPoints.Length)
                        {
                            bezierPoints[bezierCount + j].transform.SetPositionAndRotation(shellPoints[j], Quaternion.identity);
                        }
                    }
                    bezierCount += shellPoints.Length;
                    bCount += shellPoints.Length;
                    if (shellPoints.Length == 2)
                    {
                        bezierPoints[bezierCount].transform.SetPositionAndRotation(((1 - t) * shellPoints[0]) + (t * shellPoints[1]), Quaternion.identity);
                        bezierPoints[bezierCount].SetActive(true);
                        ++bezierCount;
                        ++bCount;
                    }
                }
            }
        }
        return newVec[0];
    }

    private void ActiveShells(bool status)
    {
        for (int i = 0; i < 18; ++i)
        {
            if (i < points - 1)
            {
                shellLines[i].gameObject.SetActive(status);
            }
            else
            {
                shellLines[i].gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < 190; ++i)
        {
            if (i < bCount)
            {
                bezierPoints[i].SetActive(status);
            }
            else
            {
                bezierPoints[i].SetActive(false);
            }
        }
        bCount = 0;
    }

    private void ActivePoint()
    {
        for (int i = 0; i < 20; i++)
        {
            if (i <= points)
            {
                pointPool[i].gameObject.SetActive(true);
                if (i > 0)
                {
                    controlPolyLines[i - 1].gameObject.SetActive(true);
                }
            }
            else
            {
                pointPool[i].gameObject.SetActive(false);
                if (i > 0)
                {
                    controlPolyLines[i - 1].gameObject.SetActive(false);
                }
            }
        }
    }

    private void changeMode()
    {
        mode = m_modeDropdown.value;

        if (mode == 0)
        {
            m_shellToggle.gameObject.SetActive(true);
            m_tValueSlider.gameObject.SetActive(m_shellToggle.isOn);
            m_tValueText.gameObject.SetActive(m_shellToggle.isOn);
        }
        else
        {
            m_shellToggle.gameObject.SetActive(false);
            m_tValueSlider.gameObject.SetActive(false);
            m_tValueText.gameObject.SetActive(false);
        }
    }

    private void changetText()
    {
        slideMode = m_shellToggle.isOn;
        if (slideMode)
        {
            m_tValueSlider.gameObject.SetActive(true);
            m_tValueText.gameObject.SetActive(true);
        }
        else
        {
            m_tValueSlider.gameObject.SetActive(false);
            m_tValueSlider.value = 0.5f;
            m_tValueText.gameObject.SetActive(false);
        }
    }
}