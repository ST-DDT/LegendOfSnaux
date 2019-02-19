using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class Player : MonoBehaviour
{
	private Rigidbody myRigidbody;

	public float movementSpeed = 6f;
	public float turningSpeed = 140f;

	public bool grounded = false;
	private bool performJump = false;
	//public float fallMultiplier = 2.5f;
	//public float lowJumpMultiplier = 2f;

	private void Awake()
	{
		myRigidbody = GetComponent<Rigidbody>();
		myRigidbody.freezeRotation = true;
		myRigidbody.useGravity = false;
	}

	private void OnCollisionStay(Collision collision)
	{
		grounded = true;
	}

	private void Update()
	{
		/*
		 * FIXME: This is currently a workaround as the player falls through the world
		 * when gravity is activated before the chunk is created under the player
		 */
		if (Input.GetKeyDown(KeyCode.G))
		{
			myRigidbody.useGravity = !myRigidbody.useGravity;
			Debug.Log($"Set rigidbody.useGravity to {myRigidbody.useGravity}");
			myRigidbody.velocity.Set(0, 0, 0);
		}

		//// Source: https://www.youtube.com/watch?v=7KiK0Aqtmzc
		//// Unfortunately that does not work well :(
		//if (myRigidbody.velocity.y < 0)
		//{
		//	myRigidbody.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
		//}
		//else if (myRigidbody.velocity.y > 0 && !Input.GetButton("Jump"))
		//{
		//	myRigidbody.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1f) * Time.deltaTime;
		//}

		if (Input.GetButtonDown("Jump") && grounded && !performJump)
		{
			performJump = true;
			grounded = false;
		}

		float horizontal = Input.GetAxis("Horizontal") * turningSpeed * Time.deltaTime;
		transform.Rotate(Vector3.up, horizontal);

		float vertical = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
		transform.Translate(0, 0, vertical);
	}

	private void FixedUpdate()
	{
		if (performJump)
		{
			myRigidbody.AddForce(new Vector3(0, 6, 0), ForceMode.Impulse);
			performJump = false;
		}
	}
}
