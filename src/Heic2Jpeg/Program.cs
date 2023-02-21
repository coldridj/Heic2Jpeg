using System;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using ImageMagick.Formats;

namespace Heic2Jpeg
{
    public static class Program
    {
        private static void ConvertFile(string inputPath, string outputPath)
        {
            Console.WriteLine($"Converting {inputPath} => {outputPath}");
            using var image = new MagickImage(inputPath);
            image.Write(outputPath, new JpegWriteDefines()
            {
                Extent = 3000,
            });
        }

        private static void ProcessInput((bool IsDirectory, string Path) inputPath,
            (bool IsDirectory, string Path) outputPath)
        {
            if (inputPath.IsDirectory)
            {
                if (!outputPath.IsDirectory)
                {
                    throw new InvalidOperationException("output path must be directory");
                }
                
                var files = Directory.GetFiles(inputPath.Path, "*.heic");
                Parallel.ForEach(files, inputFile =>
                {
                    var outputFile = Path.Combine(outputPath.Path,
                        $"{Path.GetFileNameWithoutExtension(inputFile)}.jpg");

                    if (File.Exists(outputFile))
                    {
                        Console.WriteLine($"Skipping previously converted file {inputFile}");
                    }
                    else
                    {
                        ConvertFile(inputFile, outputFile);
                    }
                });
            }
            else
            {
                 var outputFile = Path.Combine(outputPath.Path,
                        $"{Path.GetFileNameWithoutExtension(inputPath.Path)}.jpg");
                 ConvertFile(inputPath.Path, outputFile);
            }
        }
            
        public static void Main(string[] args)
        {
            (bool IsDirectory, string Path) inputPath;
            (bool IsDirectory, string Path) outputPath;

            if (args.Length <= 0)
            {
                throw new ArgumentException("must specify input path");
            }
            var firstArg = args[0];
            if (Directory.Exists(firstArg))
            {
                inputPath = (true, firstArg);
            }
            else if (File.Exists(firstArg))
            {
                inputPath = (false, firstArg);
            }
            else
            {
                throw new ArgumentException("first argument must be existing file or directory");
            }
            
            if (args.Length == 1)
            {
                if (inputPath.IsDirectory)
                {
                    outputPath = inputPath;
                }
                else
                {
                    outputPath = (true, Path.GetDirectoryName(inputPath.Path) 
                                        ?? throw new InvalidOperationException("input path did not contain directory"));
                }
            }
            else
            {
                var secondArg = args[2];
                if (File.GetAttributes(secondArg).HasFlag(FileAttributes.Directory))
                {
                    if (!Directory.Exists(secondArg))
                    {
                        Directory.CreateDirectory(secondArg);
                    }
                    outputPath = (true, secondArg);
                }
                else
                {
                    if (inputPath.IsDirectory)
                    {
                        throw new InvalidOperationException("output path must also be a directory");
                    }
                    if (File.Exists(secondArg))
                    {
                        throw new ArgumentException("overwriting existing file not supported");
                    }
                    outputPath = (false, secondArg);
                }
            }
            
            ProcessInput(inputPath, outputPath);
        }
        
        
    }
}
