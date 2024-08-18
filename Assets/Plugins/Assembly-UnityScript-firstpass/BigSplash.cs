using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Boo.Lang;
using UnityEngine;

[Serializable]
public class BigSplash : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024TriggerSplash_00241 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal BigSplash _0024self__00242;

			public _0024(BigSplash self_)
			{
				_0024self__00242 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					_0024self__00242.splashFlag = 1;
					_0024self__00242.bigSplash.SetActive(true);
					result = (Yield(2, new WaitForSeconds(3.5f)) ? 1 : 0);
					break;
				case 2:
					_0024self__00242.bigSplash.SetActive(false);
					_0024self__00242.splashFlag = 0;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal BigSplash _0024self__00243;

		public _0024TriggerSplash_00241(BigSplash self_)
		{
			_0024self__00243 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024self__00243);
		}
	}

	public GameObject bigSplash;

	private int splashFlag;

	public virtual void Start()
	{
		bigSplash.SetActive(false);
	}

	public virtual void Update()
	{
		if (Input.GetButtonDown("Fire1") && splashFlag == 0)
		{
			StartCoroutine(TriggerSplash());
		}
	}

	public virtual IEnumerator TriggerSplash()
	{
		return new _0024TriggerSplash_00241(this).GetEnumerator();
	}

	public virtual void Main()
	{
	}
}
