using System;
using System.Collections.Generic;
using System.Text;

namespace Shopping.Business.Exceptions
{
    public class InadequateProductException : Exception
    {
        public InadequateProductException(int quantityMissing): base($"You have requested {quantityMissing} more products than we have quantity for")
        {

        }
    }
}
