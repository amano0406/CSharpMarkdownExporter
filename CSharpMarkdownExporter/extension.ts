import * as vscode from 'vscode';
import { exec } from 'child_process';
import * as fs from 'fs';
import * as path from 'path';

/**
 * この拡張機能は、C# のプロジェクトを開いている Visual Studio Code 上で:
 * 1. Git で管理されているファイル構造（ディレクトリツリー）
 * 2. ソリューションファイル（.sln）
 * 3. プロジェクトファイル（.csproj）
 * 4. 現在開いているファイルのコード
 * を一括して Markdown に出力するためのものです。
 */

export function activate(context: vscode.ExtensionContext) {
    console.log('Extension "CSharp Markdown Exporter" is now active!');

    // 拡張機能のコマンドを登録
    const disposable = vscode.commands.registerCommand('csharp-md-exporter.export', async () => {
        try {
            // ワークスペースが開かれているかチェック
            const workspaceFolders = vscode.workspace.workspaceFolders;
            if (!workspaceFolders || workspaceFolders.length === 0) {
                vscode.window.showErrorMessage('No workspace is open. Please open a folder or workspace first.');
                return;
            }

            // 1. Git 管理のファイル構造を取得
            const gitFiles = await getGitFiles(workspaceFolders[0].uri.fsPath);

            // 2. ソリューションファイル (.sln) を検索
            const slnFiles = await findFiles('**/*.sln');

            // 3. プロジェクトファイル (.csproj) を検索
            const csprojFiles = await findFiles('**/*.csproj');

            // 4. 現在開いているファイルを取得 (tabGroups + 型アサーション)
            const openFiles = getAllOpenedTextDocuments();

            // マークダウンを組み立て
            const md = buildMarkdown(gitFiles, slnFiles, csprojFiles, openFiles);

            // Untitled のドキュメントとして表示
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
 * 拡張機能が無効化されたときの処理
 */
export function deactivate() {
    // 必要ならリソース解放などを記述
}

/**
 * Git 管理のファイル一覧を取得 (git ls-files)
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
 * ワークスペース内をパターン検索でファイルを取得
 */
async function findFiles(pattern: string): Promise<string[]> {
    const uris = await vscode.workspace.findFiles(pattern, '**/node_modules/**');
    return uris.map(u => u.fsPath);
}

/**
 * 現在開いている「すべてのタブ」（tabGroups）を走査し、
 * 対応する TextDocument の内容を取得する (アプローチB: 型アサーション)
 */
function getAllOpenedTextDocuments(): { filePath: string; language: string; content: string }[] {
    const results: { filePath: string; language: string; content: string }[] = [];

    for (const group of vscode.window.tabGroups.all) {
        for (const tab of group.tabs) {
            // tab.input が unknown のため、「(tab.input as any)」でキャスト
            const inputAsAny = tab.input as any;

            // inputAsAny.uri があれば、それを使って textDocument を探す
            if (inputAsAny?.uri) {
                const doc = vscode.workspace.textDocuments.find(d =>
                    d.uri.toString() === inputAsAny.uri.toString()
                );
                if (!doc) {
                    continue;
                }

                // 重複チェック
                if (results.some(r => r.filePath === doc.fileName)) {
                    continue;
                }

                // 拡張子からコードブロック言語を決定 (必要に応じて追加)
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
 * 取得したファイル一覧、.sln/.csproj の内容、開いているドキュメントをまとめて Markdown 化
 */
function buildMarkdown(
    gitFiles: string[],
    slnFiles: string[],
    csprojFiles: string[],
    openFiles: { filePath: string; language: string; content: string }[]
): string {
    const lines: string[] = [];

    // 1. Git 管理ファイル構造
    lines.push('# Git File Structure');
    gitFiles.forEach(f => {
        lines.push(`- ${f}`);
    });
    lines.push('');

    // 2. ソリューションファイル (.sln)
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

    // 3. プロジェクトファイル (.csproj)
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

    // 4. 現在開いているファイルのコードを出力
    for (const file of openFiles) {
        lines.push(`# ${file.filePath}`);
        lines.push('```' + (file.language || ''));
        lines.push(file.content);
        lines.push('```');
        lines.push('');
    }

    return lines.join('\n');
}
