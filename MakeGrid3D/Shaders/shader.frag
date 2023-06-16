#version 330 core
flat in vec3 startPos;
in vec3 vertPos;
in vec2 uv;
out vec4 FragColor;

uniform vec4  current_color;
uniform vec2  u_resolution;
uniform float u_dashSize;
uniform float u_gapSize;
uniform vec4 color1;
uniform vec4 color2;
uniform vec4 color3;
uniform vec4 color4;
uniform int isPoint; 
uniform int isGradient;

void main()
{
    vec2 dir  = (vertPos.xy-startPos.xy) * u_resolution/2.0;
    float dist = length(dir);

    if (fract(dist / (u_dashSize + u_gapSize)) > u_dashSize/(u_dashSize + u_gapSize))
        discard; 

    if (isPoint == 1) {
        vec2 coord = gl_PointCoord - vec2(0.5);
        if(length(coord) > 0.5)
            discard;
    }

    // from wikipedia on bilinear interpolation on unit square:
    // f(x,y) = f(0,0)(1-x)(1-y) + f(1,0)x(1-y) + f(0,1)(1-x)y + f(1,1) xy. 
    if (isGradient == 1) {
        FragColor.xyz = color1.xyz*(1-uv.x)*(1-uv.y) + color2.xyz*uv.x*(1-uv.y) + color3.xyz*(1-uv.x)*uv.y + color4.xyz*uv.x*uv.y;
        FragColor.w = 1;
    }
    else {
        FragColor = current_color; 
    }
}