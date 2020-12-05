/*
    Copyright (C) 2020 BlackMesaDude

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.IO;

namespace SATools.IMG.Types 
{
    using ArchiveEntryStream = Utility.IO.ArchiveEntryStream;

    /// <summary>
    /// Basic structure that defines a entry inside the archvie
    /// </summary>
    public class ArchiveEntry
    {
        private Archive _targetArchive; // target archive where the entry resides

        private long _entryOffset; // entry offset
        private int _entryLength; // entry length

        private string _fullEntryName; // entry full name

        private bool _isNew, _isAvailable = true; // defines wheter or not the entry is new and available for processing

        #region Getter(s) and Setter(s)

        /// <summary>
        /// Gets the entry offset
        /// </summary>
        /// <value></value>
        public long Offset { get => _entryOffset; }
        /// <summary>
        /// Gets the entry length
        /// </summary>
        /// <value></value>
        public int Length { get => _entryLength; }

        /// <summary>
        /// Gets the entry name
        /// </summary>
        /// <returns></returns>
        public string Name { get => Path.GetFileName(_fullEntryName); }
        /// <summary>
        /// Gets the entry full name (includes extension mark)
        /// </summary>
        /// <value></value>
        public string FullName { get => _fullEntryName; }

        /// <summary>
        /// Is the entry new?
        /// </summary>
        /// <value></value>
        public bool IsNew { get => _isNew; }
        /// <summary>
        /// Is the entry available for processing?
        /// </summary>
        /// <value></value>
        public bool IsAvailable { get => _isAvailable; }

        #endregion

        /// <summary>
        /// Creates a new entry based on the archive, offset, length an name
        /// </summary>
        /// <param name="target">Which archive should it target?</param>
        /// <param name="offset">Entry offset where it should reside</param>
        /// <param name="length">Entry length inside the archive</param>
        /// <param name="fullName">Name of this entry</param>
        public ArchiveEntry(Archive target, long offset, int length, string fullName) 
        {
            this._targetArchive = target;
            
            this._entryOffset = offset;
            this._entryLength = length;

            this._fullEntryName = fullName;
        }

        /// <summary>
        /// Creates a new entry based on the archive, offset, length and name
        /// </summary>
        /// <param name="target">Which archive should it target?</param>
        /// <param name="offset">Entry offset where it should reside</param>
        /// <param name="length">Entry length inside the archive</param>
        /// <param name="fullName">Name of this entry</param>
        /// <param name="newEntry">Is it a new entry?</param>
        public ArchiveEntry(Archive target, long offset, int length, string fullName, bool newEntry) 
        {
            this._targetArchive = target;
            
            this._entryOffset = offset;
            this._entryLength = length;

            this._fullEntryName = fullName;

            this._isNew = newEntry;
        }

        /// <summary>
        /// Opens this entry stream
        /// </summary>
        /// <returns></returns>
        public Stream Open()
        {
            ArchiveEntryStream tStream = null; // temporary entry stream

            try 
            { 
                // if the entry is available and the archive stream can read
                if(_isAvailable && _targetArchive.ArchiveStream.CanRead)
                {
                    byte[] buffer = new byte[_entryLength]; // create a buffer based on this entry length

                    _targetArchive.ArchiveStream.Seek(_entryLength, SeekOrigin.Begin); // set the position of the archive stream to the entry length
                    _targetArchive.ArchiveStream.Read(buffer, 0, _entryLength); // read the data from the archive buffer to this buffer until the entry length
                    
                    tStream = new ArchiveEntryStream(this); // create a new entry stream based on this entry

                    tStream.Write(buffer, 0, buffer.Length); // write the buffer to the entry stream
                    tStream.Seek(0, SeekOrigin.Begin); // reset the entry stream position

                    // when the entry stream is requested to close then refresh the data before doing it
                    tStream.OnCloseup += (entry, stream) => 
                    {
                        entry?.Refresh(stream);
                    };
                }
            }
            catch(Exception e) 
            {
                tStream?.Dispose();
                tStream = null;

                Console.WriteLine($"Unable to open entry {this.GetHashCode()} due to: {e}");
            }

            return tStream;
        }

        /// <summary>
        /// Deletes a entry
        /// </summary>
        public void Delete() 
        {
            // if its available then
            if(_isAvailable) 
            {
                _isAvailable = false; // set it to false, not available

                _targetArchive.Entries.Remove(_fullEntryName.ToLower()); // remove it from the archive list
                _targetArchive.RefreshEntry(null, null); // refresh it as null, this will attempt to change its contents to null removing it completely during the refresh
            }
        }

        /// <summary>
        /// Refreshes the contents of this entry
        /// </summary>
        /// <param name="stream">Stream that is operating on this entry</param>
        private void Refresh(ArchiveEntryStream stream) 
        {
            // if the mode isn't read
            if(_targetArchive.ArchiveMode != Types.ArchiveMode.Read)
            {
                _targetArchive.RefreshEntry(this, stream); // refresh this entry based on the given stream
            }
        }
    }
}