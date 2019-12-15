using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : RunnerController {

    public bool controlEnabled = true;
	public bool movementEnabled = false;
    private int clearState = 0;

    // Use this for initialization
    protected override void Start () {
        base.Start();   // perform parent startup
        //Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    protected override void Update () 
    {
        base.Update(); // perform parent Update. 
        //Application.targetFrameRate = 10;
        if (controlEnabled)
        {
            clearState = 0;
            HandleInput();
        }
        else if (clearState != 2)
        {
            ClearInput();
        }
	}

    void HandleInput()
    {
        // Movement
		if (movementEnabled)
		{
			input.moveLR_bDown = (Input.GetButtonDown("Horizontal"));
			input.moveUD_bDown = (Input.GetButtonDown("Vertical"));

			input.moveRight_bHeld = (Input.GetButton("Horizontal") && Input.GetAxis("Horizontal") > 0);
			input.moveLeft_bHeld = (Input.GetButton("Horizontal") && Input.GetAxis("Horizontal") < 0);
			input.moveUp_bHeld = (Input.GetButton("Vertical") && Input.GetAxis("Vertical") > 0);
			input.moveDown_bHeld = (Input.GetButton("Vertical") && Input.GetAxis("Vertical") < 0);

			input.moveLR_bUp = (Input.GetButtonUp("Horizontal"));
			input.moveUD_bUp = (Input.GetButtonUp("Vertical"));
		}

        // jump
        input.jump_bDown = (Input.GetButtonDown("Jump"));
        input.jump_bHeld = (Input.GetButton("Jump"));
        input.jump_bUp = (Input.GetButtonUp("Jump"));
    }

    void ClearInput()
    {
        switch (clearState)
        {
            case 0:
                // Movement
                input.moveLR_bDown = false;
                input.moveUD_bDown = false;

                input.moveRight_bHeld = false;
                input.moveLeft_bHeld = false;
                input.moveUp_bHeld = false;
                input.moveDown_bHeld = false;

                // jump
                input.jump_bDown = false;
                input.jump_bHeld = false;

                // call all button ups to halt movement correctly
                input.moveLR_bUp = true;
                input.moveUD_bUp = true;
                input.jump_bUp = true;

                clearState = 1;
                break;
            case 1:
                input.moveLR_bUp = false;
                input.moveUD_bUp = false;
                input.jump_bUp = false;

                clearState = 2;
                break;
            case 2:
                // do nothing
                break;
        }
    }
}
