////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      Sound.cs                                                              //
//                                                                            //
//      Sound track object                                                    //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // Sound class

    public class Sound
    {
        ///////////////////////////////////////////////////////////////////////
        // Private calss data

        string mName;   // Sound name
        bool mLoop;     // Looping flag
        Audio mAudio;   // Audio buffer
        int mSourceID;  // OpenAL source ID


        ///////////////////////////////////////////////////////////////////////
        // Construction/destruction

        // Constructor
        public Sound(string name, bool loop)
        {
            mName = name;
            mLoop = loop;

            // Load audio buffer
            mAudio = AssetManager.LoadAudio(name);

            // Generate OpenAL source ID
            mSourceID = AL.GenSource();

            // Attach the buffer to a source
            AL.Source(mSourceID, ALSourcei.Buffer, mAudio.BufferID);

            // Configure looping
            AL.Source(mSourceID, ALSourceb.Looping, loop);

            // Play
            AL.SourcePlay(mSourceID);
        }

        // Destructor
        ~Sound()
        {
            Stop();
        }


        ///////////////////////////////////////////////////////////////////////
        // Public methods

        // Stop playback
		public void Stop()
		{
			if (mSourceID >= 0)
			{
				AL.SourceStop(mSourceID);
				AL.DeleteSource(mSourceID);
				mSourceID = -1;
			}
		}

        // Set emitter position
		public void SetPosition(Vector3 position, float refDistance = 0.2f)
		{
			if (mSourceID >= 0)
			{
				AL.Source(mSourceID, ALSource3f.Position, ref position);
                AL.Source(mSourceID, ALSourcef.ReferenceDistance, refDistance);
            }
		}


        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access sound name
        public string Name
        {
            // Read-only
            get { return mName; }
        }

        // Access looping state
        public bool Looping
        {
            // Read-only
            get { return mLoop; }
        }

        // Access playing state
        public bool Playing
        {
            // Read-only
            get
            {
				if (mSourceID >= 0)
				{
					int state;
					AL.GetSource(mSourceID, ALGetSourcei.SourceState, out state);
					return (ALSourceState)state == ALSourceState.Playing;
				}

				return false;
            }
        }
    }
}
