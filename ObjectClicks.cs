using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClicks : MonoBehaviour
{
	
	private GameObject lights;

	void Start()
	{
		lights = GameObject.Find("Lights");
	}

	public void left_click()
	{
		lights.SetActive(!lights.activeInHierarchy);
	}
}
