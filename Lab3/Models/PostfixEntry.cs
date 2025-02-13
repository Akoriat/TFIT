using System;

namespace Lab3.Models
{
    public enum EEntryType 
    { 
        etCmd,
        etVar,
        etConst,
        etCmdPtr 
    }

    public enum ECmd
    {
        JMP,
        JZ,
        SET,
        ADD,
        SUB,
        MUL,
        DIV,
        AND,
        OR,
        NOT,   
        CMPE,  
        CMPNE, 
        CMPL,  
        CMPLE,
        OUTPUT 
    }

    public struct PostfixEntry
    {
        public EEntryType Type;
        public int Index;

        public static List<string> ConstTable { get; set; }
        public static List<string> IdentifierTable { get; set; }

        public override string ToString()
        {
            switch (Type)
            {
                case EEntryType.etCmd:
                    return $"etCmd: {((ECmd)Index)}";
                case EEntryType.etVar:
                    if (IdentifierTable != null && Index >= 0 && Index < IdentifierTable.Count)
                        return $"etVar: {IdentifierTable[Index]}";
                    else
                        return $"etVar: {Index}";
                case EEntryType.etConst:
                    if (ConstTable != null && Index >= 0 && Index < ConstTable.Count)
                        return $"etConst: {ConstTable[Index]}";
                    else
                        return $"etConst: {Index}";
                case EEntryType.etCmdPtr:
                    return $"etCmdPtr: {Index}";
                default:
                    return $"{Type} : {Index}";
            }
        }
    }
}
