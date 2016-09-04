namespace NxtExchange
{
    public class NxtAccount : NxtAddress
    {
        public bool IsMainAccount { get; set; }
        public string SecretPhrase { get; set; }
    }
}
