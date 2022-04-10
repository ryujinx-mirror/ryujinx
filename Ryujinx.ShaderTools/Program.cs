using CommandLine;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ryujinx.ShaderTools
{
    class Program
    {
        private class GpuAccessor : IGpuAccessor
        {
            private readonly byte[] _data;

            public GpuAccessor(byte[] data)
            {
                _data = data;
            }

            public ReadOnlySpan<ulong> GetCode(ulong address, int minimumSize)
            {
                return MemoryMarshal.Cast<byte, ulong>(new ReadOnlySpan<byte>(_data).Slice((int)address));
            }
        }

        private class Options
        {
            [Option("compute", Required = false, Default = false, HelpText = "Indicate that the shader is a compute shader.")]
            public bool Compute { get; set; }

            [Option("target-language", Required = false, Default = TargetLanguage.Glsl, HelpText = "Indicate the target shader language to use.")]
            public TargetLanguage TargetLanguage { get; set; }

            [Option("target-api", Required = false, Default = TargetApi.OpenGL, HelpText = "Indicate the target graphics api to use.")]
            public TargetApi TargetApi { get; set; }

            [Value(0, MetaName = "input", HelpText = "Binary Maxwell shader input path.", Required = true)]
            public string InputPath { get; set; }

            [Value(1, MetaName = "output", HelpText = "Decompiled shader output path.", Required = false)]
            public string OutputPath { get; set; }
        }

        static void HandleArguments(Options options)
        {
            TranslationFlags flags = TranslationFlags.DebugMode;

            if (options.Compute)
            {
                flags |= TranslationFlags.Compute;
            }

            byte[] data = File.ReadAllBytes(options.InputPath);

            TranslationOptions translationOptions = new TranslationOptions(options.TargetLanguage, options.TargetApi, flags);

            ShaderProgram program = Translator.CreateContext(0, new GpuAccessor(data), translationOptions).Translate();

            if (options.OutputPath == null)
            {
                if (program.BinaryCode != null)
                {
                    using Stream outputStream = Console.OpenStandardOutput();

                    outputStream.Write(program.BinaryCode);
                }
                else
                {
                    Console.WriteLine(program.Code);
                }
            }
            else
            {
                if (program.BinaryCode != null)
                {
                    File.WriteAllBytes(options.OutputPath, program.BinaryCode);
                }
                else
                {
                    File.WriteAllText(options.OutputPath, program.Code);
                }
            }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options => HandleArguments(options))
            .WithNotParsed(errors => errors.Output());
        }
    }
}