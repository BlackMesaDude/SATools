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

namespace SATools.IMG.Utility
{    
    /// <summary>
    /// Defines the structure of the archive
    /// </summary>
    public struct ArchiveFileData
    {
        public string[] ArchiveFiles;
        public string[] ArchiveRelatives;
    }

    /// <summary>
    /// Defines the structure of a file that resides inside the archive
    /// </summary>
    public class ArchiveFile
    {
        private static ArchiveFile _instance; // singleton instance to this class

        /// <summary>
        /// Gets the singleton instance of this class
        /// </summary>
        /// <value>Returns the instance of this file, if its null then a new one will be created</value>
        public static ArchiveFile SharedInstance 
        {
            get 
            {
                if(_instance == null)
                    _instance = new ArchiveFile();
                return _instance;
            }
        }

        private ArchiveFileData _archiveData; // defines an archive data structure
        /// <summary>
        /// Gets the file data of this file
        /// </summary>
        /// <value>Returns a struct that contains files and relatives</value>
        public ArchiveFileData FileData 
        {
            get => _archiveData;
        }

        private ArchiveFile() { /* private constructor used only for singleton purposes to avoid problems */ }

        #region Shortcut Methods

        /// <summary>
        /// Creates a file to the given directory
        /// </summary>
        /// <param name="dirName">Directory of the file</param>
        /// <param name="fileName">Name of the file</param>
        public void CreateTo(string dirName, string fileName) => CreateTo(dirName, fileName, false, Encoding.UTF8);
        /// <summary>
        /// Creates a file to the given directory
        /// </summary>
        /// <param name="dirName">Directory of the file</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="includeBase">Should it keep the base files?</param>
        public void CreateTo(string dirName, string fileName, bool includeBase) => CreateTo(dirName, fileName, includeBase, Encoding.UTF8);
        
        /// <summary>
        /// Extracts the archive file to a path
        /// </summary>
        /// <param name="pathToArchiveFile">Path to the archive file</param>
        /// <param name="finalPath">Output\Final path where it should be extracted</param>
        public void ExtractTo(string pathToArchiveFile, string finalPath) => ExtractTo(pathToArchiveFile, finalPath, Encoding.UTF8);

        /// <summary>
        /// Opens the archive with the given mode
        /// </summary>
        /// <param name="pathToArchiveFile">Path to the archive file to open</param>
        /// <param name="mode">Defines the way of how it needs to be processed during r\w</param>
        /// <returns></returns>
        public Archive Open(string pathToArchiveFile, Types.ArchiveMode mode) => Open(pathToArchiveFile, mode, Encoding.UTF8);        

        /// <summary>
        /// Open and reads the archive file
        /// </summary>
        /// <param name="pathToArchiveFile">Archive file to open</param>
        /// <returns></returns>
        public Archive OpenRead(string pathToArchiveFile) => Open(pathToArchiveFile, Types.ArchiveMode.Read);
        /// <summary>
        /// Open and reads the archive file with specified encoding
        /// </summary>
        /// <param name="pathToArchiveFile">Archive file to open</param>
        /// <param name="archiveEncoding">Archive encoding</param>
        /// <returns></returns>
        public Archive OpenRead(string pathToArchiveFile, Encoding archiveEncoding) => Open(pathToArchiveFile, Types.ArchiveMode.Read, archiveEncoding);

        #endregion

