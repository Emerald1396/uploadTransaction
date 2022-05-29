using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UploadTransaction.Domain.DataInfrastructure
{
    public class TableContext
    {
    }

    public class TransactionHistoryTableModel
    {
        [Key]
        public int ID { get; set; }
        public string TransactionID { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Status { get; set; }
        public int CreatedBy { get; set; }
    }
}
