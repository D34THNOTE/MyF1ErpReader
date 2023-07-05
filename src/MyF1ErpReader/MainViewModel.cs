using EgoEngineLibrary.Archive.Erp;

namespace EgoErpArchiver.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        
        string filePath;
        ErpFile file;
        readonly ResourcesWorkspaceViewModel resourcesWorkspace;
        readonly XmlFilesWorkspaceViewModel xmlFilesWorkspace;


        public string FilePath
        {
            get { return filePath; }
        }

        public ErpFile ErpFile
        {
            get { return file; }
        }
        public ResourcesWorkspaceViewModel ResourcesWorkspace
        {
            get { return resourcesWorkspace; }
        }

        public XmlFilesWorkspaceViewModel XmlFilesWorkspace
        {
            get { return xmlFilesWorkspace; }
        }

        public MainViewModel()
        {
            resourcesWorkspace = new ResourcesWorkspaceViewModel(this);
            xmlFilesWorkspace = new XmlFilesWorkspaceViewModel(this);
        }
        
        
        private void Open(string fileName)
        {
            try
            {
                filePath = fileName;

                this.file = new ErpFile();
                Task.Run(() => this.file.Read(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))).Wait();
                resourcesWorkspace.LoadData(file);
                xmlFilesWorkspace.LoadData(resourcesWorkspace);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}