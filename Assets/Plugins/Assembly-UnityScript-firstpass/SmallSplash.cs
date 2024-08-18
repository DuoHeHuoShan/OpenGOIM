using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Boo.Lang;
using UnityEngine;

[Serializable]
public class SmallSplash : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024TriggerSplash_00244 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal SmallSplash _0024self__00245;

			public _0024(SmallSplash self_)
			{
				_0024self__00245 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					_0024self__00245.splashFlag = 1;
					_0024self__00245.smallSplash.SetActive(true);
					result = (Yield(2, new WaitForSeconds(2.1f)) ? 1 : 0);
					break;
				case 2:
					_0024self__00245.smallSplash.SetActive(false);
					_0024self__00245.splashFlag = 0;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal SmallSplash _0024self__00246;

		public _0024TriggerSplash_00244(SmallSplash self_)
		{
			_0024self__00246 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024self__00246);
		}
	}

	public GameObject smallSplash;

	private int splashFlag;

	public virtual void Start()
	{
		smallSplash.SetActive(false);
	}

	public virtual void Update()
	{
		if (Input.GetButtonDown("Fire2") && splashFlag == 0)
		{
			StartCoroutine(TriggerSplash());
		}
	}

	public virtual IEnumerator TriggerSplash()
	{
		return new _0024TriggerSplash_00244(this).GetEnumerator();
	}

	public virtual void Main()
	{
	}
}
