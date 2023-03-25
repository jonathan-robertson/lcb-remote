using System;
using System.Collections.Generic;
using System.Linq;

namespace LcbRemote
{
    internal class ConsoleCmdLcbRemote : ConsoleCmdAbstract
    {
        private static readonly string[] Commands = new string[] {
            "lcbremote",
            "lcbr"
        };
        private readonly string help;

        public ConsoleCmdLcbRemote()
        {
            var dict = new Dictionary<string, string>() {
                { "debug", "toggle debug logging mode" },
                { "check", "check the current activation state of the lcb you are within range of" },
                { "activate", "activate lcb area frame for the lcb you are within range of (only the lcb owner will see it)" },
                { "deactivate", "deactivate lcb area frame for the lcb you are within range of" },
            };

            var i = 1; var j = 1;
            help = $"Usage:\n  {string.Join("\n  ", dict.Keys.Select(command => $"{i++}. {GetCommands()[0]} {command}").ToList())}\nDescription Overview\n{string.Join("\n", dict.Values.Select(description => $"{j++}. {description}").ToList())}";
        }

        public override string[] GetCommands()
        {
            return Commands;
        }

        public override string GetDescription()
        {
            return "Configure or adjust settings for the LCB Remote mod.";
        }

        public override string GetHelp()
        {
            return help;
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            SdtdConsole.Instance.Output("Not yet implemented.");
            throw new NotImplementedException();
        }
    }
}
