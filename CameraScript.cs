using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	public float cameraMaxHeight;
	public float cameraMinHeight;
	public float cameraMaxDistance;
	private float mouseScroll;

	void Start()
	{
		cameraMaxHeight = .678f;
		cameraMinHeight = .5f;
		cameraMaxDistance = -6f;
		mouseScroll = cameraMaxDistance;
	}

	void Update()
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float cameraLoc = mouseScroll;
		// for mouse scroll, when i come away from a surface i want it to go back to what it was before.
		// however, when i am at a surface i dont want to be scrolling from old mousescroll point,
		// i want to be scrolling from the wall.
		// so i need to keep track of old mouse scroll and new mouse scroll separately.
		// and i need to scale to be squished when next to the wall so the current scroll wheel value is for the wall.
		mouseScroll += Input.GetAxis("Mouse ScrollWheel")*3;
		if (Physics.Raycast(transform.parent.transform.position, Input.mousePosition - Camera.main.gameObject.transform.position, out hit, 100.0f))
		{	
			if (Input.GetMouseButtonDown(0)){
				if (hit.collider)
				{
					var objectClicks = hit.collider.gameObject.GetComponent<ObjectClicks>();
					if (objectClicks) {
						objectClicks.left_click();
					}
				}
			}
		}

		// either make camera go up and down when you look up and down so it passes thru the same point on the player always,
		// or put the camera to the side like fps (maybe this is best)
		// or make the person a little seethru
		// or find a way to let raycast see thru a specific object
		// or take raycast from mouse point to camera, find point on player that intersects, make ray from player point
		// to mouse point and use that for clicking. should work but, is it even good to have the player in the way of view??
		float cameraDistance = Vector3.Distance(Camera.main.gameObject.transform.position, transform.parent.transform.position);
		RaycastHit hit2;
		if (Physics.Raycast(transform.parent.transform.position, Camera.main.gameObject.transform.position - transform.parent.transform.position, out hit2, cameraDistance))
		{
			cameraLoc = -Vector3.Distance(hit2.point, transform.parent.transform.position) * .95f;
		}
		cameraLoc = Mathf.Clamp(cameraLoc, cameraMaxDistance, 0f);
		float cameraHeight = (cameraMinHeight - cameraMaxHeight) / (-cameraMaxDistance) * (cameraLoc - cameraMaxDistance) + cameraMaxHeight;
		Camera.main.gameObject.transform.localPosition = new Vector3(0, cameraHeight, cameraLoc);
	}
}
