using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BayDoor : ObjectClicks
{
	public float slerpSpeed = .0005f;
	private float timeCount = 0f;
	private Rigidbody rigidBody;
	private bool isBayDoorOpen = false;
	//// Start is called before the first frame update
	void Start()
	{
		rigidBody = this.GetComponent<Rigidbody>(); // set the rigidBody of the player
	}

	// Update is called once per frame
	void Update()
	{
		timeCount = timeCount + Time.deltaTime;
		if (isBayDoorOpen) {
			if (transform.localRotation == Quaternion.Euler(-45, 90, -90)) {
				timeCount = 0f;
			}
			rigidBody.transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(-45, 90, -90), slerpSpeed * timeCount);
		} else {
			if (transform.localRotation == Quaternion.Euler(-90, 90, -90)) {
				timeCount = 0f;
			}
			rigidBody.transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(-90, 90, -90), slerpSpeed * timeCount);
		}
	}

	public override void left_click(string name)
	{
		timeCount = 0f;
		Debug.Log($"MADE IT {name}");
		//gameObject.SetActive(!gameObject.activeInHierarchy);
		isBayDoorOpen = !isBayDoorOpen;
		Debug.Log($"isBayDoorOpen {isBayDoorOpen}");
	}
}
