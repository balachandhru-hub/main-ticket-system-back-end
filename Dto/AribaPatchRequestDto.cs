using Newtonsoft.Json;
public class AribaPatchRequestDto
{
    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("comment")]
    public AribaCommentDto Comment { get; set; }
   
    [JsonIgnore] 
    public decimal? ApprovedInvoiceAmount { get; set; }
}

public class AribaCommentDto
{
    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("visibleToSupplier")]
    public bool VisibleToSupplier { get; set; }
}

public class PatchDetailsDto
{
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("contractvalue")]
    public decimal? ContractValue { get; set; }
    [JsonProperty("valuealreadypaid")]
    public decimal? ValueAlreadyPaid { get; set; }
    [JsonProperty("remainingbalance")]
    public decimal? RemainingBalance { get; set; }
}
public class PatchContractDetailsDto
{
      [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("contractvalue")]
    public decimal? ContractValue { get; set; }
}
