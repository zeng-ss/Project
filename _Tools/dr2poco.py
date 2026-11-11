# -*- coding: utf-8 -*-
"""把 DR*.cs 从 DataRowBase 派生类改写成纯 POCO。"""
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DR_DIR = os.path.join(ROOT, "Assets", "Scripts", "DataTables")

# 保留的表：类名 -> (中文名, json文件)
KEEP = {
    "DRActivityInfo": ("活动配置表", "tbactivityinfo"),
    "DRDialogGuide": ("对话引导配置表", "tbdialogguide"),
    "DRFingerGuide": ("手指引导配置表", "tbfingerguide"),
    "DRGuide": ("引导配置表", "tbguide"),
    "DRItem": ("道具配置表", "tbitem"),
    "DRItemIAP": ("内购商品配置表", "tbitemiap"),
    "DRSound": ("音效配置表", "tbsound"),
    "DRUIForm": ("UI 界面配置表", "tbuiform"),
}

HEADER = """//------------------------------------------------------------
// 配置表：{cn}
// 数据来源：Assets/StreamingAssets/{json}.json（由 LuBan 生成）
// 读取方式：GameManager.Instance.Config.GetTable<{cls}>()
//------------------------------------------------------------

{usings}namespace CasualGame
{{
    /// <summary>
    /// {cn}。
    /// </summary>
    public class {cls}
    {{
{props}
    }}
}}
"""


def parse_props(path):
    src = open(path, encoding="utf-8-sig").read()
    props = []
    for m in re.finditer(
            r"public\s+(?:override\s+)?([A-Za-z0-9_<>\[\]]+)\s+([A-Za-z0-9_]+)\s*\n?\s*\{", src):
        typ, name = m.group(1), m.group(2)
        if typ == "bool" and name == "ParseDataRow":
            continue
        if name in ("ParseDataRow", "GeneratePropertyArray"):
            continue
        props.append((typ, name))
    return props


def main():
    for cls, (cn, json) in KEEP.items():
        path = os.path.join(DR_DIR, cls + ".cs")
        props = parse_props(path)
        needs_unity = any(t == "Vector2" for t, _ in props)
        usings = "using UnityEngine;\n\n" if needs_unity else ""
        body = "\n".join("        public %s %s { get; set; }" % (t, n) for t, n in props)
        text = HEADER.format(cn=cn, json=json, cls=cls, usings=usings, props=body)
        with open(path, "w", encoding="utf-8", newline="\n") as f:
            f.write(text)
        print("%-16s %2d 个字段" % (cls, len(props)))


if __name__ == "__main__":
    main()
