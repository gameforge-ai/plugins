using System;
using System.IO;
using UnityEngine;

namespace GameForgeAI.Utils
{
    public static class AudioClipConverter
    {
        public static byte[] ToWavBytes(AudioClip clip)
        {
            using (var memoryStream = new MemoryStream())
            {
                WriteWavHeader(memoryStream, clip);
                WriteWav(memoryStream, clip);
                return memoryStream.GetBuffer();
            }
        }

        private static void WriteWavHeader(MemoryStream memoryStream, AudioClip clip)
        {
            var frequency = clip.frequency;
            var channels = clip.channels;
            var samples = clip.samples;

            memoryStream.Seek(0, SeekOrigin.Begin);

            var riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            memoryStream.Write(riff, 0, 4);

            var chunkSize = BitConverter.GetBytes(memoryStream.Length - 8);
            memoryStream.Write(chunkSize, 0, 4);

            var wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            memoryStream.Write(wave, 0, 4);

            var fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            memoryStream.Write(fmt, 0, 4);

            var subChunk1 = BitConverter.GetBytes(16);
            memoryStream.Write(subChunk1, 0, 4);

            short one = 1;

            var audioFormat = BitConverter.GetBytes(one);
            memoryStream.Write(audioFormat, 0, 2);

            var numChannels = BitConverter.GetBytes(channels);
            memoryStream.Write(numChannels, 0, 2);

            var sampleRate = BitConverter.GetBytes(frequency);
            memoryStream.Write(sampleRate, 0, 4);

            var byteRate =
                BitConverter.GetBytes(frequency * channels *
                                      2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
            memoryStream.Write(byteRate, 0, 4);

            var blockAlign = (ushort) (channels * 2);
            memoryStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

            short bps = 16;
            var bitsPerSample = BitConverter.GetBytes(bps);
            memoryStream.Write(bitsPerSample, 0, 2);

            var datastring = System.Text.Encoding.UTF8.GetBytes("data");
            memoryStream.Write(datastring, 0, 4);

            var subChunk2 = BitConverter.GetBytes(samples * channels * 2);
            memoryStream.Write(subChunk2, 0, 4);
        }

        private static void WriteWav(MemoryStream memoryStream, AudioClip clip)
        {
            var samples = new float[clip.samples];

            clip.GetData(samples, 0);

            var intData = new short[samples.Length];

            var bytesData = new byte[samples.Length * 2];

            var rescaleFactor = 32767; //to convert float to Int16

            for (var i = 0; i < samples.Length; i++)
            {
                intData[i] = (short) (samples[i] * rescaleFactor);
                var byteArr = new byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            memoryStream.Write(bytesData, 0, bytesData.Length);
        }
    }
}