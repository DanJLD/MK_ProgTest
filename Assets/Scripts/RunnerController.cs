using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerController : MonoBehaviour
{
	[System.Serializable]
	public class InputState  // subclasses used to categorise and order variables in unity inspector
	{
		public bool moveLeft_bHeld = false;
		public bool moveRight_bHeld = false;
		public bool moveUp_bHeld = false;
		public bool moveDown_bHeld = false;

		public bool moveLR_bUp = false;
		public bool moveLR_bDown = false;
		public bool moveUD_bUp = false;
		public bool moveUD_bDown = false;

		public bool jump_bUp = false;
		public bool jump_bHeld = false;
		public bool jump_bDown = false;

		public bool restart_bDown = false;
	}
	public InputState input;

	protected virtual void Start() // virtual allows it to be overridden by child AI classes
	{

	}

	protected virtual void Update()
	{
		if (input.moveRight_bHeld)
		{
			transform.Translate(new Vector3(0.1f, 0f, 0f));
		}

		if (input.moveLeft_bHeld)
		{
			transform.Translate(new Vector3(-0.1f, 0f, 0f));
		}

		if (input.jump_bDown)
		{
			Rigidbody2D rb = GetComponent<Rigidbody2D>();
			rb.AddForce(new Vector2(0f, 300f));
		}
	}
}
