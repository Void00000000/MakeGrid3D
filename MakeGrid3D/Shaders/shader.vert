#version 330 core
layout (location = 0) in vec3 aPos;   // the position variable has attribute position 0

uniform mat4 projection;
uniform mat4 model;

flat out vec3 startPos;
out vec3 vertPos;

void main()
{
    gl_Position = vec4(aPos, 1.0) * model * projection;
    vertPos = gl_Position.xyz;
    startPos = vertPos;
}      