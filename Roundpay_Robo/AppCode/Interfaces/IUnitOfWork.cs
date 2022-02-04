using Roundpay_Robo.AppCode.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roundpay_Robo.AppCode.Interfaces
{
    public interface IUnitOfWork
    {
    
        IUserML userML { get; }
        ILoginML loginML { get; }
    }
}
