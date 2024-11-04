using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon.SyntaxHighlighting;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;
using LibGit2Sharp;

namespace ReSharperPlugin.ResharperRealtimeHighlighter.Inspection;

[RegisterConfigurableSeverity(
	SeverityId,
	CompoundItemName: null,
	Group: HighlightingGroupIds.CodeSmell,
	Title: null,
	Description: null,
	DefaultSeverity: Severity.WARNING)]
[ConfigurableSeverityHighlighting(
	SeverityId,
	CSharpLanguage.Name,
	OverlapResolve = OverlapResolveKind.ERROR,
	OverloadResolvePriority = 0)]
public class RecentChangeHighlighting : IHighlighting
{
	public RecentChangeHighlighting(IFile file, Commit commit)
	{
		File = file;
		Commit = commit;
	}

	public const string SeverityId = "RecentChangeInspection";

	public const string Description = $"ReSharper SDK: {nameof(RecentChangeHighlighting)}.{nameof(Description)}";

	public const int PrefixLength = 5;

	public IFile File { get; }

	public Commit Commit { get; }

	public bool IsValid() => File.IsValid();

	public DocumentRange CalculateRange()
	{
		var fullRange = File.GetDocumentRangeTrimWhiteSpaces();
		var text = fullRange.GetText();
		var count = 0;
		var i = 0;
		for (; i < text.Length; i++)
		{
			var c = text[i];
			if (char.IsWhiteSpace(c))
				continue;
			if (++count >= PrefixLength)
				break;
		}
		return fullRange.SetEndTo(new DocumentOffset(fullRange.Document, fullRange.StartOffset.Offset + i + 1));
	}

	public string ToolTip => $"File was recently changed in commit {Commit.Id.ToString(7)}:\n{Commit.Message}";

	public string ErrorStripeToolTip => "Recently changed file";
}
