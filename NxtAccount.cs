namespace NxtExchange
{
    public class NxtAccount : NxtAddress
    {
        public string SecretPhrase { get; set; }
        public long BalanceNqt { get; set; }
    }
}
