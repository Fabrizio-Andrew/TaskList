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

        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error number
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error number</returns>
        public static int GetErrorNumberFromDescription(string encodedErrorDescription)
        {
            if (int.TryParse(encodedErrorDescription, out int errorNumber))
            {
                return errorNumber;
            }
            return 0;
        }

        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error response
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error message and number</returns>
        public static (string decodedErrorMessage, int decodedErrorNumber) GetErrorMessage(string encodedErrorDescription)
        {

            int errorNumber = GetErrorNumberFromDescription(encodedErrorDescription);

            switch (errorNumber)
            {
                case 1:
                    {
                        return ("String is to long", errorNumber);
                    }
                case 2:
                    {
                        return ("Invalid email address", errorNumber);
                    }
                case 3:
                    {
                        return ("Invalid range", errorNumber);
                    }
                case 4:
                    {
                        return ("Must be provided", errorNumber);
                    }
                case 5:
                    {
                        return ("Missing name", errorNumber);
                    }
                case 6:
                    {
                        return ("Email must contain first or last name", errorNumber);
                    }
                default:
                    {
                        return ($"Raw Error: {encodedErrorDescription}", errorNumber);
                    }
            }
        }
    }
}
