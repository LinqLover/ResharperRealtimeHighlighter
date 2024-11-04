using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Daemon;

namespace ReSharperPlugin.ResharperRealtimeHighlighter;

[SolutionComponent]
public class DaemonInvalidator
{
	public DaemonInvalidator(RecentChangesTracker recentChangesTracker, IDaemon daemon)
	{
		recentChangesTracker.Changed += (_, _) => daemon.Invalidate("Repository was changed.");
	}
}
