using System.Text;

namespace Ryujinx.Common.Logging
{
    internal class DefaultLogFormatter : ILogFormatter
    {
        private static readonly ObjectPool<StringBuilder> StringBuilderPool = SharedPools.Default<StringBuilder>();

        public string Format(LogEventArgs args)
        {
            StringBuilder sb = StringBuilderPool.Allocate();

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
                    sb.Append(' ');
                    DynamicObjectFormatter.Format(sb, args.Data);
                }

                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Release(sb);
            }
        }
    }
}
