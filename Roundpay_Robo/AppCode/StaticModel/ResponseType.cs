namespace Roundpay_Robo.AppCode.StaticModel
{
    public class ResponseType
    {
        public const int JSON = 1;
        public const int XML = 2;
        public const int CSV = 3;
        public const int STRING = 4;
        public const int OTHER = 5;
        public const int Delimiter = 6;
    }
    public class PostContentType {
        public const int x_www_form_urlencoded = 1;
        public const int application_json = 2;
        public const int query_string = 3;
    }
    public class APITypes
    {
        public const int General = 1;
        public const int Lapu = 2;
        public const int Real = 3;
        public const int Manual = 4;
        public static string GetAPIType(int APIType)
        {
            string _apiName = "";
            if (APIType == General)
            {
                _apiName = "General";
            }
            if (APIType == Lapu)
            {
                _apiName = "Lapu";
            }
            if (APIType == Real)
            {
                _apiName = "Real";
            }
            if (APIType == Manual)
            {
                _apiName = "Manual";
            }
            return _apiName;
        }
    }
    public class Replacement
    {
        public const string MOBILE = "{MOBILE}";
        public const string AMOUNT = "{AMOUNT}";
        public const string _AMOUNT = "{_AMOUNT}";
        public const string OPERATOR = "{OPERATOR}";
        public const string TID = "{TID}";
        public const string OPTIONAL1 = "{OPTIONAL1}";
        public const string OPTIONAL2 = "{OPTIONAL2}";
        public const string OPTIONAL3 = "{OPTIONAL3}";
        public const string OPTIONAL4 = "{OPTIONAL4}";
        public const string OUTLETID = "{OUTLETID}";
        public const string CUSTMOB = "{CUSTMOB}";
        public const string GEOCODE = "{GEOCODE}";
        public const string PINCODE = "{PINCODE}";
        public const string REFID = "{REFID}";
        public const string RECHTYPE = "{RECHTYPE}";
        public const string DATE = "{DATE}";
        public const string REPLACE = "{REPLACE}";
        public const string AccountKey = "{AccountKey}";
    }

}
