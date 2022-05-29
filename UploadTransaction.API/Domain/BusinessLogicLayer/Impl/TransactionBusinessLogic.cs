using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UploadTransaction.Domain.DataInfrastructure;
using UploadTransaction.Helpers;
using UploadTransaction.Model;

namespace UploadTransaction.Domain.BusinessLogicLayer.Impl
{
    public class TransactionBusinessLogic : ITransactionBusinessLogic
    {
        private TransactionDbContext _dbContext;

        public TransactionBusinessLogic(TransactionDbContext context)
        {
            _dbContext = context;
        }

        public async Task<DataResponseModel<TransactionDetailReviewResponse>> UploadPostFileAsync(FileModel reqModel)
        {
            var respDataModel = new TransactionDetailReviewResponse();

            try
            {
                #region Read Content 
                string extension = Path.GetExtension(reqModel.fileName).ToLower();
                var readData = new ReadFileData();
                if (extension == ".csv")
                {
                    readData = ReadCSVFile(reqModel);
                }
                else if (extension == ".xml")
                {
                    readData = ReadXMLFile(reqModel);
                }
                else
                {
                    return new DataResponseModel<TransactionDetailReviewResponse>
                    {
                        RespCode = "012",
                        RespDescription = "Invalid File Format.",
                        Data = null
                    };
                }

                #endregion

                #region Extract Data
                if(readData.RespCode == "000")
                {
                    respDataModel.lstTransactionDetailReview = readData.Data;
                }
                else
                {
                    Helper.loggingFile(reqModel.fileName, reqModel.fileContent);
                    return new DataResponseModel<TransactionDetailReviewResponse>
                    {
                        RespCode = "012",
                        RespDescription = readData.RespDescription,
                        Data = respDataModel
                    };
                }
                #endregion

                return new DataResponseModel<TransactionDetailReviewResponse>
                {
                    RespCode = "000",
                    RespDescription = "Success",
                    Data = respDataModel
                };
            }
            catch (Exception ex)
            {
                return new DataResponseModel<TransactionDetailReviewResponse>
                {
                    RespCode = "012",
                    RespDescription = "Something went wrong [" + ex.Message + "]"
                };
            }
        }

        private static ReadFileData ReadCSVFile(FileModel reqModel)
        {
            try
            {
                MemoryStream stream = new MemoryStream(reqModel.fileContent);
                var configuration = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.CurrentCulture)
                {
                    HasHeaderRecord = true
                };

                using (var reader = new StreamReader(stream))
                {
                    using (var csvReader = new CsvReader(reader, configuration))
                    {
                        csvReader.Read();
                        csvReader.ReadHeader();

                        var headerRow = csvReader.HeaderRecord.ToList();
                        var IsValid = CheckFileHeaderName(headerRow);
                        if (!IsValid.isFound)
                            return new ReadFileData
                            {
                                RespCode = "012",
                                RespDescription = IsValid.remark
                            };

                        var lstTxnDetail = csvReader.GetRecords<TransactionDetailModel>().ToList();
                        if (lstTxnDetail.Count == 0)
                            return new ReadFileData
                            {
                                RespCode = "012",
                                RespDescription = "Only Header Found , Please fill Record."
                            };

                        foreach (var txn in lstTxnDetail)
                        {
                            var validateRes = ValidateFields(txn,"csv");
                            if (!validateRes.isValid)
                            {
                                return new ReadFileData
                                {
                                    RespCode = "012",
                                    RespDescription = validateRes.remark
                                };
                            }
                        }
                        return new ReadFileData
                        {
                            RespCode = "000",
                            RespDescription = "Success",
                            Data = lstTxnDetail
                        };
                    }
                }

                //using (StreamReader reader= new StreamReader(stream))
                //{
                //    string[] headers = sr.ReadLine().Split(',');
                //    foreach (string header in headers)
                //    {
                //        dt.Columns.Add(header);
                //    }
                //    while (!sr.EndOfStream)
                //    {
                //        string[] rows = sr.ReadLine().Split(',');
                //        DataRow dr = dt.NewRow();
                //        for (int i = 0; i < headers.Length; i++)
                //        {
                //            dr[i] = rows[i];
                //        }
                //        dt.Rows.Add(dr);
                //    }
                //}
            }
            catch (Exception ex)
            {
                return new ReadFileData
                {
                    RespCode = "012",
                    RespDescription = "Upload File can't read properly [" + ex.Message + "]"
                };
            }
        }

