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
using System.Text;
using System.Collections.Generic;

namespace SATools.IMG 
{
    /// <summary>
    /// Defines the default structure of an archive (IMG) and allows to access its contents
    /// </summary>
    public class Archive : IDisposable 
    {
        private Stream _stream; // defines a default stream that will operate the archive data

        private Types.ArchiveMode _archiveMode; // defines the archive access mode
        private Encoding _archiveEncoding; // default archive stream encoding, UTF8 is mostly used

        private Dictionary<string, Types.ArchiveEntry> _entries = new Dictionary<string, Types.ArchiveEntry>(); // dictionary that contains every unique entry inside the archive

        /// <summary>
        /// Gets the default archive stream that has the job to read or write the data inside the archive
        /// </summary>
        public Stream ArchiveStream { get => _stream; }
        
        /// <summary>
        /// Gets the archive access mode 
        /// </summary>
        public Types.ArchiveMode ArchiveMode { get => _archiveMode; }
        /// <summary>
        /// Gets the default encoding used to parse the archive data (UTF8 is the default in use)
        /// </summary>
        public Encoding ArchiveEncoding { get => _archiveEncoding; }

        /// <summary>
        /// Gets a dictionary that contains every unique entry that resides in the archive
        /// </summary>
        /// <value></value>
        public Dictionary<string, Types.ArchiveEntry> Entries { get => _entries; }

        /// <summary>
        /// Gets an array of every available entry
        /// </summary>
        /// <value></value>
        public Types.ArchiveEntry[] ArchiveEntries
        {
            get
            {
                return (new List<Types.ArchiveEntry>(_entries.Values)).ToArray(); // converts the dictionary values to a list then to an array
            }
        }

        /// <summary>
        /// Creates an archive based on a stream, mode and encoding
        /// </summary>
        /// <param name="stream">Stream that will take action of r\w operations</param>
        /// <param name="mode">Mode that will define how the data will be processed</param>
        /// <param name="encoding">Default encoding for other streams</param>
        public Archive(Stream stream, Types.ArchiveMode mode, Encoding encoding)
        {
            this._stream = stream;
            this._archiveMode = mode;
            this._archiveEncoding = encoding;
        }

        /// <summary>
        /// Creates a new entry inside the archive
        /// </summary>
        /// <param name="name">The new entry name</param>
        /// <returns>Returns the new ArchiveEntry object</returns>
        public Types.ArchiveEntry CreateEntry(string name)
        {
            Types.ArchiveEntry tEntry = null; // temporary entry that will store the result
            string fName = name.Trim(); // trims the name to remove some invalid chars like spaces

            char[] invalids = Path.GetInvalidPathChars(); // stores some invalid chars inside an array

            bool proceed = true; // boolean that defines whether or not to proceed the entry creation
            for(int i = 0; i < invalids.Length; i++)
            {
                // if the trimmed name contains the known invalid chars then don't proceed and repeat the control
                if(fName.Contains(new string(new char[] { invalids[i] })))
                {
                    proceed = false;
                    break;
                }
            }

            // if the creation process can proceed then 
            if(proceed)
            {
                string key = fName.ToLower(); // write the dictionary key to lowercase
                if(!(_entries.ContainsKey(key))) // if the dictionary key doesn't exists
                    _entries.Add(key, new Types.ArchiveEntry(this, _stream.Length, 0, fName, true)); // add it as a new key and create a entry for it
            }

            return tEntry; // return the processed entry
        }

        /// <summary>
        /// Gets an entry inside the archive based on its name
        /// </summary>
        /// <param name="name">Name of the entry that should be readden</param>
        /// <returns>Returns the parsed ArchiveEntry object</returns>
        public Types.ArchiveEntry GetEntry(string name)
        {
            Types.ArchiveEntry tEntry = null; // temporary entry that will store the result

            // if the given name isn't null or empty
            if (name != null)
            {
                string key = name.ToLower(); // converts the name to lowercase
                if (_entries.ContainsKey(key)) // if the dictionary contains this entry\name\key
                {
                    tEntry = _entries[key]; // then assign the value to the temporary variable
                }
            }

            return tEntry; // returns the processed entry
        }

        /// <summary>
        /// Adds an entry with new content
        /// </summary>
        /// <param name="archive">Archive where this entry resides</param>
        /// <param name="newEntry">The new entry that should be placed inside the archive</param>
        /// <param name="stream">The stream that will have the job to r\w</param>
        public void WriteEntry(Archive archive, Types.ArchiveEntry newEntry, Stream stream)
        {
            // opens a new stream based on the other stream of the archive entry to modify
            using (Stream tStream = archive.Entries[newEntry.FullName.ToLower()].Open())
            {
                byte[] tBuffer = new byte[tStream.Length]; // buffer of this stream

                tStream.Read(tBuffer, 0, tBuffer.Length); // writes the data to add into the buffer
                stream.Write(tBuffer, 0, tBuffer.Length); // uses the targetted entry stream and replaces the current data with the new one from the buffer
            }
        }

