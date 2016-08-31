﻿using MiscUtil.Conversion;
using MiscUtil.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgBinaryReader : EndianBinaryReader
    {
        public PkgBinaryReader(System.IO.Stream stream)
            : base(EndianBitConverter.Little, stream, Encoding.UTF8)
        {
        }
        public PkgBinaryReader(EndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream, Encoding.UTF8)
        {

        }

        public PkgOffsetType ReadOffsetType()
        {
            PkgOffsetType offsetType = new PkgOffsetType();
            UInt32 temp = ReadUInt32();
            offsetType.Type = (Byte)(temp >> 24);
            offsetType.Offset = (Int32)(temp & 0x00FFFFFF);
            return offsetType;
        }

        public new string ReadString()
        {
            string s = base.ReadString();
            ReadByte();

            return s;
        }

        public string ReadString(int length)
        {
            byte[] bytes = this.ReadBytes(length);

            int startEnd = bytes.Length;
            for (int i = 0; i < bytes.Length; ++i)
            {
                if (bytes[i] == 0x00)
                {
                    startEnd = i;
                    break;
                }
            }

            return Encoding.GetString(bytes, 0, startEnd);
        }
    }
}
