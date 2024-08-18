using FluffyUnderware.Curvy;
using UnityEngine;

public class PoseControl : MonoBehaviour
{
	public AnimationClip wakeup;

	public Animator anim;

	public Animator potAnim;

	public Transform leftHand;

	public Transform rightHand;

	public Transform leftElbow;

	public Transform rightElbow;

	public Transform center;

	public Transform handle;

	public Transform gripCenterLeft;

	public Transform gripCenterRight;

	public Transform rightHandGripCenter;

	public Transform leftHandGripCenter;

	public SliderJoint2D slider;

	public HingeJoint2D hinge;

	public Vector3 off;

	public Transform lookTarget;

	private Transform lookOverride;

	private float timeSinceOverride;

	private Vector3 lookVel;

	private Transform[] interestingItems;

	public Transform interestingItemParent;

	private bool animating;

	public float blendAmt;

	public float handBlend;

	public PlayerControl pc;

	public CurvySpline spline;

	public Transform dudeMeshHub;

	public Transform hammerMeshHub;

	public Transform potMeshHub;

	private float dudePotOffset = 0.02f;

	public Transform tip;

	private Vector3 leftHandReset;

	private Vector3 rightHandReset;

	private Vector3 leftGripCenterReset;

	private Vector3 rightGripCenterReset;

	private Vector3[] controlPoints;

	private void Start()
	{
		lookOverride = null;
		timeSinceOverride = 0f;
		lookVel = Vector3.zero;
		interestingItems = interestingItemParent.GetComponentsInChildren<Transform>();
		handBlend = 1f;
		leftHandReset = new Vector3(0f, 0.522f, 0f);
		rightHandReset = new Vector3(0f, 0.522f, 0f);
		leftGripCenterReset = new Vector3(0f, 0.182f, 0.09f);
		rightGripCenterReset = new Vector3(0f, 0.171f, 0.063f);
		controlPoints = new Vector3[spline.ControlPointCount];
		for (int i = 0; i < spline.ControlPointCount; i++)
		{
			controlPoints[i] = spline.ControlPoints[i].position;
		}
		LateUpdate();
		LateUpdate();
	}

	public void StopAnimator()
	{
		anim.gameObject.SetActive(false);
	}

	public void StartAnimator()
	{
		anim.gameObject.SetActive(true);
	}

	public void PlayOpeningAnimation()
	{
		animating = true;
		blendAmt = 1f;
		handBlend = 0f;
		int layerIndex = anim.GetLayerIndex("Animation");
		for (int i = 0; i < anim.layerCount; i++)
		{
			if (i == layerIndex)
			{
				anim.SetLayerWeight(i, 1f);
			}
			else
			{
				anim.SetLayerWeight(i, 0f);
			}
		}
		anim.SetTrigger("WakeUp");
		potAnim.Play("Rattle");
		pc.PauseInput(6f);
		LateUpdate();
		LateUpdate();
	}

	private float GetNearestSplineZ(Vector3 pos)
	{
		int num = 0;
		int num2 = 0;
		float num3 = 1E+09f;
		float num4 = 9999999f;
		for (int i = 0; i < controlPoints.Length; i++)
		{
			float sqrMagnitude = ((Vector2)controlPoints[i] - (Vector2)pos).sqrMagnitude;
			if (sqrMagnitude < num4)
			{
				num2 = num;
				num3 = num4;
				num4 = sqrMagnitude;
				num = i;
			}
			else if (sqrMagnitude < num3)
			{
				num2 = i;
				num3 = sqrMagnitude;
			}
		}
		Vector2 rhs = (Vector2)controlPoints[num2] - (Vector2)controlPoints[num];
		Vector2 lhs = (Vector2)controlPoints[num] - (Vector2)pos;
		float t = -1f * (Vector2.Dot(lhs, rhs) / rhs.sqrMagnitude);
		return Vector3.Lerp(controlPoints[num], controlPoints[num2], t).z - 0.5f;
	}

