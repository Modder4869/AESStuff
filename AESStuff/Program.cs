using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AESStuff
{
    
    public class FNames
    {
        public List<string>? FileName { get; set; }
    }

    class Program
    {
        static Dictionary<string, string> fileToPath = new Dictionary<string, string>();
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: AESStuff <inputFolder> <outputFolder> <password> <json path>");
                return;
            }

            string inputFolder = args[0];
            string outputFolder = args[1];

            string password = args[3];
            var jsonData = ReadJsonFile(args[2]);
          

            if (jsonData != null)
            {
                for (int i = 0; i < jsonData.FileName.Count; i++)
                {
                    string filePath = jsonData.FileName[i];
                    string fileName = Path.GetFileName(filePath);
                    fileToPath[fileName] = filePath;
                }
            }
        
        DecryptFolder(inputFolder, password, outputFolder);
        }
        public static FNames ReadJsonFile(string jsonFilePath)
        {
            try
            {
                // Read the JSON file
                string jsonContent = File.ReadAllText(jsonFilePath);

                // Parse the JSON data into a dictionary
                FNames Names = JsonConvert.DeserializeObject<FNames>(jsonContent);


                return Names;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }
        public static void DecryptFolder(string inputFolder, string password, string outputFolder)
        {
             string[] files = Directory.GetFiles(inputFolder, "*.unity3d", SearchOption.AllDirectories);

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
                    DecryptFile(fileName, filePath, password, destinationFilePath);
                }
            }
        }

        public static void DecryptFile(string filename, string filePath, string password, string destinationFilePath)
        {
            if (!fileToPath.ContainsKey(filename))
            {
                Console.WriteLine($"Missing Key/Not Encrypted for {filename}");
                return;
            }
            string value = fileToPath[filename];
            Console.WriteLine($"salt for {filename} is {value}");
            byte[] saltBytes = Encoding.UTF8.GetBytes(value);

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
