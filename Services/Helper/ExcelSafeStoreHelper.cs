using Contracts.IServices;

namespace Services
{
    public class ExcelSafeStoreHelper : IExcelSafeStoreHelper
    {
        private const int ExcelCellCharacterLimit = 32767;

        /// <summary>
        /// Make the string safe to store in Excel (e.g., size limit, invalid characters)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string? ExcelSafe(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length <= ExcelCellCharacterLimit
                ? value
                : value[..ExcelCellCharacterLimit];
        }
    }
}