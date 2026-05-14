import re
from pathlib import Path

root = Path(__file__).resolve().parent.parent
pattern = re.compile(r'new\s+([A-Za-z_][A-Za-z0-9_<>]*?)\s*\(\s*\)')


def scan_args(text, start):
    depth = 0
    i = start
    in_string = None
    esc = False
    n = len(text)
    while i < n:
        ch = text[i]
        if in_string:
            if esc:
                esc = False
            elif ch == '\\':
                esc = True
            elif ch == in_string:
                in_string = None
        else:
            if ch == '"' or ch == "'":
                in_string = ch
            elif ch == '(':
                depth += 1
            elif ch == ')':
                depth -= 1
                if depth == 0:
                    return i
        i += 1
    return -1

changed_files = []
for path in root.rglob('*.cs'):
    text = path.read_text(encoding='utf-8')
    out = []
    idx = 0
    modified = False
    while True:
        m = pattern.search(text, idx)
        if not m:
            out.append(text[idx:])
            break
        start = m.start()
        end = m.end()
        out.append(text[idx:start])
        cur = end
        set_props = []
        while True:
            while cur < len(text) and text[cur].isspace():
                cur += 1
            if cur >= len(text) or text[cur] != '.':
                break
            meth_start = cur + 1
            meth_end = meth_start
            while meth_end < len(text) and (text[meth_end].isalnum() or text[meth_end] == '_'):
                meth_end += 1
            meth = text[meth_start:meth_end]
            if not meth.startswith('Set') or meth_end >= len(text) or text[meth_end] != '(':
                break
            arg_start = meth_end
            arg_end = scan_args(text, arg_start)
            if arg_end == -1:
                break
            arg_text = text[arg_start+1:arg_end].strip()
            prop_name = meth[3:]
            if prop_name:
                set_props.append((prop_name, arg_text))
                cur = arg_end + 1
                continue
            break
        if set_props:
            line_start = text.rfind('\n', 0, start) + 1
            prefix = text[line_start:start]
            indent = ''.join(' ' if c == '\t' else c for c in prefix)
            if len(set_props) == 1:
                rep = f"new {m.group(1)} {{ {set_props[0][0]} = {set_props[0][1]} }}"
            else:
                rep_lines = [f"new {m.group(1)}", "{"]
                for prop, arg in set_props:
                    rep_lines.append(f"    {prop} = {arg},")
                rep_lines.append("}")
                rep = '\n'.join(rep_lines).replace('\n', '\n' + indent)
            out.append(rep)
            idx = cur
            modified = True
        else:
            out.append(text[start:end])
            idx = end
    if modified:
        path.write_text(''.join(out), encoding='utf-8')
        changed_files.append(path)

print(f'Processed {len(changed_files)} files')
for path in changed_files:
    print(path)
