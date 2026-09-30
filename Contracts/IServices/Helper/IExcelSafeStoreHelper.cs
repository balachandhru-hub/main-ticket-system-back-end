namespace Contracts.IServices
{
    public interface IExcelSafeStoreHelper
    {
        /// <summary>
        /// Make the string safe to store in Excel (e.g., size limit, invalid characters)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string? ExcelSafe(string? value);
    }
}