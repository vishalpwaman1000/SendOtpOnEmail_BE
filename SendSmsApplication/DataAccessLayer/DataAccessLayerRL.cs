using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using SendSmsApplication.CommonLayer.Model;
using SendSmsApplication.CommonUtility;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SendSmsApplication.DataAccessLayer
{
    public class DataAccessLayerRL : IDataAccessLayer
    {
        public readonly IConfiguration _configuration;
        public readonly SqlConnection _sqlConnection;
        public string Html = string.Empty;

        public DataAccessLayerRL(IConfiguration configuration)
        {
            _configuration = configuration;
            _sqlConnection = new SqlConnection(_configuration["ConnectionStrings:SqlServerConnectionString"]);

        }

        public async Task<SendOtpOnEmailIDResponse> SendOTPOnEmailId(SendOtpOnEmailIDRequest request)
        {
            SendOtpOnEmailIDResponse response = new SendOtpOnEmailIDResponse();
            SendOtpOnEmailIDFunctionResponse SendEmailResponse = new SendOtpOnEmailIDFunctionResponse();
            response.IsSuccess = true;
            response.Message = "OTP Send SuccessFully";

            try
            {
                int NewOtp = CreateOtp(); // Create Otp

                SendEmailResponse = SendEmailFunction(request.EmailID, NewOtp); // Send Sms
                if (!SendEmailResponse.IsSuccess)
                {
                    response.IsSuccess = false;
                    response.Message = SendEmailResponse.message;
                    return response;
                }

                string StoreProcedure = _configuration["StoreProcedure:SendOtpViaEmail"];
                using (SqlCommand sqlCommand = new SqlCommand(StoreProcedure, _sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandTimeout = 180;
                    sqlCommand.Parameters.AddWithValue("@EmailID", request.EmailID);
                    sqlCommand.Parameters.AddWithValue("@NewOtp", NewOtp);
                    await _sqlConnection.OpenAsync();
                    int Status = await sqlCommand.ExecuteNonQueryAsync();
                    if (Status <= 0)
                    {
                        response.IsSuccess = false;
                        response.Message = "Query Not Executed";
                        return response;
                    }
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            finally
            {
                await _sqlConnection.CloseAsync();
            }

            return response;
        }

        public int CreateOtp()
        {
            int OTP = 0;
            try
            {
                Random rnd = new Random();
                OTP = rnd.Next(100000, 1000000); // MinValue >= 100000 & MaxValue <= 999999
            }
            catch (Exception ex)
            {

            }
            return OTP;
        }

        public SendOtpOnEmailIDFunctionResponse SendEmailFunction(string ToEmailID, int OtpNumber)
        {
            SendOtpOnEmailIDFunctionResponse response = new SendOtpOnEmailIDFunctionResponse();
            response.IsSuccess = true;
            response.message = "SuccessFul";
            try
            {
                string FromEmailID = _configuration["EmailAuthentication:FromEmailId"];
                string EmailPassword = _configuration["EmailAuthentication:EmailPassword"];
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(FromEmailID);
                message.To.Add(new MailAddress(ToEmailID));
                message.Subject = "Verify Your OTP";
                message.IsBodyHtml = true; //to make message body as html  
                message.Body = ReturnHtmlBody(OtpNumber);
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com"; //for gmail host  
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(FromEmailID, EmailPassword);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                 smtp.Send(message);
            }
            catch (Exception ex) 
            {
                response.IsSuccess = false;
                response.message = ex.Message;
            }
            return response;
        }

        public async Task<OTpVarificationResponse> OTpVarification(OTpVarificationRequest request)
        {
            OTpVarificationResponse response = new OTpVarificationResponse();
            response.IsSuccess = true;
            response.Message = "Otp Varification Successful";
            try
            {

                if (_sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _sqlConnection.OpenAsync();
                }

                using (SqlCommand sqlCommand = new SqlCommand(SqlQueries.OTpVarification, _sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    sqlCommand.CommandTimeout = 180;
                    sqlCommand.Parameters.AddWithValue("@EmailID", request.EmailId);
                    sqlCommand.Parameters.AddWithValue("@Otp", request.Otp);

                    using (SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                    {
                        if (sqlDataReader.HasRows)
                        {
                            return response;
                        }
                        else
                        {
                            response.IsSuccess = false;
                            response.Message = "Otp Verification Failed. Please Enter Valid OTP.";
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            finally
            {
                await _sqlConnection.CloseAsync();
                await _sqlConnection.DisposeAsync();
            }

            return response;
        }

        public async Task<GetEmailOtpDetailResponse> GetEmailOtpDetail(GetEmailOtpDetailRequest request)
        {
            GetEmailOtpDetailResponse response = new GetEmailOtpDetailResponse();
            response.IsSuccess = true;
            response.Message = "Successful";

            try
            {

                if (_sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _sqlConnection.OpenAsync();
                }

                using (SqlCommand sqlCommand = new SqlCommand(SqlQueries.GetEmailOtpDetail, _sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    sqlCommand.CommandTimeout = 180;
                    sqlCommand.Parameters.AddWithValue("@Limit", (request.PageNumber - 1) * request.RecordPerPage);
                    sqlCommand.Parameters.AddWithValue("@RecordPerPage", request.RecordPerPage);
                    using (SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                    {
                        if (sqlDataReader.HasRows)
                        {
                            response.getEmailOtpDetails = new System.Collections.Generic.List<GetEmailOtpDetail>();
                            int Count = 0;
                            while (await sqlDataReader.ReadAsync())
                            {
                                GetEmailOtpDetail getDetail = new GetEmailOtpDetail();
                                getDetail.UserID = sqlDataReader["UserId"] != DBNull.Value ? Convert.ToInt32(sqlDataReader["UserId"]) : 0;
                                getDetail.EmailID = sqlDataReader["EmailID"] != DBNull.Value ? Convert.ToString(sqlDataReader["EmailID"]) : string.Empty;
                                getDetail.Date = sqlDataReader["UpdateDate"] != DBNull.Value ? Convert.ToDateTime(sqlDataReader["UpdateDate"]).ToString("dd'-'MMM'-'yyyy") : string.Empty;
                                if (String.IsNullOrEmpty(getDetail.Date))
                                {
                                    getDetail.Date = sqlDataReader["InsertionDate"] != DBNull.Value ? Convert.ToDateTime(sqlDataReader["InsertionDate"]).ToString("dd'-'MMM'-'yyyy") : string.Empty;
                                }
                                getDetail.OtpGenerateCount = sqlDataReader["OtpCount"] != DBNull.Value ? Convert.ToInt32(sqlDataReader["OtpCount"]) : 0;
                                if (Count == 0)
                                {
                                    Count++;
                                    double TotalRecord = sqlDataReader["TotalRecord"] != DBNull.Value ? Convert.ToInt32(sqlDataReader["TotalRecord"]) : 0;
                                    response.TotalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(TotalRecord / request.RecordPerPage)));
                                    response.CurrentPage = request.PageNumber;
                                }
                                response.getEmailOtpDetails.Add(getDetail);
                            }
                        }
                        else
                        {
                            response.IsSuccess = false;
                            response.Message = "Record Not Found";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            finally
            {
                await _sqlConnection.CloseAsync();
                await _sqlConnection.DisposeAsync();
            }
            return response;
        }

        public string ReturnHtmlBody(int Otp_Number)
        {
            Html = @"
                <html>
                    <head>
                        <style>
                        .container {
                            height: 100%;
                            width: 100%;
                            display: flex;
                            justify-content: center;
                            align-items: center;
                        }

                        .subContainer {
                            height: 70%;
                            width: 45%;
                        }

                        .google {
                            font-size: 26px;
                            height: 15%;
                            width: 100%;
                            display: flex;
                            align-items: center;
                            font-family: Helvetica;
                            font-weight: 400;
                        }

                        .body {
                            height: 85%;
                            width: 605px;
                            font-family: Helvetica;
                            font-size: 13px;
                            box-shadow: none;
                            border-radius: 0 0 3px 3px;
                        }

                        .header {
                            background-color: #4184f3;
                            height: 30%;
                            width: 99.5%;
                            margin-left: 0.25%;
                            border-radius: 3px 3px 0 0;
                        }

                        .h2 {
                            height: 100%;
                            font-size: 24px;
                            color: #ffffff;
                            font-weight: 400;
                            font-family: Helvetica;
                            margin: 0 0 0 40px;
                            display: flex;
                            align-items: center;
                        }
                        .subBody {
                            background-color: #fafafa;
                            height: 289px;
                            min-width: 332px;
                            max-width: 753px;
                            border: 1px solid #f0f0f0;
                            border-bottom: 1px solid #c0c0c0;
                        }
                        .innersubBody {
                            width: 86%;
                            height: 72%;
                            margin: 6% 6%;
                        }
                        .otp {
                            font-size: 24px;
                            font-weight: 600;
                            margin: 0 40%;
                        }
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                        <div class='subContainer'>
                            <div class='google'>
                                <!-- <span style = 'color: #4285f4' > G </ span >
                                < span style='color: #db4437'>o</span>
                                <span style = 'color: #f4b400' > o </ span >
                                < span style='color: #4285f4'>g</span>
                                <span style = 'color: #0f9d58' > l </ span >
                                < span style='color: #db4437'>e</span> -->
                            </div>
                            <div class='body'>
                            <div class='header' style='height: 120px'>
                                <div class='h2' style='height: 120px; padding: 40px 0'>
                                V Tech Verification Code
                                </div>
                            </div>
                            <div class='subBody'>
                                <div class='innersubBody'>
                                <div>
                                    This verification code was sent to your email for help getting
                                    back into a V Tech Account:
                                </div>
                                <br />
                                <div class='otp'>"
                   + Otp_Number +
                   @"</div>
                                <br />
                                <div>Don’t know why you received this?</div>
                                <br />
                                <div>
                                    Someone who couldn’t remember their V Tech Account details
                                    probably gave your email address by mistake. You can safely
                                    ignore this email.
                                </div>
                                <br />
                                <div>
                                    To protect your account, don’t forward this email or give this
                                    code to anyone.
                                </div>
                                <br />
                                <div>V Tech Accounts team</div>
                                <br />
                                </div>
                            </div>
                            </div>
                        </div>
                        </div>
                    </body>
                    </html>";

            return Html;
        }
    }
}
