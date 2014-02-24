using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvitto
{
    public interface IRunPlan
    {
        RunPlan.IRun[] GetArray();
    }

    public class RunPlan : IRunPlan
    {
        public interface IRun
        {
            void Add(string type, string Value);
            string hpl {get; set;}
            string tid { get; set; }
            string info { get; set; }
            string[] getTimes();
            string[] getInfos();
        }

        private class Run : IRun
        {
            private string _hpl;
            private Queue<string> _tid;
            private Queue<string> _info;
            public Run()
            {
                _tid = new Queue<string>();
                _info = new Queue<string>();
            }
            void IRun.Add(string type, string value)
            {
                switch (type)
                {
                    case "hpl":
                        _hpl = value;
                        break;
                    case "tid":
                        _tid.Enqueue(value);
                        break;
                    case "info":
                        _info.Enqueue(value);
                        break;
                    default:
                        break;
                }
            }
            string IRun.hpl { get { return _hpl; } set { _hpl = value; } }
            string IRun.tid { get { return _tid.Dequeue(); } set { _tid.Enqueue(value); } }
            string IRun.info { get { return _info.Dequeue(); } set { _info.Enqueue(value); } }
            string[] IRun.getTimes() { return _tid.ToArray(); }
            string[] IRun.getInfos() { return _info.ToArray(); }
        }

        private Queue<IRun> _plans;
        private IRun _currentRun;

        public RunPlan()
        {
            _plans = new Queue<IRun>();
        }

        public RunPlan(IParsedPlan plan)
        {
            _plans = new Queue<IRun>();
            bool first = true;
            ParsedPlan.IItem[] items = plan.GetArray();
            foreach (ParsedPlan.IItem item in items)
            {
                if (item.type == "hpl")
                    if (first)
                    {
                        NewRun();
                        first = false;
                    }
                    else
                    {
                        Done();
                        NewRun();
                    }
                Add(item.type, item.value);
                if (item.Equals(items[items.Length - 1]))
                {
                    Done();
                    NewRun();
                }
            }
            //Done();
        }

        void NewRun()
        {
            _currentRun = new Run();
        }

        bool Add(string type, string value)
        {
            _currentRun.Add(type, value);
            return true;
        }

        void Done()
        {
            _plans.Enqueue(_currentRun);
        }

        RunPlan.IRun[] IRunPlan.GetArray()
        {
            return _plans.ToArray();
        }
    }

}
