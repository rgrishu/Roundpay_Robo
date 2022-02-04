using System.Text;
using System.Text.RegularExpressions;

namespace Roundpay_Robo.Models
{
    public class LapuRechargeRequest
    {
        //USerID From tblUSers,Token From Tbl_Users,SPKEY Is OPID,Account Is RechargeMobile NO
        public int UserID { get; set; }
        public string Token { get; set; }
        public string SPKey { get; set; }
        public string Account { get; set; }
        public decimal Amount { get; set; }
        public int APIRequestID { get; set; }
    }
    public class _LapuRechargeRequest : LapuRechargeRequest
    {
        public string IPAddress { get; set; }
        public int RequestMode { get; set; }
        public int OID { get; set; }
        public string IMEI { get; set; }
        public int PromoCodeID { get; set; }
        public int CCFAmount { get; set; }
        public string PaymentMode { get; set; }
    }
    public class LapuRechargeResponse
    {
        public string ACCOUNT { get; set; }
        public decimal Amount { get; set; }
        public string RPID { get; set; }
        public int AGENTID { get; set; }
        public string OPID { get; set; }
        public int STATUS { get; set; }
        public string MSG { get; set; }
        public decimal LapuBAL { get; set; }
        public string ERRORCODE { get; set; }
        public string LapuNumber { get; set; }
    }
    public class ValidataRechargeApirequest
    {
        private string _sPKey;
        public int LoginID { get; set; }
        public string Token { get; set; }
        public string IPAddress { get; set; }
        public string SPKey { get => _sPKey ?? "SPKEY"; set => _sPKey = value; }
        public int OID { get; set; }
    }
    public class ValidataRechargeApiResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int OID { get; set; }
        public int OpType { get; set; }
        public int CircleValidationType { get; set; }
        public int LookupAPIID { get; set; }
        public string LookupReqID { get; set; }
        public string Operator { get; set; }
        public bool IsBBPS { get; set; }
        public bool IsBilling { get; set; }
        public string SPKey { get; set; }
        public string ErrorCode { get; set; }
        public string MobileU { get; set; }
        public string DBGeoCode { get; set; }
        public int OpGroupID { get; set; }
        public List<OperatorParams> OpParams { get; set; }
    }
    public class OperatorParams
    {
        public int Ind { get; set; }
        public string Param { get; set; }
        public string DataType { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string RegEx { get; set; }
        public string Remark { get; set; }
        public bool IsAccountNo { get; set; }
        public bool IsOptional { get; set; }
        public bool IsCustomerNo { get; set; }
        public bool IsDropDown { get; set; }
        public int OptionalID { get; set; }

        public bool IsErrorFound { get; set; }
        public string FormatedError { get; set; }

        public void GetFormatedError(string RequestKey, string RequestValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Please enter a valid {ParamKey} .");
            sb.Append(RequestKey ?? string.Empty);
            sb.Append(" [");
            sb.Append(Param ?? string.Empty);
            sb.Append("].");
            if (!string.IsNullOrEmpty(RegEx))
            {
                IsErrorFound = !Regex.IsMatch(RequestValue, RegEx);
            }
            if (MinLength > 0 && MaxLength > 0)
            {
                if (MinLength == MaxLength)
                {
                    sb.Append("Length should be ");
                    sb.Append(MinLength);
                    IsErrorFound = IsErrorFound == false ? RequestValue.Length != MinLength : IsErrorFound;
                }
                else
                {
                    sb.Append("Length should be between ");
                    sb.Append(MinLength);
                    sb.Append(" and ");
                    sb.Append(MaxLength);

                    IsErrorFound = IsErrorFound == false ? RequestValue.Length < MinLength || RequestValue.Length > MaxLength : IsErrorFound;
                }
            }
            else
            {
                if (MinLength > 0)
                {
                    sb.Append("Length is greater than or equal to ");
                    sb.Append(MinLength);
                    IsErrorFound = IsErrorFound == false ? RequestValue.Length < MinLength : IsErrorFound;
                }
                if (MaxLength > 0)
                {
                    sb.Append("Length is less than or equal to ");
                    sb.Append(MaxLength);
                    IsErrorFound = IsErrorFound == false ? RequestValue.Length > MaxLength : IsErrorFound;
                }
            }
            if (!string.IsNullOrEmpty(this.DataType))
            {
                if (this.DataType.ToUpper().Equals("NUMERIC"))
                {
                    sb.Append(" digit.");
                    IsErrorFound = IsErrorFound == false ? !Validators.Validate.O.IsNumeric(RequestValue) : IsErrorFound;
                }
                else
                {
                    sb.Append(" ");
                    sb.Append(this.DataType);
                    sb.Append(" characters.");
                    IsErrorFound = IsErrorFound == false ? !Validators.Validate.O.IsAlphaNumeric(RequestValue) && !RequestValue.Contains("-") && !RequestValue.Contains("_") && !RequestValue.Contains("/") : IsErrorFound;
                }
            }
            if (!string.IsNullOrEmpty(RegEx))
            {
                sb.Append("Refer regular expression ");
                sb.Append(RegEx);
            }
            FormatedError = sb.ToString();
        }
    }
}
