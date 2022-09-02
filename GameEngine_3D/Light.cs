////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      Light.cs                                                              //
//                                                                            //
//      3D scene light object                                                 //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using OpenTK;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // Light class

    public class Light
    {
        ///////////////////////////////////////////////////////////////////////
        // Public types

        public enum LightType
        {
            Ambient,     // Ambient light
            Directional  // Directional light
        }

        ///////////////////////////////////////////////////////////////////////
        // Private class data

        LightType mType;   // Light type
        float mIntensity;  // Light intensity
        Vector3 mColour;   // Light colour
        Vector3 mPosition; // Light position (Directional light only)


        ///////////////////////////////////////////////////////////////////////
        // Constructors

        // Default constructor
        public Light()
        {
            // Default transform values
            mType = LightType.Ambient; // Ambient light
            mIntensity = 0.5f;         // 50%
            mColour = Vector3.One;     // Pure white light
            mPosition = Vector3.Zero;  // Unused for Ambient light
        }


        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access light type
        public LightType Type
        {
            get { return mType; }
            set { mType = value; }
        }

        // Access light intensity
        public float Intensity
        {
            get { return mIntensity; }
            set { mIntensity = value; }
        }

        // Access light colour
        public Vector3 Colour
        {
            get { return mColour; }
            set { mColour = value; }
        }

        // Access light position
        public Vector3 Position
        {
            get { return mPosition; }
            set { mPosition = value; }
        }
    }
}
