using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskList.DataTransferObjects
{
    public class ErrorResponse
    {
        public int errorNumber { get; set; }
        public string parameterName { get; set; }
        public string parameterValue { get; set; }
        public string errorDescription { get; set; }

        // TO-DO: Add GetErrorNumberFromDescription method from Customers example
    }
}
