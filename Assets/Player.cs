using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class Player : MonoBehaviour
{
	private Rigidbody myRigidbody;
	private Vector3 input = Vector3.zero;
	private bool grounded = false;
	private Transform groundChecker;

	public float speed = 5f;
	public float runSpeedMultiplier = 1.4f;
	public float jumpHeight = 2f;
	public float groundDistance = 0.3f;
	public LayerMask ground;

	private void Awake()
	{
		myRigidbody = GetComponent<Rigidbody>();
		groundChecker = transform.GetChild(0);
	}

	private void Start()
	{
		Invoke(nameof(EnableGravity), 2f);
	}

	private void EnableGravity()
	{
		/*
		 * FIXME: This is currently a workaround as the player falls through the world
		 * when gravity is activated before the chunk is created under the player
		 */
		myRigidbody.useGravity = true;
		Debug.Log($"Enabled gravity for Player");
	}

	private void Update()
	{
		// Jump
		grounded = Physics.CheckSphere(groundChecker.position, groundDistance, ground, QueryTriggerInteraction.Ignore);
		if (Input.GetButtonDown("Jump") && grounded)
		{
			myRigidbody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
		}

		// Rotate
		float rotate = Input.GetAxis("Mouse X") * 2f;
		myRigidbody.transform.Rotate(Vector3.up * rotate);

		// Move
		input = Vector3.zero;
		float strafe = Input.GetAxis("Horizontal");
		float forward = Input.GetAxis("Vertical");
		if (forward != 0f || strafe != 0f)
		{
			input.Set(strafe, 0f, forward);

			float alpha = Mathf.Atan2(input.z, input.x);
			float beta = Mathf.Atan2(transform.forward.z, transform.forward.x);
			float gamma = alpha + beta;

			input = new Vector3(Mathf.Sin(gamma), 0f, -Mathf.Cos(gamma));
		}
	}

	private void FixedUpdate()
	{
		float movementSpeed = speed;
		if (Input.GetButton("Run"))
		{
			movementSpeed *= runSpeedMultiplier;
		}
		myRigidbody.MovePosition(myRigidbody.position + input * movementSpeed * Time.fixedDeltaTime);
	}
}
