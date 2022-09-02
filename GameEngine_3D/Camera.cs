////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      Camera.cs                                                             //
//                                                                            //
//      Abstract 3D camera object                                             //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using OpenTK;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // Camera class

    public class Camera
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class data

        float mFieldOfView;             // Field of view (aka FOV) in deg.
        private float mAspectRatio;     // Screen aspect ratio (Width / Height)
        private float mNearClipPlane;   // Near clipping plane
        private float mFarClipPlane;    // Far clipping plane
        Vector3 mPosition;              // Camera position
        Vector3 mRotation;              // Camera rotation (degrees)
        Vector3 mForwardVector;         // Camera 'forward' vector (vector parallel to looking direction)
        Vector3 mRightVector;           // Camera 'right' vector (vector facing right perpendicular to looking direction)
        Vector3 mUpVector;              // Camera 'up' vector (vector facing up perpendicular to looking direction)
        Vector3 mLookAt;                // Camera 'look at' taget
        Matrix4 mViewMatrix;            // View matrix (map world coordinate space to camera space)
        bool mViewMatrixChanged;        // View matrix changed flag
        Matrix4 mProjectionMatrix;      // Projection matrix (map camera coordinate space to screen space)
        bool mProjectionMatrixChanged;  // Projection matrix changed flag
        bool mUseRotation;              // Either use Rotation or LookAt (default) parameter


        ///////////////////////////////////////////////////////////////////////
        // Constructors

        // Standard camera constructor
        public Camera()
        {
            // Default settings
            FieldOfView = 60.0f;      // 60deg
            AspectRatio = 1.0f;       // Square window
            NearClipPlane = 0.5f;     // Clipping from 0.5
            FarClipPlane = 100.0f;    // to 100.0
            Position = Vector3.UnitZ; // Positioned at 0,0,1
            LookAt = Vector3.Zero;    // Looking at 0,0,0
        }

        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access field of view
        public float FieldOfView
        {
            get { return mFieldOfView; }
            set { mFieldOfView = value; mProjectionMatrixChanged = true; }
        }

        // Access aspect ratio
        public float AspectRatio
        {
            get { return mAspectRatio; }
            set { mAspectRatio = value; mProjectionMatrixChanged = true; }
        }

        // Access near clipping plane
        public float NearClipPlane
        {
            get { return mNearClipPlane; }
            set { mNearClipPlane = value; mProjectionMatrixChanged = true; }
        }

        // Access far clipping plane
        public float FarClipPlane
        {
            get { return mFarClipPlane; }
            set { mFarClipPlane = value; mProjectionMatrixChanged = true; }
        }

        // Access projection matrix
        public Matrix4 ProjectionMatrix
        {
            // Read-only
            get
            {
                // Matrix calculations are rather expensive. Only compute projection matrix
                // if any of projection parameters changed. Otherwise return cached
                // projection matrix.
                if (mProjectionMatrixChanged)
                    ComputeProjectionMatrix();

                return mProjectionMatrix;
            }
        }

        // Access camera position
        public Vector3 Position
        {
            get { return mPosition; }
            set { mPosition = value; mViewMatrixChanged = true; }
        }

        // Access camera rotation
        public Vector3 Rotation
        {
            get { return mRotation; }
            set
            {
                mRotation = value;
                mUseRotation = true;
                mViewMatrixChanged = true;
            }
        }

        // Access camera forward vector
        public Vector3 ForwardVector
        {
            // Readonly
            get
            {
                // Might need to update view matrix to obtain
                // fresh camera orientation parameters
                if (mViewMatrixChanged)
                    ComputeViewMatrix();

                return mForwardVector;
            }
        }

        // Access camera right vector
        public Vector3 RightVector
        {
            // Read-only
            get
            {
                // Might need to update view matrix to obtain
                // fresh camera orientation parameters
                if (mViewMatrixChanged)
                    ComputeViewMatrix();

                return mRightVector;
            }
        }

        // Access camera up vector
        public Vector3 UpVector
        {
            // Read-only
            get
            {
                // Might need to update view matrix to obtain
                // fresh camera orientation parameters
                if (mViewMatrixChanged)
                    ComputeViewMatrix();

                return mUpVector;
            }
        }

        // Access camera 'look at' target
        public Vector3 LookAt
        {
            get
            {
                // Might need to update view matrix to obtain
                // fresh camera orientation parameters
                if (mUseRotation && mViewMatrixChanged)
                    ComputeViewMatrix();

                return mLookAt;
            }

            set
            {
                mLookAt = value;
                mUseRotation = false;
                mViewMatrixChanged = true;
            }
        }

        // Access view matrix
        public Matrix4 ViewMatrix
        {
            // Read-only
            get
            {
                // Matrix calculations are rather expensive. Only compute view matrix
                // if any of view parameters changed. Otherwise return cached
                // view matrix.
                if (mViewMatrixChanged)
                    ComputeViewMatrix();

                return mViewMatrix;
            }
        }


        ///////////////////////////////////////////////////////////////////////
        // Private methods

        // Compute projection matrix
        void ComputeProjectionMatrix()
        {
            // Compute perspective projection matrix
            float fieldOfViewRad = MathHelper.DegreesToRadians(mFieldOfView);
            mProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fieldOfViewRad,
                mAspectRatio, mNearClipPlane, mFarClipPlane);
            mProjectionMatrixChanged = false;
        }

        // Compute view matrix
        void ComputeViewMatrix()
        {
            if (mUseRotation)
            {
                // First find the 'forward' vector (vector parallel to looking
                // direction)
                mForwardVector = Utility.RotateVector(Vector3.UnitZ, mRotation);

                // Find the 'right' vector (vector facing right perpendicular
                // to looking direction)
                mRightVector = Utility.RotateVector(Vector3.UnitX, mRotation);

                // Find the 'up' vector (vector facing up perpendicular
                // to looking direction)
                mUpVector = Utility.RotateVector(Vector3.UnitY, mRotation);

                // Find look at target
                mLookAt = mPosition + mForwardVector;
            }
            else
            {
                // Find the 'forward' vector (vector parallel to looking
                // direction)
                mForwardVector = mPosition - mLookAt;
                mForwardVector.Normalize();

                // Find the 'right' vector (vector facing right perpendicular
                // to looking direction)
                mRightVector = Vector3.Cross(mForwardVector, Vector3.UnitY);
                mRightVector.Normalize();

                // Find the 'up' vector (vector facing up perpendicular
                // to looking direction)
                mUpVector = Vector3.Cross(mRightVector, mForwardVector);
                mUpVector.Normalize();
            }

            // Finally compute the view matrix
            mViewMatrix = Matrix4.LookAt(mPosition, mLookAt, mUpVector);
            mViewMatrixChanged = false;
        }
    }
}
