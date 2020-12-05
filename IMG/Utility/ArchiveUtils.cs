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
        /// Gets or verifies a nullable string length from a buffer
        /// </summary>
        /// <param name="bytes">Buffer where the nullable string should be taken</param>
        /// <returns></returns>
        public static int GetNullableStringBytes(byte[] buffer)
        {
            int len = 0; // temporary variable that will store the length of the string
            // if the buffer isn't null or empty
            if(buffer != null)
            {
                len = buffer.Length; // assign the buffer length to the temporary variable
                for(int i = 0; i < buffer.Length; i++)
                { 
                    // if the buffer n element is 0 then
                    if(buffer[i] == 0)
                    {
                        len = i; // assign the value to the length
                        break; // exit from the loop
                    }
                }
            }

            return len; // returns the nullable string length
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