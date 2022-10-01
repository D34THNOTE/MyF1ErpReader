﻿using EgoEngineLibrary.IO;
using MiscUtil.Conversion;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace EgoEngineLibrary.Xml
{
    public enum XMLType
    {
        Text,
        BinXML,
        BXMLBig,
        BXMLLittle
    }

    public class XmlFile
    {
        private const uint BinXmlMagic = 1917985306; // bytes 0-3 1A 22 52 72 (."Rr)
        private const uint BxmlMagic = 1280137282; // bytes 1-4 42 58 4D 4C (BXML)

        public XMLType type { get; }
        
        public XmlDocument doc { get; }

        public static XMLType GetXmlType(Stream stream)
        {
            var pos = stream.Position;
            Span<byte> header = stackalloc byte[5];
            stream.ReadExactly(header);
            try
            {
                var binXmlMagic = BitConverter.ToUInt32(header);
                var bxmlMagic = BitConverter.ToUInt32(header[1..]);
                return binXmlMagic == BinXmlMagic ? XMLType.BinXML :
                    bxmlMagic == BxmlMagic ? header[0] == 0 ? XMLType.BXMLBig : XMLType.BXMLLittle : XMLType.Text;
            }
            finally
            {
                stream.Seek(pos, SeekOrigin.Begin);
            }
        }

        public static bool IsXmlFile(Stream stream)
        {
            var pos = stream.Position;
            Span<byte> header = stackalloc byte[5];
            stream.ReadExactly(header);
            try
            {
                var binXmlMagic = BitConverter.ToUInt32(header);
                var bxmlMagic = BitConverter.ToUInt32(header[1..]);
                return binXmlMagic == BinXmlMagic
                       || bxmlMagic == BxmlMagic
                       || IsValidXmlText(header, stream, pos);
            }
            finally
            {
                stream.Seek(pos, SeekOrigin.Begin);
            }

            static bool IsValidXmlText(ReadOnlySpan<byte> buffer, Stream stream, long startPos)
            {
                // Check for UTF8 BOM
                if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                {
                    buffer = buffer[3..];
                }
                
                // Check for valid start tags
                if (buffer[0] == '<')
                {
                    if (buffer[1] == '?' || buffer[1] == '!')
                    {
                        return true;
                    }

                    // Check valid xml element name
                    buffer = buffer[1..];
                    var charCount = Encoding.UTF8.GetCharCount(buffer);
                    if (charCount <= 0)
                    {
                        return false;
                    }

                    var chars = ArrayPool<char>.Shared.Rent(charCount);
                    try
                    {
                        Encoding.UTF8.GetChars(buffer, chars);
                        if (XmlConvert.IsStartNCNameChar(chars[0]))
                        {
                            return true;
                        }
                    }
                    finally
                    {
                        ArrayPool<char>.Shared.Return(chars);
                    }
                }

                // Last effort - see if we can read beginning.
                stream.Seek(startPos, SeekOrigin.Begin);
                var xmlReader = XmlReader.Create(stream);
                do
                {
                    if (!xmlReader.Read())
                    {
                        return false;
                    }
                } while (xmlReader.NodeType is XmlNodeType.Whitespace or XmlNodeType.SignificantWhitespace);

                return true;
            }
        }

        public XmlFile(Stream fileStream)
        {
            type = GetXmlType(fileStream);
            doc = new XmlDocument();

            if (type == XMLType.Text)
            {
                doc.Load(fileStream);
                foreach (XmlNode child in doc.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Comment)
                    {
                        if (Enum.TryParse<XMLType>(child.Value ?? string.Empty, out var pt) && Enum.IsDefined(pt))
                        {
                            type = pt;
                            break;
                        }
                    }
                }
            }
            else if (type == XMLType.BinXML)
            {
                using (XmlBinaryReader reader = new XmlBinaryReader(EndianBitConverter.Little, fileStream))
                {
                    // Section 1
                    reader.ReadByte(); // Unique Byte
                    reader.ReadBytes(3); // Same Magic
                    reader.ReadInt32(); // File Length/Size

                    // Section 2
                    reader.ReadByte(); // Unique Byte
                    reader.ReadBytes(3); // Same Magic
                    reader.ReadInt32(); // Section 3 and 4 Total Length/Size

                    // Section 3 and 4
                    var xmlStrings = new BinaryXmlString();
                    xmlStrings.Read(reader);

                    // Section 5
                    reader.ReadInt32();
                    var xmlElements = new BinaryXmlElement[reader.ReadInt32() / 24];
                    for (int i = 0; i < xmlElements.Length; i++)
                    {
                        xmlElements[i].elementNameID = reader.ReadInt32();
                        xmlElements[i].elementValueID = reader.ReadInt32();
                        xmlElements[i].attributeCount = reader.ReadInt32();
                        xmlElements[i].attributeStartID = reader.ReadInt32();
                        xmlElements[i].childElementCount = reader.ReadInt32();
                        xmlElements[i].childElementStartID = reader.ReadInt32();
                    }

                    // Section 6
                    reader.ReadInt32();
                    var xmlAttributes = new BinaryXmlAttribute[reader.ReadInt32() / 8];
                    for (int i = 0; i < xmlAttributes.Length; i++)
                    {
                        xmlAttributes[i].nameID = reader.ReadInt32();
                        xmlAttributes[i].valueID = reader.ReadInt32();
                    }

                    // Build XML
                    doc = new XmlDocument();
                    doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
                    doc.AppendChild(doc.CreateComment(type.ToString()));
                    doc.AppendChild(xmlElements[0].CreateElement(doc, xmlStrings, xmlElements, xmlAttributes));
                }
            }
            else
            {
                if (type == XMLType.BXMLBig)
                {
                    using (XmlBinaryReader reader = new XmlBinaryReader(EndianBitConverter.Big, fileStream))
                    {
                        reader.ReadBytes(5);
                        doc = new XmlDocument();
                        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
                        doc.AppendChild(doc.CreateComment(type.ToString()));
                        var rootElem = reader.ReadBxmlElement(doc);
                        if (rootElem is not null)
                            doc.AppendChild(rootElem);
                    }
                }
                else if (type == XMLType.BXMLLittle)
                {
                    using (XmlBinaryReader reader = new XmlBinaryReader(EndianBitConverter.Little, fileStream))
                    {
                        reader.ReadBytes(5);
                        doc = new XmlDocument();
                        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
                        doc.AppendChild(doc.CreateComment(type.ToString()));
                        var rootElem = reader.ReadBxmlElement(doc);
                        if (rootElem is not null)
                            doc.AppendChild(rootElem);
                    }
                }
            }
        }

        public void WriteXml(TextWriter textWriter)
        {
            doc.Save(textWriter);
        }

        public void Write(Stream stream)
        {
            Write(stream, type);
        }

        public void Write(Stream fileStream, XMLType convertType)
        {
            if (convertType == XMLType.Text)
            {
                // use text writer since by default it doesn't output the Encoding BOM
                using (var textWriter = new StreamWriter(fileStream, leaveOpen: true))
                {
                    var xmlTextWriter = new XmlTextWriter(textWriter);
                    doc.Save(xmlTextWriter);
                }
            }
            else if (convertType == XMLType.BinXML)
            {
                Dictionary<string, int> valuesToID = new Dictionary<string, int>();
                List<BinaryXmlElement> xmlElems = new List<BinaryXmlElement>();
                List<BinaryXmlAttribute> xmlAttrs = new List<BinaryXmlAttribute>();
                List<int> valueLocations = new List<int>();

                if (doc.DocumentElement is null)
                    throw new InvalidDataException("The root xml element does not exist.");

                BuildBinXml(doc.DocumentElement, valuesToID, xmlElems, xmlAttrs);
                valueLocations.Add(0);

                using (XmlBinaryWriter writer = new XmlBinaryWriter(EndianBitConverter.Little, fileStream))
                {
                    writer.Write(0x7252221A);
                    writer.Write(0);

                    // Section 2: Sections 3 and 4 Header and Total Size
                    writer.Write(0x72522217);
                    writer.Write(0); // 16 as Extra, Rest from Section 3 and 4 Length

                    // Section 3: Values/Strings
                    writer.Write(0x7252221D);
                    writer.Write(0);
                    foreach (string s in valuesToID.Keys)
                    {
                        valueLocations.Add(writer.WriteTerminatedString(s) + valueLocations[valueLocations.Count - 1] + 1);
                    }
                    // Write zero bytes to make length the same way CM made it
                    int remainder = (int)writer.BaseStream.Position % 16;
                    byte[] pad = new byte[remainder > 8 ? 24 - remainder : 8 - remainder];
                    writer.Write(pad);

                    // Section 4: Value/String Locations
                    writer.Write(0x7252221E);
                    writer.Write(4 * valuesToID.Count);
                    for (int i = 0; i < valueLocations.Count - 1; i++)
                    {
                        writer.Write(valueLocations[i]);
                    }

                    // Section 5: Element Definitions
                    writer.Write(0x7252221B);
                    writer.Write(24 * xmlElems.Count);
                    for (int i = 0; i < xmlElems.Count; i++)
                    {
                        writer.WriteBinaryXmlElement(xmlElems[i]);
                    }

                    // Section 6: Attribute Definitions
                    writer.Write(0x7252221C);
                    writer.Write(8 * xmlAttrs.Count);
                    for (int i = 0; i < xmlAttrs.Count; i++)
                    {
                        writer.WriteBinaryXmlAttribute(xmlAttrs[i]);
                    }

                    // Update Section Lenghts/Sizes
                    writer.Seek(4, System.IO.SeekOrigin.Begin);
                    writer.Write((int)writer.BaseStream.Length - 8); // Section 1: Total File

                    writer.Seek(12, System.IO.SeekOrigin.Begin);
                    writer.Write(valueLocations[valueLocations.Count - 1] + 4 * valuesToID.Count + 16 + pad.Length); // Section 2

                    writer.Seek(20, System.IO.SeekOrigin.Begin);
                    writer.Write(valueLocations[valueLocations.Count - 1] + pad.Length); // Section 3
                }
            }
            else if (convertType == XMLType.BXMLBig)
            {
                using (XmlBinaryWriter writer = new XmlBinaryWriter(EndianBitConverter.Big, fileStream))
                {
                    writer.Write((byte)0);
                    writer.Write(Encoding.UTF8.GetBytes("BXML"));

                    if (doc.DocumentElement is null)
                        throw new InvalidDataException("The root xml element does not exist.");

                    writer.WriteBxmlElement(doc.DocumentElement);

                    // File Ending: "0004 06000000" x2
                    writer.Write((Int16)0x0004);
                    writer.Write((Int16)0x0600);
                    writer.Write((Int16)0);
                    writer.Write((Int16)0x0004);
                    writer.Write((Int16)0x0600);
                    writer.Write((Int16)0);
                }
            }
            else if (convertType == XMLType.BXMLLittle)
            {
                using (XmlBinaryWriter writer = new XmlBinaryWriter(EndianBitConverter.Little, fileStream))
                {
                    writer.Write((byte)1);
                    writer.Write(Encoding.UTF8.GetBytes("BXML"));

                    if (doc.DocumentElement is null)
                        throw new InvalidDataException("The root xml element does not exist.");

                    writer.WriteBxmlElement(doc.DocumentElement);

                    // File Ending: "0004 06000000" x2
                    writer.Write((Int16)0x0004);
                    writer.Write((Int16)0x0006);
                    writer.Write((Int16)0);
                    writer.Write((Int16)0x0004);
                    writer.Write((Int16)0x0006);
                    writer.Write((Int16)0);
                }
            }
        }

        public void BuildBinXml(XmlElement elem, Dictionary<string, int> valuesToID, List<BinaryXmlElement> xmlElems, List<BinaryXmlAttribute> xmlAttrs, int childElemIndex = 0)
        {
            if (childElemIndex == 0)
            {
                xmlElems.Add(new BinaryXmlElement());
            }
            BinaryXmlElement binElem = new BinaryXmlElement();
            int currentIndex = xmlElems.Count - 1;

            // Element Name String
            if (!valuesToID.ContainsKey(elem.Name))
            {
                binElem.elementNameID = valuesToID.Count;
                valuesToID.Add(elem.Name, valuesToID.Count);
            }
            else
            {
                binElem.elementNameID = valuesToID[elem.Name];
            }

            binElem.attributeStartID = elem.Attributes.Count > 0 ? xmlAttrs.Count : 0;
            binElem.attributeCount = elem.Attributes.Count;
            foreach (XmlAttribute attr in elem.Attributes)
            {
                BinaryXmlAttribute binAttr = new BinaryXmlAttribute();

                // Attribute Name String
                if (!valuesToID.ContainsKey(attr.Name))
                {
                    binAttr.nameID = valuesToID.Count;
                    valuesToID.Add(attr.Name, valuesToID.Count);
                }
                else
                {
                    binAttr.nameID = valuesToID[attr.Name];
                }

                // Attribute Value String
                if (!valuesToID.ContainsKey(attr.Value))
                {
                    binAttr.valueID = valuesToID.Count;
                    valuesToID.Add(attr.Value, valuesToID.Count);
                }
                else
                {
                    binAttr.valueID = valuesToID[attr.Value];
                }

                xmlAttrs.Add(binAttr);
            }

            // Element Value or Child Elements
            // Set its childElementStartID to the next available index for binary xml elements 
            // AKA the current count, will be overwritten if element actually has child nodes
            binElem.childElementStartID = xmlElems.Count;
            if (elem.HasChildNodes)
            {
                // Values inside of an element are considered child nodes like <element>ThisValue</element>
                // More Specifically they're called "XmlText" or Text Nodes
                if (elem.ChildNodes[0] is XmlText textNode)
                {
                    //System.Windows.Forms.MessageBox.Show("daasda22222222222222222");
                    if (textNode.Value != null)
                    {
                        if (!valuesToID.ContainsKey(textNode.Value))
                        {
                            binElem.elementValueID = valuesToID.Count;
                            valuesToID.Add(textNode.Value, valuesToID.Count);
                        }
                        else
                        {
                            binElem.elementValueID = valuesToID[textNode.Value];
                        }
                    }
                }
                else
                {
                    binElem.childElementStartID = xmlElems.Count;
                    binElem.childElementCount = elem.ChildNodes.Count;

                    xmlElems.AddRange(new BinaryXmlElement[elem.ChildNodes.Count]);
                    int i = binElem.childElementStartID;
                    foreach (XmlElement childElem in elem.ChildNodes)
                    {
                        BuildBinXml(childElem, valuesToID, xmlElems, xmlAttrs, i);
                        i++;
                    }
                }
            }

            // Update the xmlElems Entry
            if (childElemIndex == 0)
            {
                xmlElems[currentIndex] = binElem;
            }
            else
            {
                xmlElems[childElemIndex] = binElem;
            }
        }
    }
}
