using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sr25Compare
{
    public  static class   Helper
    {
        public static string StrFormat(int dec)
        {
            string strfmt = null;

            switch (dec)
            {
                case 1:
                    strfmt = "{0,14:F1}";
                    break;
                case 2:
                    strfmt = "{0,14:F2}";
                    break;
                case 3:
                    strfmt = "{0,14:F3}";
                    break;
                default:
                    strfmt = "{0,14:F0}";
                    break;

            }
            return strfmt;
        }
    }
}
