////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      Texture.cs                                                            //
//                                                                            //
//      Texture object (OpenGL)                                               //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // Texture class

    public class Texture
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class data

        int mTexture_ID; // OpenGL texture ID
        bool mIsCubeMap; // Cubemap flag
        int mWidth;      // Texture width
        int mHeight;     // Texture height


        ///////////////////////////////////////////////////////////////////////
        // Construction / destruction

        // Standard constructor
        public Texture(string textureName, bool isCubeMap)
        {
            // Full file path
            string textureFilePath = "Assets/Textures/" + textureName;

            // Generate OpenGL texture ID
            mTexture_ID = GL.GenTexture();
            mIsCubeMap = isCubeMap;

            // This could be either a cubemap texture
            if (isCubeMap)
            {
                // Cubemap textures consist of 6 separate images one per cube face. We are
                // expecting cubemap images to be named as <cubemap_name><cube_side>.<ext>
                // e.g. Sky1_Left.png, Sky1_Right.png, Sky1_Up.png etc.
                string[] faces = new string[]
                {
                    "Left",
                    "Right",
                    "Up",
                    "Down",
                    "Front",
                    "Back"
                };

                // Parse file name
                int index = textureFilePath.LastIndexOf('.');
                string prefix = textureFilePath.Substring(0, index);
                string suffix = textureFilePath.Substring(index, textureFilePath.Length - index);

                // Load cubemap faces
                GL.BindTexture(TextureTarget.TextureCubeMap, mTexture_ID);

                for (int i = 0; i < faces.Length; i ++)
                {
                    string path = prefix + faces[i] + suffix;
                    Bitmap bmp = new Bitmap(path);
                    BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                    bmp.UnlockBits(bmp_data);

                    // Save texture size on the first cube face. All cube face images
                    // are expected to be the same size.
                    if (i == 0)
                    {
                        mWidth = bmp_data.Width;
                        mHeight = bmp_data.Height;
                    }
                }

                // Configure texture filtering and wrapping
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            }

            // or normal texture
            else
            {
                // Load texture
                GL.BindTexture(TextureTarget.Texture2D, mTexture_ID);

                Bitmap bmp = new Bitmap(textureFilePath);
                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                bmp.UnlockBits(bmp_data);

                // Save texture size
                mWidth = bmp_data.Width;
                mHeight = bmp_data.Height;

                // Configure texture filtering and wrapping
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.Ext.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
        }

        // Destructor
        ~Texture()
        {
            // Not really needed as OpenTK takes care of OpenGL
            // resources upon termination ...

            // GL.DeleteTexture(mTexture_ID);
        }


        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access OpenGL texture ID
        public int Texture_ID
        {
            // Read-only
            get { return mTexture_ID; }
        }

        // Access texture width
        public int Width
        {
            // Read-only
            get { return mWidth; }
        }

        // Access texture height
        public int Height
        {
            // Read-only
            get { return mHeight; }
        }

        // Access cubemap flag
        public bool IsCubeMap
        {
            // Read-only
            get { return mIsCubeMap; }
        }
    }
}
