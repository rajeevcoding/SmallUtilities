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
            var directoryPath = args[0];
            if(Directory.Exists(directoryPath))
            {
                var files = Directory.GetFiles(directoryPath, "*.mp4", SearchOption.AllDirectories);
                if(files.Length>0)
                {
                    var errorList = new List<string>();
                    double totalLength = 0.0;
                    foreach (var file in files)
                    {
                        try
                        {
                            totalLength = totalLength + Duration(file);
                        }
                        catch (Exception)
                        {
                            errorList.Add(Path.GetFileName(file));
                            continue;                
                        }
                    }

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
                        var secondsSuffix = Math.Truncate((totalLengthMinutes - minutesSuffix)* 60);
                        var minutesSuffixStr = minutesSuffix > 1 ? minutesSuffix + " Minutes" : minutesSuffix + " Minute";
                        var secondsSuffixStr = secondsSuffix > 1 ? secondsSuffix + " seconds" : secondsSuffix + " second";
                        suffix = " - " + minutesSuffixStr + " " +  secondsSuffixStr;
                    }

                    try
                    {
                        Directory.Move(directoryPath, directoryPath + " " + suffix);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Failed to rename the folder. Error :: " + ex.Message);
                        Environment.Exit(1);
                    }
                    
                    if (errorList.Count == 0)
                    {
                        Console.WriteLine("Folder renamed successfully with no errors.");
                    }
                    else
                    {
                        Console.WriteLine("Folder renamed but failed to get duration of following videos ::");
                        foreach (var error in errorList)
                        {
                            Console.WriteLine(Path.GetFileName(error));
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No files with extension .mp4 exists");
                }
            }
            else
            {
                Console.WriteLine("Specified directory does not exist.");
            }
        }
        public static double Duration(String file)
        {
            WindowsMediaPlayer wmp = new WindowsMediaPlayer();
            IWMPMedia mediainfo = wmp.newMedia(file);
            return mediainfo.duration;
        }
    }
}
