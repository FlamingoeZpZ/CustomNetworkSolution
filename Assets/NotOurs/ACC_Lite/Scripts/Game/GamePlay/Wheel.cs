using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PG_Physics.Wheel;

/// <summary>
/// Wheel settings and update logic.
/// </summary>
[System.Serializable]
public struct Wheel
{
	public WheelCollider WheelCollider;
	public Transform WheelView;
	public float SlipForGenerateParticle;
	public Vector3 TrailOffset;

	public float CurrentMaxSlip { get { return Mathf.Max (CurrentForwardSleep, CurrentSidewaysSleep); } }
	public float CurrentForwardSleep { get; private set; }
	public float CurrentSidewaysSleep { get; private set; }
	public WheelHit GetHit { get { return Hit; } }

	WheelHit Hit;
	TrailRenderer Trail;

	PG_WheelCollider m_PGWC;
	public PG_WheelCollider PG_WheelCollider
	{
		get
		{
			if (m_PGWC == null)
			{
				m_PGWC = WheelCollider.GetComponent<PG_WheelCollider> ();
			}
			if (m_PGWC == null)
			{
				m_PGWC = WheelCollider.gameObject.AddComponent<PG_WheelCollider> ();
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

		if (WheelCollider.GetGroundHit (out Hit))
		{
			var prevForwar = CurrentForwardSleep;
			var prevSide = CurrentSidewaysSleep;

			CurrentForwardSleep = (prevForwar + Mathf.Abs (Hit.forwardSlip)) / 2;
			CurrentSidewaysSleep = (prevSide + Mathf.Abs (Hit.sidewaysSlip)) / 2;
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
		
		UpdateTransform ();

		//if (WheelCollider.isGrounded && CurrentMaxSlip > SlipForGenerateParticle)
		Debug.DrawRay(WheelCollider.transform.position, Vector3.down, (WheelCollider.isGrounded? (CurrentMaxSlip > SlipForGenerateParticle)?Color.green: Color.yellow : Color.red),1);
		if (WheelCollider.isGrounded && CurrentMaxSlip > SlipForGenerateParticle)
		{
			//Emit particle.
			var particles = FXController.GetAspahaltParticles;
			var point = WheelCollider.transform.position;
			point.y = Hit.point.y;
			particles.transform.position = point;
			particles.Emit (1);

			if (!Trail)
			{
				//Get free or create trail.
				HitPoint = WheelCollider.transform.position;
				HitPoint.y = Hit.point.y;
				Trail = FXController.GetTrail (HitPoint);
				Transform transform;
				(transform = Trail.transform).SetParent (WheelCollider.transform);
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
		WheelCollider.GetWorldPose (out var pos, out var quat);
		WheelView.SetPositionAndRotation(pos,quat); 
	}

	public void UpdateFrictionConfig (PG_WheelColliderConfig config)
	{
		PG_WheelCollider.UpdateConfig (config);
	}
}