        private static ReadFileData ReadXMLFile(FileModel reqModel)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                MemoryStream stream = new MemoryStream(reqModel.fileContent);
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = false;
                using (var reader = new StreamReader(stream))
                {
                    doc.Load(reader);
                    ds.ReadXml(new XmlNodeReader(doc));

                    dt = ds.Tables[0];
                    var dtPayment = ds.Tables[1];

                    dt.Merge(dtPayment);

                    var headerColumns = from c in dt.Columns.Cast<DataColumn>()
                                        select c.ColumnName;

                    var IsValid = CheckFileHeaderNameXML(headerColumns.ToList());
                    if (!IsValid.isFound)
                        return new ReadFileData
                        {
                            RespCode = "012",
                            RespDescription = IsValid.remark
                        };

                    if (dt.Rows.Count == 0)
                        return new ReadFileData
                        {
                            RespCode = "012",
                            RespDescription = "Only Header Found , Please fill Record."
                        };

                    var lstTxnDetail = dt.AsEnumerable().Select((txn, index) => new TransactionDetailModel
                                                        {
                                                            TransactionID = Convert.ToString(txn["id"]),
                                                            Amount = Convert.ToString(txn["Amount"]),
                                                            CurrencyCode = Convert.ToString(txn["CurrencyCode"]),
                                                            TransactionDate = Convert.ToString(txn["TransactionDate"]),
                                                            Status = Convert.ToString(txn["Status"])
                                                        }).ToList();

                    foreach (var txn in lstTxnDetail)
                    {
                        var validateRes = ValidateFields(txn, "xml");
                        txn.TransactionDate = Convert.ToDateTime(txn.TransactionDate).ToString("dd/MM/yyyy hh:mm:ss");
                        if (!validateRes.isValid)
                        {
                            return new ReadFileData
                            {
                                RespCode = "012",
                                RespDescription = validateRes.remark
                            };
                        }
                    }


                    return new ReadFileData
                    {
                        RespCode = "000",
                        RespDescription = "Success",
                        Data = lstTxnDetail
                    };
                }
            }
            catch (Exception ex)
            {
                return new ReadFileData
                {
                    RespCode = "012",
                    RespDescription = "Upload File can't read properly [" + ex.Message + "]"
                };
            }
        }

        private static CheckHeaderNameResponse CheckFileHeaderName(List<string> headers)
        {
            var resp = new CheckHeaderNameResponse();

            List<string> fileHeader = new List<string> { "Transaction Identificator", "Amount", "Currency Code", "Transaction Date", "Status"};

            foreach (var header in fileHeader)
            {
                var found = headers.Where(x => x.Trim() == header.Trim()).FirstOrDefault();

                if (found == null && (header == "Transaction Identificator" || header == "Amount" || header == "Currency Code" || header == "Transaction Date" || header == "Status"))
                {
                    resp.isFound = false;
                    resp.remark = "Some header name are missing! Please check your template file.";
                    return resp;
                }
                else if (found == null && !(header == "Transaction Identificator" || header == "Amount" || header == "Currency Code" || header == "Transaction Date" || header == "Status"))
                {
                    resp.isFound = false;
                    resp.remark = "File header names are not math.";
                    return resp;
                }
                else
                {
                    resp.isFound = true;
                }

            }
            return resp;
        }

        private static CheckHeaderNameResponse CheckFileHeaderNameXML(List<string> headers)
        {
            var resp = new CheckHeaderNameResponse();

            List<string> fileHeader = new List<string> { "Transaction_Id", "Amount", "CurrencyCode", "TransactionDate", "Status" };

            foreach (var header in fileHeader)
            {
                var found = headers.Where(x => x.Trim() == header.Trim()).FirstOrDefault();

                if (found == null && (header == "Transaction_Id" || header == "Amount" || header == "CurrencyCode" || header == "TransactionDate" || header == "Status"))
                {
                    resp.isFound = false;
                    resp.remark = "Some header name are missing! Please check your template file.";
                    return resp;
                }
                else if (found == null && !(header == "Transaction_Id" || header == "Amount" || header == "CurrencyCode" || header == "TransactionDate" || header == "Status"))
                {
                    resp.isFound = false;
                    resp.remark = "File header names are not math.";
                    return resp;
                }
                else
                {
                    resp.isFound = true;
                }

            }
            return resp;
        }

        private static validateFieldResponse ValidateFields(TransactionDetailModel txnReview , string fileType)
        {
            decimal amount = 0;

            if (txnReview.TransactionID?.Length > 50 )
            {
                return new validateFieldResponse(false, "Exceed Maximum length in TransactionID");
            }

            else if (!decimal.TryParse(txnReview.Amount, out amount))
            {
                return new validateFieldResponse(false, "Invalid Amount");
            }

            else if (!string.IsNullOrEmpty(txnReview.TransactionDate))
            {
                var dateFormat = string.Empty;
                if (fileType == "csv")
                {
                    dateFormat = "dd/MM/yyyy hh:mm:ss";
                }
                else
                {
                    dateFormat = "yyyy-MM-ddTHH:mm:ss";
                }
                DateTime tempDate;
                bool validDate = DateTime.TryParseExact(txnReview.TransactionDate, dateFormat, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out tempDate);
                if (!validDate)
                {
                    return new validateFieldResponse(false, "Invalid Transaction Date");
                }
            }

            return new validateFieldResponse(true, string.Empty);
        }
        public async Task<ApiResponseModel> SaveTransactionHistoryAsync(SaveTransactionHistoryRequestModel requestModel)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                foreach (var txn in requestModel.lstTransactionDetail)
                {
                    var txndetail = new TransactionHistoryTableModel
                    {
                        TransactionID = txn.TransactionID,
                        Currency = txn.CurrencyCode,
                        TransactionDate =  DateTime.ParseExact(txn.TransactionDate, "dd/MM/yyyy hh:mm:ss", CultureInfo.InvariantCulture),
                        Amount = decimal.Parse(txn.Amount),
                        Status = txn.Status == "Approved" ? "A" :
                                 (txn.Status == "Failed" || txn.Status == "Rejected") ? "R" :
                                 (txn.Status == "Finished" || txn.Status == "Done") ? "D" :
                                 txn.Status,
                        UpdatedDate = DateTime.Now,
                        CreatedBy = 0
                    };
                    await _dbContext.transactionHistory.AddAsync(txndetail);
                }
                await _dbContext.SaveChangesAsync();
                transaction.Commit();
                return new ApiResponseModel { RespCode = "000", RespDescription = "Success" };
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                return new ApiResponseModel { RespCode = "012", RespDescription = "Fail to save transaction [" + ex.Message + "]" };
            }
            
        }

        public async Task<DataResponseModel<GetTransacionHistoryByFilterResponseModel>> GetTransactionListByFilterAsync(GetTransacionlistByFilterRequestModel requestModel)
        {
            var respDataModel = new GetTransacionHistoryByFilterResponseModel();
            var transctionHistory = await _dbContext.transactionHistory
                                           .Where(x => x.TransactionDate >= requestModel.StartDate && x.TransactionDate <= requestModel.EndDate
                                           && (String.IsNullOrEmpty(requestModel.Status) || x.Status == requestModel.Status)
                                           && (String.IsNullOrEmpty(requestModel.Currency) || x.Currency == requestModel.Currency))
                                           .Select(x => new TransacionHistoryModel
                                           {
                                               id = x.TransactionID,
                                               payment = x.Amount.ToString("0.00") + " " + x.Currency.ToString(),
                                               Status = x.Status
                                           }).ToListAsync();

            if (transctionHistory.Count > 0)
            {
                respDataModel.lstTransactionHistory = transctionHistory;
                return new DataResponseModel<GetTransacionHistoryByFilterResponseModel>
                {
                    RespCode = "000",
                    RespDescription = "Success",
                    Data = respDataModel
                };
            }
            else
            {
                return new DataResponseModel<GetTransacionHistoryByFilterResponseModel>
                {
                    RespCode = "012",
                    RespDescription = "No Record To Show",
                    Data = null
                };
            }

        }
    }
}
