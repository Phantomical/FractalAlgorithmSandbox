#version 330

uniform vec3 LightDir;

in float displacement;
in vec3 normal;

out vec3 Colour;

void main()
{
	Colour = vec3(clamp( dot( normal, -LightDir ), 0.0,1.0 ));
}