using System.Collections;
using System.Collections.Generic;
using Netcode;
using UnityEngine;

/// <summary>
/// For user multiplatform control.
/// </summary>
[RequireComponent (typeof (CarController))]
public class UserControl :MonoBehaviour
{

	public static CarController ControlledCar { get; private set; }

	private float _horizontal;
	private float _vertical;
	private bool _brake;

	public static MobileControlUI CurrentUIControl { get; set; }

	private void Start ()
	{
		NetworkObject no = GetComponent<NetworkObject>();
		if (!no.IsOwner)
		{
			enabled = false;
			return;
		}
		ControlledCar = GetComponent<CarController> ();
		gameObject.AddComponent<AudioListener>();
		CurrentUIControl = FindObjectOfType<MobileControlUI> ();
	}

	void Update ()
	{
		#if !UNITY_STANDALONE
		//Mobile control.
		_horizontal = CurrentUIControl.GetHorizontalAxis;
		_vertical = CurrentUIControl.GetVerticalAxis;
		
#else 
		//Standard input control (Keyboard or gamepad).
		_horizontal = Input.GetAxis ("Horizontal");
		_vertical = Input.GetAxis ("Vertical");
		_brake = Input.GetButton ("Jump");
		
#endif

		//Apply control for controlled car.
		ControlledCar.UpdateControls (_horizontal, _vertical, _brake);
	}
}
