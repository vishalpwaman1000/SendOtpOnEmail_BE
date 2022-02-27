using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SendSmsApplication.CommonLayer.Model
{
    public class SendOtpOnEmailIDRequest
    {
        [Required]
        [RegularExpression("^[0-9a-zA-Z]+([._+-][0-9a-zA-Z]+)*@[0-9a-zA-Z]+.[a-zA-Z]{2,4}([.][a-zA-Z]{2,3})?$", ErrorMessage = "Email Id Not In Valid Formate")]
        public string EmailID { get; set; }
    }

    public class SendOtpOnEmailIDResponse
    {

        public bool IsSuccess { get; set; }
        public string Message { get; set; }

    }

    public class SendOtpOnEmailIDFunctionResponse
    {
        public bool IsSuccess { get; set; }
        public string message { get; set; }
    }
}
