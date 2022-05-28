using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UploadTransaction.Helpers;
using UploadTransaction.Model;

namespace UploadTransaction.Domain.BusinessLogicLayer.Impl
{
    public class TransactionBusinessLogic : ITransactionBusinessLogic
    {
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
                    var txnDetailLst = new List<TransactionDetailModel>();
                    if (Helper.CheckNullorEmptyDatatable(readData.Data))
                    {
                        if (readData.Data.AsEnumerable().Any(i => i != null))
                        {
                            if (extension == ".csv")
                            {
                                txnDetailLst = readData.Data.AsEnumerable()
                                            .Select((txn, index) => new TransactionDetailModel
                                            {
                                                SN = index + 1,
                                                TransactionID = Convert.ToString(txn["Transaction Identificator"]),
                                                Amount = Convert.ToString(txn["Amount"]),
                                                CurrencyCode = Convert.ToString(txn["Currency Code"]),
                                                TransactionDate = Convert.ToString(txn["Transaction Date"]),
                                                Status = Convert.ToString(txn["Status"])
                                            }
                                            ).ToList();
                            }
                            else
                            {
                                var column = readData.Data.Columns;
                                txnDetailLst = readData.Data.AsEnumerable()
                                            .Select((txn, index) => new TransactionDetailModel
                                            {
                                                SN = index + 1,
                                                TransactionID = Convert.ToString(txn["Transaction_Id"]),
                                                Amount = Convert.ToString(txn["Amount"]),
                                                CurrencyCode = Convert.ToString(txn["CurrencyCode"]),
                                                TransactionDate = Convert.ToString(txn["TransactionDate"]),
                                                Status = Convert.ToString(txn["Status"])
                                            }
                                            ).ToList();

                            }
                            respDataModel.lstTransactionDetailReview = txnDetailLst;
                        }
                        else
                        {
                            return new DataResponseModel<TransactionDetailReviewResponse>
                            {
                                RespCode = "012",
                                RespDescription = "Invalid Data",
                                Data = null
                            };
                        }
                    }
                    else
                    {
                        return new DataResponseModel<TransactionDetailReviewResponse>
                        {
                            RespCode = "012",
                            RespDescription = "No Entry to Show.",
                            Data = null
                        };
                    }

                }
                else
                {
                    return new DataResponseModel<TransactionDetailReviewResponse>
                    {
                        RespCode = "012",
                        RespDescription = readData.RespDescription,
                        Data = null
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
                DataTable dt = new DataTable();
                MemoryStream stream = new MemoryStream(reqModel.fileContent);
                using (StreamReader sr = new StreamReader(stream))
                {
                    string[] headers = sr.ReadLine().Split(',');
                    foreach (string header in headers)
                    {
                        dt.Columns.Add(header);
                    }
                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(',');
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i];
                        }
                        dt.Rows.Add(dr);
                    }

                    var columnList = headers.ToList();
                    var IsValid = CheckFileHeaderName(columnList);

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

                    return new ReadFileData
                    {
                        RespCode = "000",
                        RespDescription = "Success",
                        Data = dt
                    };

                }
            }
            catch (Exception)
            {
                return new ReadFileData
                {
                    RespCode = "012",
                    RespDescription = "Upload File can't read properly."
                };
            }
        }

        private static ReadFileData ReadXMLFile(FileModel reqModel)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                XmlDocument doc = new XmlDocument();
                MemoryStream stream = new MemoryStream(reqModel.fileContent);
                doc.Load(stream);
                var xmlReader = new XmlNodeReader(doc);
                ds.ReadXml(xmlReader);
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

                return new ReadFileData
                {
                    RespCode = "000",
                    RespDescription = "Success",
                    Data = dt
                };
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
    }
}
