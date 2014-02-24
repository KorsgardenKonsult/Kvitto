using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvitto
{
    public class KvittoObj
    {
        public class TL
        {
            public string tagPlan { get; set; }
            public string ordernr { get; set; }
            public string datum { get; set; }
        }

        public class Tag
        {
            public class TagInfo
            {
                public string fromTpl { get; set; }
                public string toTpl { get; set; }
                public string fromTime { get; set; }
                public string toTime { get; set; }
                public string gangDagar { get; set; }
            }

            public string tagnr { get; set; }
            public string typ { get; set; }
            public TagInfo[] tagInfo { get; set; }
        }



        public class TplInfo
        {
            public string tpl { get; set; }
            public string[] tid { get; set; }
            public string[] info { get; set; }
            public TplInfo(int sizeTid, int sizeInfo)
            {
                tid = new string[sizeTid];
                info = new string[sizeInfo];
            }
        }

        public class Korplan
        {
            public TplInfo[] tplInfo { get; set; }
            public Korplan(int size)
            {
                tplInfo = new TplInfo[size];
            }
        }

        public class Anm
        {
            public string tagnr { get; set; }
            public string kmoternr { get; set; }
            public string hpl { get; set; }
            public string from { get; set; }
            public string to { get; set; }
            public string gangDagar { get; set; }
        }

        public KvittoObj(int sizeMot, int sizeAnm)
        {
            mottagare = new string[sizeMot];
            anm = new Anm[sizeAnm];
        }
        public TL tl { get; set; }
        public string[] mottagare { get; set; }
        public Tag tag { get; set; }
        public string tagSlag { get; set; }
        public Korplan korPlan { get; set; }
        public Anm[] anm { get; set; }
        //gruck
    }
}
