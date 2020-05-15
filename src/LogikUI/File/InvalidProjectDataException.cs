using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.File
{
    public class InvalidProjectDataException : Exception
    {
        public InvalidProjectDataException(string message) : base(message) { }
    }
}
