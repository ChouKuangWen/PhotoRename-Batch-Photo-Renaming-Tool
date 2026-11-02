namespace PhotoRename.Core.Models;

public enum IncrementMode
{
    None,

    // 中段數字遞增
    MidNumeric,

    // 尾段數字遞增
    Numeric,

    // 中段與尾段同時遞增
    BothNumeric
}