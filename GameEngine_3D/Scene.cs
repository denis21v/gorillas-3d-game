////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      Scene.cs                                                              //
//                                                                            //
//      Abstract 3D scene object                                              //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using OpenTK;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // Scene class

    public class Scene: Node
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class data

        Camera mCamera;      // Active camera
        List<Light> mLights; // Scene lights
        float mAspectRatio;  // Window aspect ratio (Width / Height)

        ///////////////////////////////////////////////////////////////////////
        // Constructors

        // Standard scene constructor
        public Scene() :
            base("") // Scene node doesn't have a name
        {
            // Default scene parameters
            mCamera = null;               // No camera
            mLights = new List<Light>();  // No lights
            mAspectRatio = 1;             // Assume square window
        }


        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access active camera
        public Camera Camera
        {
            get { return mCamera; }
            set
            {
                // Save new camera value
                mCamera = value;

                // Also update camera's aspect ratio
                if (mCamera != null)
                    mCamera.AspectRatio = mAspectRatio;
            }
        }

        // Access lights
        public List<Light> Lights
        {
            get { return mLights; }
            set { mLights = value; }
        }

        // Access aspect ratio
        public float AspectRatio
        {
            get { return mAspectRatio; }
            set
            {
                // Save new value
                mAspectRatio = value;

                // Also update active camera's aspect ratio
                if (mCamera != null)
                    mCamera.AspectRatio = mAspectRatio;
            }
        }


        ///////////////////////////////////////////////////////////////////////
        // Public methods

        // Add scene light
        public void AddLight(Light light)
        {
            // Add to the list
            mLights.Add(light);
        }

        // Remove scene light
        public void RemoveLight(Light light)
        {
            // Remove from the list
            mLights.Remove(light);
        }

        // Find scene light of specific type
        public Light FindLight(Light.LightType type)
        {
            // Use standard list find with comparison delegate
            return mLights.Find(delegate(Light light)
            {
                return light.Type == type;
            });
        }
    }
}
