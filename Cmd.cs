using BlazorState;
using Celin.PO;
using McMaster.Extensions.CommandLineUtils;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestPO
{
    [Subcommand(typeof(LoadCmd))]
    [Subcommand(typeof(LineCmd))]
    [Subcommand(typeof(OpenCmd))]
    [Subcommand(typeof(DefCmd))]
    class Cmd
    {
        [Command("load", Description = "Load Storage")]
        class LoadCmd : BaseCmd
        {
            async Task OnExecuteAsync()
            {
                try
                {
                    await Mediator.Send(new POState.LoadAction());

                    Dump();
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: {0}", e.Message);
                }
            }
            public LoadCmd(IStore store, Mediator mediator, Celin.AIS.Server e1)
                : base(store, mediator, e1) { }
        }
        [Command("line", Description = "Receipt Line")]
        class LineCmd : BaseCmd
        {
            async Task OnExecuteAsync()
            {
                try
                {
                    await Mediator.Send(new POState.LoadAction());

                    Dump();

                    foreach (var o in State.Orders)
                    {
                        Console.Write("{0}, ", o.Number);
                    }
                    Console.WriteLine();

                    var orders =  State.OpenLines
                        .Where(r => r.Adjustment != null)
                        .Select(r => new OrderDef { Company = r.z_KCOO_12, Number = r.z_DOCO_10, Type = r.z_DCTO_11 })
                        .Distinct(new OrderComparer())
                        .ToList();
                    bool addOrder = true;
                    while (addOrder)
                    {
                        var n = Prompt.GetInt("Order:");
                        try
                        {
                            var lines = State.OpenLines
                                .Where(r => r.Adjustment == null)
                                .Where(r => r.z_DOCO_10 == n);
                            if (lines.Count() > 0)
                            {
                                bool addLine = true;
                                foreach (var r in lines)
                                {
                                    Console.WriteLine("{0} {1} {2}-{3} {4}", r.z_LNID_43, r.z_UOPN_16, r.z_DOCO_10, r.z_KCOO_12, r.z_DCTO_11);
                                }
                                while (addLine)
                                {
                                    var strLine = Prompt.GetString("Line:");
                                    var line = decimal.Parse(strLine);
                                    var ol = lines.Single(r => r.z_LNID_43 == line);
                                    if (ol != null)
                                    {
                                        var o = new OrderDef
                                        {
                                            Company = ol.z_KCOO_12,
                                            Number = ol.z_DOCO_10,
                                            Type = ol.z_DCTO_11
                                        };
                                        var strQty = Prompt.GetString("Receipt:");
                                        var qty = decimal.Parse(strQty);
                                        await Mediator.Send(new POState.UpdateAdjustmentAction { Line = ol, Adjustment = qty });
                                        ol.Adjustment = qty;
                                        if (!orders.Contains(o, new OrderComparer()))
                                        {
                                            orders.Add(o);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Order {0} not Found!", n);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR: {0}", e.Message);
                        }
                        foreach (var o in orders)
                        {
                            Console.WriteLine("Order {0} with {1} Lines", o.Number,
                                State.OpenLines.Where(r => o.Equals(r)).Count());
                        }
                        addOrder = Prompt.GetYesNo("More?", false);
                    }

                    await E1.AuthenticateAsync();

                    await Mediator.Send(new POState.ReceiptOrderAction
                    {
                        Orders = orders
                    });

                    Dump();
                }
                catch (Celin.AIS.HttpWebException e)
                {
                    Console.WriteLine("ERROR: {0}", e.ErrorResponse.message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: {0}", e.Message);
                }
            }
            public LineCmd(IStore store, Mediator mediator, Celin.AIS.Server e1)
                : base(store, mediator, e1) { }
        }
        [Command("open", Description = "Get Open Orders")]
        class OpenCmd : BaseCmd
        {
            async Task OnExecuteAsync()
            {
                try
                {
                    await E1.AuthenticateAsync();

                    await Mediator.Send(new POState.OpenRequestAction());

                    Dump();
                }
                catch (Celin.AIS.HttpWebException e)
                {
                    Console.WriteLine("ERROR: {0}", e.ErrorResponse.message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: {0}", e.Message);
                }
            }
            public OpenCmd(IStore store, Mediator mediator, Celin.AIS.Server e1)
                : base(store, mediator, e1) { }
        }
        [Command("def", Description = "Get Form Definition")]
        class DefCmd : BaseCmd
        {
            [Argument(0, Description = "Form Name")]
            string FormName { get; set; }
            async Task OnExecuteAsync()
            {
                try
                {
                    await E1.AuthenticateAsync();

                    await Delay();

                    await Mediator.Send(new POState.DemoRequestAction { FormName = FormName });

                    Dump();
                }
                catch (Celin.AIS.HttpWebException e)
                {
                    Console.WriteLine("ERROR: {0}", e.ErrorResponse.message);
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
