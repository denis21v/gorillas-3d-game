#version 330

in vec2 v_UV;
in vec3 v_Normal;
in vec3 v_FragPos;


uniform sampler2D u_TextureSampler;
uniform vec3 u_AmbientLightColour;
uniform float u_AmbientLightIntensity;
uniform vec3 u_DirectionalLightColour;
uniform float u_DirectionalLightIntensity;
uniform vec3 u_DirectionalLightPosition;

out vec4 o_Color;

void main()
{
    vec3 norm = normalize(v_Normal);
    vec3 lightDir = normalize(u_DirectionalLightPosition);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = u_DirectionalLightColour * u_DirectionalLightIntensity * diff;
    vec3 ambient = u_AmbientLightIntensity * u_AmbientLightColour;
    vec3 result = (ambient + diffuse) * texture(u_TextureSampler, v_UV).rgb;
    o_Color = vec4(result, 1.0);
}
