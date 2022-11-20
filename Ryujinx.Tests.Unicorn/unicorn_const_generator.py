#!/usr/bin/env python3
# Unicorn Engine
# By Dang Hoang Vu, 2013
# Modified for Ryujinx from: https://github.com/unicorn-engine/unicorn/blob/6c1cbef6ac505d355033aef1176b684d02e1eb3a/bindings/const_generator.py
from __future__ import print_function
import sys, re, os

include = [ 'arm.h', 'arm64.h', 'unicorn.h' ]
split_common = [ 'ARCH', 'MODE', 'ERR', 'MEM', 'TCG', 'HOOK', 'PROT' ]

template = {
    'dotnet': {
            'header': "// Constants for Unicorn Engine. AUTO-GENERATED FILE, DO NOT EDIT\n\n// ReSharper disable InconsistentNaming\nnamespace Ryujinx.Tests.Unicorn.Native.Const\n{\n    public enum %s\n    {\n",
            'footer': "    }\n}\n",
            'line_format': '        %s = %s,\n',
            'out_file': os.path.join(os.path.dirname(__file__), 'Native', 'Const', '%s.cs'),
            # prefixes for constant filenames of all archs - case sensitive
            'arm.h': 'Arm',
            'arm64.h': 'Arm64',
            'unicorn.h': 'Common',
            # prefixes for filenames of split_common values - case sensitive
            'ARCH': 'Arch',
            'MODE': 'Mode',
            'ERR': 'Error',
            'MEM': 'Memory',
            'TCG': 'TCG',
            'HOOK': 'Hook',
            'PROT': 'Permission',
            'comment_open': '        //',
            'comment_close': '',
        }
}

# markup for comments to be added to autogen files
MARKUP = '//>'

