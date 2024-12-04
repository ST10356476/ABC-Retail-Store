namespace ABC_Retail.Models
{
    public class Contract
    {
        public string ContractId { get; set; }
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
        public string FileUrl { get; set; } // URL to access the file in Azure Files
    }

}
