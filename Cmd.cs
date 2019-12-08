using BlazorState;
using Celin.PO;
using McMaster.Extensions.CommandLineUtils;
using MediatR;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestPO
{
    [Subcommand(typeof(OpenCmd))]
    [Subcommand(typeof(DefCmd))]
    class Cmd
    {
        [Command("open", Description = "Get Open Orders")]
        class OpenCmd : BaseCmd
        {
            POState State => Store.GetState<POState>();
            async Task OnExecuteAsync()
            {
                try
                {
                    await E1.AuthenticateAsync();

                    await Mediator.Send(new POState.OpenRequestAction());
                    if (JsonFile.HasValue)
                    {
                        var fs = File.CreateText(JsonFile.fname);
                        var json = JsonSerializer.Serialize(State.OpenRequest, new JsonSerializerOptions { WriteIndented = true });
                        fs.Write(json);
                        fs.Close();
                    }
                    else
                    {
                        var summary = State.OpenRequest.fs_P4312_W4312F.data.gridData.summary;
                        Console.WriteLine("{0}{1} Open Orders", summary.moreRecords ? "More Than " : string.Empty, summary.records);
                        if (summary.records > 0)
                        {
                            var rows = State.OpenRequest.fs_P4312_W4312F.data.gridData.rowset;
                            var orders = rows.Select(r => new Tuple<int, string>(r.z_DOCO_10, r.z_DCTO_11))
                                .Distinct();
                            foreach (var o in orders)
                            {
                                Console.WriteLine("{0}\t{1}", o.Item1, o.Item2);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: {0}", e.Message);
                }
            }
            public OpenCmd(IStore store, Mediator mediator, Celin.AIS.Server e1)
                : base(store, mediator, e1) { }
        }
        [Command("def", Description = "Get W4312F Definition")]
        class DefCmd : BaseCmd
        {
            POState State => Store.GetState<POState>();
            async Task OnExecuteAsync()
            {
                try
                {
                    await E1.AuthenticateAsync();

                    await Mediator.Send(new POState.DemoRequestAction());

                    if (JsonFile.HasValue)
                    {
                        var fs = File.CreateText(JsonFile.fname);
                        var json = JsonSerializer.Serialize(State.DemoRequest, new JsonSerializerOptions { WriteIndented = true });
                        fs.Write(json);
                        fs.Close();
                    }
                    else
                    {
                        Regex pat = new Regex(@"^z_([A-Z|\d]+)_(\d+)?");
                        foreach (var col in State.DemoRequest.fs_P4312_W4312F.data.gridData.columns)
                        {
                            var m = pat.Match(col.Key);
                            string alias = m.Groups[1].Value;
                            string number = m.Groups[2].Value;
                            Console.WriteLine("{0}\t{1}\t{2}", number, alias, col.Value);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: {0}", e.Message);
                }
            }
            public DefCmd(IStore store, Mediator mediator, Celin.AIS.Server e1)
                : base(store, mediator, e1) { }
        }
    }
}
