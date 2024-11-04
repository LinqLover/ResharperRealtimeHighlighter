using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Application.Settings;
using JetBrains.Application.Threading;
using JetBrains.Collections;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Threading;
using JetBrains.Util;
using LibGit2Sharp;
using ReSharperPlugin.ResharperRealtimeHighlighter.Git;
using ReSharperPlugin.ResharperRealtimeHighlighter.Options;

namespace ReSharperPlugin.ResharperRealtimeHighlighter;

/// <summary>
/// Detects and manages recent changes in the solution.
/// </summary>
[SolutionComponent]
public class RecentChangesTracker : IDisposable
{
	public RecentChangesTracker(Lifetime lifetime, ISolution solution, IThreading threading, ISettingsStore settingsStore)
	{
		Lifetime = lifetime;
		Solution = solution;
		Threading = threading;

		NumberOfCommits = settingsStore.BindToContextLive(Lifetime, ContextRange.ApplicationWide)
			.GetValueProperty(Lifetime, (Settings settingsKey) => settingsKey.NumberOfCommits)
			.Value;
		settingsStore.Changed.Advise(
			lifetime,
			(args) =>
			{
				var entry = settingsStore.Schema.GetScalarEntry(
					(Settings settingsKey) => settingsKey.NumberOfCommits);
				if (!args.ChangedEntries.Contains(entry)) return;
				var newNumberOfCommits = settingsStore.BindToContextLive(lifetime, ContextRange.ApplicationWide)
					.GetValueProperty2<int>(Lifetime, entry, null, ApartmentForNotifications.Mta())
					.Value;
				UpdateChanges(isCached: NumberOfCommits >= newNumberOfCommits);
				NumberOfCommits = newNumberOfCommits;
			});
		
		InitializeRepository();
	}

	/// <summary>
	/// Emitted when recently changes files are updated.
	/// </summary>
	public event EventHandler Changed;

	protected Lifetime Lifetime { get; set; }

	protected ISolution Solution { get; set; }

	protected IThreading Threading { get; }

	protected int NumberOfCommits { get; private set; }

	protected Repository Repository { get; private set; }

	protected RepositoryWatcher RepositoryWatcher { get; private set; }

	protected ConcurrentDictionary<string, Commit> RecentChanges = new();

	public void UpdateChanges(bool isCached = false)
	{
		Task.Run(() => UpdateChangesNow(isCached), Lifetime);
	}

	/// <summary>
	/// Searches for the most recent commit that changed the given file.
	/// </summary>
	public bool TryGetRecentCommit(string file, out Commit commit)
	{
		return RecentChanges.TryGetValue(file, out commit);
	}

	public void Dispose()
	{
		RepositoryWatcher?.Dispose();
		Repository?.Dispose();
	}

	protected void InitializeRepository()
	{
		var solutionDirectoryPath = Solution.SolutionDirectory.FullPath;
		if (!Repository.IsValid(solutionDirectoryPath))
			return;
		
		Repository = new Repository(solutionDirectoryPath);

		RepositoryWatcher = new RepositoryWatcher(Repository);
		// FOR LATER: reuse existing commits when a new commit was detected
		RepositoryWatcher.HeadTipChanged += (_, _) => UpdateChanges();
		UpdateChanges();
	}

	protected void UpdateChangesNow(bool isCached = false)
	{
		RecentChanges = ReadRecentChanges(Repository, isCached);

		Threading.Dispatcher.Invoke(Lifetime, "UpdateChanges", () => Changed?.Invoke(this, EventArgs.Empty));
	}

	/// <summary>
	/// Reads the most recent changes in the repository.
	/// </summary>
	/// <param name="repository">The git repository to read changes from.</param>
	/// <param name="isCached">Indicates whether all commits to consider are already read into RecentChanges.</param>
	protected ConcurrentDictionary<string, Commit> ReadRecentChanges(Repository repository, bool isCached = false)
	{
		var recentChanges = new ConcurrentDictionary<string, Commit>();
		
		var commits = repository.Commits.Take(NumberOfCommits);

		if (isCached)
		{
			commits = commits.ToHashSet();
			foreach (var (file, commit) in RecentChanges)
			{
				if (commits.Contains(commit))
				{
					recentChanges[file] = commit;
				}
			}
			return recentChanges;
		}

		foreach (var commit in commits.Reverse())
		{
			var changedFiles = GetChangedFiles(commit, repository);
			foreach (var file in changedFiles)
			{
				recentChanges[file] = commit;
			}
		}
		return recentChanges;
	}

	protected IEnumerable<string> GetChangedFiles(Commit commit, Repository repository)
	{
		var parent = commit.Parents.FirstOrDefault();
		if (parent != null)
		{
			var changes = repository.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree);
			// exclude deleted files
			return changes.Select(c => c.Path).WhereNotNull();
		}
		else
		{
			// first commit on branch (no parent)
			return commit.Tree.Select(entry => entry.Path);
		}
	}
}
