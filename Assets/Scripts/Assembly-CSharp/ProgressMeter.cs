using System;
using FluffyUnderware.Curvy;
using UnityEngine;

public class ProgressMeter : MonoBehaviour
{
	private CurvySpline spline;

	public Transform player;

	public CameraControl camControl;

	public float progress;

	private float lastSlowProgress;

	private float lastFastProgress;

	private float newDeltaFastProgress;

	private float newDeltaSlowProgress;

	private float averageFastDelta;

	private float averageSlowDelta;

	private int secondsToWatch = 60;

	private int framesToWatch = 120;

	private float secs;

	private float[] deltaSlowProgress = new float[0];

	private float[] deltaFastProgress = new float[0];

	private float lossThreshold = -0.24f;

	private float progressThreshold = 0.15f;

	private bool paused;

	public Narrator narrator;

	public DateTimeOffset LastSeriousLossTimestamp;

	private void Start()
	{
		paused = false;
		spline = GetComponent<CurvySpline>();
		deltaFastProgress = new float[framesToWatch];
		deltaSlowProgress = new float[secondsToWatch];
		newDeltaFastProgress = 0f;
		newDeltaSlowProgress = 0f;
		averageFastDelta = 0f;
		averageSlowDelta = 0f;
		secs = 0f;
		for (int i = 0; i < secondsToWatch; i++)
		{
			deltaSlowProgress[i] = progressThreshold * (float)secondsToWatch;
		}
		for (int j = 0; j < framesToWatch; j++)
		{
			deltaFastProgress[j] = 0f;
		}
		lastSlowProgress = 0f;
		lastFastProgress = 0f;
		secs = 1f;
	}

	private void LateUpdate()
	{
		if (paused)
		{
			return;
		}
		secs += Time.deltaTime;
		progress = spline.TFToDistance(camControl.lastTFValue);
		narrator.UpdateDistance(progress);
		newDeltaFastProgress = progress - lastFastProgress;
		Array.Copy(deltaFastProgress, 0, deltaFastProgress, 1, deltaFastProgress.Length - 2);
		deltaFastProgress[0] = newDeltaFastProgress;
		averageFastDelta = 0f;
		float[] array = deltaFastProgress;
		foreach (float num in array)
		{
			averageFastDelta += num;
		}
		averageFastDelta /= framesToWatch;
		lastFastProgress = progress;
		if (secs >= 1f)
		{
			secs = 0f;
			newDeltaSlowProgress = Mathf.Max(-100f, progress - lastSlowProgress);
			Array.Copy(deltaSlowProgress, 0, deltaSlowProgress, 1, deltaSlowProgress.Length - 2);
			deltaSlowProgress[0] = newDeltaSlowProgress;
			averageSlowDelta = 0f;
			float[] array2 = deltaSlowProgress;
			foreach (float num2 in array2)
			{
				averageSlowDelta += num2;
			}
			averageSlowDelta /= secondsToWatch;
			lastSlowProgress = progress;
			if (averageFastDelta > lossThreshold && averageSlowDelta < progressThreshold)
			{
				narrator.SlowProgress();
				ResetSlowProgress();
			}
		}
		if (averageFastDelta < lossThreshold)
		{
			LastSeriousLossTimestamp = DateTimeOffset.UtcNow;
			narrator.FastRetreat();
			Array.Clear(deltaFastProgress, 0, deltaFastProgress.Length);
			ResetSlowProgress();
		}
	}

	public void Pause(bool shouldPause)
	{
		paused = shouldPause;
	}

	private void ResetSlowProgress()
	{
		for (int i = 0; i < deltaSlowProgress.Length; i++)
		{
			deltaSlowProgress[i] = progressThreshold * (float)secondsToWatch;
		}
	}
}
