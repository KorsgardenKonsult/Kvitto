using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;


namespace Kvitto
{
    class Program
    {
        enum rxTypes
        {
            undefined,
            tl,
            mottagare,
            tag1,
            tag2,
            tagslag,
            korplan,
            anm
        };

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Kvitto <katalog> <json-file>");
                System.Environment.Exit(0);
            }

            int rxState = (int) rxTypes.undefined;
            string jsonKvitto = "{\"tagPlaner\": [";
                       
            foreach (string filename in System.IO.Directory.EnumerateFiles(args[0]))
            {
                int numDiv = 0;
                IParsedPlan pPlan = new ParsedPlan();
                IRunPlan rPlan = null;
                IKvitto kvitto = new Kvitto();
                


                Console.WriteLine("Filnamn: " + filename);
                Console.WriteLine();
                StreamReader sr = new StreamReader(filename);
                string line;
                Regex rxTL = new Regex(@"TL -- (?<tl>[0-9a-zA-ZåäöÅÄÖ:\. ]+) -- Trafikverket - NR (?<ordernr>[0-9]+) -- (?<datum>[0-9a-zA-ZåäöÅÄÖ:\. ]+) --");
                Regex rxMottagare = new Regex(@"^(?<mottagare>MOTTAGARE:)");
                Regex rxTag = new Regex(@"^TÅG (?<tagnr>[0-9]+) (?<typ>(Ska Gå|SKA INSTÄLLAS))");
                Regex rxTagslag = new Regex(@"^TÅGSLAG (?<tagslag>[A-Z]+)");
                Regex rxKorplan = new Regex(@"^(?<korplan>Körplan)");
                Regex rxEnd = new Regex(@"^(?<end>ANM:)");
                while ((line = sr.ReadLine()) != null)
                {
                    MatchCollection mc = rxTL.Matches(line);
                    if (mc.Count > 0)
                    {
                        Console.WriteLine(line);
                        rxState = (int)rxTypes.tl;
                    }

                    mc = rxMottagare.Matches(line);
                    if (mc.Count > 0)
                    {
                        Console.WriteLine(line);
                        rxState = (int)rxTypes.mottagare;
                    }
                    mc = rxTag.Matches(line);
                    if (mc.Count > 0)
                    {
                        Console.WriteLine(line);
                        rxState = (int)rxTypes.tag1;
                    }
                    mc = rxTagslag.Matches(line);
                    if (mc.Count > 0)
                    {
                        Console.WriteLine(line);
                        rxState = (int)rxTypes.tagslag;
                    }
                    mc = rxKorplan.Matches(line);
                    if (mc.Count > 0)
                    {
                        Console.WriteLine(line);
                        rxState = (int)rxTypes.korplan;
                    }
                    mc = rxEnd.Matches(line);
                    if (mc.Count > 0)
                    {
                        Console.WriteLine(line);
                        rxState = (int)rxTypes.anm;
                    }
                    switch (rxState)
                    {
                        case (int)rxTypes.tl:
                            MatchCollection tlMc = rxTL.Matches(line);
                            if (tlMc.Count > 0)
                            {
                                string tl = tlMc[0].Groups["tl"].Value;
                                string ordernr = tlMc[0].Groups["ordernr"].Value;
                                string datum = tlMc[0].Groups["datum"].Value;
                                kvitto.AddTL(tl, ordernr, datum);
                            }
                            break;
                        case (int)rxTypes.mottagare:
                            Regex rxMottagarLista = new Regex(@"(?<mottagare>[0-9a-zA-ZåäöÅÄÖ ]+)[,]*");
                            MatchCollection mMc = rxMottagarLista.Matches(line);
                            if (mMc.Count > 0)
                            {
                                foreach (Match m in mMc)
                                {
                                    if (!m.Groups["mottagare"].Value.Trim().Equals("MOTTAGARE"))
                                    {
                                        kvitto.AddMottagare(m.Groups["mottagare"].Value.Trim());
                                        Console.Write(m.Groups["mottagare"].Value + "; ");
                                    }
                                }
                            }
                            Console.WriteLine();
                            break;
                        case (int)rxTypes.tag1:
                            MatchCollection tagMc = rxTag.Matches(line);
                            if (tagMc.Count > 0)
                            {                                
                                string tagnr = tagMc[0].Groups["tagnr"].Value.Trim();
                                string typ = tagMc[0].Groups["typ"].Value.Trim();
                                kvitto.AddTag(tagnr, typ);
                                rxState = (int)rxTypes.tag2;
                            }
                            Console.WriteLine(line);
                            break;
                        case (int)rxTypes.tag2:
                            Regex rxTagInfo = new Regex(@"(?<fromtpl>[a-zA-ZåäöÅÄÖ ]+)\-(?<totpl>[a-zA-ZåäöÅÄÖ ]+)[  ]+(?<from>[0-9]+)\-(?<to>[0-9]+): (?<gangdagar>[A-Z\-]+)");
                            MatchCollection tInfoMc = rxTagInfo.Matches(line);
                            if (tInfoMc.Count > 0)
                            {
                                Kvitto.ITagInfo tagInfo = new Kvitto.Taginfo();
                                tagInfo.fromtpl = tInfoMc[0].Groups["fromtpl"].Value.Trim();
                                tagInfo.totpl = tInfoMc[0].Groups["totpl"].Value.Trim();
                                tagInfo.from = tInfoMc[0].Groups["from"].Value.Trim();
                                tagInfo.to = tInfoMc[0].Groups["to"].Value.Trim();
                                tagInfo.gangdagar = tInfoMc[0].Groups["gangdagar"].Value.Trim();
                                kvitto.AddTagInfo(tagInfo);
                            }
                            break;
                        case (int)rxTypes.tagslag:
                            MatchCollection tagslagMc = rxTagslag.Matches(line);
                            if (tagslagMc.Count > 0)
                            {
                                kvitto.AddTagslag(tagslagMc[0].Groups["tagslag"].Value.Trim());
                            }
                            Console.WriteLine(line);
                            break;
                        case (int)rxTypes.korplan:
                            Regex rxDiv = new Regex(@"(?<div>[\-]{4})");
                            Regex rxAct = new Regex(@"(Traf.utb|Obevakad|Tkl ger körtillstånd|Går från [a-zA-ZåäöÅÄÖ]+|K-Möter [0-9]+|K-Möter Ej [0-9]+|\([0-9]+\-[0-9]+:|På\/Av|Fjärrbevakad)");
                            Regex rxHpl = new Regex(@"(?<hpl>[a-zA-ZåäöÅÄÖ]+)");
                            Regex rxTid = new Regex(@"^(?<tid>[0-9]{4})");
                            MatchCollection kpMc = rxDiv.Matches(line);
                            if (kpMc.Count > 0) numDiv++;

                            if (numDiv == 2)
                            {
                                bool act = false;
                                kpMc = rxAct.Matches(line);
                                if (kpMc.Count > 0)
                                {
                                    pPlan.Add("info", line);
                                    act = true;
                                }
                                kpMc = rxHpl.Matches(line);
                                if (kpMc.Count > 0 && !act)
                                {
                                    pPlan.Add("hpl", line);
                                }
                                kpMc = rxTid.Matches(line);
                                if (kpMc.Count > 0)
                                {
                                    pPlan.Add("tid", line);
                                }
                            }
                            else if (numDiv == 3)
                            {
                                rPlan = new RunPlan(pPlan);
                                kvitto.AddRunPlan(rPlan);
                                RunPlan.IRun[] runs = rPlan.GetArray();
                                foreach (RunPlan.IRun run in runs)
                                {
                                    Console.Write("Hpl: " + run.hpl + ", ");
                                    foreach (string tid in run.getTimes())
                                        Console.Write(tid + ", ");
                                    foreach (string info in run.getInfos())
                                    {
                                        Console.Write(info + ", ");

                                        Regex rxKMoter = new Regex(@"K-Möter (?<kmoter>[0-9]+)");
                                        MatchCollection kmMc = rxKMoter.Matches(info);
                                        if (kmMc.Count > 0)
                                        {
                                            Console.Write("K Möter tågnr: " + kmMc[0].Groups["kmoter"].Value + ", ");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Omatchat: " + info);
                                        }
                                    }
                                    Console.WriteLine();
                                }
                            }
                            else if (numDiv > 3)
                            {
                                Console.WriteLine("------------------------------------------------------------------------------");
                            }
                            break;
                        case (int)rxTypes.anm:
                            Regex rxANM = new Regex(@"TÅG[ ]+(?<tagnr>[0-9]+)[ ]+(K\-Möter|K\-Möter Även)[ ]+(?<kmoternr>[0-9]+)[ ]+i[ ]+(?<hpl>[a-zA-ZåäöÅÄÖ]+)[ ]+(?<from>[0-9]+)(\-(?<to>[0-9]+)\:[ ]+(?<dag>[A-ZÅÄÖ]+)){0,1}");
                            MatchCollection anmMc = rxANM.Matches(line);
                            if (anmMc.Count > 0)
                            {
                                if (anmMc[0].Groups["to"].Value != null)
                                    kvitto.AddAnm(anmMc[0].Groups["tagnr"].Value, anmMc[0].Groups["kmoternr"].Value, anmMc[0].Groups["hpl"].Value,
                                        anmMc[0].Groups["from"].Value, anmMc[0].Groups["to"].Value, anmMc[0].Groups["dag"].Value);
                                else
                                    kvitto.AddAnm(anmMc[0].Groups["tagnr"].Value, anmMc[0].Groups["kmoternr"].Value, anmMc[0].Groups["hpl"].Value,
                                        anmMc[0].Groups["from"].Value);
                                Console.WriteLine(line);
                            }
                            //end_state = false;
                            break;
                        default:
                            break;
                    }
                }
                
                jsonKvitto += kvitto.toJson() + ",";
                Console.WriteLine(kvitto.toJson());
                Console.WriteLine();
                Console.WriteLine("##############################################################################################");
                Console.WriteLine();
            }
            jsonKvitto = jsonKvitto.TrimEnd(new char[] {','});
            jsonKvitto += "]}";
            System.IO.StreamWriter sw = new StreamWriter(args[1]);
            sw.Write(jsonKvitto);
            sw.Close();
        }
    }
}
