using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UploadTransaction.Model;

namespace UploadTransaction.Domain.BusinessLogicLayer
{
    public interface ITransactionBusinessLogic
    {
        public Task<DataResponseModel<TransactionDetailReviewResponse>> UploadPostFileAsync(FileModel reqModel);
    }
}
