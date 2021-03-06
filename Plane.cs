﻿using System.Threading.Tasks;
using OpenTK;

namespace FractalAlgorithmTest
{
	public class Plane
	{
		public Vector3[] Vertices;
		public Vector3[] Normals;
		public int[] Indices;

		public delegate float NoiseFunc(float x, float y, float z);
		
		public Plane(float Width, int VertsPerSide, NoiseFunc NoiseFunction)
		{
			//The spacing between each of the vertices.
			float Interp = Width / VertsPerSide;
			//Half the width
			float hWidth = Width * 0.5f;
			//Number of indices
			int NumIndices = (VertsPerSide - 1) * (VertsPerSide - 1) * 6;

			Vertices = new Vector3[VertsPerSide * VertsPerSide];
			Normals = new Vector3[VertsPerSide * VertsPerSide];
			Indices = new int[NumIndices];

			//Create and displace vertices
			//Vertices are dispalced along the Y axis
			//Doing this in 3d wold normally be more complicated but with a plane it is simple
			Parallel.For(0, VertsPerSide, x =>
			{
				for (int z = 0; z < VertsPerSide; z++)
				{
					Vertices[x * VertsPerSide + z] = new Vector3(Interp * x - hWidth, NoiseFunction(Interp * x - hWidth, 0, Interp * z - hWidth), Interp * z - hWidth);
				}
			});

			int idx = 0;

			//Create indices
			for (int y = 0; y < VertsPerSide - 1; y++)
			{
				for (int x = 0; x < VertsPerSide - 1; x++)
				{
					//First triangle
					Indices[idx++] = (y + 1) * VertsPerSide + x;
					Indices[idx++] = y * VertsPerSide + x + 1;
					Indices[idx++] = (y * VertsPerSide + x);

					//Second triangle
					Indices[idx++] = (y + 1) * VertsPerSide + x;
					Indices[idx++] = (y + 1) * VertsPerSide + x + 1;
					Indices[idx++] = y * VertsPerSide + x + 1;
				}
			}

			//Create normals using vector cross product
			for (int i = 0; i < idx; i += 3)
			{
				Vector3 v0 = Vertices[Indices[i]];
				Vector3 v1 = Vertices[Indices[i + 1]];
				Vector3 v2 = Vertices[Indices[i + 2]];

				Vector3 Normal = Vector3.Cross(v0 - v1, v0 - v2);

				Normals[Indices[i]] += Normal;
				Normals[Indices[i + 1]] += Normal;
				Normals[Indices[i + 2]] += Normal;
			}

			//Normalize all the normals
			for(int i = 0; i < Normals.Length; i++)
			{
				Normals[i].Normalize();
			}
		}
	}
}
