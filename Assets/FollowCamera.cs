using System.Collections.Generic;
using UnityEngine;

// Source: https://code.tutsplus.com/tutorials/unity3d-third-person-cameras--mobile-11230
public class FollowCamera : MonoBehaviour
{
	public GameObject target;
	public float damping = 4;

	private Vector3 offset;

	private void Start()
	{
		offset = target.transform.position - transform.position;
	}

	private void LateUpdate()
	{
		float currentAngle = transform.eulerAngles.y;
		float desiredAngle = target.transform.eulerAngles.y;
		float angle = Mathf.LerpAngle(currentAngle, desiredAngle, Time.deltaTime * damping);

		Quaternion rotation = Quaternion.Euler(0, angle, 0);
		transform.position = target.transform.position - (rotation * offset);

		transform.LookAt(target.transform.position + Vector3.up * 0.8f);
	}
}
