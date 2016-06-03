﻿using EgoEngineLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EgoPssgEditor.ViewModel
{
    public class PssgTextureViewModel : ViewModelBase
    {
        PssgNode texture;

        public PssgNode Texture
        {
            get { return texture; }
        }
        public override string DisplayName
        {
            get { return texture.Attributes["id"].DisplayValue; }
        }
        public int Width
        {
            get { return (int)(uint)texture.Attributes["width"].Value; }
        }
        public int Height
        {
            get { return (int)(uint)texture.Attributes["height"].Value; }
        }

        #region Presentation Props
        bool isSelected;
        BitmapSource preview;
        string previewError;
        Visibility previewErrorVisibility;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    if (value) GetPreview(); else preview = null;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        public BitmapSource Preview
        {
            get { return preview; }
            set { preview = value; OnPropertyChanged("Preview"); }
        }
        public string PreviewError
        {
            get { return previewError; }
            set { previewError = value; OnPropertyChanged("PreviewError"); }
        }
        public Visibility PreviewErrorVisibility
        {
            get { return previewErrorVisibility; }
            set { previewErrorVisibility = value; OnPropertyChanged("PreviewErrorVisibility"); }
        }
        #endregion

        public PssgTextureViewModel(PssgNode texture)
        {
            this.texture = texture;
        }

        public void GetPreview()
        {
            try
            {
                this.Preview = null;
                DdsFile dds = new DdsFile(texture, false);
                dds.Write(File.Open(System.AppDomain.CurrentDomain.BaseDirectory + "\\temp.dds", FileMode.Create, FileAccess.ReadWrite, FileShare.Read), -1);
                int maxDimension = (int)Math.Max(dds.header.width, dds.header.height);

                CSharpImageLibrary.General.ImageEngineImage image = new CSharpImageLibrary.General.ImageEngineImage(System.AppDomain.CurrentDomain.BaseDirectory + "\\temp.dds", maxDimension, false);
                this.Preview = image.GetWPFBitmap(maxDimension);

                dds = null;
                image.Dispose();
                image = null;
                this.PreviewErrorVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                this.PreviewError = "Could not create preview! Export/Import may still work in certain circumstances." + Environment.NewLine + Environment.NewLine + ex.Message;
                this.PreviewErrorVisibility = Visibility.Visible;
            }
        }
    }
}
