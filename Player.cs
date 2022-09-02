using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

	public float boostStrength;
	public float movementSensitivity;
	public float groundedRotationSensitivity;
	public float floatingRotationSensitivity;
	public float magneticShoesMaxRadius;
	private float moveHorizontal;
	private float moveVertical;
	private float mouseX;
	private float mouseY;
	private bool boostKeyPressed;
	private int boostDirection;
	private bool areMagneticShoesOn;
	private bool stickIt = false;
	private bool isGrounded = false;
	private int collisionCount;
	private Rigidbody rigidBody;
	private new GameObject camera;
	private Vector3 elementClosestPoint;
	private float localFootPoint;

	void Start()
	{
		rigidBody = this.GetComponent<Rigidbody>();
		camera = Camera.main.gameObject;
		Cursor.lockState = CursorLockMode.Locked;
		boostStrength = 5f;
		boostKeyPressed = false;
		movementSensitivity = 5f;
		groundedRotationSensitivity = .7f;
		floatingRotationSensitivity = .1f;
		areMagneticShoesOn = false;
		rigidBody.maxAngularVelocity = 1f;
		magneticShoesMaxRadius = 10f;
		localFootPoint = -transform.lossyScale.y/2;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.E))
		{
			rigidBody.velocity = new Vector3(0, 0, 0);
		}
		if (Input.GetKey("space"))
		{
			boostKeyPressed = true;
			boostDirection = Input.GetKey("left shift") ? -1 : 1;
		}
		if (Input.GetKeyDown(KeyCode.F))
		{
			areMagneticShoesOn = !areMagneticShoesOn;
			if (!areMagneticShoesOn)
			{
				unGround();
			}
		}
		moveHorizontal = Input.GetAxis("Horizontal") * movementSensitivity;
		moveVertical = Input.GetAxis("Vertical") * movementSensitivity;
		mouseX = Input.GetAxis("Mouse X");
		mouseY = Input.GetAxis("Mouse Y");

		// PLAYER ROTATION
		if (areMagneticShoesOn & isGrounded)
		{ // GROUNDED PLAYER ROTATION
			rigidBody.transform.Rotate(new Vector3(0, mouseX, 0) * groundedRotationSensitivity);
			camera.transform.Rotate(new Vector3(-mouseY, 0, 0) * groundedRotationSensitivity);
		}
		else
		{ // FLYING PLAYER RORATION, REGARDLESS OF MAGNET SHOES ON OR NOT
			rigidBody.AddRelativeTorque(new Vector3(-mouseY, mouseX, 0) * floatingRotationSensitivity);
		}
	}

	void FixedUpdate()
	{
		float velX;
		float velY;
		float velZ;
		Vector3 relVel = transform.InverseTransformDirection(rigidBody.velocity);
		float magneticStrength = 5;
		Vector3 footPoint = transform.TransformPoint(new Vector3(0, localFootPoint/transform.lossyScale.y, 0));

		// PLAYER MOVEMENT
		if (areMagneticShoesOn)
		{ // MAGNET SHOES ON PLAYER MOVEMENT
			RaycastHit[] hits = Physics.SphereCastAll(transform.position, magneticShoesMaxRadius, transform.forward, 0);
			Collider closestElement = null;
			int doOrNot = 0;
			float closestElementDistance = 1000f * magneticShoesMaxRadius;
			for (int i = 0; i < hits.Length; i++)
			{
				Vector3 tempElementClosestPoint = hits[i].collider.ClosestPointOnBounds(footPoint);
				float elementDistance = Vector3.Distance(tempElementClosestPoint, footPoint);
				if (hits.Length > 1) // player is always closest element, so ignore that first element
				{
					if (elementDistance < closestElementDistance & hits[i].collider.transform.gameObject.name != "Player")
					{
						closestElement = hits[i].collider;
						elementClosestPoint = tempElementClosestPoint - footPoint;
						isGrounded = elementClosestPoint == new Vector3(0, 0, 0);
						closestElementDistance = elementDistance;
						doOrNot = 1;
					}
				}
			}
			if (!isGrounded)
			{ // MAGNETIZING DOWNWARD PLAYER MOVEMENT, STILL NOT GROUNDED
				// get ray cast from feet to surface below feet, use that distance as F = c2/(c1+d)^2
				// once i can get the distance between player and closest thing below it, i can see the force of the magnet
				// and if theres nothing below it, the for
				// if i had feet be separate, i think i would cause a torque toward the surface which would be more realistic, want to do that at the end
				rigidBody.AddForceAtPosition(elementClosestPoint.normalized * magneticStrength * doOrNot, footPoint, ForceMode.Acceleration);

				
				if (boostKeyPressed)
				{
					rigidBody.AddRelativeForce(Vector3.forward * boostStrength * boostDirection, ForceMode.Acceleration);
					if (!Input.GetKey("space"))
					{
						boostKeyPressed = false;
					}
				}	
				velX = relVel.x;
				velY = relVel.y;
				velZ = relVel.z;
				
				rigidBody.constraints = RigidbodyConstraints.None;
			}
			else
			{ // GROUNDED PLAYER MOVEMENT
				if (stickIt)
				{
					rigidBody.angularVelocity = new Vector3(0, 0, 0);
					rigidBody.velocity = new Vector3(0, 0, 0);
					stickIt = false;
				}
				rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
				rigidBody.AddRelativeForce(Vector3.down * magneticStrength, ForceMode.Acceleration);
				velX = moveHorizontal;
				velY = relVel.y;
				velZ = moveVertical;
			}
		}
		else
		{ // FLYING PLAYER MOVEMENT WITH MAGNET SHOES OFF
			if (boostKeyPressed)
			{
				rigidBody.AddRelativeForce(Vector3.forward * boostStrength * boostDirection, ForceMode.Acceleration);
				if (!Input.GetKey("space"))
				{
					boostKeyPressed = false;
				}
			}
			
			rigidBody.constraints = RigidbodyConstraints.None;
			velX = relVel.x;
			velY = relVel.y;
			velZ = relVel.z;
		}

		rigidBody.velocity = transform.rotation * new Vector3(velX, velY, velZ);
	}

	// DETECT COLLISION WITH AN OBJECT
	void OnCollisionEnter(Collision hit)
	{
		collisionCount += 1;
		if (!isGrounded & areMagneticShoesOn)
		{ // if you hit a wall while already walking on the ground, don't do anything. only do something if not grounded and you touch a surface
			// also don't do anything if you touch a surface and your magnets arent on
			stickIt = true;
		}
	}
 
	// DETECT EXITING A COLLOSION WITH AN OBJECT
	void OnCollisionExit(Collision hit)
	{
		if (collisionCount == 1)
		{ // only become ungrounded if the surface you stopped touching was the ground
			unGround();
		}
	}

	void unGround()
	{
		if (collisionCount > 0)
		{
			collisionCount -= 1;
		}
		isGrounded = false;
		stickIt = false;
	}
}
