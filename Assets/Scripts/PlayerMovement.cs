using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public CharacterController2D controller;
	public Animator animator;

	public float runSpeed = 40f;

	float horizontalMove = 0f;
	bool jump = false;
	bool crouch = false;
	bool dash = false;

	// Update is called once per frame
	void Update () {

		horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

		animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

		if (Input.GetButtonDown("Jump"))
		{
			jump = true;
			animator.SetBool("IsJumping", true);
		}
		if (Input.GetButtonDown("Crouch"))
		{
			crouch = true;
		} 
		else if (Input.GetButtonUp("Crouch"))
		{
			crouch = false;
		}
		if (Input.GetButtonDown("Dash"))
		{
			dash = true;
		}
	}

	public void OnLanding ()
	{
		animator.SetBool("IsJumping", false);
		animator.SetBool("isDoubleJump", false);
	}

	public void OnCrouching (bool isCrouching)
	{
		animator.SetBool("IsCrouching", isCrouching);
	}

	public void OnSliding ()
	{
		//
	}

	void FixedUpdate ()
	{
		// called every fixed framerate
		controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, dash);
		jump = false;
		dash = false;
	}
}
