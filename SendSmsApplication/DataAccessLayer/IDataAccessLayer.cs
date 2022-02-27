using SendSmsApplication.CommonLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendSmsApplication.DataAccessLayer
{
    public interface IDataAccessLayer
    {
        public Task<SendOtpOnEmailIDResponse> SendOTPOnEmailId(SendOtpOnEmailIDRequest request);

        public Task<OTpVarificationResponse> OTpVarification(OTpVarificationRequest request);

        public Task<GetEmailOtpDetailResponse> GetEmailOtpDetail(GetEmailOtpDetailRequest request);
    }
}
