using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverVR : IKSolver
	{
		[Serializable]
		public class Arm : BodyPart
		{
			[Serializable]
			public enum ShoulderRotationMode
			{
				YawPitch = 0,
				FromTo = 1
			}

			[Tooltip("The hand target")]
			public Transform target;

			[Tooltip("The elbow will be bent towards this Transform if 'Bend Goal Weight' > 0.")]
			public Transform bendGoal;

			[Tooltip("Positional weight of the hand target.")]
			[Range(0f, 1f)]
			public float positionWeight = 1f;

			[Tooltip("Rotational weight of the hand target")]
			[Range(0f, 1f)]
			public float rotationWeight = 1f;

			[Tooltip("Different techniques for shoulder bone rotation.")]
			public ShoulderRotationMode shoulderRotationMode;

			[Tooltip("The weight of shoulder rotation")]
			[Range(0f, 1f)]
			public float shoulderRotationWeight = 1f;

			[Tooltip("If greater than 0, will bend the elbow towards the 'Bend Goal' Transform.")]
			[Range(0f, 1f)]
			public float bendGoalWeight;

			[Tooltip("Angular offset of the elbow bending direction.")]
			[Range(-180f, 180f)]
			public float swivelOffset;

			[Tooltip("Local axis of the hand bone that points from the wrist towards the palm. Used for defining hand bone orientation.")]
			public Vector3 wristToPalmAxis = Vector3.zero;

			[Tooltip("Local axis of the hand bone that points from the palm towards the thumb. Used for defining hand bone orientation.")]
			public Vector3 palmToThumbAxis = Vector3.zero;

			[HideInInspector]
			public Vector3 IKPosition;

			[HideInInspector]
			public Quaternion IKRotation = Quaternion.identity;

			[HideInInspector]
			public Vector3 bendDirection = Vector3.back;

			[HideInInspector]
			public Vector3 handPositionOffset;

			private bool hasShoulder;

			private Vector3 chestForwardAxis;

			private Vector3 chestUpAxis;

			private Quaternion chestRotation;

			private Vector3 chestForward;

			private Vector3 chestUp;

			private Quaternion forearmRelToUpperArm;

			private const float yawOffsetAngle = 45f;

			private const float pitchOffsetAngle = -30f;

			public Vector3 position { get; private set; }

			public Quaternion rotation { get; private set; }

			private VirtualBone shoulder
			{
				get
				{
					return bones[0];
				}
			}

			private VirtualBone upperArm
			{
				get
				{
					return bones[1];
				}
			}

			private VirtualBone forearm
			{
				get
				{
					return bones[2];
				}
			}

			private VirtualBone hand
			{
				get
				{
					return bones[3];
				}
			}

			protected override void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasNeck, bool hasShoulders, bool hasToes, int rootIndex, int index)
			{
				Vector3 vector = positions[index];
				Quaternion quaternion = rotations[index];
				Vector3 vector2 = positions[index + 1];
				Quaternion quaternion2 = rotations[index + 1];
				Vector3 vector3 = positions[index + 2];
				Quaternion quaternion3 = rotations[index + 2];
				Vector3 iKPosition = positions[index + 3];
				Quaternion iKRotation = rotations[index + 3];
				if (!initiated)
				{
					IKPosition = iKPosition;
					IKRotation = iKRotation;
					hasShoulder = hasShoulders;
					bones = new VirtualBone[(!hasShoulder) ? 3 : 4];
					if (hasShoulder)
					{
						bones[0] = new VirtualBone(vector, quaternion);
						bones[1] = new VirtualBone(vector2, quaternion2);
						bones[2] = new VirtualBone(vector3, quaternion3);
						bones[3] = new VirtualBone(iKPosition, iKRotation);
					}
					else
					{
						bones[0] = new VirtualBone(vector2, quaternion2);
						bones[1] = new VirtualBone(vector3, quaternion3);
						bones[2] = new VirtualBone(iKPosition, iKRotation);
					}
					chestForwardAxis = Quaternion.Inverse(rootRotation) * (rotations[0] * Vector3.forward);
					chestUpAxis = Quaternion.Inverse(rootRotation) * (rotations[0] * Vector3.up);
				}
				if (hasShoulder)
				{
					bones[0].Read(vector, quaternion);
					bones[1].Read(vector2, quaternion2);
					bones[2].Read(vector3, quaternion3);
					bones[3].Read(iKPosition, iKRotation);
				}
				else
				{
					bones[0].Read(vector2, quaternion2);
					bones[1].Read(vector3, quaternion3);
					bones[2].Read(iKPosition, iKRotation);
				}
			}

			public override void PreSolve()
			{
				if (target != null)
				{
					IKPosition = target.position;
					IKRotation = target.rotation;
				}
				position = V3Tools.Lerp(hand.solverPosition, IKPosition, positionWeight);
				rotation = QuaTools.Lerp(hand.solverRotation, IKRotation, rotationWeight);
				shoulder.axis = shoulder.axis.normalized;
				forearmRelToUpperArm = Quaternion.Inverse(upperArm.solverRotation) * forearm.solverRotation;
			}

			public override void ApplyOffsets()
			{
				position += handPositionOffset;
			}

			public void Solve(bool isLeft)
			{
				chestRotation = Quaternion.LookRotation(rootRotation * chestForwardAxis, rootRotation * chestUpAxis);
				chestForward = chestRotation * Vector3.forward;
				chestUp = chestRotation * Vector3.up;
				if (hasShoulder && shoulderRotationWeight > 0f)
				{
					switch (shoulderRotationMode)
					{
					case ShoulderRotationMode.YawPitch:
					{
						Vector3 normalized = (position - shoulder.solverPosition).normalized;
						float num3 = ((!isLeft) ? (-45f) : 45f);
						Quaternion quaternion2 = Quaternion.AngleAxis(((!isLeft) ? 90f : (-90f)) + num3, chestUp);
						Quaternion quaternion3 = quaternion2 * chestRotation;
						Vector3 lhs = Quaternion.Inverse(quaternion3) * normalized;
						float num4 = Mathf.Atan2(lhs.x, lhs.z) * 57.29578f;
						float f = Vector3.Dot(lhs, Vector3.up);
						f = 1f - Mathf.Abs(f);
						num4 *= f;
						num4 -= num3;
						num4 = DamperValue(num4, -45f - num3, 45f - num3, 0.7f);
						Quaternion quaternion4 = Quaternion.AngleAxis(num4, Vector3.up);
						Vector3 fromDirection = Quaternion.Inverse(quaternion3) * (shoulder.solverRotation * shoulder.axis);
						Vector3 toDirection = quaternion4 * Vector3.forward;
						Quaternion quaternion5 = Quaternion.FromToRotation(fromDirection, toDirection);
						Quaternion quaternion6 = Quaternion.AngleAxis((!isLeft) ? 90f : (-90f), chestUp);
						quaternion3 = quaternion6 * chestRotation;
						quaternion3 = Quaternion.AngleAxis((!isLeft) ? 30f : (-30f), chestForward) * quaternion3;
						normalized = position - (shoulder.solverPosition + chestRotation * ((!isLeft) ? Vector3.left : Vector3.right) * base.mag);
						lhs = Quaternion.Inverse(quaternion3) * normalized;
						float num5 = Mathf.Atan2(lhs.y, lhs.z) * 57.29578f;
						num5 -= -30f;
						num5 = DamperValue(num5, -15f, 75f);
						Quaternion quaternion7 = Quaternion.AngleAxis(0f - num5, quaternion3 * Vector3.right);
						Quaternion b2 = quaternion7 * quaternion5;
						if (shoulderRotationWeight < 1f)
						{
							b2 = Quaternion.Lerp(Quaternion.identity, b2, shoulderRotationWeight);
						}
						VirtualBone.RotateBy(bones, b2);
						VirtualBone.SolveTrigonometric(bones, 1, 2, 3, position, GetBendNormal(position - upperArm.solverPosition), 1f);
						float angle = Mathf.Clamp(num5 * 2f, 0f, 180f);
						shoulder.solverRotation = Quaternion.AngleAxis(angle, shoulder.solverRotation * ((!isLeft) ? (-shoulder.axis) : shoulder.axis)) * shoulder.solverRotation;
						upperArm.solverRotation = Quaternion.AngleAxis(angle, upperArm.solverRotation * ((!isLeft) ? (-upperArm.axis) : upperArm.axis)) * upperArm.solverRotation;
						break;
					}
					case ShoulderRotationMode.FromTo:
					{
						Quaternion solverRotation = shoulder.solverRotation;
						Quaternion b = Quaternion.FromToRotation((upperArm.solverPosition - shoulder.solverPosition).normalized + chestForward, position - shoulder.solverPosition);
						b = Quaternion.Slerp(Quaternion.identity, b, 0.5f * shoulderRotationWeight);
						VirtualBone.RotateBy(bones, b);
						VirtualBone.SolveTrigonometric(bones, 0, 2, 3, position, Vector3.Cross(forearm.solverPosition - shoulder.solverPosition, hand.solverPosition - shoulder.solverPosition), 0.5f * shoulderRotationWeight);
						VirtualBone.SolveTrigonometric(bones, 1, 2, 3, position, GetBendNormal(position - upperArm.solverPosition), 1f);
						Quaternion quaternion = Quaternion.Inverse(Quaternion.LookRotation(chestUp, chestForward));
						Vector3 vector = quaternion * (solverRotation * shoulder.axis);
						Vector3 vector2 = quaternion * (shoulder.solverRotation * shoulder.axis);
						float current = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
						float num = Mathf.Atan2(vector2.x, vector2.z) * 57.29578f;
						float num2 = Mathf.DeltaAngle(current, num);
						if (isLeft)
						{
							num2 = 0f - num2;
						}
						num2 = Mathf.Clamp(num2 * 2f, 0f, 180f);
						shoulder.solverRotation = Quaternion.AngleAxis(num2, shoulder.solverRotation * ((!isLeft) ? (-shoulder.axis) : shoulder.axis)) * shoulder.solverRotation;
						upperArm.solverRotation = Quaternion.AngleAxis(num2, upperArm.solverRotation * ((!isLeft) ? (-upperArm.axis) : upperArm.axis)) * upperArm.solverRotation;
						break;
					}
					}
				}
				else
				{
					VirtualBone.SolveTrigonometric(bones, 1, 2, 3, position, GetBendNormal(position - upperArm.solverPosition), 1f);
				}
				Quaternion quaternion8 = upperArm.solverRotation * forearmRelToUpperArm;
				Quaternion quaternion9 = Quaternion.FromToRotation(quaternion8 * forearm.axis, hand.solverPosition - forearm.solverPosition);
				RotateTo(forearm, quaternion9 * quaternion8);
				hand.solverRotation = rotation;
			}

			public override void ResetOffsets()
			{
				handPositionOffset = Vector3.zero;
			}

			public override void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations)
			{
				if (hasShoulder)
				{
					solvedRotations[index] = shoulder.solverRotation;
				}
				solvedRotations[index + 1] = upperArm.solverRotation;
				solvedRotations[index + 2] = forearm.solverRotation;
				solvedRotations[index + 3] = hand.solverRotation;
			}

			private float DamperValue(float value, float min, float max, float weight = 1f)
			{
				float num = max - min;
				if (weight < 1f)
				{
					float num2 = max - num * 0.5f;
					float num3 = value - num2;
					num3 *= 0.5f;
					value = num2 + num3;
				}
				value -= min;
				float t = Mathf.Clamp(value / num, 0f, 1f);
				float t2 = Interp.Float(t, InterpolationMode.InOutQuintic);
				return Mathf.Lerp(min, max, t2);
			}

			private Vector3 GetBendNormal(Vector3 dir)
			{
				if (bendGoal != null)
				{
					bendDirection = bendGoal.position - bones[0].solverPosition;
				}
				if (bendGoalWeight < 1f)
				{
					Vector3 vector = bones[0].solverRotation * bones[0].axis;
					Vector3 down = Vector3.down;
					Vector3 toDirection = Quaternion.Inverse(chestRotation) * dir.normalized + Vector3.forward;
					Quaternion quaternion = Quaternion.FromToRotation(down, toDirection);
					Vector3 vector2 = quaternion * Vector3.back;
					down = Quaternion.Inverse(chestRotation) * vector;
					toDirection = Quaternion.Inverse(chestRotation) * dir;
					quaternion = Quaternion.FromToRotation(down, toDirection);
					vector2 = quaternion * vector2;
					vector2 = chestRotation * vector2;
					vector2 += vector;
					vector2 -= rotation * wristToPalmAxis;
					vector2 -= rotation * palmToThumbAxis * 0.5f;
					if (bendGoalWeight > 0f)
					{
						vector2 = Vector3.Slerp(vector2, bendDirection, bendGoalWeight);
					}
					if (swivelOffset != 0f)
					{
						vector2 = Quaternion.AngleAxis(swivelOffset, -dir) * vector2;
					}
					return Vector3.Cross(vector2, dir);
				}
				return Vector3.Cross(bendDirection, dir);
			}

			private void Visualize(VirtualBone bone1, VirtualBone bone2, VirtualBone bone3, Color color)
			{
				Debug.DrawLine(bone1.solverPosition, bone2.solverPosition, color);
				Debug.DrawLine(bone2.solverPosition, bone3.solverPosition, color);
			}
		}

		[Serializable]
		public abstract class BodyPart
		{
			[HideInInspector]
			public VirtualBone[] bones = new VirtualBone[0];

			protected bool initiated;

			protected Vector3 rootPosition;

			protected Quaternion rootRotation = Quaternion.identity;

			protected int index = -1;

			public float sqrMag { get; private set; }

			public float mag { get; private set; }

			protected abstract void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasNeck, bool hasShoulders, bool hasToes, int rootIndex, int index);

			public abstract void PreSolve();

			public abstract void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations);

			public abstract void ApplyOffsets();

			public abstract void ResetOffsets();

			public void Read(Vector3[] positions, Quaternion[] rotations, bool hasNeck, bool hasShoulders, bool hasToes, int rootIndex, int index)
			{
				this.index = index;
				rootPosition = positions[rootIndex];
				rootRotation = rotations[rootIndex];
				OnRead(positions, rotations, hasNeck, hasShoulders, hasToes, rootIndex, index);
				mag = VirtualBone.PreSolve(ref bones);
				sqrMag = mag * mag;
				initiated = true;
			}

			public void MovePosition(Vector3 position)
			{
				Vector3 vector = position - bones[0].solverPosition;
				VirtualBone[] array = bones;
				foreach (VirtualBone virtualBone in array)
				{
					virtualBone.solverPosition += vector;
				}
			}

			public void MoveRotation(Quaternion rotation)
			{
				Quaternion rotation2 = QuaTools.FromToRotation(bones[0].solverRotation, rotation);
				VirtualBone.RotateAroundPoint(bones, 0, bones[0].solverPosition, rotation2);
			}

			public void Translate(Vector3 position, Quaternion rotation)
			{
				MovePosition(position);
				MoveRotation(rotation);
			}

			public void TranslateRoot(Vector3 newRootPos, Quaternion newRootRot)
			{
				Vector3 vector = newRootPos - rootPosition;
				rootPosition = newRootPos;
				VirtualBone[] array = bones;
				foreach (VirtualBone virtualBone in array)
				{
					virtualBone.solverPosition += vector;
				}
				Quaternion rotation = QuaTools.FromToRotation(rootRotation, newRootRot);
				rootRotation = newRootRot;
				VirtualBone.RotateAroundPoint(bones, 0, newRootPos, rotation);
			}

			public void RotateTo(VirtualBone bone, Quaternion rotation, float weight = 1f)
			{
				if (weight <= 0f)
				{
					return;
				}
				Quaternion quaternion = QuaTools.FromToRotation(bone.solverRotation, rotation);
				if (weight < 1f)
				{
					quaternion = Quaternion.Slerp(Quaternion.identity, quaternion, weight);
				}
				for (int i = 0; i < bones.Length; i++)
				{
					if (bones[i] == bone)
					{
						VirtualBone.RotateAroundPoint(bones, i, bones[i].solverPosition, quaternion);
						break;
					}
				}
			}

			public void Visualize(Color color)
			{
				for (int i = 0; i < bones.Length - 1; i++)
				{
					Debug.DrawLine(bones[i].solverPosition, bones[i + 1].solverPosition, color);
				}
			}

			public void Visualize()
			{
				Visualize(Color.white);
			}
		}

		[Serializable]
		public class Footstep
		{
			public float stepSpeed = 3f;

			public Vector3 characterSpaceOffset;

			public Vector3 stepTo;

			private Quaternion stepFromRot = Quaternion.identity;

			private Quaternion stepToRot = Quaternion.identity;

			private Quaternion footRelativeToRoot = Quaternion.identity;

			public Vector3 position { get; private set; }

			public Quaternion rotation { get; private set; }

			public Quaternion stepToRootRot { get; private set; }

			public bool isStepping
			{
				get
				{
					return stepProgress < 1f;
				}
			}

			public float stepProgress { get; private set; }

			public Vector3 stepFrom { get; private set; }

			public Footstep(Quaternion rootRotation, Vector3 footPosition, Quaternion footRotation, Vector3 characterSpaceOffset)
			{
				this.characterSpaceOffset = characterSpaceOffset;
				Reset(rootRotation, footPosition, footRotation);
			}

			public void Reset(Quaternion rootRotation, Vector3 footPosition, Quaternion footRotation)
			{
				position = footPosition;
				rotation = footRotation;
				stepFrom = position;
				stepTo = position;
				stepFromRot = rotation;
				stepToRot = rotation;
				stepToRootRot = rootRotation;
				stepProgress = 1f;
				footRelativeToRoot = Quaternion.Inverse(rootRotation) * rotation;
			}

			public void StepTo(Vector3 p, Quaternion rootRotation)
			{
				stepFrom = position;
				stepTo = p;
				stepFromRot = rotation;
				stepToRootRot = rootRotation;
				stepToRot = rootRotation * footRelativeToRoot;
				stepProgress = 0f;
			}

			public void Update(InterpolationMode interpolation)
			{
				if (isStepping)
				{
					stepProgress = Mathf.MoveTowards(stepProgress, 1f, Time.deltaTime * stepSpeed);
					float t = Interp.Float(stepProgress, interpolation);
					position = Vector3.Lerp(stepFrom, stepTo, t);
					rotation = Quaternion.Lerp(stepFromRot, stepToRot, t);
				}
			}
		}

		[Serializable]
		public class Leg : BodyPart
		{
			[Tooltip("The toe/foot target.")]
			public Transform target;

			[Tooltip("The knee will be bent towards this Transform if 'Bend Goal Weight' > 0.")]
			public Transform bendGoal;

			[Tooltip("Positional weight of the toe/foot target.")]
			[Range(0f, 1f)]
			public float positionWeight;

			[Tooltip("Rotational weight of the toe/foot target.")]
			[Range(0f, 1f)]
			public float rotationWeight;

			[Tooltip("If greater than 0, will bend the knee towards the 'Bend Goal' Transform.")]
			[Range(0f, 1f)]
			public float bendGoalWeight;

			[Tooltip("Angular offset of the knee bending direction.")]
			[Range(-180f, 180f)]
			public float swivelOffset;

			[HideInInspector]
			public Vector3 footPositionOffset;

			[HideInInspector]
			public Vector3 heelPositionOffset;

			[HideInInspector]
			public Quaternion footRotationOffset = Quaternion.identity;

			[HideInInspector]
			public float currentMag;

			private Vector3 footPosition;

			private Quaternion footRotation;

			private Vector3 bendNormal;

			private Quaternion calfRelToThigh;

			public Vector3 IKPosition { get; private set; }

			public Quaternion IKRotation { get; private set; }

			public Vector3 position { get; private set; }

			public Quaternion rotation { get; private set; }

			public bool hasToes { get; private set; }

			public VirtualBone thigh
			{
				get
				{
					return bones[0];
				}
			}

			private VirtualBone calf
			{
				get
				{
					return bones[1];
				}
			}

			private VirtualBone foot
			{
				get
				{
					return bones[2];
				}
			}

			private VirtualBone toes
			{
				get
				{
					return bones[3];
				}
			}

			public VirtualBone lastBone
			{
				get
				{
					return bones[bones.Length - 1];
				}
			}

			public Vector3 thighRelativeToPelvis { get; private set; }

			protected override void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasNeck, bool hasShoulders, bool hasToes, int rootIndex, int index)
			{
				Vector3 vector = positions[index];
				Quaternion quaternion = rotations[index];
				Vector3 vector2 = positions[index + 1];
				Quaternion quaternion2 = rotations[index + 1];
				Vector3 iKPosition = positions[index + 2];
				Quaternion iKRotation = rotations[index + 2];
				Vector3 iKPosition2 = positions[index + 3];
				Quaternion iKRotation2 = rotations[index + 3];
				if (!initiated)
				{
					this.hasToes = hasToes;
					bones = new VirtualBone[(!hasToes) ? 3 : 4];
					if (hasToes)
					{
						bones[0] = new VirtualBone(vector, quaternion);
						bones[1] = new VirtualBone(vector2, quaternion2);
						bones[2] = new VirtualBone(iKPosition, iKRotation);
						bones[3] = new VirtualBone(iKPosition2, iKRotation2);
						IKPosition = iKPosition2;
						IKRotation = iKRotation2;
					}
					else
					{
						bones[0] = new VirtualBone(vector, quaternion);
						bones[1] = new VirtualBone(vector2, quaternion2);
						bones[2] = new VirtualBone(iKPosition, iKRotation);
						IKPosition = iKPosition;
						IKRotation = iKRotation;
					}
				}
				if (hasToes)
				{
					bones[0].Read(vector, quaternion);
					bones[1].Read(vector2, quaternion2);
					bones[2].Read(iKPosition, iKRotation);
					bones[3].Read(iKPosition2, iKRotation2);
				}
				else
				{
					bones[0].Read(vector, quaternion);
					bones[1].Read(vector2, quaternion2);
					bones[2].Read(iKPosition, iKRotation);
				}
			}

			public override void PreSolve()
			{
				if (target != null)
				{
					IKPosition = target.position;
					IKRotation = target.rotation;
				}
				footPosition = foot.solverPosition;
				footRotation = foot.solverRotation;
				position = lastBone.solverPosition;
				rotation = lastBone.solverRotation;
				if (rotationWeight > 0f)
				{
					ApplyRotationOffset(QuaTools.FromToRotation(rotation, IKRotation), rotationWeight);
				}
				if (positionWeight > 0f)
				{
					ApplyPositionOffset(IKPosition - position, positionWeight);
				}
				thighRelativeToPelvis = Quaternion.Inverse(rootRotation) * (thigh.solverPosition - rootPosition);
				calfRelToThigh = Quaternion.Inverse(thigh.solverRotation) * calf.solverRotation;
				bendNormal = Vector3.Cross(calf.solverPosition - thigh.solverPosition, foot.solverPosition - calf.solverPosition);
			}

			public override void ApplyOffsets()
			{
				ApplyPositionOffset(footPositionOffset, 1f);
				ApplyRotationOffset(footRotationOffset, 1f);
				Quaternion quaternion = Quaternion.FromToRotation(footPosition - position, footPosition + heelPositionOffset - position);
				footPosition = position + quaternion * (footPosition - position);
				footRotation = quaternion * footRotation;
				float num = 0f;
				if (bendGoal != null && bendGoalWeight > 0f)
				{
					Vector3 vector = Vector3.Cross(bendGoal.position - thigh.solverPosition, foot.solverPosition - thigh.solverPosition);
					Quaternion quaternion2 = Quaternion.LookRotation(bendNormal, thigh.solverPosition - foot.solverPosition);
					Vector3 vector2 = Quaternion.Inverse(quaternion2) * vector;
					num = Mathf.Atan2(vector2.x, vector2.z) * 57.29578f * bendGoalWeight;
				}
				float num2 = swivelOffset + num;
				if (num2 != 0f)
				{
					bendNormal = Quaternion.AngleAxis(num2, thigh.solverPosition - lastBone.solverPosition) * bendNormal;
					thigh.solverRotation = Quaternion.AngleAxis(0f - num2, thigh.solverRotation * thigh.axis) * thigh.solverRotation;
				}
			}

			private void ApplyPositionOffset(Vector3 offset, float weight)
			{
				if (!(weight <= 0f))
				{
					offset *= weight;
					footPosition += offset;
					position += offset;
				}
			}

			private void ApplyRotationOffset(Quaternion offset, float weight)
			{
				if (!(weight <= 0f))
				{
					if (weight < 1f)
					{
						offset = Quaternion.Lerp(Quaternion.identity, offset, weight);
					}
					footRotation = offset * footRotation;
					rotation = offset * rotation;
					bendNormal = offset * bendNormal;
					footPosition = position + offset * (footPosition - position);
				}
			}

			public void Solve()
			{
				VirtualBone.SolveTrigonometric(bones, 0, 1, 2, footPosition, bendNormal, 1f);
				RotateTo(foot, footRotation);
				if (hasToes)
				{
					Vector3 vector = Vector3.Cross(foot.solverPosition - thigh.solverPosition, toes.solverPosition - foot.solverPosition);
					VirtualBone.SolveTrigonometric(bones, 0, 2, 3, position, vector, 1f);
					Quaternion quaternion = thigh.solverRotation * calfRelToThigh;
					Quaternion quaternion2 = Quaternion.FromToRotation(quaternion * calf.axis, foot.solverPosition - calf.solverPosition);
					RotateTo(calf, quaternion2 * quaternion);
					toes.solverRotation = rotation;
				}
			}

			public override void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations)
			{
				solvedRotations[index] = thigh.solverRotation;
				solvedRotations[index + 1] = calf.solverRotation;
				solvedRotations[index + 2] = foot.solverRotation;
				if (hasToes)
				{
					solvedRotations[index + 3] = toes.solverRotation;
				}
			}

			public override void ResetOffsets()
			{
				footPositionOffset = Vector3.zero;
				footRotationOffset = Quaternion.identity;
				heelPositionOffset = Vector3.zero;
			}
		}

		[Serializable]
		public class Locomotion
		{
			[Tooltip("Used for blending in/out of procedural locomotion.")]
			[Range(0f, 1f)]
			public float weight = 1f;

			[Tooltip("Tries to maintain this distance between the legs.")]
			public float footDistance = 0.3f;

			[Tooltip("Makes a step only if step target position is at least this far from the current footstep or the foot does not reach the current footstep anymore or footstep angle is past the 'Angle Threshold'.")]
			public float stepThreshold = 0.4f;

			[Tooltip("Makes a step only if step target position is at least 'Step Threshold' far from the current footstep or the foot does not reach the current footstep anymore or footstep angle is past this value.")]
			public float angleThreshold = 60f;

			[Tooltip("Multiplies angle of the center of mass - center of pressure vector. Larger value makes the character step sooner if losing balance.")]
			public float comAngleMlp = 1f;

			[Tooltip("Maximum magnitude of head/hand target velocity used in prediction.")]
			public float maxVelocity = 0.4f;

			[Tooltip("The amount of head/hand target velocity prediction.")]
			public float velocityFactor = 0.4f;

			[Tooltip("How much can a leg be extended before it is forced to step to another position? 1 means fully stretched.")]
			[Range(0.9f, 1f)]
			public float maxLegStretch = 1f;

			[Tooltip("The speed of lerping the root of the character towards the horizontal mid-point of the footsteps.")]
			public float rootSpeed = 20f;

			[Tooltip("The speed of steps.")]
			public float stepSpeed = 3f;

			[Tooltip("The height of the foot by normalized step progress (0 - 1).")]
			public AnimationCurve stepHeight;

			[Tooltip("The height offset of the heel by normalized step progress (0 - 1).")]
			public AnimationCurve heelHeight;

			[Tooltip("Interpolation mode of the step.")]
			public InterpolationMode stepInterpolation = InterpolationMode.InOutSine;

			[Tooltip("Offset for the approximated center of mass.")]
			public Vector3 offset;

			[HideInInspector]
			public bool blockingEnabled;

			[HideInInspector]
			public LayerMask blockingLayers;

			[HideInInspector]
			public float raycastRadius = 0.2f;

			[HideInInspector]
			public float raycastHeight = 0.2f;

			private Footstep[] footsteps = new Footstep[0];

			private Vector3 lastComPosition;

			private Vector3 comVelocity;

			private int leftFootIndex;

			private int rightFootIndex;

			public Vector3 centerOfMass { get; private set; }

			public void Initiate(Vector3[] positions, Quaternion[] rotations, bool hasToes)
			{
				leftFootIndex = ((!hasToes) ? 16 : 17);
				rightFootIndex = ((!hasToes) ? 20 : 21);
				footsteps = new Footstep[2]
				{
					new Footstep(rotations[0], positions[leftFootIndex], rotations[leftFootIndex], footDistance * Vector3.left),
					new Footstep(rotations[0], positions[rightFootIndex], rotations[rightFootIndex], footDistance * Vector3.right)
				};
			}

			public void Reset(Vector3[] positions, Quaternion[] rotations)
			{
				lastComPosition = Vector3.Lerp(positions[1], positions[5], 0.25f) + rotations[0] * offset;
				comVelocity = Vector3.zero;
				footsteps[0].Reset(rotations[0], positions[leftFootIndex], rotations[leftFootIndex]);
				footsteps[1].Reset(rotations[0], positions[rightFootIndex], rotations[rightFootIndex]);
			}

			public void Solve(VirtualBone rootBone, Spine spine, Leg leftLeg, Leg rightLeg, Arm leftArm, Arm rightArm, out Vector3 leftFootPosition, out Vector3 rightFootPosition, out Quaternion leftFootRotation, out Quaternion rightFootRotation, out float leftFootOffset, out float rightFootOffset, out float leftHeelOffset, out float rightHeelOffset)
			{
				if (weight <= 0f)
				{
					leftFootPosition = Vector3.zero;
					rightFootPosition = Vector3.zero;
					leftFootRotation = Quaternion.identity;
					rightFootRotation = Quaternion.identity;
					leftFootOffset = 0f;
					rightFootOffset = 0f;
					leftHeelOffset = 0f;
					rightHeelOffset = 0f;
					return;
				}
				Vector3 vector = spine.pelvis.solverPosition + spine.pelvis.solverRotation * leftLeg.thighRelativeToPelvis;
				Vector3 vector2 = spine.pelvis.solverPosition + spine.pelvis.solverRotation * rightLeg.thighRelativeToPelvis;
				footsteps[0].characterSpaceOffset = footDistance * Vector3.left;
				footsteps[1].characterSpaceOffset = footDistance * Vector3.right;
				Vector3 faceDirection = spine.faceDirection;
				faceDirection.y = 0f;
				Quaternion quaternion = Quaternion.LookRotation(faceDirection);
				float num = 1f;
				float num2 = 1f;
				float num3 = 0.2f;
				float num4 = num + num2 + 2f * num3;
				centerOfMass = Vector3.zero;
				centerOfMass += spine.pelvis.solverPosition * num;
				centerOfMass += spine.head.solverPosition * num2;
				centerOfMass += leftArm.position * num3;
				centerOfMass += rightArm.position * num3;
				centerOfMass /= num4;
				centerOfMass += rootBone.solverRotation * offset;
				comVelocity = (centerOfMass - lastComPosition) / Time.deltaTime;
				lastComPosition = centerOfMass;
				comVelocity = Vector3.ClampMagnitude(comVelocity, maxVelocity) * velocityFactor;
				Vector3 vector3 = centerOfMass + comVelocity;
				Vector3 vector4 = new Vector3(spine.pelvis.solverPosition.x, rootBone.solverPosition.y, spine.pelvis.solverPosition.z);
				Vector3 vector5 = new Vector3(vector3.x, rootBone.solverPosition.y, vector3.z);
				Vector3 vector6 = Vector3.Lerp(footsteps[0].position, footsteps[1].position, 0.5f);
				Vector3 from = vector3 - vector6;
				float num5 = Vector3.Angle(from, rootBone.solverRotation * Vector3.up) * comAngleMlp;
				for (int i = 0; i < footsteps.Length; i++)
				{
					if (footsteps[i].isStepping)
					{
						Vector3 vector7 = vector5 + rootBone.solverRotation * footsteps[i].characterSpaceOffset;
						if (!StepBlocked(footsteps[i].stepFrom, vector7, rootBone.solverPosition))
						{
							footsteps[i].stepTo = Vector3.Lerp(footsteps[i].stepTo, vector7, Time.deltaTime * 8f);
						}
					}
				}
				if (CanStep())
				{
					int num6 = -1;
					float num7 = float.NegativeInfinity;
					for (int j = 0; j < footsteps.Length; j++)
					{
						if (footsteps[j].isStepping)
						{
							continue;
						}
						Vector3 vector8 = vector5 + rootBone.solverRotation * footsteps[j].characterSpaceOffset;
						float num8 = ((j != 0) ? rightLeg.mag : leftLeg.mag);
						Vector3 b = ((j != 0) ? vector2 : vector);
						float num9 = Vector3.Distance(footsteps[j].position, b);
						bool flag = false;
						if (num9 >= num8 * maxLegStretch)
						{
							vector8 = vector4 + rootBone.solverRotation * footsteps[j].characterSpaceOffset;
							flag = true;
						}
						bool flag2 = false;
						for (int k = 0; k < footsteps.Length; k++)
						{
							if (k != j && !flag)
							{
								if (!(Vector3.Distance(footsteps[j].position, footsteps[k].position) < 0.25f) || !((footsteps[j].position - vector8).sqrMagnitude < (footsteps[k].position - vector8).sqrMagnitude))
								{
									flag2 = GetLineSphereCollision(footsteps[j].position, vector8, footsteps[k].position, 0.25f);
								}
								if (flag2)
								{
									break;
								}
							}
						}
						float num10 = Quaternion.Angle(quaternion, footsteps[j].stepToRootRot);
						if (flag2 && !(num10 > angleThreshold))
						{
							continue;
						}
						float num11 = Vector3.Distance(footsteps[j].position, vector8);
						float num12 = Mathf.Lerp(stepThreshold, stepThreshold * 0.1f, num5 * 0.015f);
						if (flag)
						{
							num12 *= 0.5f;
						}
						if (j == 0)
						{
							num12 *= 0.9f;
						}
						if (!StepBlocked(footsteps[j].position, vector8, rootBone.solverPosition) && (num11 > num12 || num10 > angleThreshold))
						{
							float num13 = 0f;
							num13 -= num11;
							if (num13 > num7)
							{
								num6 = j;
								num7 = num13;
							}
						}
					}
					if (num6 != -1)
					{
						Vector3 p = vector5 + rootBone.solverRotation * footsteps[num6].characterSpaceOffset;
						footsteps[num6].stepSpeed = UnityEngine.Random.Range(stepSpeed, stepSpeed * 1.5f);
						footsteps[num6].StepTo(p, quaternion);
					}
				}
				Footstep[] array = footsteps;
				foreach (Footstep footstep in array)
				{
					footstep.Update(stepInterpolation);
				}
				leftFootPosition = footsteps[0].position;
				rightFootPosition = footsteps[1].position;
				leftFootPosition.y = leftLeg.lastBone.readPosition.y;
				rightFootPosition.y = rightLeg.lastBone.readPosition.y;
				leftFootOffset = stepHeight.Evaluate(footsteps[0].stepProgress);
				rightFootOffset = stepHeight.Evaluate(footsteps[1].stepProgress);
				leftHeelOffset = heelHeight.Evaluate(footsteps[0].stepProgress);
				rightHeelOffset = heelHeight.Evaluate(footsteps[1].stepProgress);
				leftFootRotation = footsteps[0].rotation;
				rightFootRotation = footsteps[1].rotation;
			}

			private bool StepBlocked(Vector3 fromPosition, Vector3 toPosition, Vector3 rootPosition)
			{
				if ((int)blockingLayers == -1 || !blockingEnabled)
				{
					return false;
				}
				Vector3 vector = fromPosition;
				vector.y = rootPosition.y + raycastHeight + raycastRadius;
				Vector3 direction = toPosition - vector;
				direction.y = 0f;
				RaycastHit hitInfo;
				if (raycastRadius <= 0f)
				{
					return Physics.Raycast(vector, direction, out hitInfo, direction.magnitude, blockingLayers);
				}
				return Physics.SphereCast(vector, raycastRadius, direction, out hitInfo, direction.magnitude, blockingLayers);
			}

			private bool CanStep()
			{
				Footstep[] array = footsteps;
				foreach (Footstep footstep in array)
				{
					if (footstep.isStepping && footstep.stepProgress < 0.8f)
					{
						return false;
					}
				}
				return true;
			}

			private static bool GetLineSphereCollision(Vector3 lineStart, Vector3 lineEnd, Vector3 sphereCenter, float sphereRadius)
			{
				Vector3 forward = lineEnd - lineStart;
				Vector3 vector = sphereCenter - lineStart;
				float magnitude = vector.magnitude;
				float num = magnitude - sphereRadius;
				if (num > forward.magnitude)
				{
					return false;
				}
				Quaternion rotation = Quaternion.LookRotation(forward, vector);
				Vector3 vector2 = Quaternion.Inverse(rotation) * vector;
				if (vector2.z < 0f)
				{
					return num < 0f;
				}
				return vector2.y - sphereRadius < 0f;
			}
		}

		[Serializable]
		public class Spine : BodyPart
		{
			[Tooltip("The head target.")]
			public Transform headTarget;

			[Tooltip("The pelvis target, useful with seated rigs.")]
			public Transform pelvisTarget;

			[Tooltip("Positional weight of the head target.")]
			[Range(0f, 1f)]
			public float positionWeight = 1f;

			[Tooltip("Rotational weight of the head target.")]
			[Range(0f, 1f)]
			public float rotationWeight = 1f;

			[Tooltip("Positional weight of the pelvis target.")]
			[Range(0f, 1f)]
			public float pelvisPositionWeight;

			[Tooltip("Determines how much the body will follow the position of the head.")]
			[Range(0f, 1f)]
			public float bodyPosStiffness = 0.55f;

			[Tooltip("Determines how much the body will follow the rotation of the head.")]
			[Range(0f, 1f)]
			public float bodyRotStiffness = 0.1f;

			[Tooltip("Determines how much the chest will rotate to the rotation of the head.")]
			[Range(0f, 1f)]
			public float chestRotationWeight = 0.2f;

			[Tooltip("Clamps chest rotation.")]
			[Range(0f, 1f)]
			public float chestClampWeight = 0.5f;

			[Tooltip("Clamps head rotation.")]
			[Range(0f, 1f)]
			public float headClampWeight = 0.6f;

			[Tooltip("How much will the pelvis maintain it's animated position?")]
			[Range(0f, 1f)]
			public float maintainPelvisPosition = 0.2f;

			[Tooltip("Will automatically rotate the root of the character if the head target has turned past this angle.")]
			[Range(0f, 180f)]
			public float maxRootAngle = 25f;

			[HideInInspector]
			public Vector3 pelvisPositionOffset;

			[HideInInspector]
			public Vector3 chestPositionOffset;

			[HideInInspector]
			public Vector3 headPositionOffset;

			[HideInInspector]
			public Quaternion pelvisRotationOffset = Quaternion.identity;

			[HideInInspector]
			public Quaternion chestRotationOffset = Quaternion.identity;

			[HideInInspector]
			public Quaternion headRotationOffset = Quaternion.identity;

			[HideInInspector]
			public Vector3 faceDirection;

			private Vector3 headPosition;

			private Quaternion headRotation = Quaternion.identity;

			private Quaternion anchorRelativeToHead = Quaternion.identity;

			private Quaternion pelvisRelativeRotation = Quaternion.identity;

			private Quaternion chestRelativeRotation = Quaternion.identity;

			private Vector3 headDeltaPosition;

			private Quaternion pelvisDeltaRotation = Quaternion.identity;

			private Quaternion chestTargetRotation = Quaternion.identity;

			private const int pelvisIndex = 0;

			private const int spineIndex = 1;

			private const int chestIndex = 2;

			private const int neckIndex = 3;

			private int headIndex;

			private float length;

			private bool hasNeck;

			private float headHeight;

			public Vector3 IKPositionHead { get; private set; }

			public Quaternion IKRotationHead { get; private set; }

			public Vector3 IKPositionPelvis { get; private set; }

			public VirtualBone pelvis
			{
				get
				{
					return bones[0];
				}
			}

			public VirtualBone firstSpineBone
			{
				get
				{
					return bones[1];
				}
			}

			public VirtualBone chest
			{
				get
				{
					return bones[2];
				}
			}

			private VirtualBone neck
			{
				get
				{
					return bones[3];
				}
			}

			public VirtualBone head
			{
				get
				{
					return bones[headIndex];
				}
			}

			public Quaternion anchorRotation { get; private set; }

			protected override void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasNeck, bool hasShoulders, bool hasToes, int rootIndex, int index)
			{
				Vector3 vector = positions[index];
				Quaternion quaternion = rotations[index];
				Vector3 position = positions[index + 1];
				Quaternion rotation = rotations[index + 1];
				Vector3 position2 = positions[index + 2];
				Quaternion quaternion2 = rotations[index + 2];
				Vector3 position3 = positions[index + 3];
				Quaternion rotation2 = rotations[index + 3];
				Vector3 vector2 = positions[index + 4];
				Quaternion quaternion3 = rotations[index + 4];
				if (!initiated)
				{
					this.hasNeck = hasNeck;
					headHeight = vector2.y - positions[0].y;
					bones = new VirtualBone[(!hasNeck) ? 4 : 5];
					headIndex = ((!hasNeck) ? 3 : 4);
					bones[0] = new VirtualBone(vector, quaternion);
					bones[1] = new VirtualBone(position, rotation);
					bones[2] = new VirtualBone(position2, quaternion2);
					if (hasNeck)
					{
						bones[3] = new VirtualBone(position3, rotation2);
					}
					bones[headIndex] = new VirtualBone(vector2, quaternion3);
					pelvisRotationOffset = Quaternion.identity;
					chestRotationOffset = Quaternion.identity;
					headRotationOffset = Quaternion.identity;
					anchorRelativeToHead = Quaternion.Inverse(quaternion3) * rotations[0];
					pelvisRelativeRotation = Quaternion.Inverse(quaternion3) * quaternion;
					chestRelativeRotation = Quaternion.Inverse(quaternion3) * quaternion2;
					faceDirection = rotations[0] * Vector3.forward;
					IKPositionHead = vector2;
					IKRotationHead = quaternion3;
					IKPositionPelvis = vector;
				}
				bones[0].Read(vector, quaternion);
				bones[1].Read(position, rotation);
				bones[2].Read(position2, quaternion2);
				if (hasNeck)
				{
					bones[3].Read(position3, rotation2);
				}
				bones[headIndex].Read(vector2, quaternion3);
			}

			public override void PreSolve()
			{
				if (headTarget != null)
				{
					IKPositionHead = headTarget.position;
					IKRotationHead = headTarget.rotation;
				}
				if (pelvisTarget != null)
				{
					IKPositionPelvis = pelvisTarget.position;
				}
				headPosition = V3Tools.Lerp(head.solverPosition, IKPositionHead, positionWeight);
				headRotation = QuaTools.Lerp(head.solverRotation, IKRotationHead, rotationWeight);
			}

			public override void ApplyOffsets()
			{
				headPosition += headPositionOffset;
				headPosition.y = Math.Max(rootPosition.y + 0.8f, headPosition.y);
				headRotation = headRotationOffset * headRotation;
				headDeltaPosition = headPosition - head.solverPosition;
				pelvisDeltaRotation = QuaTools.FromToRotation(pelvis.solverRotation, headRotation * pelvisRelativeRotation);
				anchorRotation = headRotation * anchorRelativeToHead;
			}

			private void CalculateChestTargetRotation(Arm[] arms)
			{
				chestTargetRotation = headRotation * chestRelativeRotation;
				AdjustChestByHands(ref chestTargetRotation, arms);
				AdjustChestByOffset(ref chestTargetRotation);
				faceDirection = Vector3.Cross(anchorRotation * Vector3.right, Vector3.up) + anchorRotation * Vector3.forward;
			}

			public void Solve(VirtualBone rootBone, Leg[] legs, Arm[] arms)
			{
				CalculateChestTargetRotation(arms);
				if (maxRootAngle < 180f)
				{
					Vector3 vector = Quaternion.Inverse(rootBone.solverRotation) * faceDirection;
					float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
					float angle = 0f;
					float num2 = 25f;
					if (num > num2)
					{
						angle = num - num2;
					}
					if (num < 0f - num2)
					{
						angle = num + num2;
					}
					rootBone.solverRotation = Quaternion.AngleAxis(angle, Vector3.up) * rootBone.solverRotation;
				}
				Vector3 solverPosition = pelvis.solverPosition;
				TranslatePelvis(legs, headDeltaPosition, pelvisDeltaRotation, 1f);
				VirtualBone.SolveFABRIK(bones, Vector3.Lerp(pelvis.solverPosition, solverPosition, maintainPelvisPosition) + pelvisPositionOffset - chestPositionOffset, headPosition - chestPositionOffset, 1f, 1f, 1, base.mag);
				Bend(bones, 0, 2, chestTargetRotation, chestClampWeight, false, chestRotationWeight);
				InverseTranslateToHead(legs, false, false, Vector3.zero, 1f);
				VirtualBone.SolveFABRIK(bones, Vector3.Lerp(pelvis.solverPosition, solverPosition, maintainPelvisPosition) + pelvisPositionOffset - chestPositionOffset, headPosition - chestPositionOffset, 1f, 1f, 1, base.mag);
				Bend(bones, 3, headIndex, headRotation, headClampWeight, true, 1f);
				SolvePelvis();
			}

			private void SolvePelvis()
			{
				if (pelvisPositionWeight > 0f)
				{
					Quaternion solverRotation = head.solverRotation;
					Vector3 vector = (IKPositionPelvis + pelvisPositionOffset - pelvis.solverPosition) * pelvisPositionWeight;
					VirtualBone[] array = bones;
					foreach (VirtualBone virtualBone in array)
					{
						virtualBone.solverPosition += vector;
					}
					Vector3 bendNormal = anchorRotation * Vector3.right;
					if (hasNeck)
					{
						VirtualBone.SolveTrigonometric(bones, 0, 1, bones.Length - 1, headPosition, bendNormal, pelvisPositionWeight * 0.6f);
						VirtualBone.SolveTrigonometric(bones, 1, 2, bones.Length - 1, headPosition, bendNormal, pelvisPositionWeight * 0.6f);
						VirtualBone.SolveTrigonometric(bones, 2, 3, bones.Length - 1, headPosition, bendNormal, pelvisPositionWeight * 1f);
					}
					else
					{
						VirtualBone.SolveTrigonometric(bones, 0, 1, bones.Length - 1, headPosition, bendNormal, pelvisPositionWeight * 0.75f);
						VirtualBone.SolveTrigonometric(bones, 1, 2, bones.Length - 1, headPosition, bendNormal, pelvisPositionWeight * 1f);
					}
					head.solverRotation = solverRotation;
				}
			}

			public override void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations)
			{
				solvedPositions[index] = bones[0].solverPosition;
				solvedRotations[index] = bones[0].solverRotation;
				solvedRotations[index + 1] = bones[1].solverRotation;
				solvedRotations[index + 2] = bones[2].solverRotation;
				if (hasNeck)
				{
					solvedRotations[index + 3] = bones[3].solverRotation;
				}
				solvedRotations[index + 4] = bones[headIndex].solverRotation;
			}

			public override void ResetOffsets()
			{
				pelvisPositionOffset = Vector3.zero;
				chestPositionOffset = Vector3.zero;
				headPositionOffset = Vector3.zero;
				pelvisRotationOffset = Quaternion.identity;
				chestRotationOffset = Quaternion.identity;
				headRotationOffset = Quaternion.identity;
			}

			private void AdjustChestByOffset(ref Quaternion chestTargetRotation)
			{
				chestTargetRotation = chestRotationOffset * chestTargetRotation;
			}

			private void AdjustChestByHands(ref Quaternion chestTargetRotation, Arm[] arms)
			{
				Quaternion quaternion = Quaternion.Inverse(anchorRotation);
				Vector3 vector = quaternion * (arms[0].position - headPosition);
				Vector3 vector2 = quaternion * (arms[1].position - headPosition);
				Vector3 forward = Vector3.forward;
				forward.x += vector.x * Mathf.Abs(vector.x);
				forward.x += vector.z * Mathf.Abs(vector.z);
				forward.x += vector2.x * Mathf.Abs(vector2.x);
				forward.x -= vector2.z * Mathf.Abs(vector2.z);
				forward.x *= 5f;
				Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.forward, forward);
				chestTargetRotation = quaternion2 * chestTargetRotation;
				Vector3 up = Vector3.up;
				up.x += vector.y;
				up.x -= vector2.y;
				up.x *= 0.5f;
				quaternion2 = Quaternion.FromToRotation(Vector3.up, anchorRotation * up);
				chestTargetRotation = quaternion2 * chestTargetRotation;
			}

			public void InverseTranslateToHead(Leg[] legs, bool limited, bool useCurrentLegMag, Vector3 offset, float w)
			{
				Vector3 vector = pelvis.solverPosition + (headPosition + offset - head.solverPosition) * w * (1f - pelvisPositionWeight);
				MovePosition((!limited) ? vector : LimitPelvisPosition(legs, vector, useCurrentLegMag));
			}

			private void TranslatePelvis(Leg[] legs, Vector3 deltaPosition, Quaternion deltaRotation, float w)
			{
				Vector3 solverPosition = head.solverPosition;
				deltaRotation = QuaTools.ClampRotation(deltaRotation, chestClampWeight, 2);
				Quaternion quaternion = ((!(w >= 1f)) ? Quaternion.Slerp(Quaternion.identity, pelvisRotationOffset, w) : pelvisRotationOffset);
				VirtualBone.RotateAroundPoint(bones, 0, pelvis.solverPosition, quaternion * Quaternion.Slerp(Quaternion.identity, deltaRotation, w * bodyRotStiffness));
				deltaPosition -= head.solverPosition - solverPosition;
				Vector3 vector = anchorRotation * Vector3.forward;
				vector.y = 0f;
				float num = deltaPosition.y * 0.35f * headHeight;
				deltaPosition += vector * num;
				MovePosition(LimitPelvisPosition(legs, pelvis.solverPosition + deltaPosition * w * bodyPosStiffness, false));
			}

			private Vector3 LimitPelvisPosition(Leg[] legs, Vector3 pelvisPosition, bool useCurrentLegMag, int it = 2)
			{
				if (useCurrentLegMag)
				{
					foreach (Leg leg in legs)
					{
						leg.currentMag = Vector3.Distance(leg.thigh.solverPosition, leg.lastBone.solverPosition);
					}
				}
				for (int j = 0; j < it; j++)
				{
					foreach (Leg leg2 in legs)
					{
						Vector3 vector = pelvisPosition - pelvis.solverPosition;
						Vector3 vector2 = leg2.thigh.solverPosition + vector;
						Vector3 vector3 = vector2 - leg2.position;
						float maxLength = ((!useCurrentLegMag) ? leg2.mag : leg2.currentMag);
						Vector3 vector4 = leg2.position + Vector3.ClampMagnitude(vector3, maxLength);
						pelvisPosition += vector4 - vector2;
					}
				}
				return pelvisPosition;
			}

			private void Bend(VirtualBone[] bones, int firstIndex, int lastIndex, Quaternion targetRotation, float clampWeight, bool uniformWeight, float w)
			{
				if (w <= 0f || bones.Length == 0)
				{
					return;
				}
				int num = lastIndex + 1 - firstIndex;
				if (num < 1)
				{
					return;
				}
				Quaternion rotation = QuaTools.FromToRotation(bones[lastIndex].solverRotation, targetRotation);
				rotation = QuaTools.ClampRotation(rotation, clampWeight, 2);
				float num2 = ((!uniformWeight) ? 0f : (1f / (float)num));
				for (int i = firstIndex; i < lastIndex + 1; i++)
				{
					if (!uniformWeight)
					{
						num2 = Mathf.Clamp((i - firstIndex + 1) / num, 0f, 1f);
					}
					VirtualBone.RotateAroundPoint(bones, i, bones[i].solverPosition, Quaternion.Slerp(Quaternion.identity, rotation, num2 * w));
				}
			}
		}

		[Serializable]
		public enum PositionOffset
		{
			Pelvis = 0,
			Chest = 1,
			Head = 2,
			LeftHand = 3,
			RightHand = 4,
			LeftFoot = 5,
			RightFoot = 6,
			LeftHeel = 7,
			RightHeel = 8
		}

		[Serializable]
		public enum RotationOffset
		{
			Pelvis = 0,
			Chest = 1,
			Head = 2
		}

		[Serializable]
		public class VirtualBone
		{
			public Vector3 readPosition;

			public Quaternion readRotation;

			public Vector3 solverPosition;

			public Quaternion solverRotation;

			public float length;

			public float sqrMag;

			public Vector3 axis;

			public VirtualBone(Vector3 position, Quaternion rotation)
			{
				Read(position, rotation);
			}

			public void Read(Vector3 position, Quaternion rotation)
			{
				readPosition = position;
				readRotation = rotation;
				solverPosition = position;
				solverRotation = rotation;
			}

			public static void SwingRotation(VirtualBone[] bones, int index, Vector3 swingTarget, float weight = 1f)
			{
				if (!(weight <= 0f))
				{
					Quaternion quaternion = Quaternion.FromToRotation(bones[index].solverRotation * bones[index].axis, swingTarget - bones[index].solverPosition);
					if (weight < 1f)
					{
						quaternion = Quaternion.Lerp(Quaternion.identity, quaternion, weight);
					}
					for (int i = index; i < bones.Length; i++)
					{
						bones[i].solverRotation = quaternion * bones[i].solverRotation;
					}
				}
			}

			public static float PreSolve(ref VirtualBone[] bones)
			{
				float num = 0f;
				for (int i = 0; i < bones.Length; i++)
				{
					if (i < bones.Length - 1)
					{
						bones[i].sqrMag = (bones[i + 1].solverPosition - bones[i].solverPosition).sqrMagnitude;
						bones[i].length = Mathf.Sqrt(bones[i].sqrMag);
						num += bones[i].length;
						bones[i].axis = Quaternion.Inverse(bones[i].solverRotation) * (bones[i + 1].solverPosition - bones[i].solverPosition);
					}
					else
					{
						bones[i].sqrMag = 0f;
						bones[i].length = 0f;
					}
				}
				return num;
			}

			public static void RotateAroundPoint(VirtualBone[] bones, int index, Vector3 point, Quaternion rotation)
			{
				for (int i = index; i < bones.Length; i++)
				{
					if (bones[i] != null)
					{
						Vector3 vector = bones[i].solverPosition - point;
						bones[i].solverPosition = point + rotation * vector;
						bones[i].solverRotation = rotation * bones[i].solverRotation;
					}
				}
			}

			public static void RotateBy(VirtualBone[] bones, int index, Quaternion rotation)
			{
				for (int i = index; i < bones.Length; i++)
				{
					if (bones[i] != null)
					{
						Vector3 vector = bones[i].solverPosition - bones[index].solverPosition;
						bones[i].solverPosition = bones[index].solverPosition + rotation * vector;
						bones[i].solverRotation = rotation * bones[i].solverRotation;
					}
				}
			}

			public static void RotateBy(VirtualBone[] bones, Quaternion rotation)
			{
				for (int i = 0; i < bones.Length; i++)
				{
					if (bones[i] != null)
					{
						if (i > 0)
						{
							Vector3 vector = bones[i].solverPosition - bones[0].solverPosition;
							bones[i].solverPosition = bones[0].solverPosition + rotation * vector;
						}
						bones[i].solverRotation = rotation * bones[i].solverRotation;
					}
				}
			}

			public static void RotateTo(VirtualBone[] bones, int index, Quaternion rotation)
			{
				Quaternion rotation2 = QuaTools.FromToRotation(bones[index].solverRotation, rotation);
				RotateAroundPoint(bones, index, bones[index].solverPosition, rotation2);
			}

			public static void SolveTrigonometric(VirtualBone[] bones, int first, int second, int third, Vector3 targetPosition, Vector3 bendNormal, float weight)
			{
				if (weight <= 0f)
				{
					return;
				}
				targetPosition = Vector3.Lerp(bones[third].solverPosition, targetPosition, weight);
				Vector3 vector = targetPosition - bones[first].solverPosition;
				float sqrMagnitude = vector.sqrMagnitude;
				if (sqrMagnitude != 0f)
				{
					float directionMag = Mathf.Sqrt(sqrMagnitude);
					float sqrMagnitude2 = (bones[second].solverPosition - bones[first].solverPosition).sqrMagnitude;
					float sqrMagnitude3 = (bones[third].solverPosition - bones[second].solverPosition).sqrMagnitude;
					Vector3 bendDirection = Vector3.Cross(vector, bendNormal);
					Vector3 directionToBendPoint = GetDirectionToBendPoint(vector, directionMag, bendDirection, sqrMagnitude2, sqrMagnitude3);
					Quaternion quaternion = Quaternion.FromToRotation(bones[second].solverPosition - bones[first].solverPosition, directionToBendPoint);
					if (weight < 1f)
					{
						quaternion = Quaternion.Lerp(Quaternion.identity, quaternion, weight);
					}
					RotateAroundPoint(bones, first, bones[first].solverPosition, quaternion);
					Quaternion quaternion2 = Quaternion.FromToRotation(bones[third].solverPosition - bones[second].solverPosition, targetPosition - bones[second].solverPosition);
					if (weight < 1f)
					{
						quaternion2 = Quaternion.Lerp(Quaternion.identity, quaternion2, weight);
					}
					RotateAroundPoint(bones, second, bones[second].solverPosition, quaternion2);
				}
			}

			private static Vector3 GetDirectionToBendPoint(Vector3 direction, float directionMag, Vector3 bendDirection, float sqrMag1, float sqrMag2)
			{
				float num = (directionMag * directionMag + (sqrMag1 - sqrMag2)) / 2f / directionMag;
				float y = (float)Math.Sqrt(Mathf.Clamp(sqrMag1 - num * num, 0f, float.PositiveInfinity));
				if (direction == Vector3.zero)
				{
					return Vector3.zero;
				}
				return Quaternion.LookRotation(direction, bendDirection) * new Vector3(0f, y, num);
			}

			public static void SolveFABRIK(VirtualBone[] bones, Vector3 startPosition, Vector3 targetPosition, float weight, float minNormalizedTargetDistance, int iterations, float length)
			{
				if (weight <= 0f)
				{
					return;
				}
				if (minNormalizedTargetDistance > 0f)
				{
					Vector3 vector = targetPosition - startPosition;
					float magnitude = vector.magnitude;
					targetPosition = startPosition + vector / magnitude * Mathf.Max(length * minNormalizedTargetDistance, magnitude);
				}
				for (int i = 0; i < iterations; i++)
				{
					bones[bones.Length - 1].solverPosition = Vector3.Lerp(bones[bones.Length - 1].solverPosition, targetPosition, weight);
					for (int num = bones.Length - 2; num > -1; num--)
					{
						bones[num].solverPosition = SolveFABRIKJoint(bones[num].solverPosition, bones[num + 1].solverPosition, bones[num].length);
					}
					bones[0].solverPosition = startPosition;
					for (int j = 1; j < bones.Length; j++)
					{
						bones[j].solverPosition = SolveFABRIKJoint(bones[j].solverPosition, bones[j - 1].solverPosition, bones[j - 1].length);
					}
				}
				for (int k = 0; k < bones.Length - 1; k++)
				{
					SwingRotation(bones, k, bones[k + 1].solverPosition);
				}
			}

			private static Vector3 SolveFABRIKJoint(Vector3 pos1, Vector3 pos2, float length)
			{
				return pos2 + (pos1 - pos2).normalized * length;
			}

			public static void SolveCCD(VirtualBone[] bones, Vector3 targetPosition, float weight, int iterations)
			{
				if (weight <= 0f)
				{
					return;
				}
				for (int i = 0; i < iterations; i++)
				{
					for (int num = bones.Length - 2; num > -1; num--)
					{
						Vector3 fromDirection = bones[bones.Length - 1].solverPosition - bones[num].solverPosition;
						Vector3 toDirection = targetPosition - bones[num].solverPosition;
						Quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection);
						if (weight >= 1f)
						{
							RotateBy(bones, num, quaternion);
						}
						else
						{
							RotateBy(bones, num, Quaternion.Lerp(Quaternion.identity, quaternion, weight));
						}
					}
				}
			}
		}

		private Transform[] solverTransforms = new Transform[0];

		private bool hasNeck;

		private bool hasShoulders;

		private bool hasToes;

		private Vector3[] readPositions = new Vector3[0];

		private Quaternion[] readRotations = new Quaternion[0];

		private Vector3[] solvedPositions = new Vector3[2];

		private Quaternion[] solvedRotations = new Quaternion[22];

		private Vector3 defaultPelvisLocalPosition;

		private Quaternion[] defaultLocalRotations = new Quaternion[21];

		private Vector3 rootV;

		private Vector3 rootVelocity;

		private Vector3 bodyOffset;

		[Tooltip("If true, will keep the toes planted even if head target is out of reach.")]
		public bool plantFeet = true;

		[Tooltip("The spine solver.")]
		public Spine spine = new Spine();

		[Tooltip("The left arm solver.")]
		public Arm leftArm = new Arm();

		[Tooltip("The right arm solver.")]
		public Arm rightArm = new Arm();

		[Tooltip("The left leg solver.")]
		public Leg leftLeg = new Leg();

		[Tooltip("The right leg solver.")]
		public Leg rightLeg = new Leg();

		[Tooltip("The procedural locomotion solver.")]
		public Locomotion locomotion = new Locomotion();

		private Leg[] legs = new Leg[2];

		private Arm[] arms = new Arm[2];

		private Vector3 headPosition;

		private Vector3 headDeltaPosition;

		private Vector3 raycastOriginPelvis;

		private Vector3 lastOffset;

		private Vector3 debugPos1;

		private Vector3 debugPos2;

		private Vector3 debugPos3;

		private Vector3 debugPos4;

		[HideInInspector]
		public VirtualBone rootBone { get; private set; }

		public void SetToReferences(VRIK.References references)
		{
			if (!references.isFilled)
			{
				Debug.LogError("Invalid references, one or more Transforms are missing.");
				return;
			}
			solverTransforms = references.GetTransforms();
			hasNeck = solverTransforms[4] != null;
			hasShoulders = solverTransforms[6] != null && solverTransforms[10] != null;
			hasToes = solverTransforms[17] != null && solverTransforms[21] != null;
			readPositions = new Vector3[solverTransforms.Length];
			readRotations = new Quaternion[solverTransforms.Length];
			DefaultAnimationCurves();
			GuessHandOrientations(references, true);
		}

		public void GuessHandOrientations(VRIK.References references, bool onlyIfZero)
		{
			if (!references.isFilled)
			{
				Debug.LogWarning("VRIK References are not filled in, can not guess hand orientations. Right-click on VRIK header and slect 'Guess Hand Orientations' when you have filled in the References.");
				return;
			}
			if (leftArm.wristToPalmAxis == Vector3.zero || !onlyIfZero)
			{
				leftArm.wristToPalmAxis = GuessWristToPalmAxis(references.leftHand, references.leftForearm);
			}
			if (leftArm.palmToThumbAxis == Vector3.zero || !onlyIfZero)
			{
				leftArm.palmToThumbAxis = GuessPalmToThumbAxis(references.leftHand, references.leftForearm);
			}
			if (rightArm.wristToPalmAxis == Vector3.zero || !onlyIfZero)
			{
				rightArm.wristToPalmAxis = GuessWristToPalmAxis(references.rightHand, references.rightForearm);
			}
			if (rightArm.palmToThumbAxis == Vector3.zero || !onlyIfZero)
			{
				rightArm.palmToThumbAxis = GuessPalmToThumbAxis(references.rightHand, references.rightForearm);
			}
		}

		public void DefaultAnimationCurves()
		{
			if (locomotion.stepHeight == null)
			{
				locomotion.stepHeight = new AnimationCurve();
			}
			if (locomotion.heelHeight == null)
			{
				locomotion.heelHeight = new AnimationCurve();
			}
			if (locomotion.stepHeight.keys.Length == 0)
			{
				locomotion.stepHeight.keys = GetSineKeyframes(0.03f);
			}
			if (locomotion.heelHeight.keys.Length == 0)
			{
				locomotion.heelHeight.keys = GetSineKeyframes(0.03f);
			}
		}

		public void AddPositionOffset(PositionOffset positionOffset, Vector3 value)
		{
			switch (positionOffset)
			{
			case PositionOffset.Pelvis:
				spine.pelvisPositionOffset += value;
				break;
			case PositionOffset.Chest:
				spine.chestPositionOffset += value;
				break;
			case PositionOffset.Head:
				spine.headPositionOffset += value;
				break;
			case PositionOffset.LeftHand:
				leftArm.handPositionOffset += value;
				break;
			case PositionOffset.RightHand:
				rightArm.handPositionOffset += value;
				break;
			case PositionOffset.LeftFoot:
				leftLeg.footPositionOffset += value;
				break;
			case PositionOffset.RightFoot:
				rightLeg.footPositionOffset += value;
				break;
			case PositionOffset.LeftHeel:
				leftLeg.heelPositionOffset += value;
				break;
			case PositionOffset.RightHeel:
				rightLeg.heelPositionOffset += value;
				break;
			}
		}

		public void AddRotationOffset(RotationOffset rotationOffset, Vector3 value)
		{
			AddRotationOffset(rotationOffset, Quaternion.Euler(value));
		}

		public void AddRotationOffset(RotationOffset rotationOffset, Quaternion value)
		{
			switch (rotationOffset)
			{
			case RotationOffset.Pelvis:
				spine.pelvisRotationOffset = value * spine.pelvisRotationOffset;
				break;
			case RotationOffset.Chest:
				spine.chestRotationOffset = value * spine.chestRotationOffset;
				break;
			case RotationOffset.Head:
				spine.headRotationOffset = value * spine.headRotationOffset;
				break;
			}
		}

		public void Reset()
		{
			UpdateSolverTransforms();
			Read(readPositions, readRotations, hasNeck, hasShoulders, hasToes);
			spine.faceDirection = rootBone.readRotation * Vector3.forward;
			locomotion.Reset(readPositions, readRotations);
			raycastOriginPelvis = spine.pelvis.readPosition;
		}

		public override void StoreDefaultLocalState()
		{
			defaultPelvisLocalPosition = solverTransforms[1].localPosition;
			for (int i = 1; i < solverTransforms.Length; i++)
			{
				if (solverTransforms[i] != null)
				{
					defaultLocalRotations[i - 1] = solverTransforms[i].localRotation;
				}
			}
		}

		public override void FixTransforms()
		{
			solverTransforms[1].localPosition = defaultPelvisLocalPosition;
			for (int i = 1; i < solverTransforms.Length; i++)
			{
				if (solverTransforms[i] != null)
				{
					solverTransforms[i].localRotation = defaultLocalRotations[i - 1];
				}
			}
		}

		public override Point[] GetPoints()
		{
			Debug.LogError("GetPoints() is not applicable to IKSolverVR.");
			return null;
		}

		public override Point GetPoint(Transform transform)
		{
			Debug.LogError("GetPoint is not applicable to IKSolverVR.");
			return null;
		}

		public override bool IsValid(ref string message)
		{
			if (solverTransforms == null || solverTransforms.Length == 0)
			{
				message = "Trying to initiate IKSolverVR with invalid bone references.";
				return false;
			}
			if (leftArm.wristToPalmAxis == Vector3.zero)
			{
				message = "Left arm 'Wrist To Palm Axis' needs to be set in VRIK. Please select the hand bone, set it to the axis that points from the wrist towards the palm. If the arrow points away from the palm, axis must be negative.";
				return false;
			}
			if (rightArm.wristToPalmAxis == Vector3.zero)
			{
				message = "Right arm 'Wrist To Palm Axis' needs to be set in VRIK. Please select the hand bone, set it to the axis that points from the wrist towards the palm. If the arrow points away from the palm, axis must be negative.";
				return false;
			}
			if (leftArm.palmToThumbAxis == Vector3.zero)
			{
				message = "Left arm 'Palm To Thumb Axis' needs to be set in VRIK. Please select the hand bone, set it to the axis that points from the palm towards the thumb. If the arrow points away from the thumb, axis must be negative.";
				return false;
			}
			if (rightArm.palmToThumbAxis == Vector3.zero)
			{
				message = "Right arm 'Palm To Thumb Axis' needs to be set in VRIK. Please select the hand bone, set it to the axis that points from the palm towards the thumb. If the arrow points away from the thumb, axis must be negative.";
				return false;
			}
			return true;
		}

		private Vector3 GetNormal(Transform[] transforms)
		{
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			for (int i = 0; i < transforms.Length; i++)
			{
				zero2 += transforms[i].position;
			}
			zero2 /= (float)transforms.Length;
			for (int j = 0; j < transforms.Length - 1; j++)
			{
				zero += Vector3.Cross(transforms[j].position - zero2, transforms[j + 1].position - zero2).normalized;
			}
			return zero;
		}

		private Vector3 GuessWristToPalmAxis(Transform hand, Transform forearm)
		{
			Vector3 vector = forearm.position - hand.position;
			Vector3 vector2 = AxisTools.ToVector3(AxisTools.GetAxisToDirection(hand, vector));
			if (Vector3.Dot(vector, hand.rotation * vector2) > 0f)
			{
				vector2 = -vector2;
			}
			return vector2;
		}

		private Vector3 GuessPalmToThumbAxis(Transform hand, Transform forearm)
		{
			if (hand.childCount == 0)
			{
				Debug.LogWarning("Hand " + hand.name + " does not have any fingers, VRIK can not guess the hand bone's orientation.", hand);
				return Vector3.zero;
			}
			float num = float.PositiveInfinity;
			int index = 0;
			for (int i = 0; i < hand.childCount; i++)
			{
				float num2 = Vector3.SqrMagnitude(hand.GetChild(i).position - hand.position);
				if (num2 < num)
				{
					num = num2;
					index = i;
				}
			}
			Vector3 lhs = Vector3.Cross(hand.position - forearm.position, hand.GetChild(index).position - hand.position);
			Vector3 vector = Vector3.Cross(lhs, hand.position - forearm.position);
			Vector3 vector2 = AxisTools.ToVector3(AxisTools.GetAxisToDirection(hand, vector));
			if (Vector3.Dot(vector, hand.rotation * vector2) < 0f)
			{
				vector2 = -vector2;
			}
			return vector2;
		}

		private static Keyframe[] GetSineKeyframes(float mag)
		{
			Keyframe[] array = new Keyframe[3];
			array[0].time = 0f;
			array[0].value = 0f;
			array[1].time = 0.5f;
			array[1].value = mag;
			array[2].time = 1f;
			array[2].value = 0f;
			return array;
		}

		private void UpdateSolverTransforms()
		{
			for (int i = 0; i < solverTransforms.Length; i++)
			{
				if (solverTransforms[i] != null)
				{
					readPositions[i] = solverTransforms[i].position;
					readRotations[i] = solverTransforms[i].rotation;
				}
			}
		}

		protected override void OnInitiate()
		{
			UpdateSolverTransforms();
			Read(readPositions, readRotations, hasNeck, hasShoulders, hasToes);
		}

		protected override void OnUpdate()
		{
			if (IKPositionWeight > 0f)
			{
				UpdateSolverTransforms();
				Read(readPositions, readRotations, hasNeck, hasShoulders, hasToes);
				Solve();
				Write();
				WriteTransforms();
			}
		}

		private void WriteTransforms()
		{
			for (int i = 0; i < solverTransforms.Length; i++)
			{
				if (solverTransforms[i] != null)
				{
					if (i < 2)
					{
						solverTransforms[i].position = V3Tools.Lerp(solverTransforms[i].position, GetPosition(i), IKPositionWeight);
					}
					solverTransforms[i].rotation = QuaTools.Lerp(solverTransforms[i].rotation, GetRotation(i), IKPositionWeight);
				}
			}
		}

		private void Read(Vector3[] positions, Quaternion[] rotations, bool hasNeck, bool hasShoulders, bool hasToes)
		{
			if (rootBone == null)
			{
				rootBone = new VirtualBone(positions[0], rotations[0]);
			}
			else
			{
				rootBone.Read(positions[0], rotations[0]);
			}
			spine.Read(positions, rotations, hasNeck, hasShoulders, hasToes, 0, 1);
			leftArm.Read(positions, rotations, hasNeck, hasShoulders, hasToes, 3, 6);
			rightArm.Read(positions, rotations, hasNeck, hasShoulders, hasToes, 3, 10);
			leftLeg.Read(positions, rotations, hasNeck, hasShoulders, hasToes, 1, 14);
			rightLeg.Read(positions, rotations, hasNeck, hasShoulders, hasToes, 1, 18);
			for (int i = 0; i < rotations.Length; i++)
			{
				if (i < 2)
				{
					solvedPositions[i] = positions[i];
				}
				solvedRotations[i] = rotations[i];
			}
			if (!base.initiated)
			{
				legs = new Leg[2] { leftLeg, rightLeg };
				arms = new Arm[2] { leftArm, rightArm };
				locomotion.Initiate(positions, rotations, hasToes);
				raycastOriginPelvis = spine.pelvis.readPosition;
				spine.faceDirection = readRotations[0] * Vector3.forward;
			}
		}

		private void Solve()
		{
			spine.PreSolve();
			Arm[] array = arms;
			foreach (Arm arm in array)
			{
				arm.PreSolve();
			}
			Leg[] array2 = legs;
			foreach (Leg leg in array2)
			{
				leg.PreSolve();
			}
			Arm[] array3 = arms;
			foreach (Arm arm2 in array3)
			{
				arm2.ApplyOffsets();
			}
			spine.ApplyOffsets();
			spine.Solve(rootBone, legs, arms);
			if (spine.pelvisPositionWeight > 0f && plantFeet)
			{
				Warning.Log("If VRIK 'Pelvis Position Weight' is > 0, 'Plant Feet' should be disabled to improve performance and stability.", root);
			}
			if (locomotion.weight > 0f)
			{
				Vector3 leftFootPosition = Vector3.zero;
				Vector3 rightFootPosition = Vector3.zero;
				Quaternion leftFootRotation = Quaternion.identity;
				Quaternion rightFootRotation = Quaternion.identity;
				float leftFootOffset = 0f;
				float rightFootOffset = 0f;
				float leftHeelOffset = 0f;
				float rightHeelOffset = 0f;
				locomotion.Solve(rootBone, spine, leftLeg, rightLeg, leftArm, rightArm, out leftFootPosition, out rightFootPosition, out leftFootRotation, out rightFootRotation, out leftFootOffset, out rightFootOffset, out leftHeelOffset, out rightHeelOffset);
				leftFootPosition += root.up * leftFootOffset;
				rightFootPosition += root.up * rightFootOffset;
				leftLeg.footPositionOffset += (leftFootPosition - leftLeg.lastBone.solverPosition) * IKPositionWeight * (1f - leftLeg.positionWeight) * locomotion.weight;
				rightLeg.footPositionOffset += (rightFootPosition - rightLeg.lastBone.solverPosition) * IKPositionWeight * (1f - rightLeg.positionWeight) * locomotion.weight;
				leftLeg.heelPositionOffset += root.up * leftHeelOffset * locomotion.weight;
				rightLeg.heelPositionOffset += root.up * rightHeelOffset * locomotion.weight;
				Quaternion b = QuaTools.FromToRotation(leftLeg.lastBone.solverRotation, leftFootRotation);
				Quaternion b2 = QuaTools.FromToRotation(rightLeg.lastBone.solverRotation, rightFootRotation);
				b = Quaternion.Lerp(Quaternion.identity, b, IKPositionWeight * (1f - leftLeg.rotationWeight) * locomotion.weight);
				b2 = Quaternion.Lerp(Quaternion.identity, b2, IKPositionWeight * (1f - rightLeg.rotationWeight) * locomotion.weight);
				leftLeg.footRotationOffset = b * leftLeg.footRotationOffset;
				rightLeg.footRotationOffset = b2 * rightLeg.footRotationOffset;
				Vector3 vector = Vector3.Lerp(leftLeg.position + leftLeg.footPositionOffset, rightLeg.position + rightLeg.footPositionOffset, 0.5f);
				vector.y = rootBone.solverPosition.y;
				rootVelocity += (vector - rootBone.solverPosition) * Time.deltaTime * 10f;
				rootBone.solverPosition += rootVelocity * Time.deltaTime * 2f * locomotion.weight;
				rootBone.solverPosition = Vector3.Lerp(rootBone.solverPosition, vector, Time.deltaTime * locomotion.rootSpeed * locomotion.weight);
				float num = leftFootOffset + rightFootOffset;
				bodyOffset = Vector3.Lerp(bodyOffset, root.up * num, Time.deltaTime * 3f);
				bodyOffset = Vector3.Lerp(Vector3.zero, bodyOffset, locomotion.weight);
			}
			Leg[] array4 = legs;
			foreach (Leg leg2 in array4)
			{
				leg2.ApplyOffsets();
			}
			if (!plantFeet)
			{
				spine.InverseTranslateToHead(legs, false, false, bodyOffset, 1f);
				Leg[] array5 = legs;
				foreach (Leg leg3 in array5)
				{
					leg3.TranslateRoot(spine.pelvis.solverPosition, spine.pelvis.solverRotation);
				}
				Leg[] array6 = legs;
				foreach (Leg leg4 in array6)
				{
					leg4.Solve();
				}
			}
			else
			{
				for (int num2 = 0; num2 < 2; num2++)
				{
					spine.InverseTranslateToHead(legs, true, num2 == 0, bodyOffset, 1f);
					Leg[] array7 = legs;
					foreach (Leg leg5 in array7)
					{
						leg5.TranslateRoot(spine.pelvis.solverPosition, spine.pelvis.solverRotation);
					}
					Leg[] array8 = legs;
					foreach (Leg leg6 in array8)
					{
						leg6.Solve();
					}
				}
			}
			for (int num5 = 0; num5 < arms.Length; num5++)
			{
				arms[num5].TranslateRoot(spine.chest.solverPosition, spine.chest.solverRotation);
				arms[num5].Solve(num5 == 0);
			}
			spine.ResetOffsets();
			Leg[] array9 = legs;
			foreach (Leg leg7 in array9)
			{
				leg7.ResetOffsets();
			}
			Arm[] array10 = arms;
			foreach (Arm arm3 in array10)
			{
				arm3.ResetOffsets();
			}
			spine.pelvisPositionOffset += GetPelvisOffset();
			spine.chestPositionOffset += spine.pelvisPositionOffset;
			Write();
		}

		private Vector3 GetPosition(int index)
		{
			if (index >= 2)
			{
				Debug.LogError("Can only get root and pelvis positions from IKSolverVR. GetPosition index out of range.");
			}
			return solvedPositions[index];
		}

		private Quaternion GetRotation(int index)
		{
			return solvedRotations[index];
		}

		private void Write()
		{
			solvedPositions[0] = rootBone.solverPosition;
			solvedRotations[0] = rootBone.solverRotation;
			spine.Write(ref solvedPositions, ref solvedRotations);
			Leg[] array = legs;
			foreach (Leg leg in array)
			{
				leg.Write(ref solvedPositions, ref solvedRotations);
			}
			Arm[] array2 = arms;
			foreach (Arm arm in array2)
			{
				arm.Write(ref solvedPositions, ref solvedRotations);
			}
		}

		private Vector3 GetPelvisOffset()
		{
			if (locomotion.weight <= 0f)
			{
				return Vector3.zero;
			}
			if ((int)locomotion.blockingLayers == -1)
			{
				return Vector3.zero;
			}
			Vector3 vector = raycastOriginPelvis;
			vector.y = spine.pelvis.solverPosition.y;
			Vector3 vector2 = spine.pelvis.readPosition;
			vector2.y = spine.pelvis.solverPosition.y;
			Vector3 direction = vector2 - vector;
			RaycastHit hitInfo;
			if (locomotion.raycastRadius <= 0f)
			{
				if (Physics.Raycast(vector, direction, out hitInfo, direction.magnitude * 1.1f, locomotion.blockingLayers))
				{
					vector2 = hitInfo.point;
				}
			}
			else if (Physics.SphereCast(vector, locomotion.raycastRadius * 1.1f, direction, out hitInfo, direction.magnitude, locomotion.blockingLayers))
			{
				vector2 = vector + direction.normalized * hitInfo.distance / 1.1f;
			}
			Vector3 vector3 = spine.pelvis.solverPosition;
			direction = vector3 - vector2;
			if (locomotion.raycastRadius <= 0f)
			{
				if (Physics.Raycast(vector2, direction, out hitInfo, direction.magnitude, locomotion.blockingLayers))
				{
					vector3 = hitInfo.point;
				}
			}
			else if (Physics.SphereCast(vector2, locomotion.raycastRadius, direction, out hitInfo, direction.magnitude, locomotion.blockingLayers))
			{
				vector3 = vector2 + direction.normalized * hitInfo.distance;
			}
			lastOffset = Vector3.Lerp(lastOffset, Vector3.zero, Time.deltaTime * 3f);
			vector3 += Vector3.ClampMagnitude(lastOffset, 0.75f);
			vector3.y = spine.pelvis.solverPosition.y;
			lastOffset = Vector3.Lerp(lastOffset, vector3 - spine.pelvis.solverPosition, Time.deltaTime * 15f);
			return lastOffset;
		}
	}
}
