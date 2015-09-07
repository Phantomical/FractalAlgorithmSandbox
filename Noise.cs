using OpenTK;
using System;

namespace FractalAlgorithmTest
{
	[Obsolete("This is just a storage class for various interpolators. They will probably be moved in the future.")]
	private static class Interpolators
	{
		/// <summary>
		/// Linear Interpolation
		/// </summary>
		/// <param name="v0"></param>
		/// <param name="v1"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		private static float lerp(float v0, float v1, float t)
		{
			return v0 + (v1 - v0) * t;
		}
		/// <summary>
		/// Cosine interpolation
		/// </summary>
		/// <param name="v0"></param>
		/// <param name="v1"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		private static float cosinerp(float v0, float v1, float t)
		{
			float ft = t * (float)Math.PI;
			float f = (1 - (float)Math.Cos(ft)) * 0.5f;
			return v0 * (1 - f) + v1 * f;
		}
		//Cubic interpolation
		private static float cuberp(float y0, float y1, float y2, float y3, float mu)
		{
			float mu2 = mu * mu;
			float a0 = y3 - y2 - y0 + y1;
			float a1 = y0 - y1 - a0;
			float a2 = y2 - y0;
			return a0 * mu * mu2 + a1 * mu2 + a2 * mu + y1;
		}
		//Hermite interpolation
		private static float hermiterp(float v0, float v1, float v2, float v3, float t, float tension, float bias)
		{
			var t2 = t * t;
			var t3 = t2 * t;
			var m0 = (v1 - v0) * (1 + bias) * (1 - tension) * 0.5f + (v2 - v1) * (1 - bias) * (1 - tension) * 0.5f;
			var m1 = (v2 - v1) * (1 + bias) * (1 - tension) * 0.5f + (v3 - v2) * (1 - bias) * (1 - tension) * 0.5f;

			var a0 = 2 * t3 - 3 * t2 + 1;
			var a1 = t3 - 2 * t2 + t;
			var a2 = t3 - t2;
			var a3 = -2 * t3 + 3 * t2;

			return a0 * v1 + a1 * m0 + a2 * m1 + a3 * v2;
		}


	}

	namespace Noise
	{
		/// <summary>
		/// Class for predefined noise module combinations.
		/// </summary>
		public static class NoiseFunctions
		{
			public static INoiseModule GetRidgedMultifractal()
			{
				return GetRidgedMultifractal(8, 0.75f, 2f, 1.9f);
			}
			public static INoiseModule GetRidgedMultifractal(int Octaves, float Persistence, float Frequency, float Lacunarity)
			{
				return new Modifier.RidgedFractal(new PerlinNoise(), Octaves, Persistence, Frequency, Lacunarity);
			}

			public static INoiseModule GetBillowNoise()
			{
				return GetBillowNoise(8, 0.75f, 2f, 1.9f);
			}
			public static INoiseModule GetBillowNoise(int Octaves, float Persistence, float Frequency, float Lacunarity)
			{
				return new Modifier.BillowFractal(new PerlinNoise(), Octaves, Persistence, Frequency, Lacunarity);
			}
		}

		public class WorleyNoise : INoiseModule
		{
			public enum DistanceFunc
			{
				Euclidian,
				Manhattan,
				Chebyshev,
				Minkowski
			}
			public enum CombinerFunc
			{
				/// <summary>
				/// value 0
				/// </summary>
				Function1,
				/// <summary>
				/// value 1 - value 0
				/// </summary>
				Function2,
				/// <summary>
				/// value 2 - value 0
				/// </summary>
				Function3
			}

			/// <summary>
			/// Order if using MinkowskiDistance
			/// </summary>
			public int Order = 3;
			public DistanceFunc DistanceFunction = DistanceFunc.Euclidian;
			public CombinerFunc CombinerFunction = CombinerFunc.Function1;
			public int Seed = 3221;

			const int OFFSET_BASIS = unchecked((int)2166136261);
			const int FNV_PRIME = 16777619;

			private float CombinerFunction1(float[] ar)
			{
				return ar[0];
			}
			private float CombinerFunction2(float[] ar)
			{
				return ar[1] - ar[0];
			}
			private float CombinerFunction3(float[] ar)
			{
				return ar[2] - ar[0];
			}

