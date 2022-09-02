////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      Utility.cs                                                            //
//                                                                            //
//      Helper utility classes and structs                                    //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using OpenTK;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // Utility class

    public static class Utility
    {
        ///////////////////////////////////////////////////////////////////////
        // Random numbers

        // Generate random float number from given range
        public static float RandomFloat(Random random, float min, float max)
        {
            return min + (max - min) * (float)random.NextDouble();
        }


        ///////////////////////////////////////////////////////////////////////
        // Helper trigonormetry functions

        // Distance between points 2D
        public static float GetDistance2D(float x1, float z1, float x2, float z2)
        {
            float dx = x1 - x2;
            float dz = z1 - z2;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        // Distance between points 2D
        public static float GetDistance2D(IVector3 pos1, IVector3 pos2)
        {
            return GetDistance2D(pos1.X, pos1.Z, pos2.X, pos2.Z);
        }

        // Distance between points 2D
        public static float GetDistance2D(Vector3 pos1, Vector3 pos2)
        {
            return GetDistance2D(pos1.X, pos1.Z, pos2.X, pos2.Z);
        }

        // Distance between points 3D
        public static float GetDistance3D(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            float dz = z1 - z2;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        // Distance between points 3D
        public static float GetDistance3D(IVector3 pos1, IVector3 pos2)
        {
            return GetDistance3D(pos1.X, pos1.Y, pos1.Z, pos2.X, pos2.Y, pos2.Z);
        }

        // Distance between points 3D
        public static float GetDistance3D(Vector3 pos1, Vector3 pos2)
        {
            return GetDistance3D(pos1.X, pos1.Y, pos1.Z, pos2.X, pos2.Y, pos2.Z);
        }

        // Angle of a point on circle
        public static float PointOnCircleGetAngle2D(float cx, float cy, float x, float y)
        {
            return 180.0f - MathHelper.RadiansToDegrees((float)Math.Atan2(cy - y, x - cx));
        }

        // Point on circle rotated by given angle
        public static void PointOnCircleRotatedByAngle2D(float cx, float cy, float x, float y, float a, ref float rx, ref float ry)
        {
            float r = MathHelper.DegreesToRadians(a - 180.0f);
            float sin = (float)Math.Sin(r);
            float cos = (float)Math.Cos(r);
            float dx = (x - cx);
            float dy = (y - cy);

            rx = cx + cos * dx - sin * dy;
            ry = cy + sin * dx + cos * dy;
        }

        // Normalize angle (int)
        public static void NormalizeAngle(ref int angle)
        {
            while (angle < 0)
                angle += 360;
            while (angle >= 360)
                angle -= 360;
        }

        // Normalize angle (float)
        public static void NormalizeAngle(ref float angle)
        {
            while (angle < 0.0f)
                angle += 360.0f;
            while (angle >= 360.0f)
                angle -= 360.0f;
        }

        // Rotate vector by Euler angles
        public static Vector3 RotateVector(Vector3 vector, float x_roll_bank, float y_yaw_heading, float z_pitch_altitude)
        {
            Vector3 result;

            // Degrees to radians
            x_roll_bank = MathHelper.DegreesToRadians(x_roll_bank);
            y_yaw_heading = MathHelper.DegreesToRadians(y_yaw_heading);
            z_pitch_altitude = MathHelper.DegreesToRadians(z_pitch_altitude);

            // For some reason it appears Qauternion * Vector3 transform is broken
            // in OpenTK 3.0. This used to work perfectly fine in OpenTK 2.0!

            #if false

            ///////////////////////////////////////////////////////////////////
            // OpenTK 2.0

            Quaternion quaternion = new Quaternion(z_pitch_altitude, y_yaw_heading, x_roll_bank);
            result = quaternion * vector;

            #else

            ///////////////////////////////////////////////////////////////////
            // OpenTK 3.0

            // The code below is an extract from OpenTK 2.0 code base

            // Quaternion constructor
            x_roll_bank *= 0.5f;
            y_yaw_heading *= 0.5f;
            z_pitch_altitude *= 0.5f;

            float num1 = (float)Math.Cos((double)y_yaw_heading);
            float num2 = (float)Math.Cos((double)z_pitch_altitude);
            float num3 = (float)Math.Cos((double)x_roll_bank);
            float num4 = (float)Math.Sin((double)y_yaw_heading);
            float num5 = (float)Math.Sin((double)z_pitch_altitude);
            float num6 = (float)Math.Sin((double)x_roll_bank);

            Vector3 xyz;
            var w = (float)((double)num1 * (double)num2 * (double)num3 - (double)num4 * (double)num5 * (double)num6);
            xyz.X = (float)((double)num4 * (double)num5 * (double)num3 + (double)num1 * (double)num2 * (double)num6);
            xyz.Y = (float)((double)num4 * (double)num2 * (double)num3 + (double)num1 * (double)num5 * (double)num6);
            xyz.Z = (float)((double)num1 * (double)num5 * (double)num3 - (double)num4 * (double)num2 * (double)num6);

            // Vector3.Transform(Quaternion)
            Vector3 temp, temp2;
            Vector3.Cross(ref xyz, ref vector, out temp);
            Vector3.Multiply(ref vector, w, out temp2);
            Vector3.Add(ref temp, ref temp2, out temp);
            Vector3.Cross(ref xyz, ref temp, out temp2);
            Vector3.Multiply(ref temp2, 2f, out temp2);
            Vector3.Add(ref vector, ref temp2, out result);

            #endif

            result.Normalize();

            return result;
        }

        // Rotate vector by Euler angles
        public static Vector3 RotateVector(Vector3 vector, Vector3 angles)
        {
            return RotateVector(vector, angles.X, angles.Y, angles.Z);
        }


        ///////////////////////////////////////////////////////////////////////
        // Basic generic animation methods based on simple easing functions

        // Linear animation
        public static float LinearAnimation(float min, float max, float time)
        {
            float range = max - min;
            return min + range * time;
        }

        // Linear animation
        public static Vector3 LinearAnimation(Vector3 min, Vector3 max, float time)
        {
            Vector3 range = max - min;
            return min + range * time;
        }

        // Sine Animation
        public static float SineAnimation(float min, float max, float time)
        {
            float range = max - min;
            float factor;

            if (time < 0.5f)
                factor = SineAnimationInFactor(time * 2) * 0.5f;
            else
                factor = 0.5f + SineAnimationOutFactor(time * 2 - 1.0f) * 0.5f;

            return min + range * factor;
        }

        // Sine Animation
        public static Vector3 SineAnimation(Vector3 min, Vector3 max, float time)
        {
            Vector3 range = max - min;
            float factor;

            if (time < 0.5f)
                factor = SineAnimationInFactor(time * 2) * 0.5f;
            else
                factor = 0.5f + SineAnimationOutFactor(time * 2 - 1.0f) * 0.5f;

            return min + range * factor;
        }

        // Sine Animation (in)
        public static float SineAnimationIn(float min, float max, float time)
        {
            float range = max - min;
            float factor = SineAnimationInFactor(time);
            return min + range * factor;
        }

        // Sine Animation (in)
        public static Vector3 SineAnimationIn(Vector3 min, Vector3 max, float time)
        {
            Vector3 range = max - min;
            float factor = SineAnimationInFactor(time);
            return min + range * factor;
        }

        // Sine Animation (out)
        public static float SineAnimationOut(float min, float max, float time)
        {
            float range = max - min;
            float factor = SineAnimationOutFactor(time);
            return min + range * factor;
        }

        // Sine Animation (out)
        public static Vector3 SineAnimationOut(Vector3 min, Vector3 max, float time)
        {
            Vector3 range = max - min;
            float factor = SineAnimationOutFactor(time);
            return min + range * factor;
        }

        // Sine in animation factor
        public static float SineAnimationInFactor(float time)
        {
            return 1.0f - (float)Math.Cos(MathHelper.PiOver2 * time);
        }

        // Sine out animation factor
        public static float SineAnimationOutFactor(float time)
        {
            return 1.0f - SineAnimationInFactor(1.0f - time);
        }
    }



    ///////////////////////////////////////////////////////////////////////
    // Integer Vector3

    public struct IVector3
    {
        public int X, Y, Z;

        // Default constructor
        public IVector3(int x = 0, int y = 0, int z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        // Comparison operators
        public static bool operator ==(IVector3 v1, IVector3 v2)
        {
            return (v1.GetHashCode() == v2.GetHashCode());
        }

        public static bool operator !=(IVector3 v1, IVector3 v2)
        {
            return (v1.GetHashCode() != v2.GetHashCode());
        }

        // Compare two IVector objects
        public override bool Equals(Object obj)
        {
            if ((obj != null) && (obj is IVector3))
            {
                IVector3 other = (IVector3)obj;
                return (GetHashCode() == other.GetHashCode());
            }

            return false;
        }

        // Unique hash for quick comparison
        public override int GetHashCode()
        {
            // As our levels will never extend beyond ~20-30 tiles
            // in either direction it is pretty safe to use 10 bits
            // per field for unique hash code
            return (X << 20) | (Y << 10) | Z;
        }
    }

}
