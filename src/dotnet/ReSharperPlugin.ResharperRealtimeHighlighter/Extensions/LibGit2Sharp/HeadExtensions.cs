using LibGit2Sharp;

namespace ReSharperPlugin.ResharperRealtimeHighlighter.Extensions.LibGit2Sharp;

internal static class HeadExtensions
{
	public static bool IsDetached(this Branch branch) => branch.CanonicalName == "(no branch)";
}
