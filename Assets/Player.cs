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
