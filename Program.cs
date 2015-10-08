
using System;

namespace FractalAlgorithmTest
{
#pragma warning disable 168

	class Program
	{
		static void Main(string[] args)
		{
			NoiseType Type = NoiseType.Mountain;
			int Seed = 0;

			if(args.Length >= 1)
			{
				if(args[0] == "help")
				{
					Console.WriteLine(
@"Command line arguments
	[NoiseType] [Seed]

	NoiseType can be any of " + String.Join(",\n\t\t ", Enum.GetNames(typeof(NoiseType))) + @".
	Seed can be any valid integer."
						);

					return;
				}
				try
				{
					//Convert the text value of the argument to an enum
					Type = (NoiseType)Enum.Parse(typeof(NoiseType), args[0], true);
				}
				catch(ArgumentException Exception)
				{
					Console.WriteLine(args[0] + " is not a valid noise type. Using Mountain Noise intstead.");
				}
			}
			
			if(args.Length > 1)
			{
				try
				{
					Seed = Convert.ToInt32(args[1]);
				}
				catch(FormatException Exception)
				{
					Console.WriteLine(args[1] + " is not a valid number. Using 0 instead.");
				}
				catch(OverflowException Exception)
				{
					Console.WriteLine(args[1] + " is not within the bounds for a seed. Using 0 instead.");
				}
			}

			using(Window Win = new Window(Type, Seed))
			{
				Win.Run();
			}
		}
	}
}
