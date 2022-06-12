#version 330 core

in vec3 aPosition;
in vec3 vertexColor;

out vec3 vColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    vColor = vertexColor;
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}