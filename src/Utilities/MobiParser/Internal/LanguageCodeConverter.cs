// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// 语言代码转换器。
/// </summary>
internal static class LanguageCodeConverter
{
    private static readonly Dictionary<uint, string> LanguageCodes = new()
    {
        { 0x0401, "ar" },      // 阿拉伯语
        { 0x0402, "bg" },      // 保加利亚语
        { 0x0403, "ca" },      // 加泰罗尼亚语
        { 0x0404, "zh-TW" },   // 繁体中文
        { 0x0405, "cs" },      // 捷克语
        { 0x0406, "da" },      // 丹麦语
        { 0x0407, "de" },      // 德语
        { 0x0408, "el" },      // 希腊语
        { 0x0409, "en" },      // 英语
        { 0x040A, "es" },      // 西班牙语
        { 0x040B, "fi" },      // 芬兰语
        { 0x040C, "fr" },      // 法语
        { 0x040D, "he" },      // 希伯来语
        { 0x040E, "hu" },      // 匈牙利语
        { 0x040F, "is" },      // 冰岛语
        { 0x0410, "it" },      // 意大利语
        { 0x0411, "ja" },      // 日语
        { 0x0412, "ko" },      // 韩语
        { 0x0413, "nl" },      // 荷兰语
        { 0x0414, "no" },      // 挪威语
        { 0x0415, "pl" },      // 波兰语
        { 0x0416, "pt" },      // 葡萄牙语
        { 0x0417, "rm" },      // 罗曼什语
        { 0x0418, "ro" },      // 罗马尼亚语
        { 0x0419, "ru" },      // 俄语
        { 0x041A, "hr" },      // 克罗地亚语
        { 0x041B, "sk" },      // 斯洛伐克语
        { 0x041C, "sq" },      // 阿尔巴尼亚语
        { 0x041D, "sv" },      // 瑞典语
        { 0x041E, "th" },      // 泰语
        { 0x041F, "tr" },      // 土耳其语
        { 0x0420, "ur" },      // 乌尔都语
        { 0x0421, "id" },      // 印度尼西亚语
        { 0x0422, "uk" },      // 乌克兰语
        { 0x0423, "be" },      // 白俄罗斯语
        { 0x0424, "sl" },      // 斯洛文尼亚语
        { 0x0425, "et" },      // 爱沙尼亚语
        { 0x0426, "lv" },      // 拉脱维亚语
        { 0x0427, "lt" },      // 立陶宛语
        { 0x0429, "fa" },      // 波斯语
        { 0x042A, "vi" },      // 越南语
        { 0x042B, "hy" },      // 亚美尼亚语
        { 0x042C, "az" },      // 阿塞拜疆语
        { 0x042D, "eu" },      // 巴斯克语
        { 0x042F, "mk" },      // 马其顿语
        { 0x0436, "af" },      // 南非荷兰语
        { 0x0437, "ka" },      // 格鲁吉亚语
        { 0x0438, "fo" },      // 法罗语
        { 0x0439, "hi" },      // 印地语
        { 0x043E, "ms" },      // 马来语
        { 0x043F, "kk" },      // 哈萨克语
        { 0x0441, "sw" },      // 斯瓦希里语
        { 0x0443, "uz" },      // 乌兹别克语
        { 0x0444, "tt" },      // 鞑靼语
        { 0x0446, "pa" },      // 旁遮普语
        { 0x0447, "gu" },      // 古吉拉特语
        { 0x0449, "ta" },      // 泰米尔语
        { 0x044A, "te" },      // 泰卢固语
        { 0x044B, "kn" },      // 卡纳达语
        { 0x044C, "ml" },      // 马拉雅拉姆语
        { 0x044E, "mr" },      // 马拉地语
        { 0x044F, "sa" },      // 梵语
        { 0x0450, "mn" },      // 蒙古语
        { 0x0451, "bo" },      // 藏语
        { 0x0452, "cy" },      // 威尔士语
        { 0x0456, "gl" },      // 加利西亚语
        { 0x0457, "kok" },     // 孔卡尼语
        { 0x045A, "syr" },     // 叙利亚语
        { 0x0461, "ne" },      // 尼泊尔语
        { 0x0465, "dv" },      // 迪维希语
        { 0x0804, "zh-CN" },   // 简体中文
        { 0x0809, "en-GB" },   // 英语（英国）
        { 0x0816, "pt-BR" },   // 葡萄牙语（巴西）
        { 0x0C04, "zh-HK" },   // 中文（香港）
        { 0x0C0A, "es-MX" },   // 西班牙语（墨西哥）
        { 0x0C0C, "fr-CA" },   // 法语（加拿大）
        { 0x1004, "zh-SG" },   // 中文（新加坡）
        { 0x1009, "en-CA" },   // 英语（加拿大）
        { 0x1404, "zh-MO" },   // 中文（澳门）
        { 0x1409, "en-NZ" },   // 英语（新西兰）
    };

    /// <summary>
    /// 将语言代码转换为语言标签。
    /// </summary>
    /// <param name="code">语言代码。</param>
    /// <returns>语言标签（如 "zh-CN"），如果未知则返回 null。</returns>
    public static string? ToLanguageTag(uint code)
    {
        if (LanguageCodes.TryGetValue(code, out var tag))
        {
            return tag;
        }

        // 尝试只匹配低 10 位
        var primaryCode = code & 0x3FF;
        foreach (var kvp in LanguageCodes)
        {
            if ((kvp.Key & 0x3FF) == primaryCode)
            {
                return kvp.Value;
            }
        }

        return null;
    }
}
