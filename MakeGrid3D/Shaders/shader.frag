#version 330 core
flat in vec3 startPos;
in vec3 vertPos;
out vec4 FragColor;

uniform vec4  current_color;
uniform vec2  u_resolution;
uniform float u_dashSize;
uniform float u_gapSize;
uniform float pointRadius;
uniform int isPoint; 

void main()
{
    vec2  dir  = (vertPos.xy-startPos.xy) * u_resolution/2.0;
    float dist = length(dir);

    if (fract(dist / (u_dashSize + u_gapSize)) > u_dashSize/(u_dashSize + u_gapSize))
        discard; 

    if (isPoint == 1) {
        vec2 coord = gl_PointCoord - vec2(pointRadius);
        if(length(coord) > pointRadius)
            discard;
    }

    FragColor = current_color;
}