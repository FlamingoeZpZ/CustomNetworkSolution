using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PG_Physics.Wheel;
using UnityEngine.Serialization;

/// <summary>
/// Wheel settings and update logic.
/// </summary>
[System.Serializable]
public struct Wheel
{
	[FormerlySerializedAs("WheelCollider")] public WheelCollider wheelCollider;
	public Transform WheelView;
	public float SlipForGenerateParticle;
	public Vector3 TrailOffset;

	public float CurrentMaxSlip { get { return Mathf.Max (CurrentForwardSleep, CurrentSidewaysSleep); } }
	public float CurrentForwardSleep { get; private set; }
	public float CurrentSidewaysSleep { get; private set; }
	public WheelHit GetHit { get { return hit; } }

	WheelHit hit;
	TrailRenderer Trail;

	PG_WheelCollider m_PGWC;
	public PG_WheelCollider PG_WheelCollider
	{
		get
		{
			if (m_PGWC == null)
			{
				m_PGWC = wheelCollider.GetComponent<PG_WheelCollider> ();
			}
			if (m_PGWC == null)
			{
				m_PGWC = wheelCollider.gameObject.AddComponent<PG_WheelCollider> ();
				m_PGWC.CheckFirstEnable ();
			}
			return m_PGWC;
		}
	}

	FXController FXController { get { return FXController.Instance; } }
	Vector3 HitPoint;

	const int SmoothValuesCount = 3;

	
	
	/// <summary>
	/// Update gameplay logic.
	/// </summary>
	
	public void FixedUpdate ()
	{

		if (wheelCollider.GetGroundHit (out hit))
		{
			//var prevForwar = CurrentForwardSleep;
			//var prevSide = CurrentSidewaysSleep;
/*
			var transform = wheelCollider.transform;
			Vector3 currentPosition = transform.position;
			Quaternion currentRotation = transform.rotation;

			// Calculate linear velocity of the wheel's center
			Vector3 linearVelocity = (currentPosition - lastPosition) / Time.deltaTime;

			// Calculate angular velocity of the wheel
			Quaternion deltaRotation = currentRotation * Quaternion.Inverse(lastRotation);
			deltaRotation.ToAngleAxis(out var angleInDegrees, out _);
			float angularVelocity = angleInDegrees * Mathf.Deg2Rad / Time.deltaTime;

			// Estimate velocity at the contact point
			Vector3 velocityAtContactPoint = linearVelocity + Vector3.Cross(wheelCollider.transform.up, -wheelCollider.radius * angularVelocity * wheelCollider.transform.right);

			// Calculate forward slip
			float forwardSlip = Mathf.Atan2(-velocityAtContactPoint.x, Mathf.Abs(velocityAtContactPoint.z)) * Mathf.Rad2Deg;

			// Calculate sideways slip
			float sidewaysSlip = Mathf.Atan2(-velocityAtContactPoint.y, Mathf.Abs(velocityAtContactPoint.z)) * Mathf.Rad2Deg;

			// Use the slip values as needed
			Debug.Log($"Forward Slip: {forwardSlip}, Sideways Slip: {sidewaysSlip}");

			// Update last position and rotation
			lastPosition = currentPosition;
			lastRotation = currentRotation;
*/
			CurrentForwardSleep = Mathf.Abs(hit.forwardSlip);// forwardSlip;//(prevForwar + Mathf.Abs (forwardSlip)) / 2;
			CurrentSidewaysSleep = Mathf.Abs(hit.sidewaysSlip);// sidewaysSlip;//(prevSide + Mathf.Abs (sidewaysSlip)) / 2;
			//Debug.Log($"Forward Slip: {CurrentForwardSleep}, Sideways Slip: {CurrentSidewaysSleep}");
		}
		else
		{
			CurrentForwardSleep = 0;
			CurrentSidewaysSleep = 0;
		}
	}

	/// <summary>
	/// Update visual logic (Transform, FX).
	/// </summary>
	/// 
	public void UpdateVisual ()
	{

		//if (WheelCollider.isGrounded && CurrentMaxSlip > SlipForGenerateParticle)
		Debug.DrawRay(wheelCollider.transform.position, Vector3.down, (wheelCollider.isGrounded? (CurrentMaxSlip > SlipForGenerateParticle)?Color.green: Color.yellow : Color.red),1);
		if (wheelCollider.isGrounded && CurrentMaxSlip > SlipForGenerateParticle)
		{
			//Emit particle.
			var particles = FXController.GetAspahaltParticles;
			var point = wheelCollider.transform.position;
			point.y = hit.point.y;
			particles.transform.position = point;
			particles.Emit (1);

			if (!Trail)
			{
				//Get free or create trail.
				HitPoint = wheelCollider.transform.position;
				HitPoint.y = hit.point.y;
				Trail = FXController.GetTrail (HitPoint);
				Transform transform;
				(transform = Trail.transform).SetParent (wheelCollider.transform);
				transform.localPosition += TrailOffset;
			}
		}
		else if (Trail)
		{
			//Set trail as free.
			FXController.SetFreeTrail (Trail);
			Trail = null;
		}
	}

	public void UpdateTransform ()
	{
		wheelCollider.GetWorldPose (out var pos, out var quat);
		WheelView.SetPositionAndRotation(pos,quat); 
	}

	public void UpdateFrictionConfig (PG_WheelColliderConfig config)
	{
		PG_WheelCollider.UpdateConfig (config);
	}
}
