using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
	public GameObject pointTo;
	private Renderer[] renderers;
	private GameObject uiPointer;

    // Start is called before the first frame update
    void Start()
    {
		uiPointer = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        setArrowPosition();
    }

	void setArrowPosition() {
		renderers = pointTo.GetComponentsInChildren<Renderer>();
		bool enable = true;
		for (int i = 0; i < renderers.Length; i++) {
			if(renderers[i].isVisible) {
				enable = false;
			} else {
				break;
			}
		}
		Vector3 pointToPosition = pointTo.transform.position;
		Vector3 pointToPositionViewport = Camera.main.WorldToViewportPoint(pointToPosition); // goes from 0 to 1
		float ang;
		if (pointToPositionViewport.z <= Camera.main.nearClipPlane) { // camera is behind
			ang = Mathf.Atan2(1f - (pointToPositionViewport.y - .5f), 1f - (pointToPositionViewport.x - .5f));
		} else {
			ang = Mathf.Atan2(pointToPositionViewport.y - .5f, pointToPositionViewport.x - .5f);
		}
		ang = ang < 0 ? ang + Mathf.PI * 2 : ang;
		transform.localEulerAngles = new Vector3(0f, 0f, ang * Mathf.Rad2Deg - 90);
		Vector3 uiPointerPosition;
		Rect rect = uiPointer.GetComponent<RectTransform>().rect;
		uiPointerPosition.x = (Screen.width - rect.height / 4) * .5f * Mathf.Cos(ang) + Screen.width * .5f;  // Place on ellipse touching
		uiPointerPosition.y = (Screen.height - rect.height / 4) * .5f * Mathf.Sin(ang) + Screen.height * .5f;  // Place on ellipse touching
		uiPointerPosition.z = Camera.main.nearClipPlane + .01f;  // Looking from neg to pos z;
		transform.position = uiPointerPosition;
		uiPointer.GetComponent<RawImage>().enabled = enable;
	}
}
