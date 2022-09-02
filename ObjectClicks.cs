using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClicks : MonoBehaviour
{
	
	private GameObject lights;
	// Start is called before the first frame update
	void Start()
	{
		lights = GameObject.Find("Lights");
	}

	// Update is called once per frame
	void Update()
	{
	}

	// FixedUpdate is called once every physics update
	private void FixedUpdate()
	{
	}

	public void left_click()
	{
		print("RAN left_click in cube 1 clicks script");
		lights.SetActive(!lights.activeInHierarchy);
	}
}
