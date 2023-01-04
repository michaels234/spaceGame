using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : ObjectClicks
{
	GameObject lights;
	void Start()
	{
		lights = GameObject.Find("Lights");
	}

	// Update is called once per frame
	void Update()
	{
	}

	public override void left_click(string name)
	{
		lights.SetActive(!lights.activeInHierarchy);
		Debug.Log($"lights {lights.activeInHierarchy}");
	}
}
