////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      Geometry.cs                                                           //
//                                                                            //
//      3D geometry object (OpenGL)                                           //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // Geometry class

    public class Geometry
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class data

        // Shader to reder this geometry
        Shader mShader;

        // Number of attributes per vertex:
        //    Position(x, y, z) + Normal(x, y, z) + Texture(u, v)
        const int mNumberOfAttributesPerVertex = 3 + 3 + 2;

        // Number of triangles
        int mNumberOfTriangles;

        // OpenGL Vertex Buffer Object IDs
        int mVBO_VertexAttributes_ID;  // Vertex attributes buffer object ID
        int mVBO_VertexIndices_ID;     // Vertex indices buffer object ID
        int mVAO_ID;                   // Vertex array object ID


        ///////////////////////////////////////////////////////////////////////
        // Construction / destruction

        // Create geometry object from raw vertex data
        public Geometry(Shader shader, float[] vertexAttributes, int[] vertexIndices)
        {
            // Backup shader program
            mShader = shader;

            // Set geometry data
            SetGeometryData(vertexAttributes, vertexIndices);
        }

        // Create geometry object from 3D model file
        public Geometry(Shader shader, string modelName)
        {
            // Backup shader program
            mShader = shader;

            // Full model path
            string modelFilePath = "Assets/Models/" + modelName;

            // Load specific 3D model geometry format based on file extension
            string ext = Path.GetExtension(modelFilePath);
            switch (ext)
            {
            case ".obj":
                // Load WaveFront OBJ file
                LoadWaveFrontOBJ(modelFilePath);
                break;

            default:
                // TBD: add support for more 3D model formats
                throw new ApplicationException("Failed to load 3D model " + modelFilePath +
                    ". Unsupported format " + ext + "!");
            }
        }

        // Destructor
        ~Geometry()
        {
            // Not really needed as OpenTK takes care of OpenGL
            // resources upon termination ...

            //GL.DeleteBuffer(mVBO_VertexAttributes_ID);
            //GL.DeleteBuffer(mVBO_VertexIndices_ID);
            //GL.DeleteVertexArray(mVAO_ID);
        }


        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access shader program
        public Shader Shader
        {
            // Read-only
            get { return mShader; }
        }

        // Access OpenGL Vertex Array Object (VAO) ID
        public int VAO_ID
        {
            // Read-only
            get { return mVAO_ID; }
        }

        // Number of trianles
        public int NumberOfTriangles
        {
            // Read-only
            get { return mNumberOfTriangles; }
        }


        ///////////////////////////////////////////////////////////////////////
        // Private methods

        // Set geometry data
        void SetGeometryData(float[] vertexAttributes, int[] vertexIndices)
        {
            // We are expecting the exact number of attributes per vertex
            if ((vertexAttributes.Length == 0) || ((vertexAttributes.Length % mNumberOfAttributesPerVertex) != 0))
            {
                throw new ApplicationException("Invalid vertex attributes buffer!");
            }

            // We are also expecting triangle primitives and therefore number of
            // indices must be multiples of 3
            if ((vertexIndices.Length == 0) || ((vertexIndices.Length % 3) != 0))
            {
                throw new ApplicationException("Invalid vertex indices buffer!");
            }

            mNumberOfTriangles = vertexIndices.Length / 3;

            // Upload vertex buffers to the corresponding VBOs and bind them to a VAO
            mVAO_ID = GL.GenVertexArray();
            GL.BindVertexArray(mVAO_ID);

            // Upload vertex attributes
            int bufferSizeActual, bufferSize = vertexAttributes.Length * sizeof(float);
            mVBO_VertexAttributes_ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_VertexAttributes_ID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)bufferSize,
                vertexAttributes, BufferUsageHint.StaticDraw);

            // Sanity check: see if buffer data has been correctly uploaded to video memory!
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSizeActual);
            if (bufferSizeActual != bufferSize)
            {
                throw new ApplicationException("Vertex attributes not uploaded to video memory!");
            }

            // Enable vertex attribute arrays:

            // Position(x, y, z)
            int stride = mNumberOfAttributesPerVertex * 4; // 4 bytes per float attribute
            int offset = 0;

            GL.EnableVertexAttribArray(mShader.AttributeLocationPosition);
            GL.VertexAttribPointer(mShader.AttributeLocationPosition, 3, VertexAttribPointerType.Float, false, stride, offset);
            offset += (3 * 4);

            // Normal(x, y, z)
            GL.EnableVertexAttribArray(mShader.AttributeLocationNormal);
            GL.VertexAttribPointer(mShader.AttributeLocationNormal, 3, VertexAttribPointerType.Float, false, stride, offset);
            offset += (3 * 4);

            // Texture(u, v)
            GL.EnableVertexAttribArray(mShader.AttributeLocationUV);
            GL.VertexAttribPointer(mShader.AttributeLocationUV, 2, VertexAttribPointerType.Float, false, stride, offset);
            offset += (2 * 4);

            // Upload vertex indices
            bufferSize = vertexIndices.Length * sizeof(int);
            mVBO_VertexIndices_ID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_VertexIndices_ID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)bufferSize,
                vertexIndices, BufferUsageHint.StaticDraw);

            // Sanity check: check if buffer data has been correctly uploaded to video memory!
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSizeActual);
            if (bufferSizeActual != bufferSize)
            {
                throw new ApplicationException("Vertex indices not uploaded to video memory!");
            }

            // Unbind VAO & VBOs
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        // Load WaveFront OBJ model
        void LoadWaveFrontOBJ(string modelFilePath)
        {
            List<float> vertices = new List<float>();
            List<float> normals = new List<float>();
            List<float> uvs = new List<float>();
            List<int> faces = new List<int>();

            try
            {
                FileStream fin = File.OpenRead(modelFilePath);
                StreamReader sr = new StreamReader(fin);

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] values = line.Split(' ');

                    if (values.Length > 0)
                    {
                        // Vertex(x y z)
                        if (values[0] == "v")
                        {
                            vertices.Add(float.Parse(values[1]));
                            vertices.Add(float.Parse(values[2]));
                            vertices.Add(float.Parse(values[3]));
                        }
                        // Vertex normal(x y z)
                        else if (values[0] == "vn")
                        {
                            normals.Add(float.Parse(values[1]));
                            normals.Add(float.Parse(values[2]));
                            normals.Add(float.Parse(values[3]));
                        }
                        // Vertex texture(u v)
                        else if (values[0] == "vt")
                        {
                            uvs.Add(float.Parse(values[1]));
                            uvs.Add(float.Parse(values[2]));
                        }
                        // Face(v/t/n v/t/n v/t/n)
                        else if (values[0] == "f")
                        {
                            for (int f = 1; f < 4; f ++)
                            {
                                string[] face = values[f].Split('/');
                                faces.Add(int.Parse(face[0]) - 1);
                                faces.Add(int.Parse(face[1]) - 1);
                                faces.Add(int.Parse(face[2]) - 1);
                            }
                        }
                    }
                }

                // Sanity check
                int numberOfTriangles = faces.Count / 9;
                if (numberOfTriangles == 0)
                    throw new ApplicationException("Failed to load 3D model " + modelFilePath + "!");

                // Build a continuous array of vertex attributes
                int a = 0, i = 0, size = numberOfTriangles * 3 * mNumberOfAttributesPerVertex;
                float[] attributes = new float[size];
                int [] indices = new int[faces.Count / 3];

                for (int f = 0; f < faces.Count; f += 3)
                {
                    int v = faces[f + 0]; // Vertex index
                    int t = faces[f + 1]; // Texture index
                    int n = faces[f + 2]; // Normal index

                    // Vertex attribute (x, y, z)
                    int index = v * 3;
                    attributes[a ++] = vertices[index + 0];
                    attributes[a ++] = vertices[index + 1];
                    attributes[a ++] = vertices[index + 2];

                    // Normal attribute (x, y, z)
                    index = n * 3;
                    attributes[a ++] = normals[index + 0];
                    attributes[a ++] = normals[index + 1];
                    attributes[a ++] = normals[index + 2];

                    // Texture attribute (u, v)
                    index = t * 2;
                    float au = uvs[index + 0];
                    float av = uvs[index + 1];
                    attributes[a ++] = uvs[index + 0];
                    attributes[a ++] = 1.0f - uvs[index + 1];

                    indices[i] = i ++;
                }

                // Finally set geometry data
                SetGeometryData(attributes, indices);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
