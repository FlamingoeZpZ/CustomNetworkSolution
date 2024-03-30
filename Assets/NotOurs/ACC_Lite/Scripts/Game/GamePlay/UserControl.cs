using System.Collections;
using System.Collections.Generic;
using Netcode;
using UnityEngine;

/// <summary>
/// For user multiplatform control.
/// </summary>
[RequireComponent(typeof(CarController)), DefaultExecutionOrder(200)]
public class UserControl : MonoBehaviour
{

	public static CarController ControlledCar { get; private set; }

	private float _horizontal;
	private float _vertical;
	private bool _brake;

	public static MobileControlUI CurrentUIControl { get; private set; }

	public static UserControl Instance { get; private set; }
	private NetworkObject no;
	private void Start ()
	{
		
		no = GetComponent<NetworkObject>();
		if (!no.IsOwner)
		{
			enabled = false;
			return;
		}
		if (Instance && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		ControlledCar = GetComponent<CarController> ();
		gameObject.AddComponent<AudioListener>();
		CurrentUIControl = FindObjectOfType<MobileControlUI> ();
	}

	void Update ()
	{
		if (GameBeginner.RaceIsEnded)
		{
			enabled = false;
			return;
		}
		#if !UNITY_STANDALONE
		//Mobile control.
		_horizontal = CurrentUIControl.GetHorizontalAxis;
		_vertical = CurrentUIControl.GetVerticalAxis;
		
#else 
		//Standard input control (Keyboard or gamepad).
		_horizontal = Input.GetAxis ("Horizontal");
		_vertical = Input.GetAxis ("Vertical");
		_brake = Input.GetButton("Jump");
		if (Input.GetButton("Reset"))
		{
			CheckPointManager.GetCurrentCheckPoint(out var x, out var y);
			ControlledCar.transform.SetPositionAndRotation(x, y);
			ControlledCar.RB.velocity = Vector3.zero;
			ControlledCar.IsRunning = false;
			
			return;
		}
		ControlledCar.IsRunning = true;
#endif

		//Apply control for controlled car.
		ControlledCar.UpdateControls (_horizontal, _vertical, _brake);
	}
}
