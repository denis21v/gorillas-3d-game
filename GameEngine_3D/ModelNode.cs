////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      ModelNode.cs                                                          //
//                                                                            //
//      3D model node                                                         //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // ModelNode class

    public class ModelNode : Node
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class data

        Geometry mGeometry; // Geometry to use
        Texture mTexture;   // Texture to use
        bool mUseLighting;  // Whether to use scene lighting

        ///////////////////////////////////////////////////////////////////////
        // Constructors

        // Minimal node constructor
        public ModelNode(string name) :
            base(name)
        {
            // Properties not know at creation, to be assigned later
            mGeometry = null;
            mTexture = null;
            mUseLighting = true;
        }

        // Full node constructor
        public ModelNode(string name, Geometry geometry, Texture texture) :
            base(name)
        {
            mGeometry = geometry;
            mTexture = texture;
            mUseLighting = true;
        }


        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access geometry property
        public Geometry Geometry
        {
            get { return mGeometry; }
            set { mGeometry = value; }
        }

        // Access texture property
        public Texture Texture
        {
            get { return mTexture; }
            set { mTexture = value; }
        }

        // Access lighting property
        public bool UseLighting
        {
            get { return mUseLighting; }
            set { mUseLighting = value; }
        }

        // Frame render handler
        public override void OnRender(Matrix4 parentModelMatrix)
        {
            // Only render if node is visible and has all of mission critical
            // properties assigned
            if (Visible && (mGeometry != null) && (mTexture != null))
            {
                // Lookup scene node as we need access to camera & lights
                Scene scene = Scene;

                // We can't really render anything unless the parent scene has an
                // active camera assigned to it
                if (scene.Camera != null)
                {
                    // Calculate combined model (world) matrix that takes into
                    // account both parent and local node transforms
                    Matrix4 model = this.Transform.ModelMatrix * parentModelMatrix;

                    // Also retrieve View & Projection matrices
                    Matrix4 view = scene.Camera.ViewMatrix;
                    Matrix4 projection = scene.Camera.ProjectionMatrix;

                    // Use shader propgram
                    Shader shader = mGeometry.Shader;
                    GL.UseProgram(shader.Program_ID);

                    // Pass Model, View, Projection matrices as separate unforms as vertex shader
                    // needs MVP and ModelView matrices for light calculations
                    GL.UniformMatrix4(shader.UniformLocationModelMatrix, false, ref model);
                    GL.UniformMatrix4(shader.UniformLocationViewMatrix, false, ref view);
                    GL.UniformMatrix4(shader.UniformLocationProjectionMatrix, false, ref projection);

                    // Use texture sampler 0
                    GL.Uniform1(shader.UniformLocationTextureSampler, 0);

                    // Cubemap models are a special case
                    if (mTexture.IsCubeMap)
                    {
                        // Bind texture
                        GL.BindTexture(TextureTarget.TextureCubeMap, mTexture.Texture_ID);

                        // Turn off depth test
                        GL.Disable(EnableCap.DepthTest);
                    }
                    else
                    {
                        // Activate texture unit 0
                        GL.ActiveTexture(TextureUnit.Texture0);

                        // Bind texture
                        GL.BindTexture(TextureTarget.Texture2D, mTexture.Texture_ID);

                        Vector3 lightColour, lightPosition;
                        float lightIntensity;

                        // Only use ambient light if enabled
                        if (mUseLighting)
                        {
                            // Either use scene ambient light if available or fallback to default
                            Light light = scene.FindLight(Light.LightType.Ambient);
                            if (light == null)
                            {
                                lightColour = Vector3.One; // White ambient light colour
                                lightIntensity = 0.5f; // 50% intensity
                            }
                            else
                            {
                                lightColour = light.Colour;
                                lightIntensity = light.Intensity;
                            }
                        }
                        else
                        {
                            // White ambient light at full power which essentially
                            // means 100 diffuse colour
                            lightColour = Vector3.One; // White ambient light colour
                            lightIntensity = 1.0f; // 100% intensity
                        }

                        GL.Uniform3(shader.UniformLocationAmbientLightColour, lightColour);
                        GL.Uniform1(shader.UniformLocationAmbientLightIntensity, lightIntensity);

                        // Only use directional light if enabled
                        if (mUseLighting)
                        {
                            // Either use scene directional light if available or fallback to default
                            Light light = scene.FindLight(Light.LightType.Directional);
                            if (light == null)
                            {
                                lightColour = Vector3.One; // White directional light colour
                                lightIntensity = 0.5f; // 50% intensity
                                lightPosition = new Vector3(0, 100, 0); // Midday sun
                            }
                            else
                            {
                                lightColour = light.Colour;
                                lightIntensity = light.Intensity;
                                lightPosition = light.Position;
                            }
                        }
                        else
                        {
                            // Directional light off
                            lightColour = Vector3.Zero;
                            lightIntensity = 0.0f;
                            lightPosition = Vector3.Zero;
                        }

                        // We need to take model transform into account. Light position must be converted
                        // to model coordinate space.
                        Vector4 lightPosition4 = new Vector4(lightPosition.X, lightPosition.Y, lightPosition.Z, 1);
                        lightPosition4 = model * lightPosition4;

                        GL.Uniform3(shader.UniformLocationDirectionalLightColour, lightColour);
                        GL.Uniform1(shader.UniformLocationDirectionalLightIntensity, lightIntensity);
                        GL.Uniform3(shader.UniformLocationDirectionalLightPosition, lightPosition4.Xyz);
                    }

                    // Render geometry
                    GL.BindVertexArray(mGeometry.VAO_ID);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, mGeometry.NumberOfTriangles * 3);

                    // Re-enable depth as we might have turned it off for the cubemap!
                    if (mTexture.IsCubeMap)
                        GL.Enable(EnableCap.DepthTest);

                    // Detach OpenGL resources
                    GL.BindVertexArray(0);
                    GL.UseProgram(0);
                }
            }
        }
    }
}
