using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using ShaderRuntime;
using System;

namespace FractalAlgorithmTest
{
	class Window : GameWindow
	{
		public Window() :
			base(
				1080, 720, GraphicsMode.Default, "Fractal Algorithm Test",
				GameWindowFlags.Default, DisplayDevice.Default,
				3, 2, GraphicsContextFlags.ForwardCompatible)
		{
			CamRot = Quaternion.Identity;
			CamPos = new Vector3(0, 0.1f, 1.5f);
			TimeScale = 1;

			Noise = NoiseModules.GetMountainModule(771);
		}

		INoiseModule Noise;

		void CreateBuffers()
		{
			Plane p = new Plane(10, 1024, (x, y, z) => 
				{
					return Noise.GetValue(x, y, z) - 3; 
				});

			int[] Indices = p.Indices;
			Vector3[] Vertices = p.Vertices;
			Vector3[] Normals = p.Normals;

			Ibo = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ibo);
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Indices.Length * sizeof(int)), Indices, BufferUsageHint.StaticDraw);

			Vbo = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertices.Length * Vector3.SizeInBytes), Vertices, BufferUsageHint.StaticDraw);

			Nbo = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, Nbo);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Normals.Length * Vector3.SizeInBytes), Normals, BufferUsageHint.StaticDraw);

			NumIndices = Indices.Length;
		}
		void CreateVAO()
		{
			Vao = GL.GenVertexArray();
			GL.BindVertexArray(Vao);
			Shader.UseShader();
			GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
			GL.VertexAttribPointer(Shader.GetParameterLocation("Vertex"), 3, VertexAttribPointerType.Float, false, 0, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, Nbo);
			GL.VertexAttribPointer(Shader.GetParameterLocation("Normal"), 3, VertexAttribPointerType.Float, true, 0, 0);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ibo);

			GL.BindVertexArray(0);

		}

		GLShader Shader;

		int Vbo;
		int Ibo;
		int Vao;
		int Nbo;
		int NumIndices;
		Vector3 CamPos;
		Quaternion CamRot;
		bool IsLine;
		double TotalTime;
		double TimeScale;

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if (Focused || true)
			{
				const float displacement = 0.01f;
				var Keyboard = OpenTK.Input.Keyboard.GetState();

				Matrix4 rot = Matrix4.CreateFromQuaternion(CamRot);

				Vector3 left = rot.Row0.Xyz;
				Vector3 up = rot.Row1.Xyz;
				Vector3 front = rot.Row2.Xyz;

				Vector3 NewCamRot = Vector3.Zero;

				if (Keyboard[OpenTK.Input.Key.W])
					CamPos -= front * displacement;
				if (Keyboard[OpenTK.Input.Key.S])
					CamPos += front * displacement;
				if (Keyboard[OpenTK.Input.Key.D])
					CamPos += left * displacement;
				if (Keyboard[OpenTK.Input.Key.A])
					CamPos -= left * displacement;
				if (Keyboard[OpenTK.Input.Key.Q])
					CamPos -= up * displacement;
				if (Keyboard[OpenTK.Input.Key.E])
					CamPos += up * displacement;

				if (Keyboard[OpenTK.Input.Key.I])
					NewCamRot.X -= 0.1f;
				if (Keyboard[OpenTK.Input.Key.K])
					NewCamRot.X += 0.1f;
				if (Keyboard[OpenTK.Input.Key.J])
					NewCamRot.Y -= 0.1f;
				if (Keyboard[OpenTK.Input.Key.L])
					NewCamRot.Y += 0.1f;
				if (Keyboard[OpenTK.Input.Key.U])
					NewCamRot.Z -= 0.1f;
				if (Keyboard[OpenTK.Input.Key.O])
					NewCamRot.Z += 0.1f;

				if (Keyboard[OpenTK.Input.Key.N])
					TimeScale *= 0.1f;
				if (Keyboard[OpenTK.Input.Key.M])
					TimeScale *= 10f;
				if (Keyboard[OpenTK.Input.Key.B])
					TimeScale = 1.0f;

				TimeScale = Math.Max(Math.Min(TimeScale, 1e5), -1e5);
				if (Math.Abs(TotalTime) > 1e6)
					TotalTime = 0;

				if (Keyboard[OpenTK.Input.Key.Space])
				{
					CamPos = new Vector3(0, 0, 1.5f);
					CamRot = Quaternion.Identity;
				}

				if (Keyboard[OpenTK.Input.Key.X])
				{
					if (IsLine)
					{
						GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
						IsLine = false;
					}
					else
					{
						GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
						IsLine = true;
					}
				}

				if (Keyboard[OpenTK.Input.Key.R])
				{
					Shader.Recompile();
				}

				if(Keyboard[OpenTK.Input.Key.Escape])
				{
					Exit();
				}


				CamRot *= Quaternion.FromMatrix(Matrix3.CreateRotationX(NewCamRot.X) * Matrix3.CreateRotationY(NewCamRot.Y) * Matrix3.CreateRotationZ(NewCamRot.Z));
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			Shader = new Shaders.PlaneShader();
			Shader.Compile();

			Shader.SetParameter("LightDir", new Vector3(0, 0.5f, 0.5f));

			CreateBuffers();
			CreateVAO();

			GL.Enable(EnableCap.DepthTest);
			GL.ClearColor(0.0f, 0.0f, 0.0f, 1f);
			//GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			//GL.Enable(EnableCap.CullFace);
			GL.FrontFace(FrontFaceDirection.Cw);

			Shader.UseShader();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			TotalTime += e.Time * TimeScale;

			if (Visible)
			{
				GL.Viewport(0, 0, Width, Height);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

				Shader.UseShader();
				Shader.SetParameter("MVP", (Matrix4.CreateFromQuaternion(CamRot) * Matrix4.CreateTranslation(CamPos)).Inverted() * Matrix4.CreatePerspectiveFieldOfView(1, (float)Width / (float)Height, 0.05f, 20));
				Shader.PassUniforms();

				GL.BindVertexArray(Vao);
				GL.DrawElements(BeginMode.Triangles, NumIndices, DrawElementsType.UnsignedInt, 0);

				SwapBuffers();
			}
		}
	}
}
