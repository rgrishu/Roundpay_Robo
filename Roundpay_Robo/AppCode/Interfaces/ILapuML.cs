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
       Task<Response> SaveVendor(VendorMaster vendormaster, LoginResponse _lr);
        Task<List<VendorMaster>> GetVendorList(LoginResponse _lr);
        Task<List<Lapu>> GetLapuList(LoginResponse _lr);
        Task<Response> LapuLogin(LapuLoginRequest lapulogireq, LoginResponse _lr, int LapuID);
        Task<Response> LapuBalance(LapuLoginRequest lapulogireq, LoginResponse _lr, int LapuID);
        Task<Response> ValidateOtp(ValidateLapuLoginOTP lapuotp, LoginResponse _lr, int LapuID);
        Task<LapuRechargeResponse> LappuApiRecharge(LapuRechargeRequest lapurecreq);


        Task<Response> SaveLapu(Lapu LapuUserDetail, LoginResponse _lr);
        Task<Response> DeleteLapu(int LapuID, LoginResponse _lr);
        Task<Lapu> GetEditLapulist(int LapuID, LoginResponse _lr);
        Task<Response> UpdateLapuStatus(int LapuID, LoginResponse _lr);
        Task<List<LapuReport>> GetLapuReport(LapuReport Filter, LoginResponse _lr);
        Task<Response> DeleteLapuVendor(int ID, LoginResponse _lr);
        Task<VendorMaster> SelectEditVendor(int ID, LoginResponse _lr);
        Task<List<Lapu>> GetVendorLapu(LoginResponse _lr);
        Task<List<LapuServices>> GetServices(LoginResponse _lr);
        Task<LapuReqRes> GetReqRes(int TID, int LapuID, LoginResponse _lr);
    }
}
