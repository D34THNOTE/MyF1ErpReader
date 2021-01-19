﻿using EgoEngineLibrary.Formats.Pssg;
using EgoEngineLibrary.Graphics;
using EgoPssgEditor.ViewModel;
using Microsoft.Win32;
using SharpGLTF.Schema2;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace EgoPssgEditor.Models3d
{
    public class ModelsWorkspaceViewModel : WorkspaceViewModel
    {
        private PssgFile _pssg;
        PssgNodeViewModel rootNode;
        readonly ObservableCollection<PssgNodeViewModel> pssgNodes;

        public override string DisplayName
        {
            get
            {
                return "Models";
            }
        }
        public PssgNodeViewModel RootNode
        {
            get { return rootNode; }
            private set
            {
                ClearData();
                rootNode = value;
                pssgNodes.Add(rootNode);
            }
        }
        public ObservableCollection<PssgNodeViewModel> PssgNodes
        {
            get { return pssgNodes; }
        }

        public ModelsWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            pssgNodes = new ObservableCollection<PssgNodeViewModel>();

            Export = new RelayCommand(Export_Execute, Export_CanExecute);
            Import = new RelayCommand(Import_Execute, Import_CanExecute);

            ExportDirt = new RelayCommand(ExportDirt_Execute, ExportDirt_CanExecute);
            ImportDirt = new RelayCommand(ImportDirt_Execute, ImportDirt_CanExecute);
            ImportGrid = new RelayCommand(ImportGrid_Execute, ImportGrid_CanExecute);

            ExportCarInterior = new RelayCommand(ExportCarInterior_Execute, ExportCarInterior_CanExecute);
            ImportCarInterior = new RelayCommand(ImportCarInterior_Execute, ImportCarInterior_CanExecute);
        }

        public override void LoadData(object data)
        {
            _pssg = (PssgFile)data;
        }

        public override void ClearData()
        {
            _pssg = null;
            rootNode = null;
            pssgNodes.Clear();
        }

        #region Menu
        public RelayCommand Export { get; }
        public RelayCommand Import { get; }

        public RelayCommand ExportDirt { get; }
        public RelayCommand ImportDirt { get; }
        public RelayCommand ImportGrid { get; }

        public RelayCommand ExportCarInterior { get; }
        public RelayCommand ImportCarInterior { get; }

        private bool Export_CanExecute(object parameter)
        {
            try
            {
                return _pssg != null && CarExteriorPssgGltfConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        private void Export_Execute(object parameter)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Gltf files|*.glb;*.gltf|All files|*.*";
            dialog.Title = "Select the model's save location and file name";
            dialog.DefaultExt = "glb";
            if (!string.IsNullOrEmpty(mainView.FilePath))
            {
                dialog.FileName = Path.GetFileNameWithoutExtension(mainView.FilePath);
                dialog.InitialDirectory = Path.GetDirectoryName(mainView.FilePath);
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var converter = new CarExteriorPssgGltfConverter();
                    var model = converter.Convert(_pssg);
                    model.Save(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool Import_CanExecute(object parameter)
        {
            try
            {
                return _pssg != null && GltfCarExteriorPssgConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        private void Import_Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Gltf files|*.glb;*.gltf|All files|*.*";
            dialog.Title = "Select a gltf model file";
            if (!string.IsNullOrEmpty(mainView.FilePath))
            {
                dialog.FileName = Path.GetFileNameWithoutExtension(mainView.FilePath);
                dialog.InitialDirectory = Path.GetDirectoryName(mainView.FilePath);
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var gltf = ModelRoot.Load(dialog.FileName);

                    var conv = new GltfCarExteriorPssgConverter();
                    conv.Convert(gltf, _pssg);

                    mainView.LoadPssg(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportDirt_CanExecute(object parameter)
        {
            try
            {
                return _pssg != null && DirtCarExteriorPssgGltfConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        private void ExportDirt_Execute(object parameter)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Gltf files|*.glb;*.gltf|All files|*.*";
            dialog.Title = "Select the model's save location and file name";
            dialog.DefaultExt = "glb";
            if (!string.IsNullOrEmpty(mainView.FilePath))
            {
                dialog.FileName = Path.GetFileNameWithoutExtension(mainView.FilePath);
                dialog.InitialDirectory = Path.GetDirectoryName(mainView.FilePath);
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var converter = new DirtCarExteriorPssgGltfConverter();
                    var model = converter.Convert(_pssg);
                    model.Save(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ImportDirt_CanExecute(object parameter)
        {
            try
            {
                return _pssg != null && GltfDirtCarExteriorPssgConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        private void ImportDirt_Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Gltf files|*.glb;*.gltf|All files|*.*";
            dialog.Title = "Select a gltf model file";
            if (!string.IsNullOrEmpty(mainView.FilePath))
            {
                dialog.FileName = Path.GetFileNameWithoutExtension(mainView.FilePath);
                dialog.InitialDirectory = Path.GetDirectoryName(mainView.FilePath);
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var gltf = ModelRoot.Load(dialog.FileName);

                    var conv = new GltfDirtCarExteriorPssgConverter();
                    conv.Convert(gltf, _pssg);

                    mainView.LoadPssg(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ImportGrid_CanExecute(object parameter)
        {
            try
            {
                return _pssg != null && GltfGridCarExteriorPssgConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        private void ImportGrid_Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Gltf files|*.glb;*.gltf|All files|*.*";
            dialog.Title = "Select a gltf model file";
            if (!string.IsNullOrEmpty(mainView.FilePath))
            {
                dialog.FileName = Path.GetFileNameWithoutExtension(mainView.FilePath);
                dialog.InitialDirectory = Path.GetDirectoryName(mainView.FilePath);
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var gltf = ModelRoot.Load(dialog.FileName);

                    var conv = new GltfGridCarExteriorPssgConverter();
                    conv.Convert(gltf, _pssg);

                    mainView.LoadPssg(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportCarInterior_CanExecute(object parameter)
        {
            try
            {
                return _pssg != null && CarInteriorPssgGltfConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        private void ExportCarInterior_Execute(object parameter)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Gltf files|*.glb;*.gltf|All files|*.*";
            dialog.Title = "Select the model's save location and file name";
            dialog.DefaultExt = "glb";
            if (!string.IsNullOrEmpty(mainView.FilePath))
            {
                dialog.FileName = Path.GetFileNameWithoutExtension(mainView.FilePath);
                dialog.InitialDirectory = Path.GetDirectoryName(mainView.FilePath);
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var converter = new CarInteriorPssgGltfConverter();
                    var model = converter.Convert(_pssg);
                    model.Save(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private bool ImportCarInterior_CanExecute(object parameter)
        {
            try
            {
                return _pssg != null && GltfCarInteriorPssgConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        private void ImportCarInterior_Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Gltf files|*.glb;*.gltf|All files|*.*";
            dialog.Title = "Select a gltf model file";
            if (!string.IsNullOrEmpty(mainView.FilePath))
            {
                dialog.FileName = Path.GetFileNameWithoutExtension(mainView.FilePath);
                dialog.InitialDirectory = Path.GetDirectoryName(mainView.FilePath);
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var gltf = ModelRoot.Load(dialog.FileName);

                    var conv = new GltfCarInteriorPssgConverter();
                    conv.Convert(gltf, _pssg);

                    mainView.LoadPssg(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion
    }
}
