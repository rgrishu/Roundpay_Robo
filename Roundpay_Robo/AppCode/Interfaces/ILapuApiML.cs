using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Microsoft.AspNetCore.Http;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roundpay_Robo.AppCode.Interfaces
{
    public interface ILapuApiML
    {
        Task<Response> LapuApiLogin(LapuLoginRequest lapuloginreq, int UserID, int LapuID, bool FromBalance = false);
        Task<Response> LapuApiBalance(LapuLoginRequest lapuloginreq, LoginResponse _lr, int LapuID);
        Task<Response> LapuLoginOtpValidate(ValidateLapuLoginOTP lapuloginotpval, LoginResponse _lr, int LapuID);
        Task<string> InitiateTransaction(InitiateTransaction initiatetransaction, int USerID, int LapuID, int TID, int SleepTime);
    }
}
