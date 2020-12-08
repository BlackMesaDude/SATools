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

namespace SATools.IMG.Utility
{
    /// <summary>
    /// Archive utilities that can help with its data processing
    /// </summary>
    public static class ArchiveUtils 
    {
        /// <summary>
        /// Verifies a null-terminated string total length based on the given buffer and offset
        /// </summary>
        /// <param name="bytes">Buffer where the nullable string should be taken</param>
        /// <param name="offset">Offset where the search should start</param>
        /// <returns></returns>
        public static int NullTerminatedSearch(byte[] buffer, int offset = 0)
        {
            int len = offset; // temporary variable that will store the length of the string

            while(len < buffer?.Length && buffer[len] != 0)
            {
                len++; // while the length is less than the buffer length and the buffer element (based on the lenght value as index) is not equal to 0 then augment len by one
            }

            return len; // returns the nullable string length
        }

        /// <summary>
        /// Verifies a null-terminated string total length based on the given buffer and offset <br/>
        /// <b>This method uses unsafe code, it can't cause major problems but <i>discretion is advised</i>.</b>
        /// </summary>
        /// <param name="buffer">Buffer where the nullable string should be taken</param>
        /// <param name="offset">Offset where the search should start</param>
        /// <returns></returns>
        public static string UnsafeNullTerminatedSearch(byte[] buffer, int offset) 
        {
            int len = offset; // temporary variable that will store the length of the string

            while(len < buffer?.Length && buffer[len] != 0)
            {
                len++; // while the length is less than the buffer length and the buffer element (based on the lenght value as index) is not equal to 0 then augment len by one
            }

            /* to disable unsafe code head to the csproj file and set AllowUnsafeBlocks to false */
            unsafe
            {
                // creates a byte pointer to the buffer (because its marked as fixed the garbage collector won't reallocate it)
                fixed(byte* ascii = buffer)
                {
                    return new String((sbyte*) ascii, offset, len - offset); // returns a new string based on the byte pointer, at offset and length = len - offset
                }
            }
        }

        /// <summary>
        /// Gets a relative path based on the original one and a relative
        /// </summary>
        /// <param name="path">Non-relative path</param>
        /// <param name="relativeToPath">Relative element that resides in the non-relative path</param>
        /// <returns></returns>
        public static string GetRelativePath(string path, string relativeToPath)
        {
            return (new Uri(relativeToPath.EndsWith("\\") ? relativeToPath : (relativeToPath.EndsWith("/") ? relativeToPath : (relativeToPath + Path.DirectorySeparatorChar)))).MakeRelativeUri(new Uri(path)).ToString();
        }
    }
}