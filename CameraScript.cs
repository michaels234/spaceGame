using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraScript : MonoBehaviour
{
	public float thirdPersonCameraLeft;
	public float thirdPersonCameraDistance;
	public float cameraHeight;
	public float wallCameraBuffer;
	private bool firstPerson;
	private float cameraLoc;
	private float cameraLeft;
	private float cameraDistance;

	void Start()
	{
		thirdPersonCameraLeft = -.8f;
		thirdPersonCameraDistance = -6f;
		cameraHeight = .5f;
		wallCameraBuffer = .9f;
		firstPerson = true; // default camera setting is firstPerson
	}

	void Update()
	{
		// MOUSE CLICK ON AN OBJECT
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, 100.0f))
		{
			if (Input.GetMouseButtonDown(0)){ // 0 = left click
				if (hit.collider)
				{
					Debug.Log($"HIT {hit.collider.gameObject.name}");
					//var name = hit.collider.gameObject.name;
					//var components = hit.collider.gameObject.GetComponents();


					//Component[] components = hit.collider.gameObject.GetComponents(typeof(Component));
					//foreach(Component component in components) {
					//	Debug.Log($"component {component.name} {hit.collider.gameObject.name}");
					//	if (component.name == hit.collider.gameObject.name) {
					//		//Type type = typeof(component.name);
					//		hit.collider.gameObject.GetComponent(Type.GetType(component.name)).left_click();
					//		break;
					//	}
					//}
					hit.collider.gameObject.GetComponent<ObjectClicks>().left_click(hit.collider.gameObject.name);
					//hit.collider.gameObject.GetComponent<T>().left_click(hit.collider.gameObject.name);
				}
			}
		}

		// FIRST PERSON / THIRD PERSON CAMERA LOCATION
		if (Input.GetKeyDown(KeyCode.Q))
		{
			firstPerson = !firstPerson;
		}
		if (firstPerson)
		{
			cameraLoc = 0;
			cameraLeft = 0;
		}
		else
		{
			cameraDistance = Vector3.Distance(transform.position, transform.parent.transform.position);
			RaycastHit hit2;
			if (Physics.Raycast(transform.parent.transform.position, transform.position - transform.parent.transform.position, out hit2, cameraDistance * 1.01f / wallCameraBuffer ))
			{
				cameraLoc = -Vector3.Distance(hit2.point, transform.parent.transform.position) * wallCameraBuffer;
			}
			else
			{
				cameraLoc = thirdPersonCameraDistance;
			}
			cameraLoc = Mathf.Clamp(cameraLoc, thirdPersonCameraDistance, 0f);
			cameraLeft = thirdPersonCameraLeft;
		}
		Camera.main.gameObject.transform.localPosition = new Vector3(cameraLeft, cameraHeight, cameraLoc);
	}
}
