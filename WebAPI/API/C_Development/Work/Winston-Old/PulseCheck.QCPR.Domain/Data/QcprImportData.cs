using System;
using PulseCheck.QCPR.Domain.Contract;

namespace PulseCheck.QCPR.Domain.Data
{
    public class QcprImportData : IQcprImportData
    {
        public Data data { get; set; }
        public Status status { get; set; }

        public void SetImportArchiveId(long importArchiveId)
        {
            if (data != null && data.procedure != null)
            {
                foreach (Procedure procedure in data.procedure)
                {
                    procedure.ImportArchiveId = importArchiveId;
                }
            }
        }

        public static void Validate(IQcprImportData importData)
        {
            if(importData == null)
                throw new ArgumentNullException(nameof(importData));

            if (importData.data == null)
                throw new ArgumentNullException(nameof(importData.data));

            if (importData.status == null)
                throw new ArgumentNullException(nameof(importData));

            if (importData.status.code != 0)
                throw new InvalidOperationException($"Invalid Code {importData.status.code}");

        }
    }
}
