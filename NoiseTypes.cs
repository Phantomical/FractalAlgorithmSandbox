
namespace FractalAlgorithmTest
{
	///Feel free to add new types here.
	///If an equivalent option is added to <see cref="Window.GetNoiseType"/>
	///it can automatically be selected from the command line.
	///The function that the type corresponds to must be named Get[TypeName]Module(int seed)
	public enum NoiseType
	{
		Mountain,
		RidgesAndValleys,
		Valley,
		Plains,
		RoundedHills,
		Custom
	}
}
