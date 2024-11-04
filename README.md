# ResharperRealtimeHighlighter

A simple ReSharper extension that highlights all files that were changed in the most recent git commits. Changed files are updated automatically and efficiently in the background. The number of commits to track can be configured in the ReSharper Options under Tools > ResharperRealtimeHighlighter.

## Installation

```
& .\buildPlugin.ps1
& .\runVisualStudio.ps1
```

After building, copy LibGit2Sharp.dll and git2-a418d9d.dll from the build directory (e.g, `src\dotnet\ReSharperPlugin.ResharperRealtimeHighlighter\bin\ReSharperPlugin.ResharperRealtimeHighlighter\Debug[\lib\win32\x64]`) into devenv folder (e.g., `C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE`).
