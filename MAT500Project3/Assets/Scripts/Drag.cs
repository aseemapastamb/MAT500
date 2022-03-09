using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Reflection;

/**
License: do whatever you want.
Author: @t_machine_org
*/
public class Drag : MonoBehaviour
{
	public GameObject target;

	// Update is called once per frame

	Vector3 screenPoint;
	Vector3 original;

	void OnMouseDown()
	{
		screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
		original = target.transform.position;
	}

	void OnMouseDrag()
	{
		Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

		Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);

		transform.position = curPosition;
	}
}
