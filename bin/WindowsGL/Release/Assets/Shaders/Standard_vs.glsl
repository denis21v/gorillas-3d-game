#version 330

in vec3 a_Position;
in vec3 a_Normal;
in vec2 a_UV;

uniform mat4 u_ModelMatrix;
uniform mat4 u_ViewMatrix;
uniform mat4 u_ProjectionMatrix;

out vec2 v_UV;
out vec3 v_Normal;
out vec3 v_FragPos;

void main()
{
    // Reverse mult order compared to OpenTK
    mat4 MVP = u_ProjectionMatrix * u_ViewMatrix * u_ModelMatrix;
    vec4 vertexPos = vec4(a_Position, 1.0);
    gl_Position = MVP * vertexPos;
    v_UV = a_UV;
    v_Normal = a_Normal;
    v_FragPos = vec3(u_ModelMatrix * vertexPos);
}
