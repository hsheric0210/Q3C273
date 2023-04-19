using Everything.Win32PE.Structs;

namespace Everything.Win32PE.PE
{
    public static class ILDecoder
    {
        public static bool IsMethodDef(long tokenValue)
        {
            var lowerValue = tokenValue & 0x00000000FFFFFFFF;
            if (lowerValue != tokenValue)
                return false;

            if (((int)lowerValue & (int)CorTokenType.mdtMethodDef) == (int)CorTokenType.mdtMethodDef)
                return true;

            return false;
        }
    }
}
