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
    public interface IUserML
    {

  
        ResponseStatus CallSignup(UserCreate _req);
       
        
    }
}