def gen(unicorn_repo_path):
    global include
    include_dir = os.path.join(unicorn_repo_path, 'include', 'unicorn')
    templ = template["dotnet"]
    for target in include:
        prefix = templ[target]
        outfile = open(templ['out_file'] %(prefix), 'wb')   # open as binary prevents windows newlines
        outfile.write((templ['header'] % (prefix)).encode("utf-8"))
        if target == 'unicorn.h':
            prefix = ''
            for cat in split_common:
                with open(templ['out_file'] %(templ[cat]), 'wb') as file:
                    file.write((templ['header'] %(templ[cat])).encode("utf-8"))
        with open(os.path.join(include_dir, target)) as f:
            lines = f.readlines()

        previous = {}
        count = 0
        skip = 0
        in_comment = False

        for lno, line in enumerate(lines):
            if "/*" in line:
                in_comment = True
            if "*/" in line:
                in_comment = False
            if in_comment:
                continue
            if skip > 0:
                # Due to clang-format, values may come up in the next line
                skip -= 1
                continue
            line = line.strip()

            if line.startswith(MARKUP):  # markup for comments
                outfile.write(("\n%s%s%s\n" %(templ['comment_open'], \
                            line.replace(MARKUP, ''), templ['comment_close'])).encode("utf-8"))
                continue

            if line == '' or line.startswith('//'):
                continue

            tmp = line.strip().split(',')
            if len(tmp) >= 2 and tmp[0] != "#define" and not tmp[0].startswith("UC_"):
                continue
            for t in tmp:
                t = t.strip()
                if not t or t.startswith('//'): continue
                f = re.split('\s+', t)

                # parse #define UC_TARGET (num)
                define = False
                if f[0] == '#define' and len(f) >= 3:
                    define = True
                    f.pop(0)
                    f.insert(1, '=')
                if f[0].startswith("UC_" + prefix.upper()) or f[0].startswith("UC_CPU"):
                    if len(f) > 1 and f[1] not in ('//', '='):
                        print("WARNING: Unable to convert %s" % f)
                        print("  Line =", line)
                        continue
                    elif len(f) > 1 and f[1] == '=':
                        # Like:
                        # UC_A =
                        #       (1 << 2)
                        # #define UC_B \
                        #              (UC_A | UC_C)
                        # Let's search the next line
                        if len(f) == 2:
                            if lno == len(lines) - 1:
                                print("WARNING: Unable to convert %s" % f)
                                print("  Line =", line)
                                continue
                            skip += 1
                            next_line = lines[lno + 1]
                            next_line_tmp = next_line.strip().split(",")
                            rhs = next_line_tmp[0]
                        elif f[-1] == "\\":
                            idx = 0
                            rhs = ""
                            while True:
                                idx += 1
                                if lno + idx == len(lines):
                                    print("WARNING: Unable to convert %s" % f)
                                    print("  Line =", line)
                                    continue
                                skip += 1
                                next_line = lines[lno + idx]
                                next_line_f = re.split('\s+', next_line.strip())
                                if next_line_f[-1] == "\\":
                                    rhs += "".join(next_line_f[:-1])
                                else:
                                    rhs += next_line.strip()
                                    break
                        else:
                            rhs = ''.join(f[2:])
                    else:
                        rhs = str(count)


                    lhs = f[0].strip()
                    #print(f'lhs: {lhs} rhs: {rhs} f:{f}')
                    # evaluate bitshifts in constants e.g. "UC_X86 = 1 << 1"
                    match = re.match(r'(?P<rhs>\s*\d+\s*<<\s*\d+\s*)', rhs)
                    if match:
                        rhs = str(eval(match.group(1)))
                    else:
                        # evaluate references to other constants e.g. "UC_ARM_REG_X = UC_ARM_REG_SP"
                        match = re.match(r'^([^\d]\w+)$', rhs)
                        if match:
                            rhs = previous[match.group(1)]

                    if not rhs.isdigit():
                        for k, v in previous.items():
                            rhs = re.sub(r'\b%s\b' % k, v, rhs)
                        rhs = str(eval(rhs))

                    lhs_strip = re.sub(r'^UC_', '', lhs)
                    count = int(rhs) + 1

                    if target == "unicorn.h":
                        matched_cat = False
                        for cat in split_common:
                            if lhs_strip.startswith(f"{cat}_"):
                                with open(templ['out_file'] %(templ[cat]), 'ab') as cat_file:
                                    cat_lhs_strip = lhs_strip
                                    if not lhs_strip.lstrip(f"{cat}_").isnumeric():
                                        cat_lhs_strip = lhs_strip.replace(f"{cat}_", "", 1)
                                    cat_file.write(
                                        (templ['line_format'] % (cat_lhs_strip, rhs)).encode("utf-8"))
                                    matched_cat = True
                                    break
                        if matched_cat:
                            previous[lhs] = str(rhs)
                            continue

                    if (count == 1):
                        outfile.write(("\n").encode("utf-8"))

                    if lhs_strip.startswith(f"{prefix.upper()}_") and not lhs_strip.replace(f"{prefix.upper()}_", "", 1).isnumeric():
                        lhs_strip = lhs_strip.replace(f"{prefix.upper()}_", "", 1)

                    outfile.write((templ['line_format'] % (lhs_strip, rhs)).encode("utf-8"))
                    previous[lhs] = str(rhs)

        outfile.write((templ['footer']).encode("utf-8"))
        outfile.close()

        if target == "unicorn.h":
            for cat in split_common:
                with open(templ['out_file'] %(templ[cat]), 'ab') as cat_file:
                    cat_file.write(templ['footer'].encode('utf-8'))

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage:", sys.argv[0], " <path to unicorn repo>")
        sys.exit(1)
    unicorn_repo_path = sys.argv[1]
    if os.path.isdir(unicorn_repo_path):
        print("Generating constants for dotnet")
        gen(unicorn_repo_path)
    else:
        print("Couldn't find unicorn repo at:", unicorn_repo_path)
