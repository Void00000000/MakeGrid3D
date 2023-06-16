#version 330 core
layout (location = 0) in vec3 aPos;   // the position variable
layout (location = 1) in vec2 uvIn; // texture coodrinates used in generating gradient color

uniform mat4 projection;
uniform mat4 model;
uniform mat4 view;

flat out vec3 startPos;
out vec3 vertPos;
out vec2 uv;

void main()
{
    gl_Position = vec4(aPos, 1.0) * model * view * projection;
    vertPos = gl_Position.xyz;
    startPos = vertPos;
    uv = uvIn;
}      