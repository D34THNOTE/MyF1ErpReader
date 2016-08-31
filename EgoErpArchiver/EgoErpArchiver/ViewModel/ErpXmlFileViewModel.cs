﻿using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Data.Pkg;
using EgoEngineLibrary.Graphics;
using EgoEngineLibrary.Xml;
using Microsoft.Win32;
using MiscUtil.Conversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EgoErpArchiver.ViewModel
{
    public class ErpXmlFileViewModel : ViewModelBase
    {
        readonly ErpResourceViewModel resView;

        public ErpResource XmlFile
        {
            get { return resView.Resource; }
        }
        public override string DisplayName
        {
            get { return XmlFile.FileName; }
        }

        #region Presentation Props
        bool isSelected;
        string preview;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    if (value)
                    {
                        Task.Run(() => GetPreview()).Wait();
                    }
                    else preview = string.Empty;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        public string Preview
        {
            get { return preview; }
            set { preview = value; OnPropertyChanged("Preview"); }
        }
        #endregion

        public ErpXmlFileViewModel(ErpResourceViewModel resView)
        {
            this.resView = resView;
        }

        public void GetPreview()
        {
            try
            {
                XmlFile xml = new XmlFile(XmlFile.Fragments[0].GetDataStream(true));
                System.Xml.XmlWriterSettings set = new System.Xml.XmlWriterSettings();
                set.Encoding = Encoding.UTF8;
                set.Indent = true;
                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = System.Xml.XmlWriter.Create(stringWriter, set))
                {
                    xml.doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    Preview = stringWriter.GetStringBuilder().ToString();
                }
            }
            catch (Exception ex)
            {
                this.Preview = "Could not create preview!" + Environment.NewLine + Environment.NewLine + ex.Message;
            }
        }
    }
}
