using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {
	public float boostStrength;
	public float jumpStrength;
	public float movementSensitivity;
	public float groundedRotationSensitivity;
	public float flyingRotationSensitivity;
	public float magneticShoesMaxRadius;
	public float weakMagnetFraction;
	public float maxMagnetStrength;
	public float playerMass;
	public float magnetLockdownStrength;
	public float magnetLockdownDistance;
	public GameObject marker;
	public GameObject arrow;
	//private GameObject markerCopy;
	//private GameObject arrowCopy;
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
	private Vector3 elementClosestPointDirection;
	private Vector3 elementClosestPoint;
	private float footDist;
	private Text speedDisplay;
	private Text magnetShoesDisplay;
	private float closestElementDistance;
	private float c1;
	private float c2;
	private float magneticStrength;
	private float tempMagneticStrength;
	private Vector3 footPoint;
	// TODO: for slerping
	//private bool groundedRotateBack;
	//private float timeCount;
	//private float slerpSpeed;

	void Start() {
		rigidBody = this.GetComponent<Rigidbody>(); // set the rigidBody of the player
		camera = Camera.main.gameObject; // set the gameObject of the camera
		Cursor.lockState = CursorLockMode.Locked; // lock the cursor into the screen
		boostStrength = 5f; // boost strength acceleration when flying
		boostKeyPressed = false; // boost key is pressed flag
		boostDirection = 1; // boost forward (1) or backward (-1)
		movementSensitivity = 5f; // keyboard player movement sensitivity when grounded
		groundedRotationSensitivity = 1f; // player/camera rotation sensitivity when grounded or magnetizing downward
		flyingRotationSensitivity = 10f; // player rotation torque sensitivity when flying
		areMagneticShoesOn = false; // magnetic shoes are on flag
		rigidBody.maxAngularVelocity = 1f; // hard set maximum angular (rotation) velocity of player
		magneticShoesMaxRadius = 10f; // radius within which magnetic shoes can lock on to an object
		footDist = -transform.lossyScale.y/2; // distance of foot point from center of player in y direction
		speedDisplay = GameObject.Find("TextSpeed").GetComponent<Text>(); // speed display text component
		speedDisplay.text = $"{0.0}"; // speed display text component's string
		magnetShoesDisplay = GameObject.Find("TextMagnetShoes").GetComponent<Text>(); // magnetic shoe text component
		magnetShoesDisplay.text = "Magnets OFF"; // magnetic shoe text component's string
		playerMass = 80f; // player mass. used for player physics
		rigidBody.mass = playerMass; // hard set player mass
		weakMagnetFraction = .5f; // magnet shoes initial far (weak) strength fraction compared to max
		maxMagnetStrength = 7f; // magnet shoes close (strong) strength for normal magnetation
		jumpStrength = 4f; // strength of jumping (when grounded)
		magnetLockdownStrength = 10f; // special "lockdown" strength to force sticking still to the surface
		magnetLockdownDistance = .5f; // distance at which the above magnetLockdownStrength is activated
		closestElementDistance = magneticShoesMaxRadius * 1.1f; // distance to the closest element to feet
		// c1, c2 used for physically accurate magnetic force calculation
		c2 = Mathf.Pow(weakMagnetFraction, .5f) * (Mathf.Pow(weakMagnetFraction, .5f) + 1f) * magneticShoesMaxRadius / (1f - weakMagnetFraction);
		c1 = Mathf.Pow(c2, 2f) * maxMagnetStrength;
		magneticStrength = c1 / Mathf.Pow(c2 + closestElementDistance, 2f); // strength of magnetic shoes
		footPoint = transform.TransformPoint(new Vector3(0, footDist/transform.lossyScale.y, 0)); // point of center of player foot in global coordinates
		// Objects for displaying points and vectors
		//markerCopy = Instantiate(marker, new Vector3(0, 0, 0), Quaternion.identity);
		//markerCopy.SetActive(false);
		//arrowCopy = Instantiate(arrow, footPoint, Quaternion.identity);
		//arrowCopy.SetActive(true);
		// TODO: for slerping
		//groundedRotateBack = false;
		//timeCount = 0.0f;
		//slerpSpeed = .005f;
	}

	void Update() { // updates with time (frames)
		// Use this to output something to the log
		//Debug.Log("Update");

		// GET KEYS PRESSED AND MOUSE MOVEMENTS
		if (Input.GetKeyDown(KeyCode.E)) { // stop player (only for testing)
			rigidBody.velocity = new Vector3(0, 0, 0);
		}
		if (Input.GetKey("space")) { // boosting
			boostKeyPressed = true;
			boostDirection = Input.GetKey("left shift") ? -1 : 1;
		}
		if (Input.GetKeyDown(KeyCode.F)) { // magnetic shoes on / off
			areMagneticShoesOn = !areMagneticShoesOn;
			magnetShoesDisplay.text = areMagneticShoesOn ? "Magnets ON" : "Magnets OFF";
			if (!areMagneticShoesOn) {
				unGround();
			}
		}
		moveHorizontal = Input.GetAxis("Horizontal") * movementSensitivity;
		moveVertical = Input.GetAxis("Vertical") * movementSensitivity;
		mouseX = Input.GetAxis("Mouse X");
		mouseY = Input.GetAxis("Mouse Y");

		// TODO: for slerping
		//timeCount = timeCount + Time.deltaTime;

		// PLAYER ROTATION
		footPoint = transform.TransformPoint(new Vector3(0, footDist/transform.lossyScale.y, 0));
		if (areMagneticShoesOn & isGrounded) {
			// GROUNDED PLAYER ROTATION
			rigidBody.transform.Rotate(new Vector3(0, mouseX, 0) * groundedRotationSensitivity); // body rotates only Y
			// rotate the camera in X direction instead of the body because body is fixed to ground
			camera.transform.Rotate(new Vector3(-mouseY, 0, 0) * groundedRotationSensitivity);
		} else if (areMagneticShoesOn) { // magnet shoes on, but not yet grounded
			// MAGNETS ON BUT NOT GROUNDED YET
			// get closest element
			RaycastHit[] hits = Physics.SphereCastAll(transform.position, magneticShoesMaxRadius, transform.forward, 0);
			Collider closestElement = null;
			closestElementDistance = magneticShoesMaxRadius;
			int isMagnetNear = hits.Length > 1 ? 1 : 0; // goes into magneticStrength as factor of 1 or 0 if magnet is or isn't near 
			if (Convert.ToBoolean(isMagnetNear)) { // element exists (player is always closest element, so ignore that first element)
				for (int i = 0; i < hits.Length; i++) { // search for closest element
					Vector3 tempElementClosestPoint = hits[i].collider.ClosestPointOnBounds(footPoint);
					float elementDistance = Vector3.Distance(tempElementClosestPoint, footPoint);
						if (elementDistance < closestElementDistance & hits[i].collider.transform.gameObject.name != "Player") {
							// element is closer than any other closer element we've checked so far
							closestElement = hits[i].collider;
							elementClosestPoint = tempElementClosestPoint;
							closestElementDistance = elementDistance;
						}
				}
				elementClosestPointDirection = (elementClosestPoint - footPoint).normalized;
				isGrounded = elementClosestPointDirection == new Vector3(0, 0, 0);
				// TODO: for slerping
				//if (isGrounded) {
				//	// this will start rotation back to forward after looking toward the magnet spot
				//	groundedRotateBack = true;
				//}
			}
			magneticStrength = c1 / Mathf.Pow(c2 + closestElementDistance, 2f) * isMagnetNear;

			// if there is a magnet locked on
			if (Convert.ToBoolean(isMagnetNear)) {	// mouse controls camera rotation
				camera.transform.Rotate(new Vector3(-mouseY, mouseX, 0) * groundedRotationSensitivity);
			} else { // otherwise if not locked on magnet
				// mouse controls body rotation
				rigidBody.AddRelativeTorque(new Vector3(-mouseY, mouseX, 0) * flyingRotationSensitivity); // note: mouse controls seem to work best in Update
			}
		} else {
			// FLYING PLAYER ROTATION, MAGNET SHOES OFF
			// mouse controls body rotation
			rigidBody.AddRelativeTorque(new Vector3(-mouseY, mouseX, 0) * flyingRotationSensitivity); // note: mouse controls seem to work best in Update
			// while flying, camera is to be pointing forward. we slerp up to it so that it doesnt move there suddenly after taking off
			// TODO: make it slerp instead of instantly looking
			//camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, Quaternion.Euler(0, 0, 0), timeCount * slerpSpeed);
			camera.transform.localRotation = Quaternion.Euler(0, 0, 0);
		}

		// DISPLAY TEXT
		speedDisplay.text = $"Speed {Mathf.Round(rigidBody.velocity.magnitude * 10f) / 10f}";
	}

	void FixedUpdate() { // updates with physics
		float velX = 0; // will be used at the end of this to set the X velocity of the player in relative coordinates
		float velY = 0; // will be used at the end of this to set the Y velocity of the player in relative coordinates
		float velZ = 0; // will be used at the end of this to set the Z velocity of the player in relative coordinates
		Vector3 relVel = transform.InverseTransformDirection(rigidBody.velocity); // current relative velocity of player
		// PLAYER MOVEMENT (NOT ROTATION)
		if (areMagneticShoesOn) {
		// MAGNET SHOES ON PLAYER MOVEMENT
			if (!isGrounded) {
			// MAGNETIZING DOWNWARD PLAYER MOVEMENT, STILL NOT GROUNDED
				rigidBody.constraints = RigidbodyConstraints.None;
				// special "lockdown" strength to force sticking still to the surface
				float lockDownFactor = closestElementDistance <= magnetLockdownDistance & closestElementDistance != 0 ? magnetLockdownStrength : 1;
				float ang = Vector3.Angle(-transform.up, elementClosestPointDirection);
				bool layingDownCheck = closestElementDistance <= .6f & closestElementDistance >= .1f & !isGrounded &  ang >= 40f & ang <= 95f;
				if (layingDownCheck) {
					Debug.Log("DID IT");
					rigidBody.AddTorque(Vector3.Cross(-transform.up, elementClosestPointDirection).normalized * 100f);
				} else {
					Debug.Log("DID NOT DO IT");
					// magnetic force toward footPoint (if magnet is not near the strength is 0)
					rigidBody.AddForceAtPosition(elementClosestPointDirection * magneticStrength * lockDownFactor, footPoint, ForceMode.Acceleration);
				}
				// still allow boosting while not totally grounded
				if (boostKeyPressed) { // boosting
					rigidBody.AddRelativeForce(Vector3.forward * boostStrength * boostDirection, ForceMode.Acceleration);
					if (!Input.GetKey("space")) {
						boostKeyPressed = false;
					}
				}	
				velX = relVel.x; // velX not controled, changes naturally based on phyisics from external forces
				velY = relVel.y; // velY not controled, changes naturally based on phyisics from external forces
				velZ = relVel.z; // velZ not controled, changes naturally based on phyisics from external forces
			} else {
			 	// GROUNDED PLAYER MOVEMENT
				rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
				if (stickIt) {
					rigidBody.angularVelocity = new Vector3(0, 0, 0);
					rigidBody.velocity = new Vector3(0, 0, 0);
					camera.transform.localRotation = Quaternion.Euler(0, 0, 0);
					stickIt = false;
				}
				if (boostKeyPressed) { // JUMP UP WHEN GROUNDED (INSTEAD OF BOOST FORWARD) BTW THEN YOU'RE NOT GROUNDED
					rigidBody.AddRelativeForce(Vector3.up * jumpStrength, ForceMode.VelocityChange);
					if (!Input.GetKey("space")) {
						boostKeyPressed = false;
					}
				}
				rigidBody.AddRelativeForce(Vector3.down * magneticStrength, ForceMode.Acceleration);
				velX = moveHorizontal; // velX controled by keyboard
				velY = relVel.y; // velY not controled, changes naturally based on phyisics from external forces
				velZ = moveVertical; // velZ controled by keyboard
			}
		} else {
			// FLYING PLAYER MOVEMENT WITH MAGNET SHOES OFF
			if (boostKeyPressed) { // boosting
				rigidBody.AddRelativeForce(Vector3.forward * boostStrength * boostDirection, ForceMode.Acceleration);
				if (!Input.GetKey("space")) {
					boostKeyPressed = false;
				}
			}
			rigidBody.constraints = RigidbodyConstraints.None;
			velX = relVel.x; // velX not controled, changes naturally based on phyisics from external forces
			velY = relVel.y; // velY not controled, changes naturally based on phyisics from external forces
			velZ = relVel.z; // velZ not controled, changes naturally based on phyisics from external forces
		}
		// translate relative velocity to global velocity and set it
		rigidBody.velocity = transform.rotation * new Vector3(velX, velY, velZ);
	}

	// DETECT COLLISION WITH AN OBJECT
	void OnCollisionEnter(Collision hit) {
		collisionCount += 1;
		if (!isGrounded & areMagneticShoesOn) {
			stickIt = true;
		}
	}
 
	// DETECT EXITING A COLLOSION WITH AN OBJECT
	void OnCollisionExit(Collision hit) {
		if (collisionCount > 0) { // don't decrease collisionCount into the negatives
			collisionCount -= 1;
		}
		if (collisionCount == 0) { // only become ungrounded if the surface you stopped touching was the last collision
			unGround();
		}
	}

	void unGround() {
		isGrounded = false;
		stickIt = false;
	}
}
