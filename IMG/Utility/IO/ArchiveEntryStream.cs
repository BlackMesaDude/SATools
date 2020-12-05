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

using System.IO;

namespace SATools.IMG.Utility.IO 
{
    /// <summary>
    /// Special stream used to read entries, it's based on the MemoryStream
    /// </summary>
    public class ArchiveEntryStream : MemoryStream
    {
        private Types.ArchiveEntry _targetEntry; // defines the target entry where this stream should operate

        /// <summary>
        /// What should the stream do when a close operation is requested on the stream?
        /// </summary>
        public event Handlers.CloseupArchiveEntryHandler OnCloseup; 

        public ArchiveEntryStream(Types.ArchiveEntry target) : base() 
        {
            this._targetEntry = target;
        }

        public ArchiveEntryStream(Types.ArchiveEntry target, byte[] buffer) : base(buffer)
        {
            this._targetEntry = target;
        }

        public ArchiveEntryStream(Types.ArchiveEntry target, int capacity) : base(capacity)
        {
            this._targetEntry = target;
        }

        public ArchiveEntryStream(Types.ArchiveEntry target, byte[] buffer, bool writeable) : base(buffer, writeable)
        {
            this._targetEntry = target;
        }

        public ArchiveEntryStream(Types.ArchiveEntry target, byte[] buffer, int index, int count) : base(buffer, index, count)
        {
            this._targetEntry = target;
        }

        public ArchiveEntryStream(Types.ArchiveEntry target, byte[] buffer, int index, int count, bool writeable) : base(buffer, index, count, writeable)
        {
            this._targetEntry = target;
        }

        public ArchiveEntryStream(Types.ArchiveEntry target, byte[] buffer, int index, int count, bool writeable, bool visible) : base(buffer, index, count, writeable, visible)
        {
            this._targetEntry = target;
        }

        /// <summary>
        /// Closes this entry stream
        /// </summary>
        public override void Close() 
        {
            if(OnCloseup != null) OnCloseup(_targetEntry, this);
            OnCloseup = null;

            base.Close();
        }
    }
}