        /// <summary>
        /// Creates a file into the archive with the given path, name and encoding
        /// </summary>
        /// <param name="dirName">Path to the archive</param>
        /// <param name="fileName">Name of the new file</param>
        /// <param name="useBase">Should it keep the base data?</param>
        /// <param name="entryEncoding">Base encoding of this file</param>
        private void CreateTo(string dirName, string fileName, bool useBase, Encoding entryEncoding)
        {
            try 
            {
                // if the directory and file name aren't null
                if((dirName ?? fileName) != null)
                {
                    string oPath = Path.GetFullPath(dirName.TrimEnd('\\', '/')); // get outputs path
                    string dPath = Path.GetFullPath(fileName); // gets directory path

                    // if the output path exists
                    if(Directory.Exists(oPath))
                    {
                        _archiveData.ArchiveFiles = Directory.GetFiles(oPath, "*", SearchOption.AllDirectories); // assigns every file path inside that directory to the array
                        _archiveData.ArchiveRelatives = new string[_archiveData.ArchiveFiles.Length]; // creates the relatives array based on the files count

                        bool isSafe = true; // defines wheter or not the data is safe to process
                        for(int i = 0; i < _archiveData.ArchiveFiles.Length; i++)
                        {
                            // gets the relative paths and assigns them
                            _archiveData.ArchiveRelatives[i] = ArchiveUtils.GetRelativePath(_archiveData.ArchiveRelatives[i], useBase ? Directory.GetParent(oPath).FullName : oPath).Replace('\\', '/');
                            // if relative path exceeds 24 bytes then don't proceed with the data processing
                            if(_archiveData.ArchiveRelatives[i].Length > 24)
                            {
                                isSafe = false;
                                break;
                            }
                        }

                        // if the data is safe then
                        if(isSafe)
                        {
                            // open a new file stream based on the directory path
                            using(FileStream aStream = File.Open(dPath, FileMode.Create))
                            {
                                // open a binary writer based on the new stream
                                using(BinaryWriter aWriter = new BinaryWriter(aStream))
                                {
                                    // gets the prev offeset
                                    int pOffset = (((_archiveData.ArchiveFiles.Length * (int) DirectoryValues.DefaultLength) % (int) DirectoryValues.MaximumLength)) == 0 ?
                                                  ((_archiveData.ArchiveFiles.Length * (int) DirectoryValues.DefaultLength) / (int) DirectoryValues.MaximumLength) : 
                                                  (((_archiveData.ArchiveFiles.Length * (int) DirectoryValues.DefaultLength) / DirectoryValues.MaximumLength) + 1);
                                    
                                    int cOffset = pOffset; // sets the current offset

                                    List<System.Tuple<string, int>> aEntries = new List<Tuple<string, int>>(); // temporary archive entries
                                    aWriter.Write(DirectoryValues.GetEntryOffset(_archiveData.ArchiveFiles.Length)); // writes with the binary writer the entry offset

                                    // for every file present in this archive\directory then
                                    for(int i = 0; i < _archiveData.ArchiveFiles.Length; i++)
                                    {
                                        long fLen = GetFileLength(_archiveData.ArchiveFiles[i]); // defines the file length
                                        int eLen = (int) (((fLen % (long) DirectoryValues.MaximumLength) == 0) ? (fLen / (long) DirectoryValues.MaximumLength) : (long) ((DirectoryValues.MaximumLength) + 1)); // defines the entry length

                                        byte[] rNameBuffer = entryEncoding?.GetBytes(_archiveData.ArchiveRelatives[i]); // raw name buffer
                                        byte[] nameBuffer = new byte[24]; // literal name buffer

                                        Array.Copy(rNameBuffer, nameBuffer, Math.Min(rNameBuffer.Length, nameBuffer.Length)); // copies the raw name buffer to the literal one based on the minimum value between the lenght of the raw buffer and the literal one

                                        /* writes buffers and offsets to the binary writer stream */
                                        aWriter.Write(cOffset);
                                        aWriter.Write(new byte[] { (byte) (eLen & 0xFF), (byte) ((eLen >> 8) & 0xFF), 0x0, 0x0 });
                                        aWriter.Write(nameBuffer);

                                        aEntries.Add(new Tuple<string, int>(_archiveData.ArchiveFiles[i], cOffset)); // adds the new entry to the list

                                        cOffset += eLen; // proceed to the next offset
                                    }

                                    // for every entry in the list
                                    for(int i = 0; i < aEntries.Count; i++)
                                    {
                                        // open a new file stream based on the current entry from the list
                                        using(FileStream eStream = File.Open(aEntries[i].Item1, FileMode.Open, FileAccess.Read))
                                        {
                                            // while the archive stream length divided by the maximum reachable length is less than the entry offset
                                            while((aStream.Length / DirectoryValues.MaximumLength) < aEntries[i].Item2)
                                            {
                                                aStream.WriteByte(0); // add a byte with the value of 0 until the offset is reached
                                            }

                                            byte[] buffer = new byte[eStream.Length]; // buffer for the entry\file stream

                                            eStream.Read(buffer, 0, (int) eStream.Length); // reads and adds the data from the file stream to the buffer
                                            aWriter.Write(buffer); // writes the buffer
                                        }
                                    }

                                    // while the archive stream length divided by the maximum reachable length is less than the entry offset
                                    while((aStream.Length / DirectoryValues.MaximumLength) < cOffset)
                                    {
                                        aStream.WriteByte(0); // add a byte with the value of 0 until the offset is reached
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e) 
            {
                Console.WriteLine($"Unable to create {fileName} to {dirName} due to: {e}");
            }
        }

        /// <summary>
        /// Extracts a file from the archive to a output directory
        /// </summary>
        /// <param name="pathToFile">Path to the file to extract</param>
        /// <param name="finalPath">Path to where the file should be extracted</param>
        /// <param name="entryEncoding">Default encoding for the file</param>
        private void ExtractTo(string pathToFile, string finalPath, Encoding entryEncoding)
        {
            // if the two paths aren't null then
            if ((pathToFile ?? finalPath) != null)
            {
                // create a temporary archive and open it based on the given path and ecoding as read mode only
                Archive tArchive = Open(pathToFile, Types.ArchiveMode.Read, entryEncoding);

                // if the output directory doesnt exists then create it
                if(!(Directory.Exists(finalPath))) 
                    Directory.CreateDirectory(finalPath);
                
                // for every entry inside the archive
                for(int i = 0; i < tArchive?.ArchiveEntries.Length; i++)
                {
                    string fPath = Path.Combine(finalPath, tArchive.ArchiveEntries[i].FullName); // defines the file path
                    string dPath = Path.GetDirectoryName(fPath); // defines the directory path

                    // if the directory doesnt exists then create it
                    if(!(Directory.Exists(dPath)))
                        Directory.CreateDirectory(dPath);

                    // opens a file stream based on the file path
                    using(FileStream fStream = File.Open(fPath, FileMode.Create))
                    {
                        // opens a stream based on the current entry
                        using(Stream stream = tArchive.ArchiveEntries[i].Open())
                        {
                            byte[] buffer = new byte[DirectoryValues.MaximumLength]; // defines a buffer based on the directory maximum length

                            int rBytes = 0; // total counted bytes

                            // while the counted bytes are proceeding and greater than 0 then
                            while((rBytes = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fStream.Write(buffer, 0, rBytes); // write the buffer until the maximum count is reached
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Opens an archive file based on the given path, mode and encoding
        /// </summary>
        /// <param name="pathToArchive">Path to the file</param>
        /// <param name="mode">Mode that defines how the file should be r\w</param>
        /// <param name="archiveEncoding">Default file encoding</param>
        /// <returns></returns>
        private Archive Open(string pathToArchive, Types.ArchiveMode mode, Encoding archiveEncoding)
        {
            Archive tArchive = null; // temporary archive

            try 
            {
                Encoding tEncoding = ((archiveEncoding == null) ? Encoding.UTF8 : archiveEncoding); // if the encoding is still null or not given then switch to UTF8

                // if the path to the archive file isn't null
                if(pathToArchive != null)
                {
                    // if the mode is set to creation or the file exists
                    if((mode == Types.ArchiveMode.Create) || File.Exists(pathToArchive)) 
                    {
                        // create a new archive based on the path, mode and encoding
                        tArchive = new Archive(File.Open(pathToArchive, (mode == Types.ArchiveMode.Create) ? FileMode.Create : FileMode.Open, 
                                              (mode != Types.ArchiveMode.Read) ? FileAccess.ReadWrite : FileAccess.Read), mode, archiveEncoding);

                        // if the mode is set to create then
                        if(mode == Types.ArchiveMode.Create) 
                        {
                            tArchive.ArchiveStream.Write(ArchiveValues.Header, 0, ArchiveValues.Header.Length); // write the new header of the file
                            // while the archive stream length is less than the directory maximum reachable length 
                            while(tArchive.ArchiveStream.Length < DirectoryValues.MaximumLength) 
                            {
                                tArchive.ArchiveStream.WriteByte(0); // write a byte with the value of 0 until the length is reached
                            }                       
                        }
                        else 
                        {
                            /* this section reads the header of the file based on each buffer sequentially */   

                            byte[] version = new byte[4]; // version buffer
                            if(tArchive.ArchiveStream.Read(version, 0, version.Length) == version.Length)
                            {
                                string rVersion = Encoding.UTF8.GetString(version); // reads the version buffer stream and converts it to a string

                                byte[] inData = new byte[4]; // internal data buffer
                                if(tArchive.ArchiveStream.Read(inData, 0, inData.Length) == inData.Length)
                                {
                                    uint entriesCount = ArchiveValues.GetEntriesCount(inData); // reads the internal data buffer and calculates the total entries

                                    // if the archive version is VER2 and the archive stream length is greater equal to the calculated entries count * 8
                                    if((rVersion == "VER2") && (tArchive.ArchiveStream.Length >= (entriesCount * 8)))
                                    {
                                        for(uint i = 0; i != entriesCount; i++) 
                                        {
                                            if(tArchive.ArchiveStream.Read(inData, 0, inData.Length) == inData.Length)
                                            {
                                                long offset = ArchiveValues.GetOffset(inData); // reads the offset from the internal buffer and get its based on the current segment

                                                byte[] other = new byte[2]; // other data buffer
                                                if(tArchive.ArchiveStream.Read(other, 0, other.Length) == other.Length)
                                                {
                                                    int length = (other[0] | (other[1] << 8)) * 2048; // defines the length of this buffer based on its data

                                                    if(tArchive.ArchiveStream.Read(other, 0, other.Length) == other.Length)
                                                    {
                                                        byte[] rawName = new byte[24]; // defines the raw name of the file

                                                        if(tArchive.ArchiveStream.Read(rawName, 0, rawName.Length) == rawName.Length)
                                                        {
                                                            int bName = ArchiveUtils.GetNullableStringBytes(rawName); // reads the name from the raw one and gets its nullable contents
                                                            // if the literal name length is greater then 0
                                                            if(bName > 0)
                                                            {
                                                                string name = tEncoding.GetString(rawName, 0, bName); // convert the raw name to a string literal based on the length of the calculated one
                                                                tArchive.Entries.Add(name.ToLower(), new Types.ArchiveEntry(tArchive, offset, length, name)); // adds anew entry to the archive entries
                                                            }
                                                            else 
                                                            {
                                                                throw new InvalidDataException("An entry naming from the archive can't be empty.");
                                                            }
                                                        }
                                                        else 
                                                        {
                                                            throw new InvalidDataException("An entry name seems to be missing.");
                                                        }
                                                    }
                                                    else 
                                                    {
                                                        throw new InvalidDataException("An entry length seems to be missing.");
                                                    }
                                                }
                                                else 
                                                {
                                                    throw new InvalidDataException("An entry length seems to be missing.");
                                                }
                                            }
                                            else 
                                            {
                                                throw new InvalidDataException("An entry offset seems to be missing.");
                                            }
                                        }
                                    }
                                    else 
                                    {
                                        throw new InvalidDataException("The given archive uses a unknown version to this system.");
                                    }
                                }
                                else 
                                {
                                    throw new InvalidDataException("The given file isn't an archive.");
                                }
                            }
                            else 
                            {
                                throw new InvalidDataException("The given file isn't an archive.");
                            }
                        }
                    }
                }
                else 
                {
                    Console.WriteLine("Please give the path to the archive before opening it!");
                }

                if(tArchive == null) throw new InvalidOperationException("The archive reading operation finished with unsuccesfull results!"); 
            }
            catch(Exception e)
            {
                tArchive?.Dispose();
                tArchive = null;

                Console.WriteLine($"Unable to open and parse the archive due to: {e}");
            }

            return tArchive;
        }

        /// <summary>
        /// Gets the default file length
        /// </summary>
        /// <param name="file">Path to file</param>
        /// <returns>Returns a long value that defines the length of the file</returns>
        private long GetFileLength(string file) 
        {
            return new FileInfo(file).Length;
        }
    }
}