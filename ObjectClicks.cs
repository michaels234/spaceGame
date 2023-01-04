using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClicks : MonoBehaviour
{
	public virtual void left_click(string name)
	{
		Debug.Log($"PARENT {name}");
	}
}