			private float EuclidianDistanceFunc(Vector3 p1, Vector3 p2)
			{
				return (p1 - p2).Length;
			}
			private float ManhattanDistanceFunc(Vector3 p1, Vector3 p2)
			{
				return (float)(Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y) + Math.Abs(p1.Z - p2.Z));
			}
			private float ChebyshevDistanceFunc(Vector3 p1, Vector3 p2)
			{
				Vector3 diff = p1 - p2;

				return (float)Math.Max(Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y)), Math.Abs(diff.Z));
			}
			public static float MinkowskiDistanceFunc(Vector3 p1, Vector3 p2, int order)
			{
				return (float)Math.Pow(Math.Pow(Math.Abs(p1.X - p2.X), order) + Math.Pow(Math.Abs(p1.Y - p2.Y), order) + Math.Pow(Math.Abs(p1.Z - p2.Z), order), (1 / order));
			}

			private float Distance(Vector3 p1, Vector3 p2)
			{
				switch(DistanceFunction)
				{
					case DistanceFunc.Euclidian:
						return EuclidianDistanceFunc(p1, p2);
					case DistanceFunc.Manhattan:
						return ManhattanDistanceFunc(p1, p2);
					case DistanceFunc.Chebyshev:
						return ChebyshevDistanceFunc(p1, p2);
					case DistanceFunc.Minkowski:
					default:
						return MinkowskiDistanceFunc(p1, p2, Order);
				}
			}
			private float Combiner(float[] ar)
			{
				switch(CombinerFunction)
				{
					case CombinerFunc.Function1:
						return CombinerFunction1(ar);
					case CombinerFunc.Function2:
						return CombinerFunction2(ar);
					case CombinerFunc.Function3:
						return CombinerFunction3(ar);
					default:
						return CombinerFunction1(ar);
				}
			}

			long probLookup(long value)
			{
				value = value & 0xffffffff;
				if (value < 393325350) return 1 & 0xffffffff;
				if (value < 1022645910) return 2 & 0xffffffff;
				if (value < 1861739990) return 3 & 0xffffffff;
				if (value < 2700834071) return 4 & 0xffffffff;
				if (value < 3372109335) return 5 & 0xffffffff;
				if (value < 3819626178) return 6 & 0xffffffff;
				if (value < 4075350088) return 7 & 0xffffffff;
				if (value < 4203212043) return 8 & 0xffffffff;
				return 9 & 0xffffffff;
			}
			void insert(float[] ar, float value)
			{
				float temp;

				int arraySize = ar.Length;

				for (int i = arraySize - 1; i >= 0; i--)
				{
					if (value > ar[i]) break;
					temp = ar[i];
					ar[i] = value;
					if (i + 1 < arraySize) ar[i + 1] = temp;
				}
			}
			long lcgRandom(long lastValue)
			{
				return (((1103515245 & 0xffffffff) * lastValue + (12345 & 0xffffffff)) % 0x100000000) & 0xffffffff;
			}
			long hash(long i, long j, long k)
			{
				return ((((((OFFSET_BASIS ^ (i & 0xffffffff)) * FNV_PRIME) ^ (j & 0xffffffff)) * FNV_PRIME)
					^ (k & 0xffffffff)) * FNV_PRIME) & 0xffffffff;
			}
			float noise(Vector3 input)
			{
				long lastRandom;
				long numberFeaturePoints;
				Vector3 randomDiff = Vector3.Zero;
				Vector3 featurePoint = Vector3.Zero;
				int cubeX, cubeY, cubeZ;

				int distanceArraySize = 3;
				float[] DistanceArray = new float[3];

				for (int i = 0; i < distanceArraySize; i++)
				{
					DistanceArray[i] = 6666;
				}

				int evalCubeX = (int)(Math.Floor(input.X));
				int evalCubeY = (int)(Math.Floor(input.Y));
				int evalCubeZ = (int)(Math.Floor(input.Z));

				for (int i = -1; i < 2; ++i)
					for (int j = -1; j < 2; ++j)
						for (int k = -1; k < 2; ++k)
						{
							cubeX = evalCubeX + i;
							cubeY = evalCubeY + j;
							cubeZ = evalCubeZ + k;

							//2. Generate a reproducible random number generator for the cube
							lastRandom = lcgRandom(hash((cubeX + Seed) & 0xffffffff, (cubeY) & 0xffffffff, (cubeZ) & 0xffffffff));
							//3. Determine how many feature points are in the cube
							numberFeaturePoints = probLookup(lastRandom);
							//4. Randomly place the feature points in the cube
							for (int l = 0; l < numberFeaturePoints; ++l)
							{
								lastRandom = lcgRandom(lastRandom);
								randomDiff.X = (float)lastRandom / (float)0x100000000;

								lastRandom = lcgRandom(lastRandom);
								randomDiff.Y = (float)lastRandom / (float)0x100000000;

								lastRandom = lcgRandom(lastRandom);
								randomDiff.Z = (float)lastRandom / (float)0x100000000;

								featurePoint.X = randomDiff.X + cubeX;
								featurePoint.Y = randomDiff.Y + cubeY;
								featurePoint.Z = randomDiff.Z + cubeZ;


								//5. Find the feature point closest to the evaluation point. 
								//This is done by inserting the distances to the feature points into a sorted list
								float v = Distance(input, featurePoint);
								insert(DistanceArray, v);
							}
							//6. Check the neighboring cubes to ensure their are no closer evaluation points.
							// This is done by repeating steps 1 through 5 above for each neighboring cube
						}

				float color = CombinerFunction1(DistanceArray);
				if (color < 0) color = 0;
				if (color > 1) color = 1;

				return color;
			}

			public float GetValue(float x, float y, float z)
			{
				return noise(new Vector3(x, y, z));
			}

			public WorleyNoise()
			{

			}
			public WorleyNoise(int Seed)
			{
				this.Seed = Seed;
			}
			public WorleyNoise(int Seed, DistanceFunc DFunc, CombinerFunc CFunc) :
				this(Seed)
			{
				DistanceFunction = DFunc;
				CombinerFunction = CFunc;
			}
		}
		public class PerlinNoise : INoiseModule
		{
			private static float Lerp(float v0, float v1, float t)
			{
				return v0 + (v1 - v0) * t;
			}

			private static readonly int[] permutation = {151, 160, 137, 91, 90, 15,
					131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23,
					190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
					88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
					77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244,
					102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
					135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
					5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
					223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
					129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228,
					251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
					49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254,
					138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
				};
			private static int[] p = new int[512];

			float noise(float x, float y, float z)
			{
				var X = (int)Math.Floor(x) & 255;                  // FIND UNIT CUBE THAT
				var Y = (int)Math.Floor(y) & 255;                  // CONTAINS POINT.
				var Z = (int)Math.Floor(z) & 255;
				x -= (int)Math.Floor(x);                                     // FIND RELATIVE X,Y,Z
				y -= (int)Math.Floor(y);                                     // OF POINT IN CUBE.
				z -= (int)Math.Floor(z);
				var u = fade(x);                                        // COMPUTE FADE CURVES
				var v = fade(y);                                        // FOR EACH OF X,Y,Z.
				var w = fade(z);

				var A = p[X] + Y;
				var AA = p[A] + Z;
				var AB = p[A + 1] + Z;                                  // HASH COORDINATES OF
				var B = p[X + 1] + Y;
				var BA = p[B] + Z;
				var BB = p[B + 1] + Z;                                  // THE 8 CUBE CORNERS,

				return Lerp(Lerp(Lerp(grad(p[AA], x, y, z),      // AND ADD
					grad(p[BA], x - 1, y, z), u),                // BLENDED
					Lerp(grad(p[AB], x, y - 1, z),               // RESULTS
					grad(p[BB], x - 1, y - 1, z), u), v),        // FROM  8
					Lerp(Lerp(grad(p[AA + 1], x, y, z - 1),      // CORNERS
					grad(p[BA + 1], x - 1, y, z - 1), u),        // OF CUBE
					Lerp(grad(p[AB + 1], x, y - 1, z - 1), 
					grad(p[BB + 1], x - 1, y - 1, z - 1), u), v), w);
			}

			float fade(float t) { return t * t * t * (t * (t * 6 - 15) + 10); }

			float grad(int hash, float x, float y, float z)
			{
				var h = hash & 15;                              // CONVERT LO 4 BITS OF HASH CODE
				var u = h < 8 ? x : y;                         // INTO 12 GRADIENT DIRECTIONS.
				var v = h < 4 ? y : h == 12 || h == 14 ? x : z;
				return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
			}

			public float GetValue(float x, float y, float z)
			{
				return noise(x, y, z);
			}

			static PerlinNoise()
			{
				for (var i = 0; i < 256; i++) p[256 + i] = p[i] = permutation[i];
			}
		}
		public class Constant : INoiseModule
		{
			public float Value;

			public Constant(float v)
			{
				Value = v;
			}

			public float GetValue(float x, float y, float z)
			{
				return Value;
			}
		}
		public class Function : INoiseModule
		{
			public delegate float NoiseFunc(float x, float y, float z);

			public NoiseFunc Func;

			public Function(NoiseFunc Func)
			{
				this.Func = Func;
			}

			public float GetValue(float x, float y, float z)
			{
				return Func(x, y, z);
			}
		}
	}
	namespace Modifier
	{
		public class RidgedFractal : INoiseModule
		{
			private INoiseModule Source;

			public int Octaves = 1;
			public float Persistence = 0.75f;
			public float Frequency = 2.0f;
			public float Lacunarity = 1.9f;

			public RidgedFractal(INoiseModule SourceModule)
			{
				Source = SourceModule;
			}
			public RidgedFractal(INoiseModule Src, int octaves, float persistence, float frequency, float lacunarity) :
				this(Src)
			{
				Octaves = octaves;
				Persistence = persistence;
				Frequency = frequency;
				Lacunarity = lacunarity;
			}
			
			public float GetValue(float x, float y, float z)
			{
				float total = 0;
				float maxAmplitude = 0;
				float amplitude = 1;
				float frequency = Frequency;

				for(int i = 0; i < Octaves; i++)
				{
					//Get the noise sample
					total += ((1 - (float)Math.Abs(Source.GetValue(x * frequency, y * frequency, z * frequency))) * 2f - 1f) * amplitude;

					frequency *= Lacunarity;
					maxAmplitude += amplitude;
					amplitude *= Persistence;
				}

				return total / maxAmplitude;
			}
		}
		public class BillowFractal : INoiseModule
		{
			private INoiseModule Source;

			public int Octaves = 1;
			public float Persistence = 0.75f;
			public float Frequency = 2.0f;
			public float Lacunarity = 1.9f;

			public BillowFractal(INoiseModule SourceModule)
			{
				Source = SourceModule;
			}
			public BillowFractal(INoiseModule Src, int octaves, float persistence, float frequency, float lacunarity) :
				this(Src)
			{
				Octaves = octaves;
				Persistence = persistence;
				Frequency = frequency;
				Lacunarity = lacunarity;
			}
			
			public float GetValue(float x, float y, float z)
			{
				float total = 0;
				float maxAmplitude = 0;
				float amplitude = 1;
				float frequency = Frequency;

				for(int i = 0; i < Octaves; i++)
				{
					//Get the noise sample
					total += (((float)Math.Abs(Source.GetValue(x * frequency, y * frequency, z * frequency))) * 2f - 1f) * amplitude;

					frequency *= Lacunarity;
					maxAmplitude += amplitude;
					amplitude *= Persistence;
				}

				return total / maxAmplitude;
			}
		}
		public class Fractal : INoiseModule
		{
			public INoiseModule Source;

			public int Octaves = 1;
			public float Persistence = 0.75f;
			public float Frequency = 2.0f;
			public float Lacunarity = 2;

			public Fractal(INoiseModule SourceModule)
			{
				Source = SourceModule;
			}
			public Fractal(INoiseModule Src, int octaves, float persistence, float frequency, float lacunarity) :
				this(Src)
			{
				Octaves = octaves;
				Persistence = persistence;
				Frequency = frequency;
				Lacunarity = lacunarity;
			}
			
			public float GetValue(float x, float y, float z)
			{
				float total = 0;
				float maxAmplitude = 0;
				float amplitude = 1;
				float frequency = Frequency;

				for(int i = 0; i < Octaves; i++)
				{
					//Get the noise sample
					total += Source.GetValue(x * frequency, y * frequency, z * frequency) * amplitude;

					frequency *= Lacunarity;
					maxAmplitude += amplitude;
					amplitude *= Persistence;
				}

				return total / maxAmplitude;
			}
		}
		public class Abs : INoiseModule
		{
			INoiseModule Source;

			public Abs(INoiseModule Src)
			{
				Source = Src;
			}

			public float GetValue(float x, float y, float z)
			{
				return (float)Math.Abs(Source.GetValue(x, y, z));
			}
		}
		public class Sin : INoiseModule
		{
			INoiseModule Source;

			public Sin(INoiseModule Src)
			{
				Source = Src;
			}

			public float GetValue(float x, float y, float z)
			{
				return (float)Math.Sin(Source.GetValue(x, y, z));
			}
		}
		public class Cos : INoiseModule
		{
			INoiseModule Source;

			public Cos(INoiseModule Src)
			{
				Source = Src;
			}

			public float GetValue(float x, float y, float z)
			{
				return (float)Math.Cos(Source.GetValue(x, y, z));
			}
		}
		public class Tan : INoiseModule
		{
			INoiseModule Source;

			public Tan(INoiseModule Src)
			{
				Source = Src;
			}

			public float GetValue(float x, float y, float z)
			{
				return (float)Math.Tan(Source.GetValue(x, y, z));
			}
		}
		public class Pow : INoiseModule
		{
			public INoiseModule Module1;
			public INoiseModule Module2;

			public Pow(INoiseModule m1, INoiseModule m2)
			{
				Module1 = m1;
				Module2 = m2;
			}

			public float GetValue(float x, float y, float z)
			{
				return (float)Math.Pow(Module1.GetValue(x, y, z), Module2.GetValue(x, y, z));
			}
		}
		public class Add : INoiseModule
		{
			public INoiseModule Module1;
			public INoiseModule Module2;

			public Add(INoiseModule m1, INoiseModule m2)
			{
				Module1 = m1;
				Module2 = m2;
			}

			public float GetValue(float x, float y, float z)
			{
				return Module1.GetValue(x, y, z) + Module2.GetValue(x, y, z);
			}
		}
		public class Subtract : INoiseModule
		{
			public INoiseModule Module1;
			public INoiseModule Module2;

			public Subtract(INoiseModule m1, INoiseModule m2)
			{
				Module1 = m1;
				Module2 = m2;
			}

			public float GetValue(float x, float y, float z)
			{
				return Module1.GetValue(x, y, z) - Module2.GetValue(x, y, z);
			}
		}
		public class Multiply : INoiseModule
		{
			public INoiseModule Module1;
			public INoiseModule Module2;

			public Multiply(INoiseModule m1, INoiseModule m2)
			{
				Module1 = m1;
				Module2 = m2;
			}

			public float GetValue(float x, float y, float z)
			{
				return Module1.GetValue(x, y, z) * Module2.GetValue(x, y, z);
			}
		}
		public class Divide : INoiseModule
		{
			public INoiseModule Module1;
			public INoiseModule Module2;

			public Divide(INoiseModule m1, INoiseModule m2)
			{
				Module1 = m1;
				Module2 = m2;
			}

			public float GetValue(float x, float y, float z)
			{
				return Module1.GetValue(x, y, z) / Module2.GetValue(x, y, z);
			}
		}
		public class ScaleCoords : INoiseModule
		{
			/// <summary>
			/// This module is the module that accepts the scaled coordinates.
			/// </summary>
			public INoiseModule Module1;
			/// <summary>
			/// The output of this module is the amount the distance is scaled by.
			/// </summary>
			public INoiseModule Module2;

			public ScaleCoords(INoiseModule m1, INoiseModule m2)
			{
				Module1 = m1;
				Module2 = m2;
			}

			public float GetValue(float x, float y, float z)
			{
				float v1 = Module2.GetValue(x, y, z);
				return Module1.GetValue(x * v1, y * v1, z * v1);
			}
		}
	}
}
