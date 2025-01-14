using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CSharpMarkdownExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            // Base directory name for the output (extension name set to "csharp-md-exporter")
            string baseDir = "csharp-md-exporter";

            // 1. Create the VSCode extension skeleton structure
            CreateDirectoryStructure(baseDir);
            WriteGitIgnore(baseDir);
            WritePackageJson(baseDir);
            WriteTsConfigJson(baseDir);
            WriteExtensionTs(baseDir); // Write extension.ts from the embedded resource
            WriteExtensionTestTs(baseDir);
            WriteLaunchJson(baseDir);
            WriteExtensionsJson(baseDir);

            // 2. Write a README.md that documents the extension (inlined content)
            WriteReadmeMd(baseDir);

            Console.WriteLine("VSCode extension skeleton has been created.");

            // 3. Build and package the extension (npm install → compile → vsce package)
            PackageExtension(baseDir);

            Console.WriteLine("Packaging process completed.");
            Console.WriteLine($"Output directory: {Path.Combine(Directory.GetCurrentDirectory(), baseDir)}");
            Console.WriteLine("If a .vsix file has been generated, the packaging succeeded.");
        }

        /// <summary>
        /// Creates the necessary directory structure.
        /// </summary>
        static void CreateDirectoryStructure(string baseDir)
        {
            // Delete the existing directory if it already exists
            if (Directory.Exists(baseDir))
            {
                // Passing 'true' as the second argument allows a recursive delete
                Directory.Delete(baseDir, true);
            }

            // Create the new directory structure
            Directory.CreateDirectory(baseDir);
            Directory.CreateDirectory(Path.Combine(baseDir, ".vscode"));
            Directory.CreateDirectory(Path.Combine(baseDir, "src"));
            Directory.CreateDirectory(Path.Combine(baseDir, "src", "test"));
            Directory.CreateDirectory(Path.Combine(baseDir, "src", "test", "suite"));
        }

        /// <summary>
        /// Writes the .gitignore file.
        /// </summary>
        static void WriteGitIgnore(string baseDir)
        {
            string filePath = Path.Combine(baseDir, ".gitignore");
            string content =
@"node_modules
out
.vscode-test
";
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Writes package.json (with the extension name changed).
        /// </summary>
        static void WritePackageJson(string baseDir)
        {
            string filePath = Path.Combine(baseDir, "package.json");
            string content =
@"{
  ""name"": ""csharp-md-exporter"",
  ""displayName"": ""CSharp Markdown Exporter"",
  ""description"": ""Export C# solution/project and opened files to Markdown for LLM use."",
  ""version"": ""0.0.1"",
  ""engines"": {
    ""vscode"": ""^1.70.0""
  },
  ""categories"": [
    ""Other""
  ],
  ""activationEvents"": [
    ""onCommand:csharp-md-exporter.export""
  ],
  ""main"": ""./out/extension.js"",
  ""contributes"": {
    ""commands"": [
      {
        ""command"": ""csharp-md-exporter.export"",
        ""title"": ""Export C# to Markdown""
      }
    ]
  },
  ""scripts"": {
    ""vscode:prepublish"": ""npm run compile"",
    ""compile"": ""tsc -p ./"",
    ""watch"": ""tsc -watch -p ./""
  },
  ""devDependencies"": {
    ""@types/vscode"": ""^1.70.0"",
    ""@types/node"": ""16.x"",
    ""typescript"": ""^4.8.0"",
    ""@types/mocha"": ""^9.1.1"",
    ""mocha"": ""^10.0.0"",
    ""eslint"": ""^8.22.0"",
    ""@typescript-eslint/parser"": ""^5.34.0"",
    ""@typescript-eslint/eslint-plugin"": ""^5.34.0""
  }
}";
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Writes tsconfig.json.
        /// </summary>
        static void WriteTsConfigJson(string baseDir)
        {
            string filePath = Path.Combine(baseDir, "tsconfig.json");
            string content =
@"{
  ""compilerOptions"": {
    ""module"": ""commonjs"",
    ""target"": ""es2022"",
    ""outDir"": ""out"",
    ""lib"": [
      ""ES2022""
    ],
    ""sourceMap"": true,
    ""rootDir"": ""src"",
    ""strict"": true,
    ""skipLibCheck"": true
  },
  ""exclude"": [
    ""node_modules"",
    "".vscode-test""
  ]
}";
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Reads src/extension.ts from the embedded resource and writes it to a file.
        /// </summary>
        static void WriteExtensionTs(string baseDir)
        {
            // Output file path
            string filePath = Path.Combine(baseDir, "src", "extension.ts");

            // Name of the embedded resource
            // By default, it often consists of "namespace + filename", but the actual naming can vary.
            // The line below assumes "CSharpMarkdownExporter.extension.ts".
            string resourceName = "CSharpMarkdownExporter.extension.ts";

            // Acquire the resource from the assembly
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                Console.WriteLine($"ERROR: Embedded resource '{resourceName}' not found.");
                return;
            }

            using var reader = new StreamReader(stream);
            string content = reader.ReadToEnd();

            // Write to file
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Writes src/test/suite/extension.test.ts.
        /// </summary>
        static void WriteExtensionTestTs(string baseDir)
        {
            string filePath = Path.Combine(baseDir, "src", "test", "suite", "extension.test.ts");
            string content =
@"import * as assert from 'assert';

suite('Extension Test Suite', () => {
    test('Sample test', () => {
        assert.strictEqual([1,2,3].indexOf(4), -1);
    });
});
";
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Writes .vscode/launch.json.
        /// </summary>
        static void WriteLaunchJson(string baseDir)
        {
            string filePath = Path.Combine(baseDir, ".vscode", "launch.json");
            string content =
@"{
  ""version"": ""0.2.0"",
  ""configurations"": [
    {
      ""name"": ""Launch Extension"",
      ""type"": ""extensionHost"",
      ""request"": ""launch"",
      ""args"": [
        ""--extensionDevelopmentPath=${workspaceFolder}""
      ],
      ""outFiles"": [
        ""${workspaceFolder}/out/**/*.js""
      ],
      ""preLaunchTask"": ""npm: compile""
    },
    {
      ""name"": ""Extension Tests"",
      ""type"": ""extensionHost"",
      ""request"": ""launch"",
      ""args"": [
        ""--extensionDevelopmentPath=${workspaceFolder}"",
        ""--extensionTestsPath=${workspaceFolder}/out/test/suite""
      ],
      ""outFiles"": [
        ""${workspaceFolder}/out/test/**/*.js""
      ],
      ""preLaunchTask"": ""npm: compile""
    }
  ]
}
";
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Writes .vscode/extensions.json.
        /// </summary>
        static void WriteExtensionsJson(string baseDir)
        {
            string filePath = Path.Combine(baseDir, ".vscode", "extensions.json");
            string content =
@"{
  ""recommendations"": [
  ]
}
";
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Writes a README.md file that VS Code / Marketplace will display
        /// when users view your extension details. (Inline content version)
        /// </summary>
        static void WriteReadmeMd(string baseDir)
        {
            string filePath = Path.Combine(baseDir, "README.md");

            string content =
@"# CSharp Markdown Exporter

This VS Code extension allows you to export:
- Git-tracked file structure
- `.sln` (solution) files
- `.csproj` (project) files
- Currently opened files in the editor

...all in a single Markdown document, making it easy to share or use with Large Language Models (LLMs).

## Features

1. **Retrieve Git-tracked files**  
   Uses `git ls-files` (or VS Code Git APIs) to list all files under version control.

2. **Force-includes `.sln` and `.csproj`**  
   Automatically searches your workspace for these files and exports their contents in Markdown code blocks.

3. **Include currently opened files**  
   Collects all open text documents in VS Code (e.g., `.cs`, `.json`, `.csproj`) and appends them as code blocks.

4. **Single Markdown Export**  
   Displays everything in one *Untitled* editor, so you can quickly copy and paste into an LLM prompt or anywhere else.

## Requirements

- .NET runtime or SDK (if building the C# solution).
- A Git repository initialized in your workspace (for Git file listing).
- VS Code 1.70.0 or higher (see `engines.vscode` in `package.json`).

## Extension Settings

Currently, there are no configurable settings. Future updates may provide
customization options (such as excluding large files or ignoring certain file patterns).

## Known Issues

- Large solutions might generate very large Markdown outputs.
- Handling multiple `.sln` or `.csproj` files is limited to basic concatenation.

## Release Notes

### 0.0.1
- Initial release with basic export functionality.

---

Happy coding!
";
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Executes 'npm install', 'npm run compile', and 'vsce package' to package the VSCode extension into a .vsix file.
        /// </summary>
        static void PackageExtension(string baseDir)
        {
            // Check if package.json exists before proceeding
            string packageJsonPath = Path.Combine(baseDir, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                Console.WriteLine("package.json does not exist. Skipping packaging.");
                return;
            }

            // npm install
            RunCommand("npm install", baseDir);

            // npm run compile
            RunCommand("npm run compile", baseDir);

            // vsce package
            // This should generate a file like "csharp-md-exporter-0.0.1.vsix"
            RunCommand("vsce package", baseDir);
        }

        /// <summary>
        /// Helper method to run the specified command in the specified directory.
        /// </summary>
        static void RunCommand(string command, string workDir)
        {
            Console.WriteLine($"> {command} @ {workDir}");

            // For Windows, we use cmd.exe /c
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                WorkingDirectory = workDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = new Process { StartInfo = psi };
            proc.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    Console.WriteLine(e.Data);
            };
            proc.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    Console.Error.WriteLine(e.Data);
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                Console.WriteLine($@"ERROR: Command exited with code {proc.ExitCode}.");
            }
        }
    }
}
