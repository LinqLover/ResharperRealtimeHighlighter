using System.Collections.Generic;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.I18n.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.ResharperRealtimeHighlighter.Inspection;

[DaemonStage]
public class RecentChangesDaemonStage(RecentChangesTracker recentChangesTracker) : DaemonStageBase<IFile>
{
    protected RecentChangesTracker RecentChangesTracker { get; } = recentChangesTracker;

    protected override IDaemonStageProcess CreateDaemonProcess(IDaemonProcess process, DaemonProcessKind processKind, IFile file,
        IContextBoundSettingsStore settingsStore)
    {
        return new RecentChangesDaemonStageProcess(process, RecentChangesTracker);
    }

    protected override IEnumerable<IFile> GetPsiFiles(IPsiSourceFile sourceFile)
    {
        if (sourceFile == null) return [];
        return [sourceFile.GetPrimaryPsiFile()];
    }
}
