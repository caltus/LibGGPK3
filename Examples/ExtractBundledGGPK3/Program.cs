using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LibBundledGGPK3;

namespace ExtractBundledGGPK3
{
    public static class Program 
    {
        public static void Main(string[] args) 
        {
            try 
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version!;
                Console.WriteLine($"ExtractBundledGGPK3 (v{version.Major}.{version.Minor}.{version.Build})  Copyright (C) 2022 aianlinb");
                Console.WriteLine();

                if (args.Length == 0) 
                {
                    args = new string[1];
                    Console.Write("Path to Content.ggpk: ");
                    args[0] = Console.ReadLine()!;
                } 
                else if (args.Length != 1) 
                {
                    Console.WriteLine("Usage: ExtractBundledGGPK3 <PathToGGPK>");
                    Console.WriteLine();
                    Console.WriteLine("Enter to exit . . .");
                    Console.ReadLine();
                    return;
                }

                if (!File.Exists(args[0])) 
                {
                    Console.WriteLine("FileNotFound: " + args[0]);
                    Console.WriteLine();
                    Console.WriteLine("Enter to exit . . .");
                    Console.ReadLine();
                    return;
                }

                if (!File.Exists("files_path.txt")) 
                {
                    Console.WriteLine("FileNotFound: files_path.txt");
                    Console.WriteLine();
                    Console.WriteLine("Enter to exit . . .");
                    Console.ReadLine();
                    return;
                }

                var ggpk = new BundledGGPK(args[0]);
                Console.WriteLine("Reading file paths from files_path.txt...");

                var lines = File.ReadAllLines("files_path.txt")
                                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"));

                string outputRoot = "root/Bundles2/";

                foreach (var line in lines) 
                {
                    var nodePath = line.TrimEnd('/');
                    var outputPath = Path.Combine(outputRoot, Path.GetDirectoryName(nodePath)!);

                    // Ensure the output directory exists
                    Directory.CreateDirectory(outputPath);

                    Console.WriteLine($"Extracting from GGPK: {nodePath} to {outputPath}");

                    if (!ggpk.Index.TryFindNode(nodePath, out var node)) 
                    {
                        Console.WriteLine("Not found in GGPK: " + nodePath);
                        continue;
                    }

                    LibBundle3.Index.ExtractParallel(node, outputPath);
                }

                Console.WriteLine("Done!");
            } 
            catch (Exception e) 
            {
                Console.Error.WriteLine(e);
            }

            Console.WriteLine();
            Console.WriteLine("Enter to exit . . .");
            Console.ReadLine();
        }
    }
}