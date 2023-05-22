using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace ComparePay
{
    public class SCRTask : Task
    {
        private const string CONTEXTXML = @"<Context MyCtx=''Client App''/>";

        public bool IsRunTask { get; set; }
        public string Namespace { get; set; }
        public DateTime DateRelativeToToday { get; set; }
        public string OrgUnit { get; set; }

        public string LogId { get; set; }
        public string Status { get; set; }      // used to capture the TaskStatus

        private int WeeksAfterToday
        {
            get
            {
                return (int)Math.Ceiling((DateRelativeToToday - DateTime.Today).TotalDays / 7.0);
            }
        }

        private DateTime FromDate
            => (WeeksAfterToday < 0) ? DateRelativeToToday : DateTime.Today;

        private DateTime ToDate
            => (WeeksAfterToday < 0) ? DateTime.Today : DateRelativeToToday;

        [JsonIgnore]
        public DateTime RunTime { get; set; }

        private Log _m_Log = null;

        public override void SetLog(Log log) => _m_Log = log;

        public override bool GetIsRunTask() => IsRunTask;

        public override string GetNamespace() => Namespace;

        public override void SetStatus(string status)
        {
            Status = status;
        }

        public override string GetLogId() => LogId;

        public bool IsJobQueuedOrInProgress()
            => base.IsJobQueuedOrInProgress(IsRunTask, Status);

        public bool IsScheduleTaskRequired()
            => base.IsScheduleTaskRequired(IsRunTask, Status);

        public bool IsFetchResultRequired()
            => base.IsFetchResultRequired(IsRunTask, Status);

        public bool IsCompareTaskRequired()
            => base.IsCompareTaskRequired(IsRunTask, Status);

        public string ParamXml(ControlDB controlDB)
        {
            var jobParam = new Dictionary<string, object>()
                            {
                                { "NumPayPeriodsAfterToday", WeeksAfterToday },
                                { "Site", GetSiteIdsByNames(controlDB, OrgUnit) },
                            };
            return ParamXMLSerializer.Serialize(jobParam);
        }

        private string PaySummaryFileName => $"{RunTime.ToString("yyyyMMddHHmmss")}_{Namespace}_{LogId}.TXT";

        public string GetJobSql(ControlDB controlDB)
        {
            string insertSql = $@"
                INSERT INTO BackgroundJobWork
                (
                    ReceivedTimeUTC,
                    Namespace,
                    ConcurrencyGroupName,
                    JobId,
                    LogId,
                    ShortName,
                    AssemblyName,
                    TypeName,
                    MethodName,
                    ParamXml,
                    ContextXml,
                    JobDefinitionId,
                    CodeName,
                    ClientId,
                    TimeoutValue,
                    UserId,
                    RoleId,
                    ProxyUserId,
                    ProxyRoleId,
                    Kind,
                    JobNature,
                    IsClientLevelJob,
                    IsSiteLevelJob,
                    IsAdminJob,
                    MaxDurationInMinutes,
                    Folder,
                    Bookmark,
                    WakeUpTimeUTC,
                    Status,
                    ScheduleTimeZoneId,
                    SuppressSiteLevelParallelization
                )
                OUTPUT CONVERT(NVARCHAR(36), INSERTED.LogId)
                VALUES
                (
                    GETUTCDATE(),
                    '{Namespace}',
                    'SharpTop.Engine.BackgroundJobs.ScheduleCostRecalc',
                    {GetJobIdForClient(controlDB, "ScheduleCostRecalc")},
                    NEWID(),
                    'Schedule Cost Recalc',
                    'BackgroundJobs.dll',
                    'SharpTop.Engine.BackgroundJobs.ScheduleCostRecalc',
                    'Execute',
                    '{ParamXml(controlDB)}',
                    '{CONTEXTXML}',
                    {GetJobDefinitionId(controlDB, "ScheduleCostRecalc")},
                    'ScheduleCostRecalc',
                    {GetClientId(controlDB)},
                    NULL,
                    0,
                    0,
                    0,
                    0,
                    1,
                    0,
                    1,
                    0,
                    0,
                    NULL,
                    '',
                    NULL,
                    NULL,
                    0,
                    15,
                    0
                )
            ";

            return insertSql;
        }

        public string GetFetchRecordSql(ControlDB controlDB)
        {
            string orgUnit;
            if (String.IsNullOrEmpty(OrgUnit))
                orgUnit = "NULL";
            else
                orgUnit = $@"'{OrgUnit}'";

            string sql = $@"
                            DECLARE @orgUnit NVARCHAR(256) = {orgUnit};
                            SELECT
                                EmployeeId,
                                PayDate,
                                BusinessDate,
                                OrgUnitId,
                                DeptJobId,
                                PayCategoryId,
                                PayAdjCodeId,
                                EmployeePayAdjustId,
                                IsPremium,
                                Rate,
                                SUM(MinuteDuration) AS MinuteDuration,
                                SUM(NetHours) AS Nethours,
                                SUM(PayAmount) AS PayAmount
                            FROM
                                EmployeeSchedulePaySummary eps WITH(NOLOCK)
                            WHERE
                                PayDate BETWEEN '{FromDate}' AND '{ToDate}'
                                AND EXISTS
                                (
                                    SELECT TOP 1 1 FROM HierarchyOrgView hov WITH(NOLOCK)
                                    WHERE
                                        hov.ChildOrgUnitId = eps.OrgUnitId
                                        AND
                                        (
                                            @orgUnit IS NULL
                                            OR
                                            hov.ParentOrgUnitId IN ({GetSiteIdsByNames(controlDB, OrgUnit, "-1")})
                                        )
                                        AND eps.BusinessDate Between hov.ChildEffectiveStart and COALESCE(hov.ChildEffectiveEnd, '2050-01-01')
                                )
                            GROUP BY 
                                EmployeeId,
                                PayDate,
                                BusinessDate,
                                OrgUnitId,
                                DeptJobId,
                                PayCategoryId,
                                PayAdjCodeId,
                                EmployeePayAdjustId,
                                IsPremium,
                                Rate
                            ORDER BY
                                EmployeeId,
                                PayDate,
                                BusinessDate,
                                OrgUnitId,
                                DeptJobId,
                                PayCategoryId,
                                PayAdjCodeId,
                                EmployeePayAdjustId,
                                IsPremium,
                                Rate,
                                MinuteDuration,
                                Nethours,
                                PayAmount
                         ";
            return sql;
        }

        public bool FetchRecords(ControlDB controlDB, string pairName, bool forceCompareOnly, string outputFolder)
        {
            if (!forceCompareOnly && !IsFetchResultRequired())
                return true;

            _m_Log.AppendLine($"Fetching schedule pay summaries for the instance '{Namespace}' from '{FromDate}' to '{ToDate}'...");

            try
            {
                string sql = GetFetchRecordSql(controlDB);

                var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));
                DataTable dt = sqlUtil.GetResultSet(sql);

                return SaveRecordsToFile(dt, pairName, outputFolder);
            }
            catch (Exception e)
            {
                Status = TaskStatus.ResultFetchFailed;

                _m_Log.AppendLine($"FAILED to fetch schedule pay summaries.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        private bool SaveRecordsToFile(DataTable dt, string pairName, string outputFolder)
        {
            try
            {
                if ((dt.Rows?.Count ?? 0) <= 0)
                    return true;

                using (var sw = new StreamWriter(GetPaySummaryFileName(outputFolder, pairName)))
                {
                    long recCount = 0;
                    long totalMinutes = 0;
                    decimal totalHours = 0m;
                    decimal totalAmount = 0m;

                    sw.WriteLine(PaySummaryHeader);

                    foreach (DataRow row in dt.Rows)
                    {
                        recCount++;
                        sw.WriteLine(BuildRecord(row, ref totalMinutes, ref totalHours, ref totalAmount));
                    }

                    sw.WriteLine(GetPaySummaryFooter(recCount, totalMinutes, totalHours, totalAmount));
                }

                Status = TaskStatus.ResultFetched;
                return true;
            }
            catch (Exception e)
            {
                Status = TaskStatus.ResultFetchFailed;

                _m_Log.AppendLine($@"FAILED to save schdule pay summary records to file for instance '{Namespace}'.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        public string GetPaySummaryFileName(string outputFolder, string pairName)
            => $"{outputFolder}{pairName}_{PaySummaryFileName}";

        private string PaySummaryHeader
            => $"Employee Schedule Pay Summary Header";

        private string GetPaySummaryFooter(long recCount, long totalMinutes, decimal totalHours, decimal totalAmount)
            => $@"

  No. of Records: {recCount}
   Total Minutes: {totalMinutes}
 Total Net Hours: {totalHours}
Total Pay Amount: {totalAmount}

";

        private string BuildRecord(DataRow row, ref long totalMinutes, ref decimal totalHours, ref decimal totalAmount)
        {
            StringBuilder sb = new StringBuilder();

            if (row["EmployeeId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeId"])}");

            sb.Append(@",");
            if (row["PayDate"] != DBNull.Value)
                sb.Append($@"{Convert.ToDateTime(row["PayDate"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["BusinessDate"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["BusinessDate"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["OrgUnitId"] != DBNull.Value)
                sb.Append($"{Convert.ToInt64(row["OrgUnitId"])}");

            sb.Append(@",");
            sb.Append(@",");
            if (row["DeptJobId"] != DBNull.Value)
                sb.Append($"{Convert.ToInt64(row["DeptJobId"])}");

            sb.Append(@",");
            if (row["PayCategoryId"] != DBNull.Value)
                sb.Append($"{Convert.ToInt64(row["PayCategoryId"])}");

            sb.Append(@",");
            if (row["PayAdjCodeId"] != DBNull.Value)
                sb.Append($"{Convert.ToInt64(row["PayAdjCodeId"])}");

            sb.Append(@",");
            if (row["EmployeePayAdjustId"] != DBNull.Value)
                sb.Append($"{Convert.ToInt64(row["EmployeePayAdjustId"])}");

            sb.Append(@",");
            if (row["IsPremium"] != DBNull.Value)
                sb.Append($"{Convert.ToBoolean(row["IsPremium"])}");

            sb.Append(@",");
            if (row["Rate"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["Rate"])}");

            sb.Append(@",");
            if (row["MinuteDuration"] != DBNull.Value)
            {
                sb.Append($"{Convert.ToInt64(row["MinuteDuration"])}");
                totalMinutes += Convert.ToInt64(row["MinuteDuration"]);
            }

            sb.Append(@",");
            if (row["Nethours"] != DBNull.Value)
            {
                sb.Append($"{Convert.ToDecimal(row["Nethours"])}");
                totalHours += Convert.ToDecimal(row["Nethours"]);
            }

            sb.Append(@",");
            if (row["PayAmount"] != DBNull.Value)
            {
                sb.Append($"{Convert.ToDecimal(row["PayAmount"])}");
                totalAmount += Convert.ToDecimal(row["PayAmount"]);
            }

            return sb.ToString();
        }

        public bool CompareFiles(SCRTask task2, string pairName, string outputFolder)
        {
            if (!IsCompareTaskRequired() && !task2.IsCompareTaskRequired())
                return true;

            string fileName1 = GetPaySummaryFileName(outputFolder, pairName);
            string fileName2 = task2.GetPaySummaryFileName(outputFolder, pairName);

            _m_Log.AppendLine($@"Comparing file '{fileName1}' and '{fileName2}'...");

            if (!File.Exists(fileName1))
            {
                Status = TaskStatus.CompareFailed;
                _m_Log.AppendLine($@"File '{fileName1}' is not found.");
                return false;
            }

            if (!File.Exists(fileName2))
            {
                task2.Status = TaskStatus.CompareFailed;
                _m_Log.AppendLine($@"File '{fileName2}' is not found.");
                return false;
            }

            try
            {
                int status = TaskScheduler.CompareFiles(fileName1, fileName2);
                if (status == 0)
                {
                    Status = TaskStatus.CompareMismatch;
                    task2.Status = TaskStatus.CompareMismatch;
                }
                else if (status == 1)
                {
                    Status = TaskStatus.CompareAllMatch;
                    task2.Status = TaskStatus.CompareAllMatch;
                }

                return (status == 1);
            }
            catch (Exception e)
            {
                Status = TaskStatus.CompareFailed;
                task2.Status = TaskStatus.CompareFailed;

                _m_Log.AppendLine($"FAILED to compare files.  Error:");
                _m_Log.AppendLine(e.Message);

                return false;
            }
        }

    }
}
