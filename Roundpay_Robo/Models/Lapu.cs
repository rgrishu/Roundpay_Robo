namespace Roundpay_Robo.Models
{
    public class Lapu
    {
        public int LapuID { get; set; }
        public int UserID { get; set; }
        public int VendorID { get; set; }
        public string VendorName { get; set; }
        public string OtherVendorName { get; set; }
        public int ProviderID { get; set; }
        public string ProviderName { get; set; }
        public string LapuNickName { get; set; }
        public string LapuNo { get; set; }
        public string LapuUserID { get; set; }
        public string Password { get; set; }
        public string Pin { get; set; }
        public string ProviderSessionID { get; set; }
        public string ProviderTokenID { get; set; }
        public decimal LapuBalance { get; set; }
        public bool LapuStatus { get; set; }
        public int LapuLoginStatus { get; set; }
        public DateTime EntryDate { get; set; }
        public bool IsDeleted { get; set; }
        public int LapuTypeID { get; set; }
    }
    public class LapuLoginRequest
    {
        public string mobile { get; set; }
        public string password { get; set; }
        public string token { get; set; }

        public string access_token { get; set; }
    }

    public class LapuAppSetting
    {

        public string Token { get; set; }
        public string URL { get; set; }
    }
    public class Category
    {
        public string id { get; set; }
        public List<string> modules { get; set; }
    }

    public class Data
    {
        public string feSessionId { get; set; }
        public string token { get; set; }
        public string currentBal { get; set; }
        public string Name { get; set; }
        public string status { get; set; }
        public bool isBdayWeek { get; set; }
        public bool isBdayDay { get; set; }
        public string lastLogin { get; set; }
        public string actorType { get; set; }
        public string actorCreationDate { get; set; }
        public string actorCategory { get; set; }
        public string codCategory { get; set; }
        public List<Category> categories { get; set; }
        public List<string> quickModules { get; set; }
        public string locationTL { get; set; }
        public string agentId { get; set; }
        public string npciAgentId { get; set; }
        public string shopLat { get; set; }
        public string shopLong { get; set; }
        public string pinCode { get; set; }
        public bool isSkipAvailable { get; set; }
        public bool isFirstLogin { get; set; }
        public bool isTrainingComplete { get; set; }
        public int skipCount { get; set; }
        public int trainingDayLeft { get; set; }
        public bool freezeFlag { get; set; }
        public string accountNumber { get; set; }
        public string messageText { get; set; }
        public string code { get; set; }
        public string errorCode { get; set; }
        public string appVersion { get; set; }
        public string voltTxnId { get; set; }
        public decimal balAfterTxn { get; set; }
    }

    public class LapuLoginResponse
    {
        public string Resp_code { get; set; }
        public string Resp_desc { get; set; }
        public Data data { get; set; }
    }


    public class ValidateLapuLoginOTP
    {
        public string token { get; set; }
        public string mobile { get; set; }
        public string otpval { get; set; }
        public string verificationToken { get; set; }
    }

    public class LapuError
    {
        public string Resp_code { get; set; }
        public string Resp_desc { get; set; }
    }


    public class LapuApiReqResForDB
    {
        public int UserID { get; set; }
        public int TID { get; set; }
        public int LapuID { get; set; }
        public string URL { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string ClassName { get; set; }
        public string Method { get; set; }
    }

    public class LapuTransaction : Lapu
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public string Amount { get; set; }
        public int TID { get; set; }
        public string Mobile { get; set; }
        public string ApiOPCode { get; set; }
        public string Category { get; set; }
        public int Type { get; set; }
        public string TransactionID { get; set; }

        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public decimal Balance { get; set; }
        public string LiveID { get; set; }
        public string Account { get; set; }
        public string LapuRechargeNumber { get; set; }
        public bool IsExistAPIRequestID { get; set; }
        public decimal BalanceAmount { get; set; }
    }
    public class InitiateTransaction
    {
        public string token { get; set; }
        public string access_token { get; set; }
        public string mpin { get; set; }
        public string amount { get; set; }
        public string mobile { get; set; }
        public string biller { get; set; }
        public string category { get; set; }
        public string lat { get; set; }
        public string @long { get; set; }
    }

    public class LapuReport
    {
        public int Top { get; set; }
        public int LapuID { get; set; }
        public int UserID { get; set; }
        public string TransactionID { get; set; }
        public string AccountNo { get; set; }
        public string OutletName { get; set; }
        public string Provider { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public string LapuOpening { get; set; }
        public decimal RechargeAmount { get; set; }
        public string LapuClosing { get; set; }
        public string LapuNo { get; set; }
        public string LiveID { get; set; }        
        public int Type { get; set; }
        public int TID { get; set; }
        public string TransactionStatus { get; set; }
    }
    public class LapuServices : Lapu
    {
        public int ID { get; set; }
        public int LapuTypeID { get; set; }
        public int SID { get; set; }
        public string Name { get; set; }
    }
    public class LapuReqRes: Lapu
    {
        public int LapuID { get; set; }
        public int UserID { get; set; }
        public string URL { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
    }


    public class SeviceVendorVM
    {
        public List<Lapu> listLAPU { get; set; }
        public List<LapuServices> listLapuServices { get; set; }
    }
}
