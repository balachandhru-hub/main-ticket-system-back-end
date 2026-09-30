namespace Dto
{
    public class GrnRow
    {
        public string PO_Number { get; set; }
        public string Line_Item { get; set; }
        public decimal PO_Quantity { get; set; }
        public string PO_Line_Item { get; set; }
        public decimal GRN_Quantity { get; set; }
        public decimal GRN_Amount { get; set; }
        public string GRN_Description { get; set; }
    }

    public class S4Row
    {
        public string PO_Number { get; set; }
        public string Line_Item { get; set; }
        public decimal S4_Quantity { get; set; }
        public decimal S4_Amount { get; set; }
    }

    public class InvoiceRow
    {
        public string PO_Number { get; set; }
        public string Line_Item { get; set; }
        public decimal Invoice_Quantity { get; set; }
        public decimal Invoice_Amount { get; set; }
    }

    public class ResultRow
    {
        public string PO_Number { get; set; }
        public string Line_Item { get; set; }
        public decimal S4_Quantity_Total { get; set; }
        public decimal PO_Quantity_GRN_Total { get; set; }
        public decimal S4_Duplicate_Quantity { get; set; }
        public string Invoice_PO_Number { get; set; }
        public string Invoice_Line_Item { get; set; }
        public decimal Invoice_Total_Quantity { get; set; }
        public decimal Invoice_Duplicate_Quantity { get; set; }
        public decimal GRN_Amount_Total { get; set; }
        public decimal PO_Quantity { get; set; }
        public decimal S4_Amount_Total { get; set; }
        public decimal Invoice_Amount_Total { get; set; }
        public string GRN_Description { get; set; }
    }
}
