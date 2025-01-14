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
            // 出力先のベースフォルダ名
            // ※ 拡張機能名を "csharp-md-exporter" に変更
            string baseDir = "csharp-md-exporter";

            // 1. VSCode拡張スケルトン構成を作成
            CreateDirectoryStructure(baseDir);
            WriteGitIgnore(baseDir);
            WritePackageJson(baseDir);
            WriteTsConfigJson(baseDir);
            WriteExtensionTs(baseDir);       // extension.ts をリソースから書き出す
            WriteExtensionTestTs(baseDir);
            WriteLaunchJson(baseDir);
            WriteExtensionsJson(baseDir);

            Console.WriteLine("VSCode拡張スケルトンを生成しました。");

            // 2. 拡張をビルド＆パッケージング (npm install → compile → vsce package)
            PackageExtension(baseDir);

            Console.WriteLine("パッケージング処理が完了しました。");
            Console.WriteLine($"出力先: {Path.Combine(Directory.GetCurrentDirectory(), baseDir)}");
            Console.WriteLine("※ 拡張子 .vsix のファイルが生成されていれば成功です。");
        }

        /// <summary>
        /// 必要なディレクトリ構造を作成
        /// </summary>
        static void CreateDirectoryStructure(string baseDir)
        {
            // 既存のディレクトリが存在する場合は削除
            if (Directory.Exists(baseDir))
            {
                Directory.Delete(baseDir, true); // 第二引数をtrueにすると再帰的に削除
            }

            // 新しいディレクトリ構造を作成

            // csharp-md-exporter/
            Directory.CreateDirectory(baseDir);

            // csharp-md-exporter/.vscode/
            Directory.CreateDirectory(Path.Combine(baseDir, ".vscode"));

            // csharp-md-exporter/src/
            Directory.CreateDirectory(Path.Combine(baseDir, "src"));

            // csharp-md-exporter/src/test/
            Directory.CreateDirectory(Path.Combine(baseDir, "src", "test"));

            // csharp-md-exporter/src/test/suite/
            Directory.CreateDirectory(Path.Combine(baseDir, "src", "test", "suite"));
        }

        /// <summary>
        /// .gitignoreファイルを書き出す
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
        /// package.json を書き出す (拡張機能名を変更)
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
        /// tsconfig.json を書き出す
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
        /// src/extension.ts をリソースから読み出し、ファイルとして書き出す
        /// </summary>
        static void WriteExtensionTs(string baseDir)
        {
            // 出力ファイルパス
            string filePath = Path.Combine(baseDir, "src", "extension.ts");

            // 埋め込みリソース名
            // 既定では「名前空間 + ファイル名」になることが多いですが、
            // 実際の組込み名はプロジェクトやnamespaceによって異なる場合があります。
            // 下記は「CSharpMarkdownExporter.extension.ts」を想定。
            string resourceName = "CSharpMarkdownExporter.extension.ts";

            // アセンブリからリソースを取得
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                Console.WriteLine($"ERROR: 埋め込みリソース '{resourceName}' が見つかりません。");
                return;
            }

            using var reader = new StreamReader(stream);
            string content = reader.ReadToEnd();

            // 書き出し
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// src/test/suite/extension.test.ts を書き出す
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
        /// .vscode/launch.json を書き出す
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
        /// .vscode/extensions.json を書き出す
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
        /// npm i → npm run compile → vsce package を実行し、
        /// VSCode拡張を .vsix にパッケージングする
        /// </summary>
        static void PackageExtension(string baseDir)
        {
            // 実行前に、package.jsonがあるか軽くチェックしておく
            string packageJsonPath = Path.Combine(baseDir, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                Console.WriteLine("package.json が存在しません。パッケージングをスキップします。");
                return;
            }

            // npm install
            RunCommand("npm install", baseDir);

            // npm run compile
            RunCommand("npm run compile", baseDir);

            // vsce package
            // → "csharp-md-exporter-0.0.1.vsix" のようなファイルが生成されるはず
            RunCommand("vsce package", baseDir);
        }

        /// <summary>
        /// 指定コマンドを指定ディレクトリで実行するヘルパー
        /// </summary>
        static void RunCommand(string command, string workDir)
        {
            Console.WriteLine($"> {command} @ {workDir}");

            // Windows向けに cmd.exe /c で実行
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
                Console.WriteLine($@"ERROR: コマンド終了コード {proc.ExitCode} です。");
            }
        }
    }
}
