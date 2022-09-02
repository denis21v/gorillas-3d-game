#version 330

in vec3 v_UV;

uniform samplerCube u_TextureSampler;

out vec4 o_Color;

void main()
{
    o_Color = texture(u_TextureSampler, v_UV);
}
