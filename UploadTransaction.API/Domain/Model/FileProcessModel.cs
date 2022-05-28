using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace UploadTransaction.Model
{
    public class FileProcessModel
    {
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
        public DataTable Data { get; set; }

        //public ReadFileData(string respCode, string respDescription, DataTable dt)
        //{
        //    RespCode = respCode;
        //    RespDescription = respDescription;
        //    Data = dt;
        //}
    }
    public class CheckHeaderNameResponse
    {
        public bool isFound { get; set; }
        public string remark { get; set; }
    }
}
