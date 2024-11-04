using JetBrains.Application.UI.Controls.FileSystem;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.IDE.UI;
using JetBrains.IDE.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.UnitTestFramework.Resources;

namespace ReSharperPlugin.ResharperRealtimeHighlighter.Options;

[OptionsPage(Pid, PageTitle, typeof(UnitTestingThemedIcons.Session),
	ParentId = ToolsPage.PID)]
public class OptionsPage : BeSimpleOptionsPage
{
	private const string Pid = nameof(OptionsPage);
	private const string PageTitle = "ResharperRealtimeHighlighter";

	public OptionsPage(Lifetime lifetime,
		OptionsPageContext optionsPageContext,
		OptionsSettingsSmartContext optionsSettingsSmartContext,
		IconHostBase iconHost,
		ICommonFileDialogs dialogs)
		: base(lifetime, optionsPageContext, optionsSettingsSmartContext)
	{
		AddText("This plugin highlights recent changes in your solution.");
		AddSpacer();
		
		AddIntOption((Settings x) => x.NumberOfCommits, "Number of commits to highlight");
	}
}
