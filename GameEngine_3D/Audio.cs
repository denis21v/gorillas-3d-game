////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      Audiuo.cs                                                             //
//                                                                            //
//      Audio buffer object                                                   //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // Audio class

    public class Audio
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class data

        int mBufferID; // OpenAL buffer ID


        ///////////////////////////////////////////////////////////////////////
        // Construction/destruction

        // Constructor (raw audio data)
        public Audio(int numChannels, int bitsPerSample, int sampleRate, byte[] audioData)
        {
            // Create audio buffer
            Create(numChannels, bitsPerSample, sampleRate, audioData);
        }

        // Constructor (audio file name)
        public Audio(string audioName)
        {
            // Full file path
            string audioFilePath = "Assets/Audio/" + audioName;

            // Open audio file
            Stream stream = File.Open(audioFilePath, FileMode.Open);
            if (stream == null)
                throw new FileNotFoundException("Failed to open " + audioFilePath);

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException(audioFilePath + " is not a wave file");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException(audioFilePath + " is not a wave file");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException(audioFilePath + " is not supported");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();
                int left = format_chunk_size - 16;
                if (left > 0)
                {
                    // Skip past the end of format chunk!
                    reader.ReadBytes(left);
                }

                while (reader.BaseStream.Length > 0)
                {
                    string chunk_signature = new string(reader.ReadChars(4));
                    int chunk_size = reader.ReadInt32();
                    if (chunk_signature == "data")
                        break;
                    reader.ReadBytes(chunk_size);
                }

                byte[] audio_data = reader.ReadBytes((int)reader.BaseStream.Length);
                if (audio_data == null)
                    throw new IOException("Failed to read " + audioFilePath);

                // Create audio buffer
                Create(num_channels, bits_per_sample, sample_rate, audio_data);
            }
        }

        // Destructor
        ~Audio()
        {
            // Tidy up
            AL.DeleteBuffer(mBufferID);
        }


        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access buffer ID
        public int BufferID
        {
            // Read-only
            get { return mBufferID; }
        }


        ///////////////////////////////////////////////////////////////////////
        // Private methods

        // Create audio buffer
        void Create(int numChannels, int bitsPerSample, int sampleRate, byte[] audioData)
        {
            // Map audio data format to OpenAL format
            ALFormat audio_format =
                numChannels == 1 && bitsPerSample == 8 ? ALFormat.Mono8 :
                numChannels == 1 && bitsPerSample == 16 ? ALFormat.Mono16 :
                numChannels == 2 && bitsPerSample == 8 ? ALFormat.Stereo8 :
                numChannels == 2 && bitsPerSample == 16 ? ALFormat.Stereo16 :
                (ALFormat)0; // unknown

            // Generate OpenAL buffer ID
            mBufferID = AL.GenBuffer();

            // Upload audio data to OpenAL buffer
            AL.BufferData(mBufferID, audio_format, audioData, audioData.Length, sampleRate);

            // Success?
            if (AL.GetError() != ALError.NoError)
                throw new NotSupportedException("Specified wave audio format is not supported");
        }
    }
}
