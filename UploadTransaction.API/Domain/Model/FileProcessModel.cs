using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadTransaction.Model
{
    public class FileProcessModel
    {
    }

    public class postFileRequestModel
    {
        public string fileName { get; set; }
        public IFormFile file { get; set; }
    }

    public class FileModel 
    {
        public string fileName { get; set; }
        public long fileSize { get; set; }
        public byte[] fileContent { get; set; }
    }

    public class ReadFileData
    {
        public string RespCode { get; set; }
        public string RespDescription { get; set; }
        public List<TransactionDetailModel> Data { get; set; }
    }
    public class CheckHeaderNameResponse
    {
        public bool isFound { get; set; }
        public string remark { get; set; }
    }

    public class validateFieldResponse
    {
        public bool isValid { get; set; }
        public string remark { get; set; }

        public validateFieldResponse(bool valid, string issue)
        {
            isValid = valid;
            remark = issue;
        }
    }
}
