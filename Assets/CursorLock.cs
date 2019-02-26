using System.Collections.Generic;
using UnityEngine;

public class CursorLock : MonoBehaviour
{
	private CursorLockMode wantedMode;

	private void Update()
	{
		// Lock cursor on left click
		if (Input.GetMouseButtonDown(0))
		{
			wantedMode = CursorLockMode.Locked;
		}

		// Release cursor on escape keypress
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			wantedMode = CursorLockMode.None;
		}

		SetCursorState();
	}

	// Apply requested cursor state
	private void SetCursorState()
	{
		Cursor.lockState = wantedMode;
		// Hide cursor when locking
		Cursor.visible = (CursorLockMode.Locked != wantedMode);
	}
}
