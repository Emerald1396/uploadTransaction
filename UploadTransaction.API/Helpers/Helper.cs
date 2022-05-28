using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace UploadTransaction.Helpers
{
    public static class Helper
    {
        #region Exception
        public static string ExceptionLineAndFile(Exception ex)
        {
            var st = new StackTrace(ex, true);
            var frame = st.GetFrame(0);
            var line = frame.GetFileLineNumber();

            return frame.GetFileName() + ": Line No. " + frame.GetFileLineNumber();
        }
        #endregion

        public static bool CheckNullorEmptyDatatable(DataTable dt)
        {
            if (dt != null && dt.Rows.Count > 0)
            {
                return true;

            }
            else
            {
                return false;
            }
        }
    }
}
