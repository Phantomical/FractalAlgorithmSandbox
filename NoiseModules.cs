
namespace FractalAlgorithmTest
{
	class NoiseModules
	{
		public static INoiseModule GetMountainModule(int Seed)
		{
			return new Modifier.ScaleCoords(
				new Modifier.Add(
					new Modifier.RidgedFractal(new Noise.PerlinNoise(), 16, 0.52f, 2, 2f),
					new Modifier.Multiply(
						new Noise.WorleyNoise(Seed, Noise.WorleyNoise.DistanceFunc.Manhattan, Noise.WorleyNoise.CombinerFunc.Function1),
						new Noise.Constant(5))),
				new Noise.Constant(0.1f));
		}

		public static INoiseModule PlainsModule
		{
			get
			{
				return new Modifier.Multiply(
					new Modifier.ScaleCoords(
						new Modifier.Fractal(
							new Noise.PerlinNoise(), 16, 0.5f, 2, 2),
						new Noise.Constant(0.1f)),
					new Noise.Constant(0.5f));
			}
		}
	}
}
