using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using ShaderRuntime;
using System;
using System.Reflection;
using System.Linq;

namespace FractalAlgorithmTest
{
	public class Window : GameWindow
	{
		private static INoiseModule GetNoiseType(NoiseType Type, int seed)
		{
			switch (Type)
			{
				case NoiseType.Custom:
					//Feel free to test new modules here

					///If you merge them into the repository
					///please add the algorithm to the <see cref="NoiseType"/> enum
					return null;
				default:
					///Get type module by calling the corresponding method
					///in <see cref="NoiseModules"> using reflection
					string ModuleName = Enum.GetName(typeof(NoiseType), Type);

					MethodInfo TargetMethod = typeof(NoiseModules).GetMethod("Get" + ModuleName + "Module", new Type[]{typeof(int)});

					return (INoiseModule)TargetMethod.Invoke(null, new object[]{seed});
			}
		}

		public Window(NoiseType Type, int Seed) :
			base(
				1080, 720, GraphicsMode.Default, "Fractal Algorithm Sandbox",
				GameWindowFlags.Default, DisplayDevice.Default,
				3, 2, GraphicsContextFlags.ForwardCompatible)
		{
			CamRot = Quaternion.Identity;
			CamPos = new Vector3(0, 0.2f, 1.5f);

			Noise = GetNoiseType(Type, Seed);
		}

		INoiseModule Noise;

		void CreateBuffers()
		{
			//Create the plane's mesh data
			Plane p = new Plane(10, 1024, (x, y, z) => 
				{
					return Noise.GetValue(x, y, z); 
				});

			//Assign mesh data
			int[] Indices = p.Indices;
			Vector3[] Vertices = p.Vertices;
			Vector3[] Normals = p.Normals;

			//Create buffers
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
			//Create the vertex array object
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
		bool XDown;

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if (Focused)
			{
				//Amount that the camera can move per update
				const float displacement = 0.02f;
				const float rotation = 0.02f;
				var Keyboard = OpenTK.Input.Keyboard.GetState();

				Matrix4 rot = Matrix4.CreateFromQuaternion(CamRot);

				//Get current camera axes
				Vector3 left = rot.Row0.Xyz;
				Vector3 up = rot.Row1.Xyz;
				Vector3 front = rot.Row2.Xyz;

				Vector3 NewCamRot = Vector3.Zero;

				//Move camera
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

				//Rotate camera
				if (Keyboard[OpenTK.Input.Key.I])
					NewCamRot.X -= rotation;
				if (Keyboard[OpenTK.Input.Key.K])
					NewCamRot.X += rotation;
				if (Keyboard[OpenTK.Input.Key.J])
					NewCamRot.Y -= rotation;
				if (Keyboard[OpenTK.Input.Key.L])
					NewCamRot.Y += rotation;
				if (Keyboard[OpenTK.Input.Key.U])
					NewCamRot.Z -= rotation;
				if (Keyboard[OpenTK.Input.Key.O])
					NewCamRot.Z += rotation;

				//Reset camera position and rotation
				if (Keyboard[OpenTK.Input.Key.Space])
				{
					CamPos = new Vector3(0, 0, 1.5f);
					CamRot = Quaternion.Identity;
				}

				//Toggle between wireframe and normal rendering
				if (Keyboard[OpenTK.Input.Key.X])
				{
					if (!XDown)
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

						XDown = true;
					}
				}
				else
				{
					XDown = false;
				}

				//Recompile shader
				if (Keyboard[OpenTK.Input.Key.R])
				{
					Shader.Recompile();
				}

				//Close window
				if(Keyboard[OpenTK.Input.Key.Escape])
				{
					Exit();
				}

				//Update rotation
				CamRot *= Quaternion.FromMatrix(Matrix3.CreateRotationX(NewCamRot.X) * Matrix3.CreateRotationY(NewCamRot.Y) * Matrix3.CreateRotationZ(NewCamRot.Z));
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			//Create the shader
			Shader = new Shaders.PlaneShader();
			//Compile it
			Shader.Compile();

			//Set light direction
			Shader.SetParameter("LightDir", new Vector3(0, 0.5f, 0.5f));

			//Create buffers
			CreateBuffers();
			//Create vertex array object
			CreateVAO();

			GL.Enable(EnableCap.DepthTest);
			GL.ClearColor(0.0f, 0.0f, 0.0f, 1f);
			//GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			//GL.Enable(EnableCap.CullFace);
			GL.FrontFace(FrontFaceDirection.Cw);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			//Don't render when the window isn't active
			if (Visible)
			{
				GL.Viewport(0, 0, Width, Height);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

				//Prepare shader
				Shader.UseShader();
				//Set the model view position matrix
				Shader.SetParameter("MVP", (Matrix4.CreateFromQuaternion(CamRot) * Matrix4.CreateTranslation(CamPos)).Inverted() * Matrix4.CreatePerspectiveFieldOfView(1, (float)Width / (float)Height, 0.05f, 20));
				//Pass shader uniforms
				Shader.PassUniforms();

				//Bind the vertex array object
				GL.BindVertexArray(Vao);
				//Draw
				GL.DrawElements(BeginMode.Triangles, NumIndices, DrawElementsType.UnsignedInt, 0);

				SwapBuffers();
			}
		}
	}
}
