using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlanets : MonoBehaviour
{
	public GameObject planet;
	private GameObject ship;
	public float spawnRadius = 1000.0f;
	public float spawnMaxAngle = 45.0f;
	private float timer = 0.0f;
	private int timeToSpawn;
	public float speed = 50.0f;

    // Start is called before the first frame update
    void Start()
    {
		timeToSpawn = Random.Range(1, 5);
		ship = GameObject.Find("Ship");
    }

	void SpawnPlanet()
	{
		Quaternion shipRotation = ship.transform.rotation;
		float randAngleY = ( Random.Range(-spawnMaxAngle, spawnMaxAngle) + shipRotation.y + 90 ) / 360.0f * 2.0f * Mathf.PI;
		float randAngleX = Random.Range(-spawnMaxAngle, spawnMaxAngle) / 360.0f * 2.0f * Mathf.PI;
		float x = Mathf.Cos(randAngleY) * spawnRadius * Mathf.Cos(randAngleX);
		float z = Mathf.Sin(randAngleY) * spawnRadius * Mathf.Cos(randAngleX);
		float y = spawnRadius * Mathf.Sin(randAngleX);
		Vector3 randPosition = new Vector3(x, y, z) + ship.transform.position;
		GameObject spawned = Instantiate(planet, randPosition, Quaternion.identity);
		spawned.SetActive(true);
		spawned.GetComponent<Rigidbody>().velocity = new Vector3(Mathf.Cos(shipRotation.y - 90) * speed, 0, Mathf.Sin(shipRotation.y - 90) * speed);
	}

    // Update is called once per frame
    void Update()
    {
		timer += Time.deltaTime;
		if ( timer >= timeToSpawn)
		{
			SpawnPlanet();
			timer = 0;
			timeToSpawn = Random.Range(1, 5);
		}
    }
}
