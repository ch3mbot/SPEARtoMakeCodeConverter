using System;
using System.IO;
using System.Collections.Generic;

// one 'frame' of a changing sine wave
class AudioFrame
{
    public float freq;
    public float amp;

    public AudioFrame(float freq, float amp)
    {
        this.freq = freq;
        this.amp = amp;
    }
}

// a sine wave that changes frequency and amplitude over time
class AudioLine
{
    public float firstFrame;
    public float lastFrame;
    public int index;
    public List<AudioFrame> frames;

    public AudioLine(float firstFrame, int index)
    {
        this.firstFrame = firstFrame;
        this.index = index;
        this.frames = new List<AudioFrame>();
    }
}

class Program
{
    static string fileInPath;
    static string fileOutPath;

    static void Main(string[] args)
    {
        GetPaths();
        ProcessSPEARFile();
    }

    static void GetPaths()
    {
        Console.WriteLine("Path to SPEAR text file: ");

        fileInPath = Console.ReadLine();

        Console.WriteLine("Output path: ");

        fileOutPath = Console.ReadLine();
    }

    static void ProcessSPEARFile()
    {
        //each audio wave has a unique channel, so dictionary used
        Dictionary<int, AudioLine> audioLines = new Dictionary<int, AudioLine> ();

        try
        {
            using (StreamReader sr = new StreamReader(fileInPath))
            {
                //skip 5 lines of header
                for(int i = 0; i < 5; i++)
                    sr.ReadLine();

                //process actual data
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    //first two numbers are start frame and track count
                    string[] numbers = line.Split(' ');
                    float frame = float.Parse(numbers[0]);
                    int trackCount = int.Parse(numbers[1]);

                    //get wave data in 3 nubmer chunks
                    for (int i = 2; i < numbers.Length; i += 3)
                    {
                        int channel = int.Parse(numbers[i]);
                        float freq = float.Parse(numbers[i + 1]);
                        float amp = float.Parse(numbers[i + 2]);

                        AudioFrame audFrame = new AudioFrame(freq, amp);
                        if (!audioLines.ContainsKey(channel))
                        {
                            audioLines.Add(channel, new AudioLine(frame, channel));
                            Console.WriteLine("added new wave on channel: " + channel);
                        }
                        audioLines[channel].frames.Add(audFrame);
                        audioLines[channel].lastFrame = frame + 0.01f;
                    }
                }
            }

            //process output to makecode compatible string. FIXME: Make into buffer instead later.
            string outputStr = "[\n";
            foreach (AudioLine audioLine in audioLines.Values)
            {
                outputStr += $"\t[";

                outputStr += $"[[{audioLine.firstFrame}, {audioLine.lastFrame}]], [\n";

                for(int j = 0; j < audioLine.frames.Count; j++)
                {
                    outputStr += $"\t\t[{audioLine.frames[j].freq}, {audioLine.frames[j].amp}],\n";
                }

                //add duplicate of last frame so that the last frame has a play time. FIXME verify if this is necessary or not
                outputStr += $"\t\t[{audioLine.frames[audioLine.frames.Count - 1].freq}, {audioLine.frames[audioLine.frames.Count - 1].amp}],\n";

                outputStr += "\t],\n";

                outputStr += $"],\n";
            }

            File.WriteAllText(fileOutPath, outputStr);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
}
