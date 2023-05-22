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
    public class JobStepRecalcTask : Task
    {
        private const string CONTEXTXML = @"<Context MyCtx=''Client App''/>";

        public bool IsRunTask { get; set; }
        public string Namespace { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string OrgUnit { get; set; }

        public string LogId { get; set; }
        public string Status { get; set; }      // used to capture the TaskStatus

        private int WeeksBeforeToday
        {
            get
            {
                return (int)Math.Ceiling((DateTime.Today - FromDate).TotalDays / 7.0);
            }
        }

        private int WeeksAfterToday
        {
            get
            {
                return (int)Math.Ceiling((ToDate - DateTime.Today).TotalDays / 7.0);
            }
        }

        [JsonIgnore]
        public DateTime RunTime { get; set; }

        private Log _m_Log = null;
 
        public override void SetLog(Log log)
            => _m_Log = log;

        public override bool GetIsRunTask()
            => IsRunTask;

        public override string GetNamespace()
            => Namespace;

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

        private string OutputFileName => $"{RunTime.ToString("yyyyMMddHHmmss")}_{Namespace}_{LogId}.TXT";
        public string GetOutputFileName(string outputFolder, string pairName)
            => $"{outputFolder}{pairName}_{OutputFileName}";

        private string EmployeeJobStepRateHeader
            => $@"**Employee Job Step Rate**
EmployeeId,EmployeeWorkAssignmentId,EffectiveStart,EffectiveEnd,JobSetLevelRateId,Rate,Source";

        private string SectionSeparator
            => $@"

==================================================================

";
        private string EmployeeJobStepTransactionHeader
            => $@"**Employee Job Step Transaction**
EmployeeId,EmployeeWorkAssignmentId,BusinessDate,Delta,Source";

        public string ParamXml(ControlDB controlDB)
        {
            /*
                <Parameter NumWeeksBeforeToday="2" NumWeeksAfterToday="1" Site="1003" />
             */
            var jobParam = new Dictionary<string, object>()
                            {
                                { "NumWeeksBeforeToday", WeeksBeforeToday },
                                { "NumWeeksAfterToday", WeeksAfterToday },
                                { "Site", GetSiteIdsByNames(controlDB, OrgUnit) }
                            };
            return ParamXMLSerializer.Serialize(jobParam);
        }

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
                    'SharpTop.Engine.BackgroundJobs.JobStepRecalc',
                    {GetJobIdForClient(controlDB, "JOB_STEP_RECALC")},
                    NEWID(),
                    'Job Step Rate Recalc',
                    'BackgroundJobs.dll',
                    'SharpTop.Engine.BackgroundJobs.JobStepRecalc',
                    'Execute',
                    '{ParamXml(controlDB)}',
                    '{CONTEXTXML}',
                    {GetJobDefinitionId(controlDB, "JOB_STEP_RECALC")},
                    'JOB_STEP_RECALC',
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

        public string GetEmployeeJobStepRateSql(ControlDB controlDB)
        {
            string orgUnit;
            if (String.IsNullOrEmpty(OrgUnit))
                orgUnit = "NULL";
            else
                orgUnit = $@"'{OrgUnit}'";

            string sql = $@"
                            DECLARE @orgUnit NVARCHAR(256) = {orgUnit};
                            SELECT
                                ewa.EmployeeId,
                                ejsr.EmployeeWorkAssignmentId,
                                ejsr.EffectiveStart,
                                ejsr.EffectiveEnd,
                                ejsr.JobSetLevelRateId,
                                ejsr.Rate,
                                ejsr.Source
                            FROM
                                EmployeeJobStepRate ejsr WITH(NOLOCK)
                                INNER JOIN EmployeeWorkAssignment ewa WITH(NOLOCK) ON ewa.Id = ejsr.EmployeeWorkAssignmentId
                                INNER JOIN HierarchyOrgView hov WITH(NOLOCK) ON hov.ChildOrgUnitId = ewa.OrgUnitId
                            WHERE
                                ejsr.EffectiveStart BETWEEN '{FromDate}' AND '{ToDate}'
                                AND Deleted = 0
                                AND
                                (
                                    @orgUnit IS NULL
                                    OR
                                    hov.ParentOrgUnitId IN ({GetSiteIdsByNames(controlDB, OrgUnit, "-1")})
                                )
                                AND ewa.EffectiveStart < COALESCE(hov.ChildEffectiveEnd, '2050-01-01') AND COALESCE(ewa.EffectiveEnd, '2050-01-01') >= hov.ChildEffectiveStart
                                AND hov.ParentOrgLevelId = 999
                            ORDER BY
                                ewa.EmployeeId,
                                ejsr.EmployeeWorkAssignmentId,
                                ejsr.EffectiveStart,
                                ejsr.EffectiveEnd,
                                ejsr.JobSetLevelRateId,
                                ejsr.Rate,
                                ejsr.Source
                         ";
            return sql;
        }

        public string GetEmployeeJobStepTransactionSql(ControlDB controlDB)
        {
            string orgUnit;
            if (String.IsNullOrEmpty(OrgUnit))
                orgUnit = "NULL";
            else
                orgUnit = $@"'{OrgUnit}'";

            string sql = $@"
                            DECLARE @orgUnit NVARCHAR(256) = {orgUnit};
                            SELECT
                                ewa.EmployeeId,
                                ejst.EmployeeWorkAssignmentId,
                                ejst.BusinessDate,
                                ejst.Delta,
                                ejst.Source
                            FROM
                                EmployeeJobStepTransaction ejst WITH(NOLOCK)
                                INNER JOIN EmployeeWorkAssignment ewa WITH(NOLOCK) ON ewa.Id = ejst.EmployeeWorkAssignmentId
                                INNER JOIN HierarchyOrgView hov WITH(NOLOCK) ON hov.ChildOrgUnitId = ewa.OrgUnitId
                            WHERE
                                ejst.BusinessDate BETWEEN '{FromDate}' AND '{ToDate}'
                                AND Deleted = 0
                                AND
                                (
                                    @orgUnit IS NULL
                                    OR
                                    hov.ParentOrgUnitId IN ({GetSiteIdsByNames(controlDB, OrgUnit, "-1")})
                                )
                                AND ewa.EffectiveStart < COALESCE(hov.ChildEffectiveEnd, '2050-01-01') AND COALESCE(ewa.EffectiveEnd, '2050-01-01') >= hov.ChildEffectiveStart
                                AND hov.ParentOrgLevelId = 999
                            ORDER BY
                                ewa.EmployeeId,
                                ejst.EmployeeWorkAssignmentId,
                                ejst.BusinessDate,
                                ejst.Delta,
                                ejst.Source
                         ";
            return sql;
        }

        public bool FetchRecords(ControlDB controlDB, string pairName, bool forceCompareOnly, string outputFolder)
        {
            Status = TaskStatus.ResultFetchFailed;

            if (!FetchEmployeeJobStepRateRecords(controlDB, pairName, forceCompareOnly, outputFolder))
                return false;

            if (!FetchEmployeeJobStepTransactionRecords(controlDB, pairName, forceCompareOnly, outputFolder))
                return false;

            Status = TaskStatus.ResultFetched;
            return true;
        }

        public bool FetchEmployeeJobStepRateRecords(ControlDB controlDB, string pairName, bool forceCompareOnly, string outputFolder)
        {
            if (!forceCompareOnly && !IsFetchResultRequired())
                return true;

            _m_Log.AppendLine($"Fetching employee job step rate records for the instance '{Namespace}' from '{FromDate}' to '{ToDate}'...");

            try
            {
                string sql = GetEmployeeJobStepRateSql(controlDB);
                var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));
                DataTable dt = sqlUtil.GetResultSet(sql);
                return SaveEmployeeJobStepRateRecordsToFile(dt, pairName, outputFolder);
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($"FAILED to fetch employee job step rate records.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        public bool FetchEmployeeJobStepTransactionRecords(ControlDB controlDB, string pairName, bool forceCompareOnly, string outputFolder)
        {
            if (!forceCompareOnly && !IsFetchResultRequired())
                return true;

            _m_Log.AppendLine($"Fetching employee job step transaction records for the instance '{Namespace}' from '{FromDate}' to '{ToDate}'...");

            try
            {
                string sql = GetEmployeeJobStepTransactionSql(controlDB);
                var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));
                DataTable dt = sqlUtil.GetResultSet(sql);
                return SaveEmployeeJobStepTransactionRecordsToFile(dt, pairName, outputFolder);
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($"FAILED to fetch employee job step transaction records.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        private string GetFooter(long recCount)
            => $@"

No. of Records: {recCount}

";

        private bool SaveEmployeeJobStepRateRecordsToFile(DataTable dt, string pairName, string outputFolder)
        {
            try
            {
                if ((dt.Rows?.Count ?? 0) <= 0)
                    return true;

                using (var sw = new StreamWriter(GetOutputFileName(outputFolder, pairName)))
                {
                    long recCount = 0;

                    sw.WriteLine(EmployeeJobStepRateHeader);

                    foreach (DataRow row in dt.Rows)
                    {
                        recCount++;
                        sw.WriteLine(BuildEmployeeJobStepRateRecord(row));
                    }

                    sw.WriteLine(GetFooter(recCount));
                }

                return true;
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($@"FAILED to save employee job step rate records to file for instance '{Namespace}'.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        private string BuildEmployeeJobStepRateRecord(DataRow row)
        {
            StringBuilder sb = new StringBuilder();

            if (row["EmployeeId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeId"])}");

            sb.Append(@",");
            if (row["EmployeeWorkAssignmentId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeWorkAssignmentId"])}");

            sb.Append(@",");
            if (row["EffectiveStart"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["EffectiveStart"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["EffectiveEnd"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["EffectiveEnd"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["JobSetLevelRateId"] != DBNull.Value)
                sb.Append($"{Convert.ToInt64(row["JobSetLevelRateId"])}");

            sb.Append(@",");
            if (row["Rate"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["Rate"])}");

            sb.Append(@",");
            if (row["Source"] != DBNull.Value)
                sb.Append($"{Convert.ToInt32(row["Source"])}");

            return sb.ToString();
        }

        private bool SaveEmployeeJobStepTransactionRecordsToFile(DataTable dt, string pairName, string outputFolder)
        {
            try
            {
                if ((dt.Rows?.Count ?? 0) <= 0)
                    return true;

                using (var sw = new StreamWriter(GetOutputFileName(outputFolder, pairName), true))
                {
                    long recCount = 0;

                    sw.WriteLine(SectionSeparator);
                    sw.WriteLine(EmployeeJobStepTransactionHeader);

                    foreach (DataRow row in dt.Rows)
                    {
                        recCount++;
                        sw.WriteLine(BuildEmployeeJobStepTransactionRecord(row));
                    }

                    sw.WriteLine(GetFooter(recCount));
                }

                return true;
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($@"FAILED to save employee job step transaction records to file for instance '{Namespace}'.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }
        private string BuildEmployeeJobStepTransactionRecord(DataRow row)
        {
            StringBuilder sb = new StringBuilder();

            if (row["EmployeeId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeId"])}");

            sb.Append(@",");
            if (row["EmployeeWorkAssignmentId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeWorkAssignmentId"])}");

            sb.Append(@",");
            if (row["BusinessDate"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["BusinessDate"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["Delta"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["Delta"])}");

            sb.Append(@",");
            if (row["Source"] != DBNull.Value)
                sb.Append($"{Convert.ToInt32(row["Source"])}");

            return sb.ToString();
        }

        public bool CompareFiles(JobStepRecalcTask task2, string pairName, string outputFolder)
        {
            if (!IsCompareTaskRequired() && !task2.IsCompareTaskRequired())
                return true;

            string fileName1 = GetOutputFileName(outputFolder, pairName);
            string fileName2 = task2.GetOutputFileName(outputFolder, pairName);

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
