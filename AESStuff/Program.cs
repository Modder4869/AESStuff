using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AESStuff
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: AESStuff <inputFolder> <outputFolder>");
                return;
            }

            string inputFolder = args[0];
            string outputFolder = args[1];

            string password = args[2];

            DecryptFolder(inputFolder, password, outputFolder);
        }

        public static void DecryptFolder(string inputFolder, string password, string outputFolder)
        {
            string[] files = Directory.GetFiles(inputFolder);

            byte[] saltBytes = null;

            foreach (string filePath in files)
            {
                if (File.Exists(filePath))
                {
                    string fileName = Path.GetFileName(filePath);
                    saltBytes = Encoding.UTF8.GetBytes(fileName);
                    break;
                }
            }

            if (saltBytes == null)
            {
                Console.WriteLine("No files found in the input folder.");
                return;
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            foreach (string filePath in files)
            {
                if (File.Exists(filePath))
                {
                    string fileName = Path.GetFileName(filePath);
                    string destinationFilePath = Path.Combine(outputFolder, fileName);
                    DecryptFile(filePath, password, destinationFilePath);
                }
            }
        }

        public static void DecryptFile(string filePath, string password, string destinationFilePath)
        {
            byte[] saltBytes = Encoding.UTF8.GetBytes(Path.GetFileName(filePath));

            using (Stream baseStream = File.OpenRead(filePath))
            {
                using (FileStream sourceStream = File.OpenRead(filePath))
                {
                    using (FileStream destinationStream = File.OpenWrite(destinationFilePath))
                    {
                        using (SeekableAesStream aesStream = new SeekableAesStream(sourceStream, password, saltBytes))
                        {
                            aesStream.CopyTo(destinationStream);
                        }
                    }
                }
            }
        }
    }
}
