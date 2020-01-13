using System;
using System.Reflection;
using System.Text;

namespace Ryujinx.Common.Logging
{
    internal class DefaultLogFormatter : ILogFormatter
    {
        private static readonly ObjectPool<StringBuilder> _stringBuilderPool = SharedPools.Default<StringBuilder>();

        public string Format(LogEventArgs args)
        {
            StringBuilder sb = _stringBuilderPool.Allocate();

            try
            {
                sb.Clear();

                sb.AppendFormat(@"{0:hh\:mm\:ss\.fff}", args.Time);
                sb.Append(" | ");

                if (args.ThreadName != null)
                {
                    sb.Append(args.ThreadName);
                    sb.Append(' ');
                }

                sb.Append(args.Message);

                if (args.Data != null)
                {
                    PropertyInfo[] props = args.Data.GetType().GetProperties();

                    sb.Append(' ');

                    foreach (var prop in props)
                    {
                        sb.Append(prop.Name);
                        sb.Append(": ");

                        if (typeof(Array).IsAssignableFrom(prop.PropertyType))
                        {
                            Array enumerable = (Array)prop.GetValue(args.Data);
                            foreach (var item in enumerable)
                            {
                                sb.Append(item.ToString());
                                sb.Append(", ");
                            }

                            sb.Remove(sb.Length - 2, 2);
                        }
                        else
                        {
                            sb.Append(prop.GetValue(args.Data));
                        }

                        sb.Append(" - ");
                    }

                    // We remove the final '-' from the string
                    if (props.Length > 0)
                    {
                        sb.Remove(sb.Length - 3, 3);
                    }
                }

                return sb.ToString();
            }
            finally
            {
                _stringBuilderPool.Release(sb);
            }
        }
    }
}
