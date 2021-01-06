using System;
using System.IO;
using System.Windows.Controls;
using System.Xml;


namespace barcode_Reader
{
    public class Config
    {
        
        private DateTime _configModification;
        private XmlDocument _xmlConfig;
        public string ButtonColor { get; private set; }
        public string Logo { get; private set; }
        public int Debug { get; private set; }
        private readonly string _userConfig;
        public int Frames { get; set; }
        public int Freeze { get; set; }
        public int Sound { get; set; }
        public int Aim { get; set; }
        public int Timeout { get; set; }
        public int DefaultCam{ get; set; }
        private string documentFolder = "\\Barcode-Reader\\";


        public Config()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            String userConfig = path + "\\Barcode-Reader\\config.xml";
            _userConfig = userConfig;

            if (!Directory.Exists(path + documentFolder))
            {
                Directory.CreateDirectory(path + documentFolder);
            }
            
            if (!File.Exists(userConfig))
            {
                File.Copy("Resources\\template.xml", userConfig, true);
            }
        }

        public void GetDataGridContentData( DataGrid dataGrid)
        {
            var xDoc = ReturnConfig();
            var domainNodes = xDoc.SelectNodes("/Config/Domain");
            if (domainNodes != null)
            {
                string[] domains = new string[domainNodes.Count];
            

                for (int i = 0; i < domainNodes.Count; i++)
                {
                    var xmlAttributeCollection = domainNodes[i].Attributes;
                    if (xmlAttributeCollection != null)
                    {
                        domains[i] = xmlAttributeCollection["Value"].Value;

                        var data = new DatagridItems
                        {
                            Prefix = (xmlAttributeCollection["PrefixValue"].Value),
                            Domain = (xmlAttributeCollection["Value"].Value)
                        };
                        dataGrid.Items.Add(data);
                    }
                }
            }
        }

        public void AddwhitelistConfig(string domain, string prefix)
        {
            
            var xDoc = ReturnConfig();
            var currentdomainNodes = xDoc.SelectNodes("/Config/Domain");
            var domainNodes = xDoc.SelectNodes("/Config");
            
            var duplicate = false;
            if (currentdomainNodes != null)
                for (int i = 0; i < currentdomainNodes.Count; i++)
                {
                    var xmlAttributeCollection = currentdomainNodes[i].Attributes;
                    if (xmlAttributeCollection != null && (xmlAttributeCollection["Value"].Value == domain &&
                                                           (xmlAttributeCollection["PrefixValue"].Value == prefix)))
                    {
                        duplicate = true;
                    }
                }

            if (duplicate == false)
            {

                XmlElement elem = xDoc.CreateElement("Domain");
                elem.SetAttribute("PrefixValue", prefix);
                elem.SetAttribute("Value", domain);

                if (domainNodes != null) domainNodes[0].AppendChild(elem);
                SaveConfig(xDoc);

            }
            
        }

        private XmlDocument ReturnConfig()
        {
            string ConfigFile = _userConfig;
            DateTime tempFileModificaton = File.GetLastWriteTime(@ConfigFile);

            if (DateTime.Compare(_configModification, tempFileModificaton) < 0)
            {
                try
                {

                    _configModification = tempFileModificaton;
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(ConfigFile);
                    _xmlConfig = xDoc;
                    var configFrames = xDoc.SelectNodes("/Config/Base");
                    ButtonColor = configFrames[0].Attributes["Color"].Value;
                    Logo = configFrames[0].Attributes["Logo"].Value;
                    Debug = Convert.ToInt32(configFrames[0].Attributes["Debug"].Value);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
            return _xmlConfig;
        }
        
        private void SaveConfig(XmlDocument xDoc)
        {
            xDoc.Save(_userConfig);

        }


        public void DelwhitelistConfig( DataGrid dataGrid)
        {
            var xDoc = ReturnConfig();

            var domainNodes = xDoc.SelectNodes("/Config/Domain");
            var domainNode = xDoc.SelectNodes("/Config");
            Object currentItem = dataGrid.Items[dataGrid.SelectedIndex];
            

            if (domainNodes != null)
                for (int i = 0; i < domainNodes.Count; i++)
                {
                    var xmlAttributeCollection = domainNodes[i].Attributes;
                    if (xmlAttributeCollection != null && (xmlAttributeCollection["Value"].Value ==
                                                           ((DatagridItems) currentItem).Domain) && (xmlAttributeCollection["PrefixValue"].Value ==
                                                                                                                    ((DatagridItems) currentItem).Prefix))
                    {
                        if (domainNode != null) domainNode[0].RemoveChild(domainNodes[i]);
                    }
                }

            SaveConfig(xDoc);
        }



        public void SetConfigBase(String type, string value )
        {
            var xDoc = ReturnConfig();
            xDoc.SelectNodes("/Config/Base")[0].Attributes[type].Value = value;
            SaveConfig(xDoc);
        }
        public XmlDocument GetConfig()
        {
            return _xmlConfig;
        }
        
        public void GetConfigBase()
        {
            var xDoc = ReturnConfig();
            var configFrames = xDoc.SelectNodes("/Config/Base");
            Frames = Int32.Parse(configFrames[0].Attributes["Frames"].Value);
            Timeout = Int32.Parse(configFrames[0].Attributes["Timeout"].Value);
            Freeze = Int32.Parse(configFrames[0].Attributes["Freeze"].Value);
            Sound = Int32.Parse(configFrames[0].Attributes["Sound"].Value);
            Aim = Int32.Parse(configFrames[0].Attributes["Aim"].Value);
            DefaultCam = Int32.Parse(configFrames[0].Attributes["DefaultCam"].Value);

        }


    }
}