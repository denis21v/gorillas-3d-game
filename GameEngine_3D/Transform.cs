////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      Transform.cs                                                          //
//                                                                            //
//      Standard 3D TRS (Translate, Rotate, Scale) transform                  //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using OpenTK;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // Transform class

    public class Transform
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class data

        Vector3 mTranslation;     // Translation component
        Vector3 mScale;           // Scale component
        Vector3 mRotation;        // Rotation component
        Matrix4 mModelMatrix;     // Model matrix (map object local coordinate space to world space)
        bool mModelMatrixChanged; // Model matrix changed flag


        ///////////////////////////////////////////////////////////////////////
        // Constructors

        // Default constructor
        public Transform()
        {
            // Default transform values
            mTranslation = Vector3.Zero; // Origin
            mScale = Vector3.One;        // No scaling
            mRotation = Vector3.Zero;    // No rotation
            mModelMatrixChanged = true;  // Force model matrix update
        }

        // Scalar values constructor
        public Transform(float translation_x, float translation_y, float translation_z,
                        float scale_x = 1.0f, float scale_y = 1.0f, float scale_z = 1.0f,
                        float rotation_x = 0.0f, float rotation_y = 0.0f, float rotation_z = 0.0f)
        {
            mTranslation = new Vector3(translation_x, translation_y, translation_z);
            mScale = new Vector3(scale_x, scale_y, scale_z);
            mRotation = new Vector3(rotation_x, rotation_y, rotation_z);
            mModelMatrixChanged = true;  // Force model matrix update
        }

        // Vector values constructor
        public Transform(Vector3 translation, Vector3 scale, Vector3 rotation)
        {
            mTranslation = translation;
            mScale = scale;
            mRotation = rotation;
            mModelMatrixChanged = true;  // Force model matrix update
        }


        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access transform translation parameter
        public Vector3 Translation
        {
            get { return mTranslation; }
            set { mTranslation = value; mModelMatrixChanged = true; }
        }

        // Access transform scale parameter
        public Vector3 Scale
        {
            get { return mScale; }
            set { mScale = value; mModelMatrixChanged = true; }
        }

        // Access transform rotation parameter
        public Vector3 Rotation
        {
            get { return mRotation; }
            set { mRotation = value; mModelMatrixChanged = true; }
        }

        // Access Model matrix
        public Matrix4 ModelMatrix
        {
            // Read-only
            get
            {
                // Matrix calculations are rather expensive. Only compute model matrix
                // if any of transform parameters changed. Otherwise return cached
                // Model matrix.
                if (mModelMatrixChanged)
                    ComputeModelMatrix();

                return mModelMatrix;
            }
        }


        ///////////////////////////////////////////////////////////////////////
        // Public methods

        // Translate to position
        public void TranslateTo(float translation_x, float translation_y, float translation_z)
        {
            mTranslation.X = translation_x;
            mTranslation.Y = translation_y;
            mTranslation.Z = translation_z;
            mModelMatrixChanged = true;
        }

        public void TranslateTo(Vector3 translation)
        {
            mTranslation = translation;
            mModelMatrixChanged = true;
        }

        public void TranslateXTo(float translation_x)
        {
            mTranslation.X = translation_x;
            mModelMatrixChanged = true;
        }

        public void TranslateYTo(float translation_y)
        {
            mTranslation.Y = translation_y;
            mModelMatrixChanged = true;
        }

        public void TranslateZTo(float translation_z)
        {
            mTranslation.Z = translation_z;
            mModelMatrixChanged = true;
        }

        // Translate by amount
        public void TranslateBy(float translation_x, float translation_y, float translation_z)
        {
            mTranslation.X += translation_x;
            mTranslation.Y += translation_y;
            mTranslation.Z += translation_z;
            mModelMatrixChanged = true;
        }

        public void TranslateBy(Vector3 translation)
        {
            mTranslation += translation;
            mModelMatrixChanged = true;
        }

        public void TranslateXBy(float translation_x)
        {
            mTranslation.X += translation_x;
            mModelMatrixChanged = true;
        }

        public void TranslateYBy(float translation_y)
        {
            mTranslation.Y += translation_y;
            mModelMatrixChanged = true;
        }

        public void TranslateZBy(float translation_z)
        {
            mTranslation.Z += translation_z;
            mModelMatrixChanged = true;
        }

        // Scale to value
        public void ScaleTo(float scale_x, float scale_y, float scale_z)
        {
            mScale.X = scale_x;
            mScale.Y = scale_y;
            mScale.Z = scale_z;
            mModelMatrixChanged = true;
        }

        public void ScaleTo(Vector3 scale)
        {
            mScale = scale;
            mModelMatrixChanged = true;
        }

        public void ScaleXTo(float scale_x)
        {
            mScale.X = scale_x;
            mModelMatrixChanged = true;
        }

        public void ScaleYTo(float scale_y)
        {
            mScale.Y = scale_y;
            mModelMatrixChanged = true;
        }

        public void ScaleZTo(float scale_z)
        {
            mScale.Z = scale_z;
            mModelMatrixChanged = true;
        }

        // Scale by amount
        public void ScaleBy(float scale_x, float scale_y, float scale_z)
        {
            mScale.X += scale_x;
            mScale.Y += scale_y;
            mScale.Z += scale_z;
            mModelMatrixChanged = true;
        }

        public void ScaleBy(Vector3 scale)
        {
            mScale += scale;
            mModelMatrixChanged = true;
        }

        public void ScaleXBy(float scale_x)
        {
            mScale.X += scale_x;
            mModelMatrixChanged = true;
        }

        public void ScaleYBy(float scale_y)
        {
            mScale.Y += scale_y;
            mModelMatrixChanged = true;
        }

        public void ScaleZBy(float scale_z)
        {
            mScale.Z += scale_z;
            mModelMatrixChanged = true;
        }

        // Rotate to value
        public void RotateTo(float rotation_x, float rotation_y, float rotation_z)
        {
            mRotation.X = rotation_x;
            mRotation.Y = rotation_y;
            mRotation.Z = rotation_z;
            mModelMatrixChanged = true;
        }

        public void RotateTo(Vector3 scale)
        {
            mRotation = scale;
            mModelMatrixChanged = true;
        }

        public void RotateXTo(float rotation_x)
        {
            mRotation.X = rotation_x;
            mModelMatrixChanged = true;
        }

        public void RotateYTo(float rotation_y)
        {
            mRotation.Y = rotation_y;
            mModelMatrixChanged = true;
        }

        public void RotateZTo(float rotation_z)
        {
            mRotation.Z = rotation_z;
            mModelMatrixChanged = true;
        }

        // Rotate by amount
        public void RotateBy(float rotation_x, float rotation_y, float rotation_z)
        {
            mRotation.X += rotation_x;
            mRotation.Y += rotation_y;
            mRotation.Z += rotation_z;
            mModelMatrixChanged = true;
        }

        public void RotateBy(Vector3 rotation)
        {
            mRotation += rotation;
            mModelMatrixChanged = true;
        }

        public void RotateXBy(float rotation_x)
        {
            mRotation.X += rotation_x;
            mModelMatrixChanged = true;
        }

        public void RotateYBy(float rotation_y)
        {
            mRotation.Y += rotation_y;
            mModelMatrixChanged = true;
        }

        public void RotateZBy(float rotation_z)
        {
            mRotation.Z += rotation_z;
            mModelMatrixChanged = true;
        }


        ///////////////////////////////////////////////////////////////////////
        // Private methods

        // Compute Model matrix based on TRS
        void ComputeModelMatrix()
        {
            Matrix4 matrixTranslation, matrixScale, matrixRotation;

            // Compute translation matrix
            if (mTranslation == Vector3.Zero)
                matrixTranslation = Matrix4.Identity;
            else
                Matrix4.CreateTranslation(ref mTranslation, out matrixTranslation);

            // Compute scale matrix
            if (mScale == Vector3.One)
                matrixScale = Matrix4.Identity;
            else
                Matrix4.CreateScale(ref mScale, out matrixScale);

            // Compute rotation matrix
            matrixRotation = Matrix4.Identity;
            if (mRotation.X != 0.0f)
                matrixRotation *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(mRotation.X));
            if (mRotation.Y != 0.0f)
                matrixRotation *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(mRotation.Y));
            if (mRotation.Z != 0.0f)
                matrixRotation *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(mRotation.Z));

            // Compute the final TRS transform matrix
            mModelMatrix = matrixRotation * matrixScale * matrixTranslation;
            mModelMatrixChanged = false;
        }
    }
}
