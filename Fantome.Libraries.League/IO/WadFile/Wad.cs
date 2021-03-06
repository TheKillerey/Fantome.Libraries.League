﻿using Fantome.Libraries.League.Helpers.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Fantome.Libraries.League.IO.WadFile
{
    public class Wad : IDisposable
    {
        internal const int HEADER_SIZE_V3 = 272;

        public byte[] Signature { get; private set; }

        public ReadOnlyDictionary<ulong, WadEntry> Entries { get; private set; }
        private Dictionary<ulong, WadEntry> _entries = new();

        internal Stream _stream;

        private bool _isDisposed = false;
        private bool _leaveOpen = false;

        internal Wad(Stream stream, bool shouldRead, bool leaveOpen) : this(stream)
        {
            this._stream = stream;
            this._leaveOpen = leaveOpen;

            if(shouldRead)
            {
                Read(this._stream);
            }
        }
        internal Wad(Stream stream)
        {
            this._stream = stream;
            this.Entries = new(this._entries);
        }

        public static Wad Mount(string fileLocation, bool leaveOpen) => Mount(File.OpenRead(fileLocation), leaveOpen);
        public static Wad Mount(Stream stream, bool leaveOpen)
        {
            return new Wad(stream, true, leaveOpen);
        }

        private void Read(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                string magic = Encoding.ASCII.GetString(br.ReadBytes(2));
                if (magic != "RW")
                {
                    throw new InvalidFileSignatureException();
                }

                byte major = br.ReadByte();
                byte minor = br.ReadByte();
                if (major > 3)
                {
                    throw new UnsupportedFileVersionException();
                }

                ulong dataChecksum = 0; // probably not "dataChecksum"

                if (major == 2)
                {
                    byte ecdsaLength = br.ReadByte();
                    this.Signature = br.ReadBytes(ecdsaLength);
                    br.ReadBytes(83 - ecdsaLength);

                    dataChecksum = br.ReadUInt64();
                }
                else if (major == 3)
                {
                    this.Signature = br.ReadBytes(256);
                    dataChecksum = br.ReadUInt64();
                }

                if (major == 1 || major == 2)
                {
                    ushort tocStartOffset = br.ReadUInt16();
                    ushort tocFileEntrySize = br.ReadUInt16();
                }

                uint fileCount = br.ReadUInt32();
                for (int i = 0; i < fileCount; i++)
                {
                    WadEntry entry = new WadEntry(this, br, major);

                    if(this._entries.ContainsKey(entry.XXHash))
                    {
                        throw new InvalidOperationException("Tried to read a Wad Entry with the same path hash as an already existing entry: " + entry.XXHash);
                    }
                    else
                    {
                        this._entries.Add(entry.XXHash, entry);
                    }
                }
            }
        }

        internal void Write(Stream stream)
        {
            using (BinaryWriter bw = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                bw.Write(Encoding.ASCII.GetBytes("RW"));
                bw.Write((byte)3); // major
                bw.Write((byte)0); // minor

                // Writing signature
                bw.Write(new byte[256]);

                bw.Write((long)0);

                int tocSize = 32;
                long tocOffset = stream.Position + 4;

                bw.Write(this.Entries.Count);

                stream.Seek(tocOffset + (tocSize * this.Entries.Count), SeekOrigin.Begin);

                List<ulong> entryKeys = this._entries.Keys.ToList();
                entryKeys.Sort();

                // Write TOC
                stream.Seek(tocOffset, SeekOrigin.Begin);
                foreach(ulong entryKey in entryKeys)
                {
                    this._entries[entryKey].Write(bw, 3);
                }
            }
        }

        internal void AddEntry(WadEntry entry)
        {
            if(this._entries.ContainsKey(entry.XXHash))
            {
                throw new InvalidOperationException("Tried to add an entry with an already existing XXHash: " + entry.XXHash);
            }
            else
            {
                this._entries.Add(entry.XXHash, entry);
            }
        }

        public void Dispose()
        {
            if (!this._isDisposed)
            {
                if (!this._leaveOpen) this._stream?.Close();

                this._isDisposed = true;
            }
        }
    }
}
