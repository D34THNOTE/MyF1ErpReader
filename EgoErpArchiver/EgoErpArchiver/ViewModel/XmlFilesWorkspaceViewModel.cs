﻿using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Graphics;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace EgoErpArchiver.ViewModel
{
    public class XmlFilesWorkspaceViewModel : WorkspaceViewModel
    {
        #region Data
        readonly ObservableCollection<ErpXmlFileViewModel> xmlFiles;

        public override string DisplayName
        {
            get { return "Xml Files"; }
        }

        public ObservableCollection<ErpXmlFileViewModel> XmlFiles
        {
            get { return xmlFiles; }
        }
        #endregion

        #region Presentation Data
        readonly CollectionView xmlFilesViewSource;
        string filterText;

        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;
                xmlFilesViewSource.Refresh();
            }
        }
        #endregion

        public XmlFilesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            xmlFiles = new ObservableCollection<ErpXmlFileViewModel>();
            xmlFilesViewSource = (CollectionView)CollectionViewSource.GetDefaultView(XmlFiles);
            xmlFilesViewSource.Filter += XmlFilesFilter;

            export = new RelayCommand(Export_Execute, Export_CanExecute);
            import = new RelayCommand(Import_Execute, Import_CanExecute);
            exportTextures = new RelayCommand(ExportTextures_Execute, ExportTextures_CanExecute);
            importTextures = new RelayCommand(ImportTextures_Execute, ImportTextures_CanExecute);
        }

        public override void LoadData(object data)
        {
            ClearData();
            foreach (var resView in ((ResourcesWorkspaceViewModel)data).Resources)
            {
                switch (resView.Resource.ResourceType)
                {
                    case "AISplineData":
                    case "AIBrakeSettings":
                    case "FFBD":
                    case "PracticeTA":
                    case "Spline":
                    case "TM":
                    case "TS":
                    case "VTF":
                        XmlFiles.Add(new ErpXmlFileViewModel((ErpResourceViewModel)resView));
                        break;
                }
            }
        }

        public override void ClearData()
        {
            xmlFiles.Clear();
        }

        private bool XmlFilesFilter(object item)
        {
            if (String.IsNullOrEmpty(FilterText))
                return true;
            else
                return ((item as ErpXmlFileViewModel).DisplayName.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        #region Menu
        readonly RelayCommand export;
        readonly RelayCommand import;
        readonly RelayCommand exportTextures;
        readonly RelayCommand importTextures;

        public RelayCommand Export
        {
            get { return export; }
        }
        public RelayCommand Import
        {
            get { return import; }
        }
        public RelayCommand ExportTextures
        {
            get { return exportTextures; }
        }
        public RelayCommand ImportTextures
        {
            get { return importTextures; }
        }

        private bool Export_CanExecute(object parameter)
        {
            return parameter != null;
        }
        private void Export_Execute(object parameter)
        {
            ErpTextureViewModel texView = (ErpTextureViewModel)parameter;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Dds files|*.dds|All files|*.*";
            dialog.Title = "Select the dds save location and file name";
            dialog.FileName = texView.DisplayName + ".dds";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    texView.ExportDDS(dialog.FileName, false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool Import_CanExecute(object parameter)
        {
            return parameter != null;
        }
        private void Import_Execute(object parameter)
        {
            ErpTextureViewModel texView = (ErpTextureViewModel)parameter;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Dds files|*.dds|All files|*.*";
            dialog.Title = "Select a dds file";
            dialog.FileName = texView.DisplayName + ".dds";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    texView.ImportDDS(dialog.FileName, null);
                    texView.GetPreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportTextures_CanExecute(object parameter)
        {
            return XmlFiles.Count > 0;
        }
        private void ExportTextures_Execute(object parameter)
        {
            try
            {
                Directory.CreateDirectory(mainView.FilePath.Replace(".", "_") + "_textures");
                foreach (ErpXmlFileViewModel texView in XmlFiles)
                {
                    string fileName = mainView.FilePath.Replace(".", "_") + "_textures" + "\\" + Path.Combine(texView.XmlFile.Folder, texView.XmlFile.FileName).Replace("?", "%3F") + ".dds";
                    string directoryPath = Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
                    //texView.ExportDDS(fileName, true);
                }
                MessageBox.Show("Textures exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("There was an error, could not export all textures!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool ImportTextures_CanExecute(object parameter)
        {
            return XmlFiles.Count > 0;
        }
        private void ImportTextures_Execute(object parameter)
        {
            try
            {
                string directory = mainView.FilePath.Replace(".", "_") + "_textures";
                string mipMapDirectory = mainView.FilePath.Replace(".", "_") + "_mipmaps";
                if (Directory.Exists(directory) == true)
                {
                    foreach (string filePath in Directory.GetFiles(directory, "*.dds", SearchOption.AllDirectories))
                    {
                        foreach (ErpXmlFileViewModel texView in XmlFiles)
                        {
                            string fileName = mainView.FilePath.Replace(".", "_") + "_textures" + "\\" + Path.Combine(texView.XmlFile.Folder, texView.XmlFile.FileName).Replace("?", "%3F") + ".dds";
                            if (Path.Equals(filePath, fileName))
                            {
                                string mipMapSaveLocation = filePath.Replace(directory, mipMapDirectory) + ".mipmaps";
                                Directory.CreateDirectory(Path.GetDirectoryName(mipMapSaveLocation));
                                //texView.ImportDDS(filePath, mipMapSaveLocation);
                                if (texView.IsSelected)
                                {
                                    texView.GetPreview();
                                }
                                break;
                            }
                        }
                    }

                    MessageBox.Show("Textures imported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Could not find textures folder!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("There was an error, could not import all textures!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
