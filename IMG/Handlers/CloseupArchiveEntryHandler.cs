namespace SATools.IMG.Handlers
{
    /// <summary>
    /// Delegate used to handle closeup event for the ArchiveEntryStream.
    /// </summary>
    /// <param name="entry">Archive entry that is being closed up</param>
    /// <param name="stream">Stream that is manipulating the entry</param>
    public delegate void CloseupArchiveEntryHandler(Types.ArchiveEntry entry, Utility.IO.ArchiveEntryStream stream);
}