        /// <summary>
        /// Tries to update or refresh a entry inside the archive
        /// </summary>
        /// <param name="entry">Entry that should be refreshed</param>
        /// <param name="archiveStream">Entry stream that is reading the entry to refresh</param>
        public void RefreshEntry(Types.ArchiveEntry entry, Utility.IO.ArchiveEntryStream archiveStream)
        {
            try
            {
                /// if the mode isn't read-only
                if (_archiveMode != Types.ArchiveMode.Read)
                {
                    string tPath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".img"; // generates the path to the temporary .img file inside the default temp folder

                    // if the temp path already exists then delete it and re-create
                    if (File.Exists(tPath))
                        File.Delete(tPath);
                    
                    // opens a file stream that will operate on the temporary file
                    using (FileStream tStream = File.Open(tPath, FileMode.Create))
                    {
                        byte[] buffer = new byte[EntryValues.MaximumBufferSize]; // creates a empty buffer with the maximum size that a entry can reach
                        this._stream.Seek(0L, SeekOrigin.Begin); // sets the current position of the stream to the given value

                        int bLen = 0; // defines the byte length of the entry
                        long sLen = this._stream.Length; // defines the stream length
                        // while the minum between the stream length - its position and the buffer length is greater than 0 then
                        while ((bLen = Math.Min((int) (sLen - this._stream.Position), buffer.Length)) > 0)
                        {   
                            // if the reading of the buffer reads until the given length
                            if (this._stream.Read(buffer, 0, bLen) == bLen)
                                tStream.Write(buffer, 0, bLen); // write the buffer at offset 0 and based on the final length
                            else
                                break; // otherwhise exit from the loop
                        }
                    }

                    /// uses a temporary archive object to read the temporary file
                    using (Archive tArchive = Utility.ArchiveFile.SharedInstance.OpenRead(tPath))
                    {
                        _entries.Clear(); // clears the entries on the dictionary

                        this._stream.SetLength(0L); // sets the stream back to its original length

                        int eCount = tArchive._entries.Values.Count + ((entry == null) ? 0 : (entry.IsNew ? 1 : 0)); // defines the entry amount
                        int pOffset = EntryValues.GetEntryOffset(eCount); // defines the entry prev offset
                        int cOffset = pOffset; // defines the entry current offset

                        List<Types.ArchiveEntry> nEntries = new List<Types.ArchiveEntry>(); // new entris that should be replaced in the archive

                        byte[] header = DirectoryValues.GetEntryOffset(eCount); // gets the header buffer and assigns it to an array
                        this._stream.Write(header, 0, header.Length); // writes the header in the buffer

                        Dictionary<string, Types.ArchiveEntry> tEntries = new Dictionary<string, Types.ArchiveEntry>(tArchive._entries); // temporary dictionary that will store the result

                        if (entry != null)
                        {
                            if (entry.IsNew)
                            {
                                tEntries.Add(entry.FullName.ToLower(), new Types.ArchiveEntry(tArchive, 0L, (int)(_stream.Length), entry.FullName)); // if the entry has been marked as new then add it to the dictionary
                            }
                        }

                        /*
                            for each pair inside the dictionary 

                            TODO: replace foreach with a non-resource-heavy loop
                        */
                        foreach (KeyValuePair<string, Types.ArchiveEntry> currentEntry in tEntries)
                        {
                            int eLen = EntryValues.GetEntryLength(entry, currentEntry, _stream); // defines the entry length from the current pair with the given stream

                            /* defines the default values of the entry */
                            byte[] eOffset = EntryValues.GetEntryByteOffset(cOffset); // entry offset
                            byte[] eLength = EntryValues.GetEntryLength(eLen); // entry length
                            byte[] rEntryName = _archiveEncoding.GetBytes(currentEntry.Value.FullName); // entry raw name (in bytes)
                            byte[] eName = new byte[24]; // entry name (nulled)

                            Array.Copy(rEntryName, eName, Math.Min(rEntryName.Length, eName.Length)); // copies the raw name to the literal name based on the minimal value between the length of the raw entry and the literal

                            /* writes offset, length and name of the stream buffer */
                            this._stream.Write(eOffset, 0, eOffset.Length);
                            this._stream.Write(eLength, 0, eLength.Length);
                            this._stream.Write(eName, 0, eName.Length);

                            Types.ArchiveEntry nEntry = new Types.ArchiveEntry(this, cOffset * 2048, eLen * 2048, currentEntry.Value.FullName); // creates a new entry based on the calcualted values
                            nEntries.Add(nEntry); // adds the new entry to the new list of entries
                            _entries.Add(nEntry.FullName.ToLower(), nEntry); // adds to the list of entries the new entry

                            cOffset += eLen; // go to next offset
                        }

                        /*
                            for each entry inside the new entries list

                            TODO: replace foreach with a non-resource-heavy loop
                        */
                        foreach (Types.ArchiveEntry cEntry in nEntries)
                        {
                            /// while the stream length isn't at at the current entry offset
                            while (this._stream.Length < cEntry.Offset)
                            {
                                this._stream.WriteByte(0); // write a byte with the value of 0 until the cEntry offset is reached
                            }

                            if (entry != null)
                            {
                                // if the entry name is equal to the current entry name
                                if (entry.FullName == cEntry.FullName)
                                {
                                    byte[] buffer = new byte[_stream.Length]; // create a buffer based on the stream length

                                    _stream.Seek(0L, SeekOrigin.Begin); // restore the stream position
                                    _stream.Read(buffer, 0, buffer.Length); // read the data and add it to the buffer

                                    this._stream.Write(buffer, 0, buffer.Length); // write the data to the buffer
                                }
                                else
                                {
                                    WriteEntry(tArchive, cEntry, this._stream); // add a new entry
                                }
                            }
                            else
                            {
                                WriteEntry(tArchive, cEntry, this._stream); // add a new entry
                            }
                        }

                        // while the stream length / 2048 doesnt exceed the current offset 
                        while ((this._stream.Length / 2048) < cOffset)
                        {
                            this._stream.WriteByte(0); // write a byte of value 0 until the offset is reached
                        }
                    }

                    // if the temporary file exists delete it
                    if (File.Exists(tPath))
                    {
                        File.Delete(tPath);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Unable to refresh the entry due to: {e}");
            }
        }

        /// <summary>
        /// Tries to dispose the archive
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}