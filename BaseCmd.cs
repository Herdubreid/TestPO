using BlazorState;
using McMaster.Extensions.CommandLineUtils;
using MediatR;

namespace TestPO
{
    abstract class BaseCmd
    {
        protected Celin.AIS.Server E1 { get; }
        protected Mediator Mediator { get; }
        protected IStore Store { get; }
        [Option("-j|--json", CommandOptionType.SingleValue, Description = "Output result to Json File")]
        protected (bool HasValue, string fname) JsonFile { get; set; }
        public BaseCmd(IStore store, Mediator mediator, Celin.AIS.Server e1)
        {
            Store = store;
            Mediator = mediator;
            E1 = e1;
        }
    }
}
