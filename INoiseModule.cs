using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalAlgorithmTest
{
	public interface INoiseModule
	{
		float GetValue(float x, float y, float z);
	}
}
