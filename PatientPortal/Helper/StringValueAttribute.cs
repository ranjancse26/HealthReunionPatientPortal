using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace PatientPortal.Helper
{
    public class StringValueAttribute : System.Attribute
    {
        private string _value;

        public StringValueAttribute(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the string values associated with the enum.
        /// </summary>
        /// <returns>IEnumerable of strings</returns>
        public static IEnumerable<string> GetStringValues(Type enumType)
        {
            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in enumType.GetFields())
            {
                //Check for our custom attribute
                var stringValueAttributes = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (stringValueAttributes.Length > 0)
                {
                    yield return stringValueAttributes[0].Value;
                }
            }
        }
    }    
}
