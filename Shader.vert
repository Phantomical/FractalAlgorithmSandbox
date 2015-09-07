#version 330

in vec3 Vertex;
in vec3 Normal;

uniform mat4 MVP;

smooth out float displacement;
smooth out vec3 normal;

void main()
{
	gl_Position = MVP * vec4(Vertex, 1.0);
	displacement = Vertex.y;
	normal = Normal;
}