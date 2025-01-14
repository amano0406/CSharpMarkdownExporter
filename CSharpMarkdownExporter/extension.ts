import * as vscode from 'vscode';
import { exec } from 'child_process';
import * as fs from 'fs';
import * as path from 'path';

/**
 * This extension ("CSharp Markdown Exporter") works within a Visual Studio Code
 * environment where a C# project is open. It accomplishes the following:
 * - Retrieves the Git-tracked file structure (via `git ls-files`)
 * - Finds any .sln files and .csproj files within the workspace
 * - Collects the content of all currently open files in the editor
 * - Outputs all gathered information into a single Markdown document
 *
 * Code structure overview:
 * - activate(context): Registers the extension command and handles the main logic.
 * - deactivate(): Cleans up resources when the extension is disabled (if necessary).
 * - getGitFiles(rootPath): Executes "git ls-files" to retrieve Git-managed files.
 * - findFiles(pattern): Finds files in the workspace based on a given pattern.
 * - getAllOpenedTextDocuments(): Gathers all currently open text documents.
 * - buildMarkdown(...): Constructs the final Markdown string from all data sources.
 */
export function activate(context: vscode.ExtensionContext) {
    console.log('Extension "CSharp Markdown Exporter" is now active!');

    // Register the extension command
    const disposable = vscode.commands.registerCommand('csharp-md-exporter.export', async () => {
        try {
            // Check if a workspace is open
            const workspaceFolders = vscode.workspace.workspaceFolders;
            if (!workspaceFolders || workspaceFolders.length === 0) {
                vscode.window.showErrorMessage('No workspace is open. Please open a folder or workspace first.');
                return;
            }

            // Retrieve the list of Git-managed files
            const gitFiles = await getGitFiles(workspaceFolders[0].uri.fsPath);

            // Find solution files (.sln)
            const slnFiles = await findFiles('**/*.sln');

            // Find project files (.csproj)
            const csprojFiles = await findFiles('**/*.csproj');

            // Get all currently open text documents
            const openFiles = getAllOpenedTextDocuments();

            // Build the Markdown content
            const md = buildMarkdown(gitFiles, slnFiles, csprojFiles, openFiles);

            // Display in a new "Untitled" document with Markdown syntax
            const doc = await vscode.workspace.openTextDocument({
                content: md,
                language: 'markdown',
            });
            vscode.window.showTextDocument(doc, vscode.ViewColumn.Beside);

        } catch (err) {
            vscode.window.showErrorMessage(`Error: ${err}`);
        }
    });

    context.subscriptions.push(disposable);
}

/**
 * Called when the extension is deactivated
 */
export function deactivate() {
    // Clean-up logic can go here if needed
}

/**
 * Retrieve the list of Git-managed files using "git ls-files"
 */
function getGitFiles(rootPath: string): Promise<string[]> {
    return new Promise((resolve, reject) => {
        exec('git ls-files', { cwd: rootPath }, (error, stdout) => {
            if (error) {
                return reject(error);
            }
            const fileList = stdout
                .split('\n')
                .map(line => line.trim())
                .filter(line => line.length > 0);
            resolve(fileList);
        });
    });
}

/**
 * Find files within the workspace using a given pattern
 */
async function findFiles(pattern: string): Promise<string[]> {
    const uris = await vscode.workspace.findFiles(pattern, '**/node_modules/**');
    return uris.map(u => u.fsPath);
}

/**
 * Gather all currently open text documents by iterating through
 * all tab groups (using a type assertion for tab.input).
 */
function getAllOpenedTextDocuments(): { filePath: string; language: string; content: string }[] {
    const results: { filePath: string; language: string; content: string }[] = [];

    for (const group of vscode.window.tabGroups.all) {
        for (const tab of group.tabs) {
            // tab.input is unknown, so cast it to any
            const inputAsAny = tab.input as any;

            // If inputAsAny.uri exists, use it to find the textDocument
            if (inputAsAny?.uri) {
                const doc = vscode.workspace.textDocuments.find(d =>
                    d.uri.toString() === inputAsAny.uri.toString()
                );
                if (!doc) {
                    continue;
                }

                // Prevent duplicates
                if (results.some(r => r.filePath === doc.fileName)) {
                    continue;
                }

                // Determine the code block language based on the file extension
                const ext = path.extname(doc.fileName).toLowerCase();
                let language = '';
                switch (ext) {
                    case '.cs':
                        language = 'csharp';
                        break;
                    case '.csproj':
                        language = 'xml';
                        break;
                    case '.json':
                        language = 'json';
                        break;
                    default:
                        language = doc.languageId || '';
                        break;
                }

                results.push({
                    filePath: doc.fileName,
                    language,
                    content: doc.getText(),
                });
            }
        }
    }

    return results;
}

/**
 * Constructs the final Markdown output from:
 *  - Git-managed file paths
 *  - .sln files
 *  - .csproj files
 *  - Open text documents
 */
function buildMarkdown(
    gitFiles: string[],
    slnFiles: string[],
    csprojFiles: string[],
    openFiles: { filePath: string; language: string; content: string }[]
): string {
    const lines: string[] = [];

    // Git-tracked file structure
    lines.push('# Git File Structure');
    gitFiles.forEach(f => {
        lines.push(`- ${f}`);
    });
    lines.push('');

    // Solution files (.sln)
    if (slnFiles.length === 0) {
        lines.push('**[Warning] No .sln file found.**\n');
    } else {
        for (const slnPath of slnFiles) {
            lines.push(`# ${slnPath}`);
            lines.push('```xml');
            lines.push(fs.readFileSync(slnPath, 'utf-8'));
            lines.push('```');
            lines.push('');
        }
    }

    // Project files (.csproj)
    if (csprojFiles.length === 0) {
        lines.push('**[Warning] No .csproj file found.**\n');
    } else {
        for (const projPath of csprojFiles) {
            lines.push(`# ${projPath}`);
            lines.push('```xml');
            lines.push(fs.readFileSync(projPath, 'utf-8'));
            lines.push('```');
            lines.push('');
        }
    }

    // Open files
    for (const file of openFiles) {
        lines.push(`# ${file.filePath}`);
        lines.push('```' + (file.language || ''));
        lines.push(file.content);
        lines.push('```');
        lines.push('');
    }

    return lines.join('\n');
}
