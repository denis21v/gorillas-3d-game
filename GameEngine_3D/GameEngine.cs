////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      GameEngine.cs                                                         //
//                                                                            //
//      Abstract game engine object                                           //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // GameEngine class

    public class GameEngine : GameWindow
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class data

        Vector4 mClearColour;               // Clear colour (RGBA)
        int mViewportWidth;                 // Viewport width
        int mViewportHeight;                // Viewport height
        double mGameTime;                   // Total in-game time since strating application (seconds)
        float mDeltaTime;                   // Delta time between last two sequential frames (seconds)
        List<IRenderTarget> mRenderTargets; // A list of active render targets
        List<Sound> mSounds;                // A list of active sound tracks
        AudioContext mAudioContext;         // OpenAL audio context


        ///////////////////////////////////////////////////////////////////////
        // Constructors

        // Standard game engine constructor
        public GameEngine(int windowWidth, int windowHeight, string windowTitle, bool startMaximized = false) :
           base(windowWidth, windowHeight, GraphicsMode.Default, windowTitle, 0,
           DisplayDevice.Default, 3, 2, // We want OpenGL 3.2 or later
           /*GraphicsContextFlags.Default*/ GraphicsContextFlags.ForwardCompatible)
        {
            // Check OpenGL and GLSL versions
            Console.WriteLine("GL Version = {0}, GLSL Version={1}",
                GL.GetString(StringName.Version),
                GL.GetString(StringName.ShadingLanguageVersion));

            mGameTime = 0;
            mRenderTargets = new List<IRenderTarget>();
            mSounds = new List<Sound>();
            mAudioContext = new AudioContext();

            // Bind input event delegates
            this.MouseDown += OnMouseButtonDown;
            this.MouseUp += OnMouseButtonUp;
            this.MouseMove += OnMouseMove;
            this.MouseWheel += OnMouseWheel;
            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;

            // Center game window
            //this.X = DisplayDevice.Default.Width / 2 - windowWidth / 2;
            //this.Y = DisplayDevice.Default.Height / 2 - windowHeight / 2;

            // Maximize game window if needed
            if (startMaximized)
                this.WindowState = WindowState.Maximized;

            // Default to black clear colour
            ClearColour = new Vector4(0, 0, 0, 1);
        }

        ///////////////////////////////////////////////////////////////////////
        // Public properties

        // Access clear colour
        public Vector4 ClearColour
        {
            get { return mClearColour; }
            set
            {
                mClearColour = value;

                // Must convert to Color struct first
                Color color = Color.FromArgb(
                    (int)(mClearColour.W * 255),
                    (int)(mClearColour.X * 255),
                    (int)(mClearColour.Y * 255),
                    (int)(mClearColour.Z * 255));

                // Set as OpenGL clear color
                GL.ClearColor(color);
            }
        }

        // Access viewport width
        public int ViewportWidth
        {
            // Read-only
            get { return mViewportWidth; }
        }

        // Access viewport height
        public int ViewportHeight
        {
            // Read-only
            get { return mViewportHeight; }
        }

        // Access game time
        public double GameTime
        {
            // Read-only
            get { return mGameTime; }
        }

        // Access delta time
        public float DeltaTime
        {
            // Read-only
            get { return mDeltaTime; }
        }


        ///////////////////////////////////////////////////////////////////////
        // Public methods

        // Add render target
        public void AddRenderTarget(IRenderTarget renderTarget)
        {
            mRenderTargets.Add(renderTarget);
        }

        // Remove render target
        public void RemoveRenderTarget(IRenderTarget renderTarget)
        {
            mRenderTargets.Remove(renderTarget);
        }

        // Play sound
        public Sound PlaySound(string name, bool loop = false)
        {
			if (mSounds != null)
			{
                // Add to active track list
				Sound sound = new Sound(name, loop);
				mSounds.Add(sound);
				return sound;
			}

			return null;
        }

        // Stop sound playback
        public void Stop(Sound sound)
        {
			if (mSounds != null)
			{
				sound.Stop();
				mSounds.Remove(sound);
			}
        }

        // Stop sound playback (by name)
        public void StopSound(string name)
        {
			if (mSounds != null)
			{
				// We want reverse order as we are deleting items!
				for (int i = mSounds.Count - 1; i >= 0; i --)
                //foreach (Sounds sound in mSounds)
				{
					Sound sound = mSounds[i];
					if (sound.Name == name)
					{
						sound.Stop();
						mSounds.Remove(sound);
					}
				}
			}
        }

        // Stop audio subsystem
        public void StopAllSounds()
        {
            // Stop any active sound tracks
			if (mSounds != null)
			{
				foreach (Sound sound in mSounds)
					sound.Stop();
			}
        }

        // Set audio listener position
		public void SetAudioListenerPosition(Vector3 position, Vector3 direction, Vector3 up)
		{
			AL.Listener(ALListener3f.Position, ref position);
            AL.Listener(ALListenerfv.Orientation, ref direction, ref up);
		}

        // Quit application
        public void Quit()
        {
            // Abort any playing audio tracks
            StopAllSounds();

            // Also prevent from playing any further sounds as we
            // are about to exit the app in few frames time
            mSounds = null;

            // Trigger termination
            Exit();
        }


        ///////////////////////////////////////////////////////////////////////
        // Overrides

        // On Load
        protected override void OnLoad(EventArgs e)
        {
            // Base
            base.OnLoad(e);

            // Enable essential OpenGL capabilities
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Invoke render targets OnLoad handlers
            foreach (IRenderTarget renderTarget in mRenderTargets)
                renderTarget.OnLoad();
        }

        // On Unload
        protected override void OnUnload(EventArgs e)
        {
            // Base
            base.OnUnload(e);

            // TBD: any special exit handling goes here
        }

        // On update frame
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // Base
            base.OnUpdateFrame(e);

            // Track in-game time
            mDeltaTime = (float)e.Time;
            mGameTime += mDeltaTime;

            // Invoke render targets OnUpdate handlers
            foreach (IRenderTarget renderTarget in mRenderTargets)
            {
                renderTarget.OnUpdate();
            }

            // Tidy up the active sound track list by removing the tracks
            // that already finished playing
            if (mSounds != null)
			{
				// We want reverse order as we are deleting items!
				for (int i = mSounds.Count - 1; i >= 0; i --)
                //foreach (Sounds sound in mSounds)
				{
					Sound sound = mSounds[i];
					if (!sound.Looping && !sound.Playing)
						mSounds.Remove(sound);
				}
			}
        }

        // On render frame
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // Base
            base.OnRenderFrame(e);

            // Clear buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Invoke render targets OnRender handlers
            foreach (IRenderTarget renderTarget in mRenderTargets)
                renderTarget.OnRender();

            // Swap frame buffers
            GL.Flush();
            SwapBuffers();
        }

        // On Resize
        protected override void OnResize(EventArgs e)
        {
            // Base
            base.OnResize(e);

            // Backup viewport dimensions
            mViewportWidth = ClientRectangle.Width;
            mViewportHeight = ClientRectangle.Height;

            // Resize viewport
            GL.Viewport(0, 0, mViewportWidth, mViewportHeight);

            // Invoke render targets OnResize handlers
            foreach (IRenderTarget renderTarget in mRenderTargets)
                renderTarget.OnResize();
        }

        // On mouse button down
        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Invoke render targets OnMouseDown handlers
            foreach (IRenderTarget renderTarget in mRenderTargets)
            {
                // Keep trying until input event handled
                if (renderTarget.OnMouseDown(e))
                    break;
            }
        }

        // On mouse button up
        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Invoke render targets OnMouseUp handlers
            foreach (IRenderTarget renderTarget in mRenderTargets)
            {
                // Keep trying until input event handled
                if (renderTarget.OnMouseUp(e))
                    break;
            }
        }

        // On mouse move
        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            // Invoke render targets OnMouseMove handlers
            foreach (IRenderTarget renderTarget in mRenderTargets)
            {
                // Keep trying until input event handled
                if (renderTarget.OnMouseMove(e))
                    break;
            }
        }

        // On mouse wheel
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Invoke render targets OnMouseWheel handlers
            foreach (IRenderTarget renderTarget in mRenderTargets)
            {
                // Keep trying until input event handled
                if (renderTarget.OnMouseWheel(e))
                    break;
            }
        }

        // On key down
        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            // Invoke render targets OnKeyDown handlers
            foreach (IRenderTarget renderTarget in mRenderTargets)
            {
                // Keep trying until input event handled
                if (renderTarget.OnKeyDown(e))
                    break;
            }
        }

        // On key up
        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            // Invoke render targets OnKeyUp handlers
            foreach (IRenderTarget renderTarget in mRenderTargets)
            {
                // Keep trying until input event handled
                if (renderTarget.OnKeyUp(e))
                    break;
            }
        }
    }
}
