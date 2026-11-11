# -*- coding: utf-8 -*-
"""
根据已转换的 JSON 反向生成 LuBan Excel 表（#tbxxx.xlsx），
让配置表回到「Excel → gen.bat → JSON」的正规管线。
"""
import json
import os

import openpyxl

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
JSON_DIR = os.path.join(ROOT, "Assets", "StreamingAssets")
XLSX_DIR = os.path.join(ROOT, "LubanConfig", "MiniTemplate", "Datas")

# 表名 -> [(字段名, Luban类型, 注释)]
TABLES = {
    "tbsound": [
        ("id", "int", "音效ID"),
        ("assetId", "int", "资源ID"),
        ("soundGroup", "string", "声音组"),
        ("spatialBlend", "float", "空间混合"),
        ("time", "float", "延迟播放时间"),
        ("volume", "float", "音量"),
        ("priority", "int", "优先级"),
        ("maxDistance", "float", "最大距离"),
        ("dopplerLevel", "float", "多普勒级别"),
        ("pitch", "float", "音调"),
        ("loop", "bool", "是否循环"),
    ],
    "tbitem": [
        ("id", "int", "道具ID"),
        ("name", "string", "道具名"),
        ("icons", "string", "图标"),
        ("style", "int", "显示样式"),
    ],
    "tbitemiap": [
        ("id", "int", "商品ID"),
        ("productId", "string", "商店商品ID"),
        ("productType", "int", "商品类型"),
        ("platform", "int", "平台"),
        ("prizeValue", "float", "价格"),
        ("tikTokBeans", "int", "TikTok 豆"),
        ("revType", "int", "收入类型"),
        ("baseProductId", "string", "基础商品ID"),
        ("originalPrize", "float", "原价"),
        ("eventName", "string", "埋点事件名"),
    ],
    "tbuiform": [
        ("id", "int", "界面ID"),
        ("name", "string", "界面名"),
        ("directoryName", "string", "目录名"),
        ("uIGroupName", "string", "UI组名"),
        ("allowMultiInstance", "bool", "允许重复打开"),
        ("pauseCoveredUIForm", "bool", "覆盖时暂停"),
        ("mutex", "bool", "是否互斥"),
    ],
    "tbguide": [
        ("id", "int", "引导ID"),
        ("waitUIOpenType", "int", "等待打开的界面"),
        ("guideType", "int", "引导类型"),
        ("nextId", "int", "下一个引导ID"),
    ],
    "tbfingerguide": [
        ("id", "int", "引导ID"),
        ("sceneName", "string", "场景名"),
        ("rootName", "string", "根节点名"),
        ("higtLightTargetPath", "string", "高亮目标路径"),
        ("buttonPath", "string", "按钮路径"),
        ("pivot", "vector2", "指针偏移"),
    ],
    "tbdialogguide": [
        ("id", "int", "引导ID"),
        ("anchorID", "int", "锚点ID"),
        ("clothID", "int", "立绘ID"),
        ("content", "string", "对话内容"),
        ("audioAssetID", "int", "语音资源ID"),
    ],
    "tbactivityinfo": [
        ("id", "int", "活动ID"),
        ("unlockLv", "int", "解锁等级"),
        ("conditionType", "int", "条件类型"),
        ("duration", "int", "持续时长"),
        ("interval", "int", "间隔"),
        ("spinePath", "string", "Spine 资源路径"),
    ],
}


def cell_value(value, luban_type):
    if value is None:
        return None
    if luban_type == "vector2":
        return "{},{}".format(value.get("x", 0), value.get("y", 0))
    return value


def main():
    for table, fields in TABLES.items():
        path = os.path.join(JSON_DIR, table + ".json")
        rows = json.load(open(path, encoding="utf-8"))

        wb = openpyxl.Workbook()
        ws = wb.active
        ws.title = "Sheet1"

        ws.append(["##var"] + [f[0] for f in fields])
        ws.append(["##type"] + [f[1] for f in fields])
        ws.append(["##group"] + [None] * len(fields))
        ws.append(["##"] + [f[2] for f in fields])

        for row in rows:
            ws.append([None] + [cell_value(row.get(f[0]), f[1]) for f in fields])

        out = os.path.join(XLSX_DIR, "#" + table + ".xlsx")
        wb.save(out)
        print("%-16s %3d 行 -> %s" % (table, len(rows), os.path.basename(out)))


if __name__ == "__main__":
    main()
