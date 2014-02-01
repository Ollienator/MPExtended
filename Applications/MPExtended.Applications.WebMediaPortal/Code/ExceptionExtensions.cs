using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public static class ExceptionExtensions
    {
        public static bool Contains(this Exception e, Type[] exceptionTypes)
        {
            bool result = false;

            while (e != null)
            {
                if (exceptionTypes.Contains(e.GetType()))
                {
                    result = true;
                    break;
                }
                e = e.InnerException;
            }

            return result;
        }
    }
}