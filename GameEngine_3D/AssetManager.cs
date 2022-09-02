////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      AssetManager.cs                                                       //
//                                                                            //
//      Asset manager object                                                  //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // AssetManager class

    public class AssetManager
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class data

        // Static resource caches for reusability
        static Dictionary<string, Texture> mTextureCache = new Dictionary<string, Texture>();
        static Dictionary<string, Shader> mShaderCache = new Dictionary<string, Shader>();
        static Dictionary<string, Geometry> mGeometryCache = new Dictionary<string, Geometry>();
        static Dictionary<string, Audio> mAudioCache = new Dictionary<string, Audio>();

        ///////////////////////////////////////////////////////////////////////
        // Public methods

        // Load texture
        public static Texture LoadTexture(string name, bool isCubeMap = false)
        {
            // First attempt to find texture in texture cache. Chances are it might
            // be already present.
            Texture texture;
            mTextureCache.TryGetValue(name, out texture);
            if (texture == null)
            {
                // Not present yet so load a fresh copy
                texture = new Texture(name, isCubeMap);

                // Add to texture cache so we can reuse it next time
                mTextureCache.Add(name, texture);
            }

            return texture;
        }

        // Load shader
        public static Shader LoadShader(string name)
        {
            // First attempt to find shader in shader cache. Chances are it might
            // be already present.
            Shader shader;
            mShaderCache.TryGetValue(name, out shader);
            if (shader == null)
            {
                // Not present yet so load a fresh copy
                shader = new Shader(name);

                // Add to shader cache so we can reuse it next time
                mShaderCache.Add(name, shader);
            }

            return shader;
        }

        // Load geometry
        public static Geometry LoadGeometry(Shader shader, string name)
        {
            // First attempt to find geometry in geometry cache. Chances are it might
            // be already present.
            Geometry geometry;
            mGeometryCache.TryGetValue(name, out geometry);
            if (geometry == null)
            {
                // Not present yet so load a fresh copy
                geometry = new Geometry(shader, name);

                // Add to geometry cache so we can reuse it next time
                mGeometryCache.Add(name, geometry);
            }

            return geometry;
        }

        // Load aduio
        public static Audio LoadAudio(string name)
        {
            // First attempt to find audio in audio cache. Chances are it might
            // be already present.
            Audio audio;
            mAudioCache.TryGetValue(name, out audio);
            if (audio == null)
            {
                // Not present yet so load a fresh copy
                audio = new Audio(name);

                // Add to audio cache so we can reuse it next time
                mAudioCache.Add(name, audio);
            }

            return audio;
        }
    }
}
