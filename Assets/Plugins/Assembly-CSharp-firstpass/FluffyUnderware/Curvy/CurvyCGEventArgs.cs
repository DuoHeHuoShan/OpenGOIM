using System;
using FluffyUnderware.Curvy.Generator;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class CurvyCGEventArgs : EventArgs
	{
		public MonoBehaviour Sender;

		public readonly CurvyGenerator Generator;

		public readonly CGModule Module;

		public CurvyCGEventArgs(CGModule module)
		{
			Sender = module;
			Generator = module.Generator;
			Module = module;
		}

		public CurvyCGEventArgs(CurvyGenerator generator, CGModule module)
		{
			Sender = generator;
			Generator = generator;
			Module = module;
		}
	}
}