	private void LateUpdate()
	{
		float num = 100f;
		float num2 = 2f;
		float num3 = 1f;
		float num4 = 5f;
		if (lookOverride == null)
		{
			timeSinceOverride += Time.deltaTime;
			lookTarget.position = Vector3.SmoothDamp(lookTarget.position, tip.position, ref lookVel, 0.2f);
			if (Random.value < Time.deltaTime / num2 && timeSinceOverride > num4)
			{
				Transform[] array = interestingItems;
				foreach (Transform transform in array)
				{
					if ((dudeMeshHub.position + new Vector3(2f, 2f, 0f) - transform.position).sqrMagnitude < num)
					{
						lookOverride = transform;
						timeSinceOverride = 0f;
						break;
					}
				}
			}
		}
		else
		{
			timeSinceOverride += Time.deltaTime;
			lookTarget.position = Vector3.SmoothDamp(lookTarget.position, lookOverride.position, ref lookVel, 0.2f);
			if ((Random.value < Time.deltaTime / num2 && timeSinceOverride > num3) || (dudeMeshHub.position + new Vector3(2f, 2f, 0f) - lookOverride.position).sqrMagnitude > num * 1.5f)
			{
				lookOverride = null;
				timeSinceOverride = 0f;
			}
		}
		float jointAngle = hinge.jointAngle;
		float nearestSplineZ = GetNearestSplineZ(center.position);
		dudeMeshHub.position = new Vector3(dudeMeshHub.position.x, dudeMeshHub.position.y, nearestSplineZ);
		potMeshHub.position = new Vector3(potMeshHub.position.x, potMeshHub.position.y, nearestSplineZ - dudePotOffset);
		float num5 = ((!(slider.jointTranslation >= 0f)) ? (slider.jointTranslation / slider.limits.min) : ((0f - slider.jointTranslation) / slider.limits.max));
		int layerIndex = anim.GetLayerIndex("Animation");
		if (animating)
		{
			if (blendAmt <= 0f)
			{
				animating = false;
				anim.StopPlayback();
			}
			else
			{
				for (int j = 0; j < anim.layerCount; j++)
				{
					if (j == layerIndex)
					{
						anim.SetLayerWeight(j, blendAmt);
					}
					else
					{
						anim.SetLayerWeight(j, 1f - blendAmt);
					}
				}
			}
		}
		leftHand.localPosition = leftHandReset;
		rightHand.localPosition = rightHandReset;
		leftHandGripCenter.localPosition = leftGripCenterReset;
		rightHandGripCenter.localPosition = rightGripCenterReset;
		anim.SetFloat("Angle", Mathf.Repeat(0f - jointAngle + 90f - hinge.referenceAngle - hammerMeshHub.localEulerAngles.z, 360f) / 180f - 1f);
		anim.SetFloat("Extension", (0f - num5) * 0.25f);
		anim.Update(Time.deltaTime);
		hammerMeshHub.position = new Vector3(hammerMeshHub.position.x, hammerMeshHub.position.y, rightHandGripCenter.position.z);
		Quaternion rotation = leftHand.rotation;
		Quaternion rotation2 = rightHand.rotation;
		Quaternion rotation3 = leftElbow.rotation;
		Quaternion rotation4 = rightElbow.rotation;
		Vector3 position = leftHand.position;
		Vector3 position2 = rightHand.position;
		for (int k = 0; k < 3; k++)
		{
			Vector3 vector = leftHandGripCenter.position - gripCenterLeft.position;
			leftHand.position -= vector;
			Vector3 vector2 = rightHandGripCenter.position - gripCenterRight.position;
			rightHand.position -= vector2;
			leftElbow.rotation *= Quaternion.FromToRotation(Vector3.up, leftElbow.InverseTransformPoint(leftHand.position));
			rightElbow.rotation *= Quaternion.FromToRotation(Vector3.up, rightElbow.InverseTransformPoint(rightHand.position));
			leftHand.rotation = rotation;
			rightHand.rotation = rotation2;
		}
		leftElbow.rotation = Quaternion.Slerp(rotation3, leftElbow.rotation, handBlend);
		rightElbow.rotation = Quaternion.Slerp(rotation4, rightElbow.rotation, handBlend);
		leftHand.position = Vector3.Lerp(position, leftHand.position, handBlend);
		rightHand.position = Vector3.Lerp(position2, rightHand.position, handBlend);
		leftHand.rotation = Quaternion.Slerp(rotation, leftHand.rotation, handBlend);
		rightHand.rotation = Quaternion.Slerp(rotation2, rightHand.rotation, handBlend);
	}

	public void EnableHammer(bool enable)
	{
		tip.GetComponent<Rigidbody2D>().simulated = enable;
	}
}
