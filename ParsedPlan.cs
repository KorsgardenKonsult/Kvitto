using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvitto
{
    public interface IParsedPlan
    {
        ParsedPlan.IItem[] GetArray();
        void Add(string type, string value);
    }

    public class ParsedPlan : IParsedPlan
    {
        public interface IItem
        {
            string type { get; set; }
            string value { get; set; }
        }

        private class Item : IItem
        {
            private string _type;
            private string _value;
            public Item(string type, string value)
            {
                _type = type;
                _value = value;
            }
            public string type { get { return _type; } set { _type = value; } }
            public string value { get { return _value; } set { _value = value; } }
        }

        Queue<Item> _things;

        public ParsedPlan()
        {
            _things = new Queue<Item>();            
        }

        void IParsedPlan.Add(string type, string value)
        {
            Item item = new Item(type, value);
            _things.Enqueue(item);
        }

        IItem[] IParsedPlan.GetArray()
        {
            // Correct infos who wrongfully parsed to hpls, hpl must be followed by a tid
            for (int i = 0; i < _things.Count; i++)
            {
                try
                {
                    if (_things.ElementAt(i).type == "hpl" && _things.ElementAt(i + 1).type != "tid")
                    {
                        _things.ElementAt(i).type = "info";
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    _things.ElementAt(i).type = "info";
                }
            }

            return _things.ToArray();
        }
    }
}
