namespace SmartReportLog.Model.Atm.Submit
{
    public sealed class AtmScanCassetteDto
    {
        public int I { get; set; }      // Id
        public long Dn { get; set; }    // Denomination
        public int Pk { get; set; }     // TotalPickup
        public int Dp { get; set; }     // TotalDispense
        public int Rj { get; set; }     // TotalReject
        public int Lk { get; set; }     // LastKnownCount
    }
}
