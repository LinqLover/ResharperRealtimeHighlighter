using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.Util;

namespace ReSharperPlugin.ResharperRealtimeHighlighter.Inspection;

public class RecentChangesDaemonStageProcess(
	IDaemonProcess process,
	RecentChangesTracker recentChangesTracker) : IDaemonStageProcess
{
	public IDaemonProcess DaemonProcess { get; } = process;

	protected RecentChangesTracker RecentChangesTracker { get; } = recentChangesTracker;

	protected const int PrefixLength = 5;

	public void Execute(Action<DaemonStageResult> committer)
	{
		if (DaemonProcess.InterruptFlag)
			return;

		var highlightingInfos = GetHighlightingInfos();
		committer(new DaemonStageResult(highlightingInfos.ToList()));
	}

	protected IEnumerable<HighlightingInfo> GetHighlightingInfos()
	{
		var path = DaemonProcess.SourceFile.GetLocation().MakeRelativeTo(DaemonProcess.Solution.SolutionDirectory).NormalizeSeparators(FileSystemPathEx.SeparatorStyle.Unix);
		if (!RecentChangesTracker.TryGetRecentCommit(path, out var commit))
			yield break;

		var file = DaemonProcess.SourceFile.GetPrimaryPsiFile();
		if (file == null)
			yield break;
		var highlighting = new RecentChangeHighlighting(file, commit);
		yield return new HighlightingInfo(highlighting.CalculateRange(), highlighting);
	}
}
