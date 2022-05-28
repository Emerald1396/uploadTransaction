using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UploadTransaction.Model
{
    public class TransactionModel
    {
    }
    public class ApiResponseModel
    {
        public string RespCode { get; set; }
        public string RespDescription { get; set; }
        public ApiResponseModel() { }
        public ApiResponseModel(string respCode, string respDescription)
        {
            RespCode = respCode;
            RespDescription = respDescription;
        }
    }

    public class DataResponseModel
    {
        public string RespCode { get; set; }
        public string RespDescription { get; set; }
    }

    public class DataResponseModel<T> : DataResponseModel
    {
        public T Data { get; set; }
    }

    public class TransactionDetailReviewResponse
    {
       public List<TransactionDetailModel> lstTransactionDetailReview { get; set; }
    }

    public class TransactionDetailModel
    {
        public int SN { get; set; }
        public string ID { get; set; }
        public string TransactionID { get; set; }
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string TransactionDate { get; set; }
        public string Status { get; set; }
    }
}
