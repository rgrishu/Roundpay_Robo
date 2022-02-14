using Roundpay_Robo.Models;

namespace Roundpay_Robo.AppCode.Model
{
    public class CommonResponse
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public string CommonStr { get; set; }
        public int CommonInt { get; set; }
        public bool CommonBool { get; set; }
       

    }
    public class Response
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
    }
}
