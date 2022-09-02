////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      ShaderProgram.cs                                                      //
//                                                                            //
//      Shader program object (OpenGL)                                        //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // Texture class

    public class Shader
    {
        ///////////////////////////////////////////////////////////////////////
        // Public class types

        ///////////////////////////////////////////////////////////////////////
        // Private class data

        int mProgram_ID;                               // OpenGL shader program ID
        int mAttributeLocationPosition;                // Attribute location for vertex position
        int mAttributeLocationNormal;                  // Attribute location for vertex normal
        int mAttributeLocationUV;                      // Attribute location for texture position
        int mUniformLocationTextureSampler;            // Uniform location for texture sampler
        int mUniformLocationModelMatrix;               // Uniform location for model matrix
        int mUniformLocationViewMatrix;                // Uniform location for view matrix
        int mUniformLocationProjectionMatrix;          // Uniform location for projection matrix
        int mUniformLocationAmbientLightColour;        // Uniform location for ambient light colour
        int mUniformLocationAmbientLightIntensity;     // Uniform location for ambient light intensity
        int mUniformLocationDirectionalLightColour;    // Uniform location for directional light colour
        int mUniformLocationDirectionalLightIntensity; // Uniform location for directional light intensity
        int mUniformLocationDirectionalLightPosition;  // Uniform location for directional light position


        ///////////////////////////////////////////////////////////////////////
        // Constructors

        // Standard constructor
        public Shader(string shaderName)
        {
            // Create shader program object
            mProgram_ID = GL.CreateProgram();

            // Compile vertex and fragment shaders
            CompileShader("Assets/Shaders/" + shaderName + "_vs.glsl", ShaderType.VertexShader);
            CompileShader("Assets/Shaders/" + shaderName + "_fs.glsl", ShaderType.FragmentShader);

            // Link the program
            GL.LinkProgram(mProgram_ID);
            //Console.WriteLine(GL.GetProgramInfoLog(mProgram_ID));

            // Lookup vertex attribute locations
            mAttributeLocationPosition = GL.GetAttribLocation(mProgram_ID, "a_Position");
            mAttributeLocationNormal = GL.GetAttribLocation(mProgram_ID, "a_Normal");
            mAttributeLocationUV = GL.GetAttribLocation(mProgram_ID, "a_UV");

            // Lookup uniform locations
            mUniformLocationTextureSampler = GL.GetUniformLocation(mProgram_ID, "u_TextureSampler");
            mUniformLocationModelMatrix = GL.GetUniformLocation(mProgram_ID, "u_ModelMatrix");
            mUniformLocationViewMatrix = GL.GetUniformLocation(mProgram_ID, "u_ViewMatrix");
            mUniformLocationProjectionMatrix = GL.GetUniformLocation(mProgram_ID, "u_ProjectionMatrix");
            mUniformLocationAmbientLightColour = GL.GetUniformLocation(mProgram_ID, "u_AmbientLightColour");
            mUniformLocationAmbientLightIntensity = GL.GetUniformLocation(mProgram_ID, "u_AmbientLightIntensity");
            mUniformLocationDirectionalLightColour = GL.GetUniformLocation(mProgram_ID, "u_DirectionalLightColour");
            mUniformLocationDirectionalLightIntensity = GL.GetUniformLocation(mProgram_ID, "u_DirectionalLightIntensity");
            mUniformLocationDirectionalLightPosition = GL.GetUniformLocation(mProgram_ID, "u_DirectionalLightPosition");
        }


        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access OpenGL program ID
        public int Program_ID
        {
            // Read-only
            get { return mProgram_ID; }
        }

        // Access OpenGL shader attribute and uniform locations (read-only)
        public int AttributeLocationPosition { get { return mAttributeLocationPosition; } }
        public int AttributeLocationNormal { get { return mAttributeLocationNormal; } }
        public int AttributeLocationUV { get { return mAttributeLocationUV; } }
        public int UniformLocationTextureSampler { get { return mUniformLocationTextureSampler; } }
        public int UniformLocationModelMatrix { get { return mUniformLocationModelMatrix; } }
        public int UniformLocationViewMatrix { get { return mUniformLocationViewMatrix; } }
        public int UniformLocationProjectionMatrix { get { return mUniformLocationProjectionMatrix; } }
        public int UniformLocationAmbientLightColour { get { return mUniformLocationAmbientLightColour; } }
        public int UniformLocationAmbientLightIntensity { get { return mUniformLocationAmbientLightIntensity; } }
        public int UniformLocationDirectionalLightColour { get { return mUniformLocationDirectionalLightColour; } }
        public int UniformLocationDirectionalLightIntensity { get { return mUniformLocationDirectionalLightIntensity; } }
        public int UniformLocationDirectionalLightPosition { get { return mUniformLocationDirectionalLightPosition; } }


        ///////////////////////////////////////////////////////////////////////
        // Private methods

        // Compile shader
        void CompileShader(String shaderFilePath, ShaderType type)
        {
            // Load shader source file
            int shader_ID = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(shaderFilePath))
            {
                GL.ShaderSource(shader_ID, sr.ReadToEnd());
            }

            // Compile shader and attach to the program
            GL.CompileShader(shader_ID);
            GL.AttachShader(mProgram_ID, shader_ID);
            //Console.WriteLine(GL.GetShaderInfoLog(shader_ID));

            // Make sure shader compiled OK
            int result;
            GL.GetShader(shader_ID, ShaderParameter.CompileStatus, out result);
            if (result == 0)
            {
                throw new ApplicationException("Failed to compile shader " + shaderFilePath + "!");
            }
        }
    }
}
