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

namespace SATools.IMG 
{
    /// <summary>
    /// Standard archive values for VER2 archive(s)
    /// </summary>
    public static class ArchiveValues 
    {
        /// <summary>
        /// Default header data position
        /// </summary>
        /// <value></value>
        public static byte[] Header => new byte[] { 0x56, 0x45, 0x52, 0x32, 0x0, 0x0, 0x0, 0x0 };

        /// <summary>
        /// Gets offset(s) of the internal data, such as entry reading positions
        /// </summary>
        /// <param name="internalData">Buffer which contains the internal data of the archive</param>
        /// <returns>Returns a long value which defines the offset(s) inside the internal data buffer</returns>
        public static long GetOffset(byte[] internalData) => (internalData[0] | (((long) (internalData[1])) << 8) | (((long) (internalData[2])) << 16) | (((long) (internalData[3])) << 24)) * 2048L;
        /// <summary>
        /// Gets the total entries count inside the archive
        /// </summary>
        /// <param name="internalData">Buffer which contains the internal data of the archive</param>
        /// <returns>Returns a unsigend integer which defines the amount of entries inside the archive</returns>
        public static uint GetEntriesCount(byte[] internalData) => internalData[0] | (((uint) (internalData[1])) << 8) | (((uint) (internalData[2])) << 16) | (((uint) (internalData[3])) << 24);
    }

    /// <summary>
    /// Standard directory values for VER2 archive(s)
    /// </summary>
    public static class DirectoryValues 
    {
        /// <summary>
        /// Minimal or standard length of the directory 
        /// </summary>
        public static int DefaultLength => 34;
        /// <summary>
        /// Maximum reachable length for each the directory
        /// </summary>
        public static int MaximumLength => 2048;

        /// <summary>
        /// Length of a nulled string that defines the directory name
        /// </summary>
        public static int NamingLength => 24;

        /// <summary>
        /// Gets the starting entry offset based on the directory length
        /// </summary>
        /// <param name="dirLength">Directory length</param>
        /// <returns>Returns a byte buffer that contains the offset based on the given length</returns>
        public static byte[] GetEntryOffset(int dirLength)
        {
            return new byte[] { 0x56, 0x45, 0x52, 0x32, (byte) (dirLength & 0xFF), (byte) ((dirLength >> 8) & 0xFF), 0x0, 0x0 };
        }
    }

    /// <summary>
    /// Standard entry values for VER2 archive(s)
    /// </summary>
    public static class EntryValues 
    {
        /// <summary>
        /// Maximum reachable entry buffer size
        /// </summary>
        public static int MaximumBufferSize => 4096;

        /// <summary>
        /// Sets a entry based on a paired one
        /// </summary>
        /// <param name="entry">Where should the entry be placed?</param>
        /// <param name="currentEntry">Defines which pair we should convert into a entry</param>
        /// <param name="stream">Defines the current stream that is operating on the pair\entry</param>
        /// <returns>Returns a signed integer value that defines the total length of that specific entry</returns>
        public static int GetEntryLength(Types.ArchiveEntry entry, System.Collections.Generic.KeyValuePair<string, Types.ArchiveEntry> currentEntry, System.IO.Stream stream)
        {
            return (int) ((entry == null) ? (((currentEntry.Value.Length % 2048L) == 0L) ? (currentEntry.Value.Length / 2048L) : ((currentEntry.Value.Length / 2048L) + 1L)) : 
                                            ((entry.FullName.ToLower() == currentEntry.Key) ? (((stream.Length % 2048L) == 0) ? (stream.Length / 2048L) : ((stream.Length / 2048L) + 1)) : 
                                            (((currentEntry.Value.Length % 2048L) == 0L) ? (currentEntry.Value.Length / 2048L) : ((currentEntry.Value.Length / 2048L) + 1L))));
        }

        /// <summary>
        /// Gets a buffer that defines the entry length data in-between
        /// </summary>
        /// <param name="length">Internal data length or offset where this entry resides</param>
        /// <returns></returns>
        public static byte[] GetEntryLength(int length) 
        {
            return new byte[] { (byte) (length & 0xFF), (byte) ((length >> 8) & 0xFF), 0x0, 0x0 };
        }

        /// <summary>
        /// Gets a buffer that defines the entry offset data in-between
        /// </summary>
        /// <param name="offset">Offset where the data should be taken</param>
        /// <returns></returns>
        public static byte[] GetEntryByteOffset(int offset)
        {
            return new byte[] { (byte) (offset & 0xFF), (byte) ((offset >> 8) & 0xFF), (byte) ((offset >> 16) & 0xFF), (byte) ((offset >> 24) & 0xFF) };
        }

        /// <summary>
        /// Similiar to <see cref="GetEntryOffset(int)">, returns a integer instead of a byte buffer that defines the entry offset length
        /// </summary>
        /// <param name="count">How many entries should be taken in count?</param>
        /// <returns></returns>
        public static int GetEntryOffset(int count)
        {
            return ((((count * 32) % 2048) == 0) ? ((count * 32) / 2048) : (((count * 32) / 2048) + 1));
        }
    }
}