using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kvitto
{
    public interface IKvitto
    {
        Kvitto.ITL tl { get; }
        string toJson();
        void AddTL(string tl, string ordernr, string datum);
        void AddTag(string tagnr, string typ);
        void AddTagInfo(Kvitto.ITagInfo tagInfo);
        void AddTagslag(string tagslag);
        void AddMottagare(string mottagare);
        void AddRunPlan(IRunPlan runPlan);
        void AddAnm(string tagnr, string kmoternr, string tpl, string from, string to, string dag);
        void AddAnm(string tagnr, string kmoternr, string tpl, string from);
    }

    public class Kvitto : IKvitto
    {
        // TL class
        public interface ITL
        {
            string tl { get; set; }
            string ordernr { get; set; }
            string datum { get; set; }
        }

        class TL : ITL
        {
            string _tagPlan;
            string _ordernr;
            string _datum;
            public TL(string tl, string ordernr, string datum)
            {
                _tagPlan = tl;
                _ordernr = ordernr;
                _datum = datum;
            }
            string ITL.tl { get { return _tagPlan; } set { _tagPlan = value; } }
            string ITL.ordernr { get { return _ordernr; } set { _ordernr = value; } }
            string ITL.datum { get { return _datum; } set { _datum = value; } }
        }

        public interface ITag{
            string tagnr {get; set;}
            string typ {get; set;}
            void Add(ITagInfo tagInfo);
            Queue<ITagInfo> tagInfo { get; set; }
        }

        class Tag : ITag
        {
            string _tagnr;
            string _typ;
            Queue<ITagInfo> _tagInfo;
            public Tag(string tagNr, string typ)
            {
                _tagnr = tagNr;
                _typ = typ;
                _tagInfo = new Queue<ITagInfo>();
            }
            string ITag.tagnr { get { return _tagnr; } set { _tagnr = value; } }
            string ITag.typ { get { return _typ; } set { _typ = value; } }
            void ITag.Add(ITagInfo tagInfo)
            {
                _tagInfo.Enqueue(tagInfo);
            }
            Queue<ITagInfo> ITag.tagInfo { get { return _tagInfo; } set { _tagInfo = value; } }
        }

        public interface ITagInfo
        {
            string fromtpl { get; set; }
            string totpl { get; set; }
            string from { get; set; }
            string to { get; set; }
            string gangdagar { get; set; }
        }

        public class Taginfo : ITagInfo
        {
            string _fromtpl;
            string _totpl;
            string _from;
            string _to;
            string _gangdagar;
            public Taginfo()
            {

            }
            public Taginfo(string fromTpl, string toTpl, string fromTime, string toTime, string gangDagar)
            {
                _fromtpl = fromTpl;
                _totpl = toTpl;
                _from = fromTime;
                _to = toTime;
                _gangdagar = gangDagar;
            }
            string ITagInfo.fromtpl { get { return _fromtpl; } set { _fromtpl = value; } }
            string ITagInfo.totpl { get { return _totpl; } set { _totpl = value; } }
            string ITagInfo.from { get { return _from; } set { _from = value; } }
            string ITagInfo.to { get { return _to; } set { _to = value; } }
            string ITagInfo.gangdagar { get { return _gangdagar; } set { _gangdagar = value; } }
        }

        public interface IAnm
        {
            string tagnr { get; set; }
            string kmoternr { get; set; }
            string hpl { get; set; }
            string from { get; set; }
            string to { get; set; }
            string dag { get; set; }
        }

        class Anm : IAnm
        {
            string _tagnr;
            string _kmoternr;
            string _hpl;
            string _from;
            string _to;
            string _dag;
            public Anm(string tagnr, string kmoternr, string hpl, string from, string to, string dag)
            {
                _tagnr = tagnr;
                _kmoternr = kmoternr;
                _hpl = hpl;
                _from = from;
                _to = to;
                _dag = dag;
            }
            public Anm(string tagnr, string kmoternr, string hpl, string from)
            {
                _tagnr = tagnr;
                _kmoternr = kmoternr;
                _hpl = hpl;
                _from = from;
                _to = null;
                _dag = null;
            }
            string IAnm.tagnr { get { return _tagnr; } set { _tagnr = value; } }
            string IAnm.kmoternr { get { return _kmoternr; } set { _kmoternr = value; } }
            string IAnm.hpl { get { return _hpl; } set { _hpl = value; } }
            string IAnm.from { get { return _from; } set { _from = value; } }
            string IAnm.to { get { return _to; } set { _to = value; } }
            string IAnm.dag { get { return _dag; } set { _dag = value; } }
        }
        // Start of Kvitto members
        ITL _tl;
        ITag _tag;
        string _tagslag;
        Queue<string> _mottagare;
        IRunPlan _runPlan;
        Queue<IAnm> _anm;

        KvittoObj _kvitto;

        public Kvitto()
        {
            _mottagare = new Queue<string>();
            _anm = new Queue<IAnm>();
        }

        ITL IKvitto.tl { get { return _tl; } }

        string IKvitto.toJson()
        {
            _kvitto = new KvittoObj(_mottagare.Count, _anm.Count);
            _kvitto.tl = new KvittoObj.TL();
            _kvitto.tl.tagPlan = this._tl.tl;
            _kvitto.tl.ordernr = this._tl.ordernr;
            _kvitto.tl.datum = this._tl.datum;
            _kvitto.mottagare = _mottagare.ToArray();
            _kvitto.tag = new KvittoObj.Tag();
            _kvitto.tag.tagnr = this._tag.tagnr;
            _kvitto.tag.typ = this._tag.typ;
            _kvitto.tag.tagInfo = new KvittoObj.Tag.TagInfo[this._tag.tagInfo.Count];
            Kvitto.ITagInfo[] tagInfos = this._tag.tagInfo.ToArray();
            for (int i = 0; i < tagInfos.Length; i++)
            {
                _kvitto.tag.tagInfo[i] = new KvittoObj.Tag.TagInfo();
                _kvitto.tag.tagInfo[i].fromTpl = tagInfos[i].fromtpl;
                _kvitto.tag.tagInfo[i].toTpl = tagInfos[i].totpl;
                _kvitto.tag.tagInfo[i].fromTime = tagInfos[i].from;
                _kvitto.tag.tagInfo[i].toTime = tagInfos[i].to;
                _kvitto.tag.tagInfo[i].gangDagar = tagInfos[i].gangdagar;
            }
            _kvitto.tagSlag = this._tagslag;
            if (_runPlan != null)
            {
                RunPlan.IRun[] runs = _runPlan.GetArray();
                _kvitto.korPlan = new KvittoObj.Korplan(runs.Length);
                for (int i = 0; i < runs.Length; i++)
                {
                    string[] tider = runs[i].getTimes();
                    string[] infos = runs[i].getInfos();
                    _kvitto.korPlan.tplInfo[i] = new KvittoObj.TplInfo(tider.Length, infos.Length);
                    _kvitto.korPlan.tplInfo[i].tpl = runs[i].hpl;
                    _kvitto.korPlan.tplInfo[i].tid = tider;
                    _kvitto.korPlan.tplInfo[i].info = infos;
                }
            }
            _kvitto.anm = new KvittoObj.Anm[_anm.Count];
            Kvitto.IAnm[] anm = _anm.ToArray();
            for (int i = 0; i < _anm.Count; i++)
            {
                _kvitto.anm[i] = new KvittoObj.Anm();
                _kvitto.anm[i].tagnr = anm[i].tagnr;
                _kvitto.anm[i].kmoternr = anm[i].kmoternr;
                _kvitto.anm[i].hpl = anm[i].hpl;
                _kvitto.anm[i].from = anm[i].from;
                _kvitto.anm[i].to = anm[i].to;
                _kvitto.anm[i].gangDagar = anm[i].dag;
            }
            string json = JsonConvert.SerializeObject(_kvitto);
            return json;
        }

        void IKvitto.AddTL(string tl, string ordernr, string datum)
        {
            _tl = new TL(tl, ordernr, datum);
        }

        void IKvitto.AddTag(string tagnr, string typ)
        {
            _tag = new Tag(tagnr, typ);
        }

        void IKvitto.AddTagInfo(Kvitto.ITagInfo tagInfo)
        {
            _tag.Add(tagInfo);
        }

        void IKvitto.AddTagslag(string tagslag)
        {
            _tagslag = tagslag;
        }

        void IKvitto.AddMottagare(string mottagare)
        {
            _mottagare.Enqueue(mottagare);
        }

        void IKvitto.AddRunPlan(IRunPlan runPlan)
        {
            _runPlan = runPlan;
        }

        void IKvitto.AddAnm(string tagnr, string kmoternr, string hpl, string from, string to, string dag)
        {
            _anm.Enqueue(new Anm(tagnr, kmoternr, hpl, from, to, dag));
        }

        void IKvitto.AddAnm(string tagnr, string kmoternr, string hpl, string from)
        {
            _anm.Enqueue(new Anm(tagnr, kmoternr, hpl, from));
        }
    }
}