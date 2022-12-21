using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClicks : MonoBehaviour
{

	//void Start()
	//{
	//	lights = GameObject.Find("Lights");
	//}

	public virtual void left_click(string name)
	{
		Debug.Log($"PARENT {name}");
		//lights.SetActive(!lights.activeInHierarchy);
	}
}
