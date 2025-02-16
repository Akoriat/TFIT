namespace Lab4.Models
{
    public enum EEntryType { EtCmd, EtVar, EtConst, EtCmdPtr }

    public enum ECmd
    {
        JMP,
        JZ,
        SET,
        ADD,
        SUB,
        AND,
        OR,
        CMPE,
        CMPNE,
        CMPL,
        MUL,
        DIV,
        CMPG,
        NOT,
        OUT
    }

    public class PostfixEntry
    {
        public EEntryType Type;
        public int Index;
    }
}