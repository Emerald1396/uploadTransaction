using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UploadTransaction.Model;

namespace UploadTransaction.Helpers
{
    public class TransactionUtility : ControllerBase
    {
        public async Task<IActionResult> TransactionApiResponse(string RespCode, string RespDesc)
        {
            ApiResponseModel model = new ApiResponseModel();
            model.RespCode = RespCode;
            model.RespDescription = RespDesc;

            if (RespCode != "000")
            {
                return BadRequest(model);
            }
            return Ok(model);
        }

        public async Task<IActionResult> TransactionApiResponse<T>(DataResponseModel<T> responseModel)
        {

            var response = JsonConvert.SerializeObject(responseModel);

            return Ok(response);
        }

        public async Task<IActionResult> TransactionApiResponse(Exception ex)
        {

            ApiResponseModel model = new ApiResponseModel();
            model.RespCode = "012";
            model.RespDescription = ex.Message;

            return BadRequest(model);
        }
    }
}
