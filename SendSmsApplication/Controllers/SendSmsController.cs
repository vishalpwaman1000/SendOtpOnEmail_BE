using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SendSmsApplication.CommonLayer.Model;
using SendSmsApplication.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendSmsApplication.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class SendSmsController : ControllerBase
    {
        public readonly IDataAccessLayer _dataAccessLayer;
        public SendSmsController(IDataAccessLayer dataAccessLayer)
        {
            _dataAccessLayer = dataAccessLayer;
        }

        [HttpPost]
        public async Task<IActionResult> SendOTPOnEmailId(SendOtpOnEmailIDRequest request)
        {
            SendOtpOnEmailIDResponse response = new SendOtpOnEmailIDResponse();
            try
            {

                response = await _dataAccessLayer.SendOTPOnEmailId(request);

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> OtpVarification(OTpVarificationRequest request)
        {
            OTpVarificationResponse response = new OTpVarificationResponse();

            try
            {
                response = await _dataAccessLayer.OTpVarification(request);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> GetEmailOtpDetail(GetEmailOtpDetailRequest request)
        {
            GetEmailOtpDetailResponse response = new GetEmailOtpDetailResponse();

            try
            {
                response = await _dataAccessLayer.GetEmailOtpDetail(request);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return Ok(response);
        }

    }
}
