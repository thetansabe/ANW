using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Lame;

namespace ANWebServices.API
{
    public class AudioConverter
    {
        public static async Task WavToMp3(Stream input, string outputPath, int bitRate=128)
        {
            using (var reader = new WaveFileReader(input))
            {
                using (var writer = new LameMP3FileWriter(outputPath, reader.WaveFormat, bitRate))
                {
                    reader.CopyTo(writer);
                    reader.Flush();
                    writer.Flush();
                }
            }
        }
    }
}
