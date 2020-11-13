using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QR_Code_Reader
{
    public class Config
    {
        private string _myDomain;

        public string Domain
        {
            get { return _myDomain; }
            set { _myDomain = value; }
        }

        private Boolean _myReadOnly;

        public Boolean ReadOnly
        {
            get { return _myReadOnly; }
            set { _myReadOnly = value; }
        }
        private string _myPrefix;

        public string Prefix
        {
            get { return _myPrefix; }
            set { _myPrefix = value; }
        }
      

    }
}
