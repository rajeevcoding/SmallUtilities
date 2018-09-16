using System;
using WMPLib;
using System.IO;
using System.Collections.Generic;

namespace GetVideoLength
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length == 0 || args.Length > 1)
            {
                Console.WriteLine("Invalid Arguments. Please type /? for help");
            }
            if(args[0] == "/?")
            {
                ShowHelp();
            }
            else
            {
                VideoDirectoryProcessor.Process(args[0]);
            }
            
        }

        private static void ShowHelp()
        {
            Console.WriteLine("GetVideoLength");
            Console.WriteLine("Please provide only Parent Directory path.");
        }
    }

    class VideoDirectory
    {
        public string AbsolutePath { get; set; }
        public double TotalDuration { get; set; }
        public double FileDuration { get; set; }
        public List<string> ErrorList { get; set; }


        public List<VideoDirectory> SubVideoDirectories { get; set; }

        public VideoDirectory(string absolutePath)
        {
            AbsolutePath = absolutePath;
            SetSubVideoDirectories();
        }

        public void SetSubVideoDirectories()
        {
            var subDirectories = Directory.GetDirectories(AbsolutePath, "*", SearchOption.TopDirectoryOnly);
            if(subDirectories.Length>0)
            {
                SubVideoDirectories = new List<VideoDirectory>();
                foreach (var subDirectory in subDirectories)
                {
                    SubVideoDirectories.Add(new VideoDirectory(subDirectory));
                }
            }
        }

        public void Process()
        {
            
            ProcessFiles();
            
            if(SubVideoDirectories != null)
            {
                foreach (var subVideoDirectory in SubVideoDirectories)
                {
                    subVideoDirectory.Process();
                    TotalDuration = TotalDuration + subVideoDirectory.TotalDuration;
                }
                
            }
            TotalDuration = TotalDuration + FileDuration;
        }

        public void RenameDirectories()
        {
            if (SubVideoDirectories!=null)
            {
                foreach (var subDirectory in SubVideoDirectories)
                {
                    subDirectory.RenameDirectories();
                }
            }
            string suffix = GetSuffix(TotalDuration);
            RenameDirectory(suffix);

        }


        public void ProcessFiles()
        {
            ErrorList = new List<string>();
            var files = Directory.GetFiles(AbsolutePath, "*.mp4", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                FileDuration = 0.0;
                foreach (var file in files)
                {
                    try
                    {
                        FileDuration = FileDuration + GetLength(file);
                    }
                    catch (Exception)
                    {
                        ErrorList.Add(Path.GetFileName(file));
                        continue;
                    }
                }
            }
            else
            {
                Console.WriteLine("No files with extension .mp4 exists in - {0}", AbsolutePath);
            }
            
        }

        private void RenameDirectory(string suffix)
        {
            var newAbsolutePath = AbsolutePath + " " + suffix;
            try
            {
                Directory.Move(AbsolutePath, newAbsolutePath);
                AbsolutePath = newAbsolutePath;
            }
            catch (IOException ex)
            {
                throw new IOException("Failed to rename the folder. Error :: " + ex.Message);
            }

            if (ErrorList.Count == 0)
            {
                Console.WriteLine("Folder renamed successfully with no errors.");
            }
            else
            {
                Console.WriteLine("Folder renamed but failed to get duration of following videos ::");
                foreach (var error in ErrorList)
                {
                    Console.WriteLine(Path.GetFileName(error));
                }
            }
        }

        public double GetLength(String file)
        {
            WindowsMediaPlayer wmp = new WindowsMediaPlayer();
            IWMPMedia mediainfo = wmp.newMedia(file);
            return mediainfo.duration;
        }

        private string GetSuffix(double totalLength)
        {
            string suffix = string.Empty;
            double totalLengthMinutes = totalLength / 60;

            if (totalLengthMinutes > 60)
            {
                double totalLengthHours = totalLengthMinutes / 60;
                var hoursSuffix = Math.Truncate(totalLengthHours);
                var minutesSuffix = Math.Truncate((totalLengthHours - hoursSuffix) * 60);
                var minutesSuffixStr = minutesSuffix > 1 ? minutesSuffix + " Minutes" : minutesSuffix + " Minute";
                var hoursSuffixStr = hoursSuffix > 1 ? hoursSuffix + " Hours" : hoursSuffix + " hour";
                suffix = " - " + hoursSuffixStr + " " + minutesSuffixStr;
            }
            else
            {
                var minutesSuffix = Math.Truncate(totalLengthMinutes);
                var secondsSuffix = Math.Truncate((totalLengthMinutes - minutesSuffix) * 60);
                var minutesSuffixStr = minutesSuffix > 1 ? minutesSuffix + " Minutes" : minutesSuffix + " Minute";
                var secondsSuffixStr = secondsSuffix > 1 ? secondsSuffix + " seconds" : secondsSuffix + " second";
                suffix = " - " + minutesSuffixStr + " " + secondsSuffixStr;
            }

            return suffix;
        }
    }

    class VideoDirectoryProcessor
    {
       public static void Process(string parentDirectoryPath)
        {            
            if (Directory.Exists(parentDirectoryPath))
            {
                var MainVideoDirectory = new VideoDirectory(parentDirectoryPath);
                MainVideoDirectory.Process();
                MainVideoDirectory.RenameDirectories();
            }
            else
            {
                Console.WriteLine("Specified directory does not exist.");
            }
        }
    }
}
