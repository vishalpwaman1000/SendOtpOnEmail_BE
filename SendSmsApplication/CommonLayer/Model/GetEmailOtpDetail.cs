using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendSmsApplication.CommonLayer.Model
{
    public class GetEmailOtpDetailRequest
    {
        public int PageNumber { get; set; }
        public int RecordPerPage { get; set; } 
    }
    public class GetEmailOtpDetailResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<GetEmailOtpDetail> getEmailOtpDetails { get; set; } 
    }

    public class GetEmailOtpDetail
    {
        public int UserID { get; set; }
        public string EmailID { get; set; }
        public int OtpGenerateCount { get; set; }
        public string Date { get; set; }
    }
}
