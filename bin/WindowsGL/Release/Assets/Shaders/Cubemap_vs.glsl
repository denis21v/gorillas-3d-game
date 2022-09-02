#version 330

in vec3 a_Position;
in vec3 a_Normal;
in vec2 a_UV;

uniform mat4 u_ModelMatrix;
uniform mat4 u_ViewMatrix;
uniform mat4 u_ProjectionMatrix;

out vec3 v_UV;

void main()
{
    // Reverse mult order compared to OpenTK
    mat4 MVP = u_ProjectionMatrix * u_ViewMatrix * u_ModelMatrix;
    gl_Position = MVP * vec4(a_Position, 1.0);
    v_UV = a_Position;
}
