using System;

namespace Dto
{
    public class CreateVenueDto
    {
        public string VenueName { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string CostCenter { get; set; }
        public string GLAccount { get; set; }
        public string ShipToPlant { get; set; }
        public string PurchaseOrg { get; set; }
        public string PurchaseGroup { get; set; }
        public string BillTo { get; set; }
    }

    public class MasterSupplierDto
    {
        public Guid Id { get; set; }
        public Guid DomainId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
    }


    public class VenueDto
    {
        public Guid Id { get; set; }
        public Guid DomainId { get; set; }
        public string VenueName { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string CostCenter { get; set; }
        public string GLAccount { get; set; }
        public string ShipToPlant { get; set; }
        public string PurchaseOrg { get; set; }
        public string PurchaseGroup { get; set; }
        public string BillTo { get; set; }
    }

    public class CreateVenueSupplierDto
    {
        public string Type { get; set; }   // "VENUE" or "SUPPLIER"

        public List<CreateVenueDto>? Venues { get; set; }

        public List<CreateSupplierDto>? Suppliers { get; set; }
    }
    public class CreateSupplierDto
    {
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
    }
    public class GetVenueSupplierRequestDto
    {
        public string Type { get; set; }        // "VENUE" or "SUPPLIER"
        public string? Identifier { get; set; } // VenueName OR SupplierId
        public string? SearchTerm { get; set; }
        public int Index { get; set; } = 0;
        public int Limit { get; set; } = 10;
    }
    public class UpdateSupplierDto
    {
        public string? SupplierName { get; set; }
        public string? SupplierId { get; set; }
    }
    public class UpdateVenueDto
    {
        public string? VenueName { get; set; }
        public string? CompanyCode { get; set; }
        public string? CompanyName { get; set; }
        public string? CostCenter { get; set; }
        public string? GLAccount { get; set; }
        public string? ShipToPlant { get; set; }
        public string? PurchaseOrg { get; set; }
        public string? PurchaseGroup { get; set; }
        public string? BillTo { get; set; }
    }
    public class UpdateVenueSupplierDto
    {
        public string Type { get; set; }      // "VENUE" or "SUPPLIER"
        public string Identifier { get; set; } // VenueName OR SupplierId

        public UpdateVenueDto? Venue { get; set; }
        public UpdateSupplierDto? Supplier { get; set; }
    }


}
