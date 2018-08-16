using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE.HOS.Diagnostics
{
    static class Demangler
    {
        private static readonly Dictionary<string, string> BuiltinTypes = new Dictionary<string, string>
        {
            { "v", "void" },
            { "w", "wchar_t" },
            { "b", "bool" },
            { "c", "char" },
            { "a", "signed char" },
            { "h", "unsigned char" },
            { "s", "short" },
            { "t", "unsigned short" },
            { "i", "int" },
            { "j", "unsigned int" },
            { "l", "long" },
            { "m", "unsigned long" },
            { "x", "long long" },
            { "y", "unsigned long long" },
            { "n", "__int128" },
            { "o", "unsigned __int128" },
            { "f", "float" },
            { "d", "double" },
            { "e", "long double" },
            { "g", "__float128" },
            { "z", "..." },
            { "Dd", "__iec559_double" },
            { "De", "__iec559_float128" },
            { "Df", "__iec559_float" },
            { "Dh", "__iec559_float16" },
            { "Di", "char32_t" },
            { "Ds", "char16_t" },
            { "Da", "decltype(auto)" },
            { "Dn", "std::nullptr_t" },
        };

        private static readonly Dictionary<string, string> SubstitutionExtra = new Dictionary<string, string>
        {
            {"Sa", "std::allocator"},
            {"Sb", "std::basic_string"},
            {"Ss", "std::basic_string<char, ::std::char_traits<char>, ::std::allocator<char>>"},
            {"Si", "std::basic_istream<char, ::std::char_traits<char>>"},
            {"So", "std::basic_ostream<char, ::std::char_traits<char>>"},
            {"Sd", "std::basic_iostream<char, ::std::char_traits<char>>"}
        };

        private static int FromBase36(string encoded)
        {
            string base36 = "0123456789abcdefghijklmnopqrstuvwxyz";
            char[] reversedEncoded = encoded.ToLower().ToCharArray().Reverse().ToArray();
            int result = 0;
            for (int i = 0; i < reversedEncoded.Length; i++)
            {
                char c = reversedEncoded[i];
                int value = base36.IndexOf(c);
                if (value == -1)
                    return -1;
                result += value * (int)Math.Pow(36, i);
            }
            return result;
        }

        private static string GetCompressedValue(string compression, List<string> compressionData, out int pos)
        {
            string res = null;
            bool canHaveUnqualifiedName = false;
            pos = -1;
            if (compressionData.Count == 0 || !compression.StartsWith("S"))
                return null;

            if (compression.Length >= 2 && SubstitutionExtra.TryGetValue(compression.Substring(0, 2), out string substitutionValue))
            {
                pos = 1;
                res = substitutionValue;
                compression = compression.Substring(2);
            }
            else if (compression.StartsWith("St"))
            {
                pos = 1;
                canHaveUnqualifiedName = true;
                res = "std";
                compression = compression.Substring(2);
            }
            else if (compression.StartsWith("S_"))
            {
                pos = 1;
                res = compressionData[0];
                canHaveUnqualifiedName = true;
                compression = compression.Substring(2);
            }
            else
            {
                int id = -1;
                int underscorePos = compression.IndexOf('_');
                if (underscorePos == -1)
                    return null;
                string partialId = compression.Substring(1, underscorePos - 1);

                id = FromBase36(partialId);
                if (id == -1 || compressionData.Count <= (id + 1))
                {
                    return null;
                }
                res = compressionData[id + 1];
                pos = partialId.Length + 1;
                canHaveUnqualifiedName= true;
                compression = compression.Substring(pos);
            }
            if (res != null)
            {
                if (canHaveUnqualifiedName)
                {
                    List<string> type = ReadName(compression, compressionData, out int endOfNameType);
                    if (endOfNameType != -1 && type != null)
                    {
                        pos  += endOfNameType;
                        res = res + "::" + type[type.Count - 1];
                    }
                }
            }
            return res;
        }

        private static List<string> ReadName(string mangled, List<string> compressionData, out int pos, bool isNested = true)
        {
            List<string> res = new List<string>();
            string charCountString = null;
            int charCount = 0;
            int i;

            pos = -1;
            for (i = 0; i < mangled.Length; i++)
            {
                char chr = mangled[i];
                if (charCountString == null)
                {
                    if (ReadCVQualifiers(chr) != null)
                    {
                        continue;
                    }
                    if (chr == 'S')
                    {
                        string data = GetCompressedValue(mangled.Substring(i), compressionData, out pos);
                        if (pos == -1)
                        {
                            return null;
                        }
                        if (res.Count == 0)
                            res.Add(data);
                        else
                            res.Add(res[res.Count - 1] + "::" + data);
                        i += pos;
                        if (i < mangled.Length && mangled[i] == 'E')
                        {
                            break;
                        }
                        continue;
                    }
                    else if (chr == 'E')
                    {
                        break;
                    }
                }
                if (Char.IsDigit(chr))
                {
                    charCountString += chr;
                }
                else
                {
                    if (!int.TryParse(charCountString, out charCount))
                    {
                        return null;
                    }
                    string demangledPart = mangled.Substring(i, charCount);
                    if (res.Count == 0)
                        res.Add(demangledPart);
                    else
                        res.Add(res[res.Count - 1] + "::" + demangledPart);
                    i = i + charCount - 1;
                    charCount = 0;
                    charCountString = null;
                    if (!isNested)
                        break;
                }
            }
            if (res.Count == 0)
            {
                return null;
            }
            pos = i;
            return res;
        }

        private static string ReadBuiltinType(string mangledType, out int pos)
        {
            string res = null;
            string possibleBuiltinType;
            pos = -1;
            possibleBuiltinType = mangledType[0].ToString();
            if (!BuiltinTypes.TryGetValue(possibleBuiltinType, out res))
            {
                if (mangledType.Length >= 2)
                {
                    // Try to match the first 2 chars if the first call failed
                    possibleBuiltinType = mangledType.Substring(0, 2);
                    BuiltinTypes.TryGetValue(possibleBuiltinType, out res);
                }
            }
            if (res != null)
                pos = possibleBuiltinType.Length;
            return res;
        }

        private static string ReadCVQualifiers(char qualifier)
        {
            if (qualifier == 'r')
                return "restricted";
            else if (qualifier == 'V')
                return "volatile";
            else if (qualifier == 'K')
                return "const";
            return null;
        }

        private static string ReadRefQualifiers(char qualifier)
        {
            if (qualifier == 'R')
                return "&";
            else if (qualifier == 'O')
                return "&&";
            return null;
        }

        private static string ReadSpecialQualifiers(char qualifier)
        {
            if (qualifier == 'P')
                return "*";
            else if (qualifier == 'C')
                return "complex";
            else if (qualifier == 'G')
                return "imaginary";
            return null;
        }

        private static List<string> ReadParameters(string mangledParams, List<string> compressionData, out int pos)
        {
            List<string> res = new List<string>();
            List<string> refQualifiers = new List<string>();
            string parsedTypePart = null;
            string currentRefQualifiers = null;
            string currentBuiltinType = null;
            string currentSpecialQualifiers = null;
            string currentCompressedValue = null;
            int i = 0;
            pos = -1;

            for (i = 0; i < mangledParams.Length; i++)
            {
                if (currentBuiltinType != null)
                {
                    string currentCVQualifier = String.Join(" ", refQualifiers);
                    // Try to mimic the compression indexing
                    if (currentRefQualifiers != null)
                    {
                        compressionData.Add(currentBuiltinType + currentRefQualifiers);
                    }
                    if (refQualifiers.Count != 0)
                    {
                        compressionData.Add(currentBuiltinType + " " + currentCVQualifier + currentRefQualifiers);
                    }
                    if (currentSpecialQualifiers != null)
                    {
                        compressionData.Add(currentBuiltinType + " " + currentCVQualifier + currentRefQualifiers + currentSpecialQualifiers);
                    }
                    if (currentRefQualifiers == null && currentCVQualifier == null && currentSpecialQualifiers == null)
                    {
                        compressionData.Add(currentBuiltinType);
                    }
                    currentBuiltinType = null;
                    currentCompressedValue = null;
                    currentCVQualifier = null;
                    currentRefQualifiers = null;
                    refQualifiers.Clear();
                    currentSpecialQualifiers = null;
                }
                char chr = mangledParams[i];
                string part = mangledParams.Substring(i);

                // Try to read qualifiers
                parsedTypePart = ReadCVQualifiers(chr);
                if (parsedTypePart != null)
                {
                    refQualifiers.Add(parsedTypePart);

                    // need more data
                    continue;
                }

                parsedTypePart = ReadRefQualifiers(chr);
                if (parsedTypePart != null)
                {
                    currentRefQualifiers = parsedTypePart;

                    // need more data
                    continue;
                }

                parsedTypePart = ReadSpecialQualifiers(chr);
                if (parsedTypePart != null)
                {
                    currentSpecialQualifiers = parsedTypePart;

                    // need more data
                    continue;
                }

                // TODO: extended-qualifier?

                if (part.StartsWith("S"))
                {
                    parsedTypePart = GetCompressedValue(part, compressionData, out pos);
                    if (pos != -1 && parsedTypePart != null)
                    {
                        currentCompressedValue = parsedTypePart;
                        i += pos;
                        res.Add(currentCompressedValue + " " + String.Join(" ", refQualifiers) + currentRefQualifiers + currentSpecialQualifiers);
                        currentBuiltinType = null;
                        currentCompressedValue = null;
                        currentRefQualifiers = null;
                        refQualifiers.Clear();
                        currentSpecialQualifiers = null;
                        continue;
                    }
                    pos = -1;
                    return null;
                }
                else if (part.StartsWith("N"))
                {
                    part = part.Substring(1);
                    List<string> name = ReadName(part, compressionData, out pos);
                    if (pos != -1 && name != null)
                    {
                        i += pos + 1;
                        res.Add(name[name.Count - 1]  + " " + String.Join(" ", refQualifiers) + currentRefQualifiers + currentSpecialQualifiers);
                        currentBuiltinType = null;
                        currentCompressedValue = null;
                        currentRefQualifiers = null;
                        refQualifiers.Clear();
                        currentSpecialQualifiers = null;
                        continue;
                    }
                }

                // Try builting
                parsedTypePart = ReadBuiltinType(part, out pos);
                if (pos == -1)
                {
                    return null;
                }
                currentBuiltinType = parsedTypePart;
                res.Add(currentBuiltinType + " " + String.Join(" ", refQualifiers) + currentRefQualifiers + currentSpecialQualifiers);
                i = i + pos -1;
            }
            pos = i;
            return res;
        }

        private static string ParseFunctionName(string mangled)
        {
            List<string> compressionData = new List<string>();
            int pos = 0;
            string res;
            bool isNested = mangled.StartsWith("N");

            // If it's start with "N" it must be a nested function name
            if (isNested)
                mangled = mangled.Substring(1);
            compressionData = ReadName(mangled, compressionData, out pos, isNested);
            if (pos == -1)
                return null;
            res = compressionData[compressionData.Count - 1];
            compressionData.Remove(res);
            mangled = mangled.Substring(pos + 1);

            // more data? maybe not a data name so...
            if (mangled != String.Empty)
            {
                List<string> parameters = ReadParameters(mangled, compressionData, out pos);
                // parameters parsing error, we return the original data to avoid information loss.
                if (pos == -1)
                    return null;
                parameters = parameters.Select(outer => outer.Trim()).ToList();
                res += "(" + String.Join(", ", parameters) + ")";
            }
            return res;
        }

        public static string Parse(string originalMangled)
        {
            if (originalMangled.StartsWith("_Z"))
            {
                // We assume that we have a name (TOOD: support special names)
                string res = ParseFunctionName(originalMangled.Substring(2));
                if (res == null)
                    return originalMangled;
                return res;
            }
            return originalMangled;
        }
    }
}