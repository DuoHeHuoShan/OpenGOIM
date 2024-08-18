using System;
using UnityEngine;

[Serializable]
public class SaveState
{
	public float hingePos { get; set; }

	public float hingeVel { get; set; }

	public float sliderPos { get; set; }

	public float sliderVel { get; set; }

	public Vector3 camPos { get; set; }

	public Vector3 playerPos { get; set; }

	public Quaternion playerRot { get; set; }

	public Vector2[] rbLinearVelocities { get; set; }

	public float[] rbAngularVelocities { get; set; }

	public Vector2[] rbPositions { get; set; }

	public float[] rbAngles { get; set; }

	public int saveNum { get; set; }

	public int currentCondolence { get; set; }

	public int currentObservation { get; set; }

	public string keyDialogDone { get; set; }

	public string condolenceDialogDone { get; set; }

	public string observationDialogDone { get; set; }

	public float timePlayed { get; set; }

	public string version { get; set; }

	public bool speedrun { get; set; }

	public SaveState()
	{
		hingePos = 0f;
		hingeVel = 0f;
		sliderPos = 0f;
		sliderVel = 0f;
		camPos = new Vector3(-43.57285f, -1.2893757f, -20f);
		playerPos = Vector3.zero;
		playerRot = Quaternion.identity;
		rbLinearVelocities = new Vector2[1];
		rbPositions = new Vector2[1];
		rbAngularVelocities = new float[1];
		rbAngles = new float[1];
		saveNum = -1;
		currentCondolence = 0;
		currentObservation = 0;
		keyDialogDone = string.Empty;
		condolenceDialogDone = string.Empty;
		observationDialogDone = string.Empty;
		timePlayed = 0f;
		version = string.Empty;
		speedrun = false;
	}
}
