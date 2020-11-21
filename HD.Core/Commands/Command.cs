using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace HD
{
    public abstract class Command
    {
        public string Name { get; set; }

        public abstract string Description { get; }

        protected Command()
        {
            Name = this.GetType().Name;

            if (Name.EndsWith("Command"))
                Name = Name.Remove(Name.Length - "Command".Length);

            RegisterOverloads();
        }

        List<MethodInfo> runMethods;

        void RegisterOverloads()
        {
            foreach (var method in this.GetType().GetMethods())
            {
                if (method.Name == Name)
                {
                    if (runMethods == null)
                        runMethods = new List<MethodInfo>();
                    runMethods.Add(method);
                }
            }
        }

        protected string[] SplitArgs(string args)
        {
            args = args.Trim();
            var argList = new List<string>();

            var stream = new System.IO.StringReader(args);
            StringBuilder arg = new StringBuilder();
            bool inQuotes = false;

            int value = stream.Read();
            while (value > 0)
            {
                var c = (char)value;

                switch (c)
                {
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                        if (inQuotes)
                            goto default;
                        else if (arg.Length > 0)
                        {
                            argList.Add(arg.ToString());
                            arg = new StringBuilder();
                        }
                        break;
                    case '"':
                        inQuotes = !inQuotes;
                        if(!inQuotes && arg.Length > 0)
                        {
                            argList.Add(arg.ToString());
                            arg = new StringBuilder();
                        }
                        break;
                    default:
                        arg.Append(c);
                        break;
                }

                value = stream.Read();
            }

            if (arg.Length > 0)
                argList.Add(arg.ToString());

            return argList.ToArray();
        }

        public abstract string Execute(Player player, string args);

        public virtual bool IsAvailable(Player player)
        {
            return true;
        }
    }
}