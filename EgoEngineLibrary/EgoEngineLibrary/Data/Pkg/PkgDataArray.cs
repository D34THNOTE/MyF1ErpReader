﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgDataArray : PkgArray
    {
        protected override string ChunkType
        {
            get
            {
                return "!vca";
            }
        }

        public PkgDataArray(PkgFile parentFile)
            : base(parentFile)
        {
        }

        internal override void UpdateOffsets()
        {
            PkgValue._offset += 8 + 4 * Elements.Count;

            foreach (PkgValue val in Elements)
            {
                UpdatePaddingLength(val);
                val.UpdateOffsets();
            }
        }
        private void UpdatePaddingLength(PkgValue val)
        {
            int padding;

            PkgData data = (PkgData)val.ComplexValueData;
            padding = data.GetPaddingLength(PkgValue._offset);

            PkgValue._offset += padding;
        }

        public string GetData(PkgOffsetType offsetType)
        {
            return ((PkgData)Elements[offsetType.Type].ComplexValueData).GetData(offsetType);
        }
        public void SetData(string data, PkgOffsetType offsetType)
        {
            string type = data.Remove(4);
            data = data.Substring(5);

            offsetType.Type = (byte)AddData(type);

            ((PkgData)Elements[offsetType.Type].ComplexValueData).SetData(data, offsetType);
        }
        private int AddData(string type)
        {
            int typeIndex = Elements.FindIndex(x => ((PkgData)x.ComplexValueData).Type == type);
            
            if (typeIndex < 0)
            {
                typeIndex = (byte)Elements.Count;
                PkgValue val = new PkgValue(ParentFile);
                val.ValueOffsetType.Type = 128;
                if (type == "stri")
                {
                    PkgStringData stringData = new PkgStringData(ParentFile);
                    val.ComplexValueData = stringData;
                }
                else
                {
                    PkgByteData byteData = new PkgByteData(ParentFile, type);
                    val.ComplexValueData = byteData;
                }
                Elements.Add(val);
                return typeIndex;
            }
            else
            {
                return typeIndex;
            }
        }
    }
}
