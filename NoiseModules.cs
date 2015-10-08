
namespace FractalAlgorithmTest
{
	class NoiseModules
	{
		public static INoiseModule GetMountainModule(int seed)
		{
			return new Modifier.ScaleCoords(
				new Modifier.Add(
					new Modifier.RidgedFractal(new Noise.PerlinNoise(), 16, 0.52f, 2, 2f),
					new Modifier.Multiply(
						new Noise.WorleyNoise(seed, Noise.WorleyNoise.DistanceFunc.Euclidian, Noise.WorleyNoise.CombinerFunc.Function1),
						new Noise.Constant(5))),
				new Noise.Constant(0.1f));
		}
		public static INoiseModule GetRoundedHillsModule(int seed)
		{
			return new Modifier.Subtract(new Noise.Constant(1), GetMountainModule(seed));
		}
		public static INoiseModule GetRidgesAndValleysModule(int seed)
		{
			return 
				new Modifier.ScaleCoords(
					new Modifier.Multiply(
						new Modifier.AddToCoords(
							new Modifier.Fractal(
								new Noise.PerlinNoise(), 16, 0.5f, 2, 2),
							new Modifier.Fractal(
								new Noise.PerlinNoise(), 16, 0.5f, 2, 2)),
						GetMountainModule(seed)),
					new Noise.Constant(0.1f));
		}
		public static INoiseModule GetValleyModule(int seed)
		{
			return 
				new Modifier.Add(
					new Noise.SinCoords(),
					new Modifier.Multiply(
						new Modifier.RidgedFractal(
							new Noise.PerlinNoise(), 8, 0.5f, 2, 2),
						new Noise.Constant(0.1f)
					)
				);
		}
		public static INoiseModule GetPlainsModule(int seed)
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
