using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Util
{
    public static class Output
    {
        public static void WriteError(string text, Exception err)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"\n{ text }\n---> { err }");

            Console.ForegroundColor = old;
        }

        public static void WriteWarning(string text)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"WARNING: { text }");

            Console.ForegroundColor = old;
        }
    }
}
