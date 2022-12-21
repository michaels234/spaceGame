using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {
	public float boostStrength = 5f; // boost strength acceleration when flying
	public float jumpStrength = 4f; // strength of jumping (when grounded)
	public float movementSensitivity = 5f; // keyboard player movement sensitivity when grounded
	public float groundedRotationSensitivity = 1f; // player/camera rotation sensitivity when grounded or magnetizing downward
	public float flyingRotationSensitivity = 20f; // player rotation torque sensitivity when flying
	public float magneticShoesMaxRadius = 10f; // radius within which magnetic shoes can lock on to an object
	public float weakMagnetFraction = .5f; // magnet shoes initial far (weak) strength fraction compared to max
	public float maxMagnetStrength = 7f; // magnet shoes close (strong) strength for normal magnetation
	public float playerMass = 80f; // player mass. used for player physics
	public float magnetLockdownStrength = 10f; // special "lockdown" strength to force sticking still to the surface
	public float magnetLockdownDistance = .5f; // distance at which the above magnetLockdownStrength is activated
	public float maxAngularVelocity = 1f; // for setting maximum angular (rotation) velocity of player
	public float slerpSpeed = .001f;
	public GameObject marker; // drag and job marker object into this public property field in the player script
	//public GameObject vectorArrow; // drag and job marker object into this public property field in the player script
	//private GameObject markerCopy;
	//private GameObject markerCopy2;
	//private GameObject vectorArrowCopy;
	private float moveHorizontal;
	private float moveVertical;
	private float mouseX;
	private float mouseY;
	private int collisionCount;
	private Vector3 elementClosestPointDirection;
	private Vector3 elementClosestPoint;
	private float footDist; // distance of foot point from center of player in y direction
	private float closestElementDistance; // distance to the closest element to feet
	private float c1; // c1, c2 used for physically accurate magnetic force calculation
	private float c2; // c1, c2 used for physically accurate magnetic force calculation
	private float magneticStrength; // strength of magnetic shoes
	private Vector3 footPoint; // point of center of player foot in global coordinates
	private bool boostKeyPressed = false; // boost key is pressed flag
	private bool isSpaceSuitOn = true;
	private int boostDirection = 1; // boost forward (1) or backward (-1)
	private bool areMagneticShoesOn = false; // magnetic shoes are on flag
	private bool isGrounded = false;
	private Rigidbody rigidBody; // for setting the rigidBody of the player
	private new GameObject camera; // for setting the gameObject of the camera
	private Text speedDisplay; // speed display text component
	private Text magnetShoesDisplay; // magnetic shoe on / off text component
	private Text spaceSuitDisplay; // space suit on / off text component
	private float timeCount = 0f;

	void Start() {
		rigidBody = this.GetComponent<Rigidbody>(); // set the rigidBody of the player
		camera = Camera.main.gameObject; // set the gameObject of the camera
		Cursor.lockState = CursorLockMode.Locked; // lock the cursor into the screen
		rigidBody.maxAngularVelocity = maxAngularVelocity;
		footDist = -transform.lossyScale.y/2;
		speedDisplay = GameObject.Find("TextSpeed").GetComponent<Text>();
		magnetShoesDisplay = GameObject.Find("TextMagnetShoes").GetComponent<Text>();
		spaceSuitDisplay = GameObject.Find("TextSpaceSuit").GetComponent<Text>();
		speedDisplay.text = $"{0.0}";
		magnetShoesDisplay.text = "Magnets OFF";
		spaceSuitDisplay.text = "Space Suit ON";
		rigidBody.mass = playerMass;
		closestElementDistance = magneticShoesMaxRadius * 1.1f;
		c2 = Mathf.Pow(weakMagnetFraction, .5f) * (Mathf.Pow(weakMagnetFraction, .5f) + 1f) * magneticShoesMaxRadius / (1f - weakMagnetFraction);
		c1 = Mathf.Pow(c2, 2f) * maxMagnetStrength;
		magneticStrength = c1 / Mathf.Pow(c2 + closestElementDistance, 2f);
		footPoint = transform.TransformPoint(new Vector3(0, footDist/transform.lossyScale.y, 0));
		// Objects for displaying points and vectors
		//markerCopy = Instantiate(marker, new Vector3(0, 0, 0), Quaternion.identity);
		//markerCopy.SetActive(true);
		//markerCopy2 = Instantiate(marker, new Vector3(0, 0, 0), Quaternion.identity);
		//markerCopy2.SetActive(true);
		//vectorArrowCopy = Instantiate(vectorArrow, footPoint, Quaternion.identity);
		//vectorArrowCopy.SetActive(true);
	}

	void Update() { // updates with time (frames)
		// Use this to output something to the log
		//Debug.Log("Update");

		// GET KEYS PRESSED AND MOUSE MOVEMENTS
		if (Input.GetKeyDown(KeyCode.E)) { // stop player (only for testing)
			rigidBody.velocity = new Vector3(0, 0, 0);
		}
		if (Input.GetKeyDown(KeyCode.C)) { // space suit on / off
			timeCount = 0f;
			isSpaceSuitOn = !isSpaceSuitOn;
			spaceSuitDisplay.text = isSpaceSuitOn ? "Space Suit ON" : "Space Suit OFF";
		}
		if (Input.GetKey("space") & (isSpaceSuitOn | isGrounded)) { // boosting
			boostKeyPressed = true;
			boostDirection = Input.GetKey("left shift") ? -1 : 1;
		}
		if (Input.GetKeyDown(KeyCode.F)) { // magnetic shoes on / off
			timeCount = 0f;
			areMagneticShoesOn = !areMagneticShoesOn;
			magnetShoesDisplay.text = areMagneticShoesOn ? "Magnets ON" : "Magnets OFF";
			if (!areMagneticShoesOn) {
				isGrounded = false;
			}
		}
		moveHorizontal = Input.GetAxis("Horizontal") * movementSensitivity;
		moveVertical = Input.GetAxis("Vertical") * movementSensitivity;
		mouseX = Input.GetAxis("Mouse X");
		mouseY = -Input.GetAxis("Mouse Y");

		timeCount = timeCount + Time.deltaTime;

		// PLAYER ROTATION
		footPoint = transform.TransformPoint(new Vector3(0, footDist/transform.lossyScale.y, 0));
		if (areMagneticShoesOn & isGrounded) {
			// GROUNDED PLAYER ROTATION
			rigidBody.transform.Rotate(new Vector3(0, mouseX, 0) * groundedRotationSensitivity); // body rotates only Y
			// rotate the camera in X direction instead of the body because body is fixed to ground
			camera.transform.Rotate(new Vector3(mouseY, 0, 0) * groundedRotationSensitivity);
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
				isGrounded = elementClosestPointDirection == new Vector3(0, 0, 0); // TODO: add check for perpendicular normals, then check for a while if this made it so it never gets grownded diagonally
				Debug.Log($"{elementClosestPointDirection}, {isGrounded}, {closestElement.transform.gameObject.name}");
			}
			magneticStrength = c1 / Mathf.Pow(c2 + closestElementDistance, 2f) * isMagnetNear;

			// MAGNET LOCKED ON
			if (Convert.ToBoolean(isMagnetNear)) {	// mouse controls camera rotation
				// before magnet locks on, you're flying, which means camera is forward
				// when locked on and magnetizing down, allow camera to look down, don't allow body to rotate up/down
				camera.transform.Rotate(new Vector3(mouseY, 0, 0) * groundedRotationSensitivity);
				rigidBody.AddRelativeTorque(new Vector3(0, mouseX, 0) * flyingRotationSensitivity); // note: mouse controls seem to work best in Update
			} else { // MAGNET NOT LOCKED ON (EXACTLY THE SAME AS FLYING PLAYER ROTATION, SEE BELOW)
				if (isSpaceSuitOn) {
					rigidBody.AddRelativeTorque(new Vector3(mouseY, mouseX, 0) * flyingRotationSensitivity); // note: mouse controls seem to work best in Update
					if (camera.transform.localRotation == Quaternion.Euler(0, 0, 0)) {
						timeCount = 0f;
					}
					camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, Quaternion.Euler(0, 0, 0), timeCount * slerpSpeed);
				}
			}
		} else {
			// FLYING PLAYER ROTATION, MAGNET SHOES OFF
			if (isSpaceSuitOn) {
				// mouse controls body rotation
				rigidBody.AddRelativeTorque(new Vector3(mouseY, mouseX, 0) * flyingRotationSensitivity); // note: mouse controls seem to work best in Update
				// while flying, camera is to be pointing forward. we slerp up to it so that it doesnt move there suddenly after taking off
				if (camera.transform.localRotation == Quaternion.Euler(0, 0, 0)) {
					timeCount = 0f;
				}
				camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, Quaternion.Euler(0, 0, 0), timeCount * slerpSpeed);
			}
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
				// ang and layingDownCheck are used to check if player got stuck laying down and needs to magnetize up
				float ang = Vector3.Angle(-transform.up, elementClosestPointDirection);
				bool fixLayingDown = closestElementDistance <= .51f & closestElementDistance >= .01f & !isGrounded & ang >= 40f & ang <= 91f;
				//markerCopy.transform.position = elementClosestPoint;
				//markerCopy2.transform.position = footPoint;
				if (fixLayingDown) {
					// if foot magnet got player stuck laying down, torque to stand up
					rigidBody.AddTorque(Vector3.Cross(-transform.up, elementClosestPointDirection).normalized * 100f);
				} else {
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
		// TODO: in here i can use collision to get normals. should i do that to check if i am perpendicular. should this be what is done in stick it instead of what was done before?
	}
 
	// DETECT EXITING A COLLOSION WITH AN OBJECT
	void OnCollisionExit(Collision hit) {
		if (collisionCount > 0) { // don't decrease collisionCount into the negatives
			collisionCount -= 1;
		}
		if (collisionCount == 0) { // only become ungrounded if the surface you stopped touching was the last collision
			isGrounded = false;
		}
	}
}
