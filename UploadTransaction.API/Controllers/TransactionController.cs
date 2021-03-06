using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UploadTransaction.Domain.BusinessLogicLayer;
using UploadTransaction.Helpers;
using UploadTransaction.Model;

namespace UploadTransaction.Controllers
{
    [Route("API/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionUtility _txnUtility;
        private readonly ITransactionBusinessLogic _businessLayer;
        public TransactionController(ITransactionBusinessLogic businessLayer)
        {
            _txnUtility = new TransactionUtility();
            _businessLayer = businessLayer;
        }

        [HttpPost]
        [Route("PostFile")]
        public async Task<IActionResult> PostFileAsync()
        {
            try
            {
                var fileInfo = new FileModel();
                IFormFile postFile = Request.Form.Files.FirstOrDefault();
                string extension = Path.GetExtension(postFile.FileName).ToLower();
                if (extension.ToLower() == ".csv" || extension.ToLower() == ".xml")
                { }
                else
                {
                    return await _txnUtility.TransactionApiResponse("012", "Unknown format");
                }

                if (postFile != null && postFile.Length > 0)
                {
                    fileInfo.fileName = postFile.FileName;
                    fileInfo.fileSize = postFile.Length;

                    using (var stream = new MemoryStream())
                    {
                        postFile.OpenReadStream().CopyTo(stream);
                        fileInfo.fileContent = stream.ToArray();
                    }
                    var returnData = await _businessLayer.UploadPostFileAsync(fileInfo);
                    return await _txnUtility.TransactionApiResponse(returnData);
                }
                else
                {
                    return await _txnUtility.TransactionApiResponse("012", "Invalid File.");
                }

            }
            catch (Exception ex)
            {
                return await _txnUtility.TransactionApiResponse(ex);
            }
        }

        [HttpPost]
        [Route("UploadValidFile")]
        public async Task<IActionResult> UploadValidFileAsync([FromBody] FileModel requestModel)
        {
            var errMsg = string.Empty;

            try
            {
                #region File Validation
                if (string.IsNullOrEmpty(requestModel.fileName))
                {
                    errMsg = "File Name is required.";
                    return await _txnUtility.TransactionApiResponse("012", errMsg);
                }
                if (requestModel.fileSize == 0)
                {
                    errMsg = "File Size is required.";
                    return await _txnUtility.TransactionApiResponse("012", errMsg);
                }
                if (requestModel.fileContent == null)
                {
                    errMsg = "File Content is required.";
                    return await _txnUtility.TransactionApiResponse("012", errMsg);
                }
                string extension = Path.GetExtension(requestModel.fileName).ToLower();

                if (extension.ToLower() == ".csv" || extension.ToLower() == ".xml")
                { }
                else
                {
                    return await _txnUtility.TransactionApiResponse("012", "Unknown format");
                }
                #endregion

                var returnData = await _businessLayer.UploadPostFileAsync(requestModel);
                return await _txnUtility.TransactionApiResponse(returnData);
            }
            catch (Exception ex)
            {
                return await _txnUtility.TransactionApiResponse(ex);
            }
        }

        [HttpPost]
        [Route("SaveTransactionHistory")]
        public async Task<IActionResult> SaveTransactionHistoryAsync([FromBody] SaveTransactionHistoryRequestModel requestModel)
        {
            try
            {
                var returnData = await _businessLayer.SaveTransactionHistoryAsync(requestModel);
                return await _txnUtility.TransactionApiResponse(returnData.RespCode, returnData.RespDescription);

            }
            catch (Exception ex)
            {
                return await _txnUtility.TransactionApiResponse(ex);
            }

        }

        [HttpPost]
        [Route("GetTransactionListByFilter")]
        public async Task<IActionResult> GetTransactionListByFilterAsync([FromBody] GetTransacionlistByFilterRequestModel requestModel)
        {
            try
            {
                var returnData = await _businessLayer.GetTransactionListByFilterAsync(requestModel);
                return await _txnUtility.TransactionApiResponse(returnData);
            }
            catch (Exception ex)
            {
                return await _txnUtility.TransactionApiResponse(ex);
            }
        }
    } 
}
