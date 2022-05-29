using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

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
        [Name("Transaction Identificator")]
        public string TransactionID { get; set; }
        public string Amount { get; set; }
        [Name("Currency Code")]
        public string CurrencyCode { get; set; }
        [Name("Transaction Date")]
        public string TransactionDate { get; set; }
        public string Status { get; set; }
    }

    public class SaveTransactionHistoryRequestModel
    {
        public List<TransactionDetailReviewModel> lstTransactionDetail { get; set; }
    }

    public class TransactionDetailReviewModel
    {
        public string TransactionID { get; set; }
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string TransactionDate { get; set; }
        public string Status { get; set; }
    }

    public class GetTransacionlistByFilterRequestModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
    }

    public class GetTransacionHistoryByFilterResponseModel
    {
        public List<TransacionHistoryModel> lstTransactionHistory { get; set; }
    }

    public class TransacionHistoryModel
    {
        public string id { get; set; }
        public string payment { get; set; }
        public string Status { get; set; }
    }
}
