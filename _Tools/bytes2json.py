# -*- coding: utf-8 -*-
"""
把旧 GameFramework DataTable 的 .bytes 转成 JSON。
格式：连续 [7bit长度][行字节]，行内按 DR*.ParseDataRow(byte[]) 的字段顺序 7bit 编码。
"""
import os
import re
import json
import struct

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DR_DIR = os.path.join(ROOT, "Assets", "Scripts", "DataTables")
BYTES_DIR = os.path.join(ROOT, "Assets", "Game", "DataTables")
OUT_DIR = os.path.join(ROOT, "Assets", "StreamingAssets")

# DR 类名 -> (输出json名, 表名)
TABLES = {
    "DRActivityInfo": "tbactivityinfo",
    "DRDialogGuide": "tbdialogguide",
    "DRFingerGuide": "tbfingerguide",
    "DRGuide": "tbguide",
    "DRItem": "tbitem",
    "DRItemIAP": "tbitemiap",
    "DRSound": "tbsound",
    "DRUIForm": "tbuiform",
}


class Reader:
    def __init__(self, data):
        self.d = data
        self.p = 0

    def eof(self):
        return self.p >= len(self.d)

    def varint(self):
        result = 0
        shift = 0
        while True:
            b = self.d[self.p]
            self.p += 1
            result |= (b & 0x7F) << shift
            if (b & 0x80) == 0:
                break
            shift += 7
        return result

    def read(self, t):
        if t == "int":
            v = self.varint() & 0xFFFFFFFF
            return v - 0x100000000 if v > 0x7FFFFFFF else v
        if t == "long":
            v = self.varint() & 0xFFFFFFFFFFFFFFFF
            return v - 0x10000000000000000 if v > 0x7FFFFFFFFFFFFFFF else v
        if t == "bool":
            v = self.d[self.p]
            self.p += 1
            return v != 0
        if t == "float":
            v = struct.unpack_from("<f", self.d, self.p)[0]
            self.p += 4
            return round(v, 6)
        if t == "string":
            n = self.varint()
            s = self.d[self.p:self.p + n].decode("utf-8")
            self.p += n
            return s
        if t == "string[]":
            n = struct.unpack_from("<i", self.d, self.p)[0]
            self.p += 4
            return [self.read("string") for _ in range(n)]
        if t == "vector2":
            return {"x": self.read("float"), "y": self.read("float")}
        raise ValueError("unknown type " + t)


def parse_fields(cls_name):
    """从 DR*.cs 的 ParseDataRow(byte[]...) 里提取字段序列。"""
    path = os.path.join(DR_DIR, cls_name + ".cs")
    src = open(path, encoding="utf-8-sig").read()
    # 找 byte[] 版本的方法体
    m = re.search(r"ParseDataRow\(byte\[\][^)]*\)(.*?)\n        \}", src, re.S)
    body = m.group(1)
    fields = re.findall(
        r"(\w+)\s*=\s*(?:this\.)?(\w+)?\s*;?\s*$" if False else
        r"(?:m_)?(\w+)\s*=\s*(?:\w+\.)?(\w+)?",
        body)
    # 更精确：逐行匹配
    result = []
    for line in body.splitlines():
        line = line.strip()
        mm = re.match(r"(?:m_)?(\w+)\s*=\s*binaryReader\.(Read\w+)\(", line)
        if mm:
            name, method = mm.group(1), mm.group(2)
            t = {
                "Read7BitEncodedInt32": "int",
                "Read7BitEncodedInt64": "long",
                "ReadSingle": "float",
                "ReadString": "string",
                "ReadBoolean": "bool",
                "ReadStringArray": "string[]",
                "ReadVector2": "vector2",
            }[method]
            result.append((name, t))
    return result


def to_camel(name):
    # m_Id -> Id -> id ; AssetId -> assetId
    if name.startswith("m_"):
        name = name[2:]
    if not name:
        return name
    return name[0].lower() + name[1:]


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    for cls_name, out_name in TABLES.items():
        fields = parse_fields(cls_name)
        bpath = os.path.join(BYTES_DIR, cls_name[2:] + ".bytes")
        if not os.path.exists(bpath):
            print("MISS %s" % bpath)
            continue
        data = open(bpath, "rb").read()
        r = Reader(data)
        rows = []
        while not r.eof():
            length = r.varint()
            end = r.p + length
            row = {}
            for name, t in fields:
                row[to_camel(name)] = r.read(t)
            if r.p != end:
                raise ValueError("%s: row misaligned (%d != %d)" % (cls_name, r.p, end))
            rows.append(row)
        outpath = os.path.join(OUT_DIR, out_name + ".json")
        with open(outpath, "w", encoding="utf-8") as f:
            json.dump(rows, f, ensure_ascii=False, indent=2)
        print("%-16s %3d 行 -> %s.json  字段: %s" % (
            cls_name, len(rows), out_name, ",".join(to_camel(n) for n, _ in fields)))


if __name__ == "__main__":
    main()
