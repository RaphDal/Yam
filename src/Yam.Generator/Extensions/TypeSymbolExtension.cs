using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Yam.Generator.Extensions;

public static class TypeSymbolExtensions
{
    public static bool IsNativeType(this ITypeSymbol symbol)
    {
        Debug.Assert(symbol != null);

        return (symbol?.SpecialType) switch
        {
            SpecialType.System_Boolean or
            SpecialType.System_SByte or
            SpecialType.System_Int16 or
            SpecialType.System_Int32 or
            SpecialType.System_Int64 or
            SpecialType.System_Byte or
            SpecialType.System_UInt16 or
            SpecialType.System_UInt32 or
            SpecialType.System_UInt64 or
            SpecialType.System_Single or
            SpecialType.System_Double or
            SpecialType.System_Char => true,
            _ => false,
        };
    }
}
