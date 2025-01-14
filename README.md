# CSharpMarkdownExporter

## Table of Contents
1. [Solution Overview](#solution-overview)
2. [VS Code Extension Overview](#vs-code-extension-overview)  
   2.1. [Use Cases](#use-cases)  
   2.2. [Functional Requirements](#functional-requirements)  
   2.3. [Workflow / How It Works](#workflow--how-it-works)  
   2.4. [Implementation Notes](#implementation-notes)  
   2.5. [Sample Output](#sample-output)
3. [How to Build and Run](#how-to-build-and-run)
4. [License](#license)
5. [Contact / Author](#contact--author)

---

## 1. Solution Overview

Here you can describe the **C# solution** in detail. For example:

- **Project Structure**  
  - `.sln` file  
  - `.csproj` file(s)  
  - Key C# source files (`Program.cs`, etc.)  
- **Purpose and Features** of the console application (if any).
- **Directory Layout** (e.g., how your solution is organized).
- **Dependencies / Requirements** (e.g., .NET version).

You might also include a snippet of the `.sln` or `.csproj` here, or refer to them as needed:

<details>
<summary>Solution File Snippet</summary>

```xml
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
...
```
</details>

---

## 2. VS Code Extension Overview

This section focuses on the **VS Code extension** (e.g., `extension.ts`, the `package.json` configs, etc.).

### 2.1 Use Cases

1. **Code Reviews and Fix Requests to LLM**  
   Summarize your entire solution (including `.sln`, `.csproj`) so that an LLM can see all relevant config and dependencies.

2. **Multiple-File Updates**  
   When you need changes spanning multiple files, you can send them all at once to an LLM.

### 2.2 Functional Requirements

1. **Retrieve Git-Managed Files**  
   - Shows a list or simplified tree of files under version control.

2. **Force-Output `.sln` and `.csproj` Files**  
   - If missing, display a warning or error.

3. **Include Currently Opened Files**  
   - Collect C# code and any other relevant files from open VS Code tabs.

4. **Display All Gathered Data as Markdown**  
   - Open it in a new unsaved (Untitled) editor window.

### 2.3 Workflow / How It Works

1. **Command Execution**  
   - Trigger via the Command Palette (`Ctrl+Shift+P` → `csharp-md-exporter.export`, or whichever command name you chose).

2. **Gather Git File Structure**  
   - Use `git ls-files` or the VS Code Git API to retrieve tracked files.

3. **Locate `.sln` / `.csproj`**  
   - Search workspace recursively for those files.

4. **Extract Opened Files**  
   - Use VS Code APIs to get file paths and content (including unsaved changes).

5. **Build Markdown and Display**  
   - Generate a single Markdown document with all the information.

### 2.4 Implementation Notes

- **Git File Retrieval**  
  Uses child processes (`git ls-files`) or the VS Code Git extension API.

- **File Pattern Matching**  
  Recursively searches for `.sln` / `.csproj`.

- **Code Block Language Detection**  
  For `.cs`, uses ```csharp; for `.csproj`, uses ```xml, etc.

- **Performance Considerations**  
  Large projects may result in big outputs—consider limiting file size or count.

### 2.5 Sample Output

Below is an excerpt of the kind of Markdown output the extension might generate:

<details>
<summary>Sample Markdown</summary>

```
# Git File Structure
- .gitignore
- MyProject
  - MyProject.sln
  - MyProject
    - MyProject.csproj
    - Program.cs
    - SomeClass.cs
  ...

# /path/to/MyProject.sln
\`\`\`xml
(Solution file content)
\`\`\`

# /path/to/MyProject/MyProject.csproj
\`\`\`xml
(Project file content)
\`\`\`

# /path/to/MyProject/Program.cs
\`\`\`csharp
using System;

namespace MyProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
\`\`\`
```
</details>

---

## 3. How to Build and Run

1. **.NET Build**  
   - Install [.NET 8.0 SDK or later](https://dotnet.microsoft.com/en-us/download).
   - Open the solution in Visual Studio, Visual Studio Code, or run `dotnet build`.

2. **VS Code Extension**  
   - Go to the `csharp-md-exporter` folder.
   - Run `npm install`, `npm run compile`, and `vsce package` (assuming you have [VSCE](https://code.visualstudio.com/api/working-with-extensions/publishing-extension) installed).

3. **Installing the Extension**  
   - Install the resulting `.vsix` file by going to VS Code → Extensions panel → `...` → `Install from VSIX...`.

---

## 4. License

Include your license information here, for example:

```
MIT License
Copyright ...
Permission is hereby granted, free of charge, ...
```

---

## 5. Contact / Author

- **Author**: Yutaro Amano (GitHub handle, https://github.com/amano0406)  
- **Issues / Feedback**: Please open an [issue](https://github.com/your-repo/issues) on GitHub.

---

Feel free to adjust the headings or the level of detail. The key point is to keep **Solution Explanation** (C# side) and **Extension Explanation** (VS Code side) as separate top-level sections in your README, with a clear, clickable table of contents for easy navigation.