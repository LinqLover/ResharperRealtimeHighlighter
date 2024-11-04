using System;
using System.IO;
using LibGit2Sharp;
using ReSharperPlugin.ResharperRealtimeHighlighter.Extensions.LibGit2Sharp;

namespace ReSharperPlugin.ResharperRealtimeHighlighter.Git;

/// <summary>
/// Watches a git repository for changes to the HEAD and the HEAD tip.
/// </summary>
public class RepositoryWatcher : IDisposable
{
    public RepositoryWatcher(Repository repository)
    {
        Repository = repository;

        HeadWatcher = new FileSystemWatcher
        {
            Path = repository.Info.Path,
            Filter = "HEAD",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
            EnableRaisingEvents = true
        };
        HeadWatcher.Created += HeadModified;
        HeadWatcher.Changed += HeadModified;
        HeadWatcher.Renamed += HeadModified;
        UpdateHead();
    }

	/// <summary>
	/// Emitted when the tip of the HEAD changes.
	/// </summary>
	public event EventHandler HeadTipChanged;

    protected Repository Repository { get; }

    protected FileSystemWatcher HeadWatcher { get; }

    protected FileSystemWatcher HeadTipWatcher { get; private set; }

    protected string HeadName { get; private set; }

    protected string HeadTipHash { get; private set; }

    public void Dispose()
    {
        HeadWatcher?.Dispose();
        HeadTipWatcher?.Dispose();
    }

    protected void UpdateHead()
    {
        var headName = Repository.Head.CanonicalName;
        if (headName == HeadName) return;
        HeadName = headName;

        HeadTipWatcher?.Dispose();

        if (Repository.Head.IsDetached())
        {
            HeadTipWatcher = null;
        }
        else
        {
            var referencePath = Path.Combine(Repository.Info.Path, Repository.Head.Reference.TargetIdentifier);
            HeadTipWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(referencePath),
                Filter = Path.GetFileName(referencePath),
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
			HeadTipWatcher.Created += HeadTipModified;
            HeadTipWatcher.Changed += HeadTipModified;
            HeadTipWatcher.Renamed += HeadTipModified;
        }

        UpdateHeadTip();
    }

    protected void UpdateHeadTip()
    {
        var headTipHash = Repository.Head.Tip.Id.Sha;
        if (headTipHash == HeadTipHash) return;
        HeadTipHash = headTipHash;

        HeadTipChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HeadModified(object sender, FileSystemEventArgs e)
    {
        UpdateHead();
    }

    private void HeadTipModified(object sender, FileSystemEventArgs e)
    {
        UpdateHeadTip();
    }
}
