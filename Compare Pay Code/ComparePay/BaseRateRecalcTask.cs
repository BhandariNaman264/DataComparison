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
    public class BaseRateRecalcTask : Task
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

        private string EmployeeAppliedBaseRateHeader
            => $@"**Employee Applied Base Rates**
EmployeeId,EmployeeEmploymentStatusId,EffectiveStart,EffectiveEnd,RateSetId,RateSetLevelRateId,LevelValue,RateValue,RateApplyType";

        private string SectionSeparator
            => $@"

==================================================================

";
        private string EmployeeEmploymentStatusHeader
            => $@"**Employee Employment Status**
EmployeeId,EmployeeEmploymentStatusId,EffectiveStart,EffectiveEnd,BaseRate,BaseSalary,BaseRateManuallySet";

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
                    'SharpTop.Engine.BackgroundJobs.BaseRateRecalc',
                    {GetJobIdForClient(controlDB, "BaseRateRecalc")},
                    NEWID(),
                    'Base Rate Recalc',
                    'BackgroundJobs.dll',
                    'SharpTop.Engine.BackgroundJobs.BaseRateRecalc',
                    'Execute',
                    '{ParamXml(controlDB)}',
                    '{CONTEXTXML}',
                    {GetJobDefinitionId(controlDB, "BaseRateRecalc")},
                    'BaseRateRecalc',
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

        public string GetEmployeeAppliedBaseRateSql(ControlDB controlDB)
        {
            string orgUnit;
            if (String.IsNullOrEmpty(OrgUnit))
                orgUnit = "NULL";
            else
                orgUnit = $@"'{OrgUnit}'";

            string sql = $@"
                            DECLARE @orgUnit NVARCHAR(256) = {orgUnit};
                            SELECT
                                ees.EmployeeId,
                                eabr.EmployeeEmploymentStatusId,
                                ees.EffectiveStart,
                                ees.EffectiveEnd,
                                eabr.RateSetId,
                                eabr.RateSetLevelRateId,
                                rslr.LevelValue,
                                rslr.RateValue,
                                rslrt.ShortName AS RateApplyType
                            FROM
                                EmployeeAppliedBaseRates eabr WITH(NOLOCK)
                                INNER JOIN RateSetLevelRate rslr WITH(NOLOCK) ON rslr.RateSetLevelRateId = eabr.RateSetLevelRateId
                                INNER JOIN RateSetLevelRateType rslrt WITH(NOLOCK) ON rslrt.RateSetLevelRateTypeId = rslr.RateSetLevelRateTypeId
                                INNER JOIN EmployeeEmploymentStatus ees WITH(NOLOCK) ON ees.EmployeeEmploymentStatusId = eabr.EmployeeEmploymentStatusId
                                INNER JOIN EmployeeWorkAssignment ewa WITH(NOLOCK) ON ewa.EmployeeId = ees.EmployeeId
                                INNER JOIN HierarchyOrgView hov WITH(NOLOCK) ON hov.ChildOrgUnitId = ewa.OrgUnitId
                            WHERE
                                ees.EffectiveStart BETWEEN '{FromDate}' AND '{ToDate}'
                                AND
                                (
                                    @orgUnit IS NULL
                                    OR
                                    hov.ParentOrgUnitId IN ({GetSiteIdsByNames(controlDB, OrgUnit, "-1")})
                                )
                                AND ewa.EffectiveStart < COALESCE(hov.ChildEffectiveEnd, '2050-01-01') AND ewa.EffectiveEnd >= hov.ChildEffectiveStart
                                AND hov.ParentOrgLevelId = 999
                            ORDER BY
                                ees.EmployeeId,
                                ees.EffectiveStart,
                                ees.EffectiveEnd,
                                rslr.LevelValue
                         ";
            return sql;
        }

        public string GetEmployeeEmploymentStatusSql(ControlDB controlDB)
        {
            string orgUnit;
            if (String.IsNullOrEmpty(OrgUnit))
                orgUnit = "NULL";
            else
                orgUnit = $@"'{OrgUnit}'";

            string sql = $@"
                            DECLARE @orgUnit NVARCHAR(256) = {orgUnit};
                            SELECT
                                ees.EmployeeId,
                                ees.EmployeeEmploymentStatusId,
                                ees.EffectiveStart,
                                ees.EffectiveEnd,
                                ees.BaseRate,
                                ees.BaseSalary,
                                ees.BaseRateManuallySet
                            FROM
                                EmployeeEmploymentStatus ees WITH(NOLOCK)
                                INNER JOIN EmployeeWorkAssignment ewa WITH(NOLOCK) ON ewa.EmployeeId = ees.EmployeeId
                                INNER JOIN HierarchyOrgView hov WITH(NOLOCK) ON hov.ChildOrgUnitId = ewa.OrgUnitId
                            WHERE
                                ees.EffectiveStart BETWEEN '{FromDate}' AND '{ToDate}'
                                AND
                                (
                                    @orgUnit IS NULL
                                    OR
                                    hov.ParentOrgUnitId IN ({GetSiteIdsByNames(controlDB, OrgUnit, "-1")})
                                )
                                AND ewa.EffectiveStart < COALESCE(hov.ChildEffectiveEnd, '2050-01-01') AND ewa.EffectiveEnd >= hov.ChildEffectiveStart
                                AND hov.ParentOrgLevelId = 999
                            ORDER BY
                                ees.EmployeeId,
                                ees.EffectiveStart,
                                ees.EffectiveEnd,
                                ees.BaseRate
                         ";
            return sql;
        }

        public bool FetchRecords(ControlDB controlDB, string pairName, bool forceCompareOnly, string outputFolder)
        {
            Status = TaskStatus.ResultFetchFailed;

            if (!FetchEmployeeAppliedBaseRateRecords(controlDB, pairName, forceCompareOnly, outputFolder))
                return false;

            if (!FetchEmployeeEmploymentStatusRecords(controlDB, pairName, forceCompareOnly, outputFolder))
                return false;

            Status = TaskStatus.ResultFetched;
            return true;
        }

        public bool FetchEmployeeAppliedBaseRateRecords(ControlDB controlDB, string pairName, bool forceCompareOnly, string outputFolder)
        {
            if (!forceCompareOnly && !IsFetchResultRequired())
                return true;

            _m_Log.AppendLine($"Fetching employee applied base rate records for the instance '{Namespace}' from '{FromDate}' to '{ToDate}'...");

            try
            {
                string sql = GetEmployeeAppliedBaseRateSql(controlDB);
                var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));
                DataTable dt = sqlUtil.GetResultSet(sql);
                return SaveEmployeeAppliedBaseRateRecordsToFile(dt, pairName, outputFolder);
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($"FAILED to fetch employee applied base rate records.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        public bool FetchEmployeeEmploymentStatusRecords(ControlDB controlDB, string pairName, bool forceCompareOnly, string outputFolder)
        {
            if (!forceCompareOnly && !IsFetchResultRequired())
                return true;

            _m_Log.AppendLine($"Fetching employee employment status records for the instance '{Namespace}' from '{FromDate}' to '{ToDate}'...");

            try
            {
                string sql = GetEmployeeEmploymentStatusSql(controlDB);
                var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));
                DataTable dt = sqlUtil.GetResultSet(sql);
                return SaveEmployeeEmploymentStatusRecordsToFile(dt, pairName, outputFolder);
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($"FAILED to fetch employee employment status records.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        private string GetFooter(long recCount)
            => $@"

No. of Records: {recCount}

";

        private bool SaveEmployeeAppliedBaseRateRecordsToFile(DataTable dt, string pairName, string outputFolder)
        {
            try
            {
                if ((dt.Rows?.Count ?? 0) <= 0)
                    return true;

                using (var sw = new StreamWriter(GetOutputFileName(outputFolder, pairName)))
                {
                    long recCount = 0;

                    sw.WriteLine(EmployeeAppliedBaseRateHeader);

                    foreach (DataRow row in dt.Rows)
                    {
                        recCount++;
                        sw.WriteLine(BuildEmployeeAppliedBaseRateRecord(row));
                    }

                    sw.WriteLine(GetFooter(recCount));
                }

                return true;
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($@"FAILED to save employee applied base rate records to file for instance '{Namespace}'.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        private string BuildEmployeeAppliedBaseRateRecord(DataRow row)
        {
            StringBuilder sb = new StringBuilder();


            //EmployeeId,EmployeeEmploymentStatusId,EffectiveStart,EffectiveEnd,RateSetId,RateSetLevelRateId,LevelValue,RateValue,RateApplyType";

            if (row["EmployeeId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeId"])}");

            sb.Append(@",");
            if (row["EmployeeEmploymentStatusId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeEmploymentStatusId"])}");

            sb.Append(@",");
            if (row["EffectiveStart"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["EffectiveStart"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["EffectiveEnd"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["EffectiveEnd"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["RateSetId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["RateSetId"])}");

            sb.Append(@",");
            if (row["RateSetLevelRateId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["RateSetLevelRateId"])}");

            sb.Append(@",");
            if (row["LevelValue"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["LevelValue"])}");

            sb.Append(@",");
            if (row["RateValue"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["RateValue"])}");

            sb.Append(@",");
            if (row["RateApplyType"] != DBNull.Value)
                sb.Append($"{Convert.ToString(row["RateApplyType"])}");

            return sb.ToString();
        }

        private bool SaveEmployeeEmploymentStatusRecordsToFile(DataTable dt, string pairName, string outputFolder)
        {
            try
            {
                if ((dt.Rows?.Count ?? 0) <= 0)
                    return true;

                using (var sw = new StreamWriter(GetOutputFileName(outputFolder, pairName), true))
                {
                    long recCount = 0;

                    sw.WriteLine(SectionSeparator);
                    sw.WriteLine(EmployeeEmploymentStatusHeader);

                    foreach (DataRow row in dt.Rows)
                    {
                        recCount++;
                        sw.WriteLine(BuildEmployeeEmploymentStatusRecord(row));
                    }

                    sw.WriteLine(GetFooter(recCount));
                }

                return true;
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($@"FAILED to save employee employment status records to file for instance '{Namespace}'.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }
        private string BuildEmployeeEmploymentStatusRecord(DataRow row)
        {
            StringBuilder sb = new StringBuilder();

            // EmployeeId,EmployeeEmploymentStatusId,EffectiveStart,EffectiveEnd,BaseRate,BaseSalary,BaseRateManuallySet";

            if (row["EmployeeId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeId"])}");

            sb.Append(@",");
            if (row["EmployeeEmploymentStatusId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeEmploymentStatusId"])}");

            sb.Append(@",");
            if (row["EffectiveStart"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["EffectiveStart"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["EffectiveEnd"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["EffectiveEnd"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["BaseRate"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["BaseRate"])}");

            sb.Append(@",");
            if (row["BaseSalary"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["BaseSalary"])}");

            sb.Append(@",");
            if (row["BaseRateManuallySet"] != DBNull.Value)
                sb.Append($"{Convert.ToInt16(row["BaseRateManuallySet"])}");

            return sb.ToString();
        }

        public bool CompareFiles(BaseRateRecalcTask task2, string pairName, string outputFolder)
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
