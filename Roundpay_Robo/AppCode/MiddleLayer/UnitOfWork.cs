using Roundpay_Robo.AppCode;
using Roundpay_Robo.AppCode.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Roundpay_Robo.AppCode.Interfaces;

namespace Roundpay_Robo.AppCode.MiddleLayer
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
  
        private IUserML user;
        private ILoginML login;


        public UnitOfWork(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
        }
     

        public IUserML userML
        {
            get
            {
                return user ?? (user = new UserML(_accessor, _env));
            }
        }

        public ILoginML loginML
        {
            get
            {
                return login ?? (login = new LoginML(_accessor, _env));
            }
        }

      
    }
}
