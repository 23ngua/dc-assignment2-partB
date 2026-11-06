using BankWebApp.Models.Enums;

namespace BankWebApp.Helpers
{
    // helps converts the int from the API into string label
    public static class TransactionTypeName
    {
        public static string ToDisplayName(int type)
        {
            if (!Enum.IsDefined(typeof(TransactionType), type))
            {
                return "Unknown";
            }

            TransactionType transactionType = (TransactionType)type;
            return transactionType.ToString();
        }
    }
}