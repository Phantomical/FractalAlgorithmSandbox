using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using FractalAlgorithmTest;

namespace UnitTests
{
	[Serializable]
	public class DummyException : Exception
	{
		public DummyException() { }
		public DummyException(string message) : base(message) { }
		public DummyException(string message, Exception inner) : base(message, inner) { }
		protected DummyException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	[TestClass]
	public class UnitTest
	{
		static volatile int Counter = 0;

		static float CounterMethod(float x, float y, float z)
		{
			Counter++;
			return 0;
		}
	}
}
