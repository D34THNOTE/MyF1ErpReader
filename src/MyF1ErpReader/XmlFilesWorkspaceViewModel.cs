using EgoEngineLibrary.Formats.Erp;
using EgoEngineLibrary.Xml;

namespace EgoErpArchiver.ViewModel
{
    public class XmlFilesWorkspaceViewModel : WorkspaceViewModel
    {
        private readonly List<ErpXmlFileViewModel> xmlFiles;
        public List<ErpXmlFileViewModel> XmlFiles
        {
            get { return xmlFiles; }
        }

        public XmlFilesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            xmlFiles = new List<ErpXmlFileViewModel>();
        }

        public override void LoadData(object data)
        {
            ClearData();
            foreach (var resView in ((ResourcesWorkspaceViewModel)data).Resources)
            {
                var resource = resView.Resource;
                foreach (var fragment in resource.Fragments)
                {
                    try
                    {
                        using var ds = fragment.GetDecompressDataStream(true);
                        if (XmlFile.IsXmlFile(ds))
                        {
                            XmlFiles.Add(new ErpXmlFileViewModel(resView, fragment));
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            DisplayName = "XML Files " + xmlFiles.Count;
        }

        public override void ClearData()
        {
            xmlFiles.Clear();
        }
    }
}
