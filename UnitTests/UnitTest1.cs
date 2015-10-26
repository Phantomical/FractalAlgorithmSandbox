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
		static int Counter = 0;

		static float CounterMethod(float x, float y, float z)
		{
			Counter++;
			return 0;
		}
		static float ThrowMethod(float x, float y, float z)
		{
			throw new DummyException();
		}

		[TestMethod]
		public void PlaneCallsMethod()
		{
			try
			{
				new Plane(32f, 32, ThrowMethod);
			}
			catch(DummyException)
			{
				//Test Succeeded
				Assert.IsTrue(true);
				return;
			}

			//The method was not called
			Assert.Fail("Plane did not call the method");
		}

		[TestMethod]
		public void TestPlane()
		{
			const int vpside = 32;

			new Plane(1f, vpside, CounterMethod);

			Assert.AreEqual(Counter, vpside * vpside, "Noise method not called for all vertices.");

			Counter = 0;
		}
	}
}
