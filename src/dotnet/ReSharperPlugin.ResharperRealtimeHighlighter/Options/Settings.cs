using JetBrains.Application.Settings;
using JetBrains.Application.Settings.WellKnownRootKeys;


namespace ReSharperPlugin.ResharperRealtimeHighlighter.Options;

[SettingsKey(
	Parent: typeof(EnvironmentSettings),
	Description: "ResharperRealtimeHighlighter Settings",
	KeyNameOverride = "ResharperRealtimeHighlighterSettings")]
public class Settings
{
	[SettingsEntry(DefaultValue: 42, Description: "Number of most recent commits to highlight")]
	public int NumberOfCommits;
}
