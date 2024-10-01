using System.Diagnostics;
using System.Text;

namespace Ryujinx.Common.Logging.Formatters
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

                sb.Append($@"{args.Time:hh\:mm\:ss\.fff}");
                sb.Append($" |{args.Level.ToString()[0]}| ");

                if (args.ThreadName != null)
                {
                    sb.Append(args.ThreadName);
                    sb.Append(' ');
                }

                sb.Append(args.Message);

                if (args.Data is not null)
                {
                    if (args.Data is StackTrace trace)
                    {
                        sb.Append('\n');
                        sb.Append(trace);

                        return sb.ToString();
                    }

                    sb.Append(' ');
                    DynamicObjectFormatter.Format(sb, args.Data);
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
