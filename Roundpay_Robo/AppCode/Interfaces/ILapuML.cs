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
    public interface ILapuML
    {
       Task<CommonResponse> SaveVendor(VendorMaster vendormaster, LoginResponse _lr);
        Task<List<VendorMaster>> GetVendorList(LoginResponse _lr);
        Task<List<Lapu>> GetLapuList(LoginResponse _lr);
        Task<CommonResponse> LapuLogin(LapuLoginRequest lapulogireq, LoginResponse _lr, int LapuID);
        Task<CommonResponse> LapuBalance(LapuLoginRequest lapulogireq, LoginResponse _lr, int LapuID);
        Task<CommonResponse> ValidateOtp(ValidateLapuLoginOTP lapuotp, LoginResponse _lr, int LapuID);
        Task<LapuRechargeResponse> LappuApiRecharge(LapuRechargeRequest lapurecreq);


        Task<CommonResponse> SaveLapu(Lapu LapuUserDetail, LoginResponse _lr);
        Task<CommonResponse> DeleteLapu(int LapuID, LoginResponse _lr);
        Task<Lapu> GetEditLapulist(int LapuID, LoginResponse _lr);
        Task<CommonResponse> UpdateLapuStatus(int LapuID, LoginResponse _lr);
        Task<List<LapuReport>> GetLapuReport(LapuReport Filter, LoginResponse _lr);
        Task<CommonResponse> DeleteLapuVendor(int ID, LoginResponse _lr);
        Task<VendorMaster> SelectEditVendor(int ID, LoginResponse _lr);
        Task<List<Lapu>> GetVendorLapu(LoginResponse _lr);
        Task<List<LapuServices>> GetServices(LoginResponse _lr);
        Task<LapuReqRes> GetReqRes(int TID, int LapuID, LoginResponse _lr);
        Task<LapuApiTransactionRecord> LapuTransactioDataFromAPi(LapuApiTransacrionReq lapuapitransacrionreq, int UserID, int LapuID);
        Task<LapuProcUpdateTranResposne> UpdateTransaction(LapuTransaction ltr, bool IsCallback = false, bool IsFromApi = false);
         Task<Response> LapuTransactions();
    }
}
