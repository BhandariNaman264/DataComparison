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
    public class AwardEntitlementTask : Task
    {
        private const string CONTEXTXML = @"<Context MyCtx=''Client App''/>";

        public bool IsRunTask { get; set; }
        public string Namespace { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string OrgUnit { get; set; }
        public string Policy { get; set; }

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

        private string SectionSeparator
            => $@"

==================================================================

";

        private string NoRecords
            => $@"...No Records...";

        private string EmployeeBalancePeriodHeader
            => $@"**Employee Balance Period **
EmployeeId,EmployeeBalanceId,StartDate,EndDate,Value,GrantValue,MinValue,MaxValue";


        private string EmployeeBalanceTransactionHeader
            => $@"**Employee Balance Transaction**
EmployeeId,EmployeeBalanceId,BalanceDate,BalanceTransactionType,Delta,GrantDelta,TAFWId,TransactionSource,Status,ExpiryDate,PayOutOriginalEntitlementDate,IsBalancePayout";

        private string EmployeeBalanceGrantDateHeader
            => $@"**Employee Balance Grant Date**
EmployeeId,EmployeeBalanceId,BusinessDate,Value";

        private string EmployeePayAdjustHeader
            => $@"**Employee Pay Adjust**
EmployeeId,PayDate,BusinessDate,PayAdjCodeId,PayCategoryId,OrgUnitId,NetHours,MinuteDuration,TAFWId";


        public string ParamXml(ControlDB controlDB)
        {
            /*
                <Parameter NumWeeksBeforeToday="2" NumWeeksAfterToday="1" Site="1003" />
             */
            var jobParam = new Dictionary<string, object>()
                            {
                                { "SpecificStartDate", FromDate },
                                { "SpecificEndDate", ToDate },
                                { "Site", GetSiteIdsByNames(controlDB, OrgUnit) },
                                { "Policy", GetPolicyIdsByNames(controlDB, Policy) },
                                { "RecalculateBalancePeriods", true }
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
                    'SharpTop.Engine.BackgroundJobs.AwardEntitlements',
                    {GetJobIdForClient(controlDB, "AwardEntitlementsToEmployees")},
                    NEWID(),
                    'Award Entitlements To Employees',
                    'BackgroundJobs.dll',
                    'SharpTop.Engine.BackgroundJobs.AwardEntitlements',
                    'Execute',
                    '{ParamXml(controlDB)}',
                    '{CONTEXTXML}',
                    {GetJobDefinitionId(controlDB, "AwardEntitlementsToEmployees")},
                    'AwardEntitlementsToEmployees',
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

        public string GetPolicyIdsByNames(ControlDB controlDB, string rawPolicyList, string defaultValue = "")
        {
            string policyNameList = GetPolicyNames(rawPolicyList);
            if (String.IsNullOrEmpty(policyNameList))
                return defaultValue;

            string sql = $@"SELECT EntitlementPolicyId FROM EntitlementPolicy WITH(NOLOCK) WHERE ShortName IN ({policyNameList})";
            var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(GetNamespace()));

            return sqlUtil.GetValue<int>(sql).ToString();
        }

        public string GetPolicyNames(string policyList)
        {
            string policyNames = "";
            if (String.IsNullOrEmpty(policyList))
                return policyNames;

            string[] policies = policyList.Split('~');
            foreach (var policy in policies)
            {
                if (!String.IsNullOrEmpty(policyNames))
                    policyNames += ",";

                policyNames += $"'{policy}'";
            }

            return policyNames;
        }



        /*
        select * from employeebalancetransaction
        select * from employeebalanceperiod
        select * from EmployeePayAdjust
        select * from EmployeeBalanceGrantDate

         */

        public string GetEmployeeBalancePeriodSql(ControlDB controlDB)
        {
            string orgUnit;
            if (String.IsNullOrEmpty(OrgUnit))
                orgUnit = "NULL";
            else
                orgUnit = $@"'{OrgUnit}'";

            string sql = $@"
                            DECLARE @orgUnit NVARCHAR(256) = {orgUnit};
                            SELECT
                                ebp.EmployeeId,
                                ebp.EmployeeBalanceId,
                                ebp.StartDate,
                                ebp.EndDate,
                                ebp.Value,
                                ebp.GrantValue,
                                ebp.MinValue,
                                ebp.MaxValue
                            FROM
                                EmployeeBalancePeriod ebp WITH(NOLOCK)
                                INNER JOIN EmployeeWorkAssignment ewa WITH(NOLOCK) ON ewa.EmployeeId = ebp.EmployeeId
                                INNER JOIN HierarchyOrgView hov WITH(NOLOCK) ON hov.ChildOrgUnitId = ewa.OrgUnitId
                            WHERE
                                (
                                    ebp.StartDate BETWEEN '{FromDate}' AND '{ToDate}'
                                    OR COALESCE(ebp.EndDate, '{ToDate}') BETWEEN '{FromDate}' AND '{ToDate}'
                                )
                                AND
                                (
                                    @orgUnit IS NULL
                                    OR
                                    hov.ParentOrgUnitId IN ({GetSiteIdsByNames(controlDB, OrgUnit, "-1")})
                                )
                                AND ewa.EffectiveStart < COALESCE(hov.ChildEffectiveEnd, '2050-01-01') AND ewa.EffectiveEnd >= hov.ChildEffectiveStart
                                AND hov.ParentOrgLevelId = 999
                            ORDER BY
                                ebp.EmployeeId,
                                ebp.EmployeeBalanceId,
                                ebp.StartDate,
                                ebp.EndDate
                         ";
            return sql;
        }

        public string GetEmployeeBalanceTransactionSql(ControlDB controlDB)
        {
            string orgUnit;
            if (String.IsNullOrEmpty(OrgUnit))
                orgUnit = "NULL";
            else
                orgUnit = $@"'{OrgUnit}'";

            string sql = $@"
                            DECLARE @orgUnit NVARCHAR(256) = {orgUnit};
                            SELECT
                                ebt.EmployeeId,
                                ebt.EmployeeBalanceId,
                                ebt.BalanceDate,
                                btt.ShortName AS BalanceTransactionType,
                                ebt.Delta,
                                ebt.GrantDelta,
                                ebt.TAFWId,
                                ebt.TransactionSource,
                                ebt.Status,
                                ebt.ExpiryDate,
                                ebt.PayOutOriginalEntitlementDate,
                                ebt.IsBalancePayout
                            FROM
                                EmployeeBalanceTransaction ebt WITH(NOLOCK)
                                INNER JOIN EmployeeWorkAssignment ewa WITH(NOLOCK) ON ewa.EmployeeId = ebt.EmployeeId
                                INNER JOIN HierarchyOrgView hov WITH(NOLOCK) ON hov.ChildOrgUnitId = ewa.OrgUnitId
                                LEFT JOIN BalanceTransactionType btt WITH(NOLOCK) ON btt.BalanceTransactionTypeId = ebt.BalanceTransactionTypeId
                            WHERE
                                ebt.BalanceDate BETWEEN '{FromDate}' AND '{ToDate}'
                                AND
                                (
                                    @orgUnit IS NULL
                                    OR
                                    hov.ParentOrgUnitId IN ({GetSiteIdsByNames(controlDB, OrgUnit, "-1")})
                                )
                                AND ewa.EffectiveStart < COALESCE(hov.ChildEffectiveEnd, '2050-01-01') AND ewa.EffectiveEnd >= hov.ChildEffectiveStart
                                AND hov.ParentOrgLevelId = 999
                            ORDER BY
                                ebt.EmployeeId,
                                ebt.EmployeeBalanceId,
                                ebt.BalanceDate,
                                ebt.BalanceTransactionTypeId,
                                ebt.Delta,
                                ebt.GrantDelta,
                                ebt.TransactionSource
                         ";
            return sql;
        }

        public string GetEmployeeBalanceGrantDateSql(ControlDB controlDB)
        {
            string orgUnit;
            if (String.IsNullOrEmpty(OrgUnit))
                orgUnit = "NULL";
            else
                orgUnit = $@"'{OrgUnit}'";

            string sql = $@"
                            DECLARE @orgUnit NVARCHAR(256) = {orgUnit};
                            SELECT
                                ebgd.EmployeeId,
                                ebgd.EmployeeBalanceId,
                                ebgd.BusinessDate,
                                ebgd.Value
                            FROM
                                EmployeeBalanceGrantDate ebgd WITH(NOLOCK)
                                INNER JOIN EmployeeWorkAssignment ewa WITH(NOLOCK) ON ewa.EmployeeId = ebgd.EmployeeId
                                INNER JOIN HierarchyOrgView hov WITH(NOLOCK) ON hov.ChildOrgUnitId = ewa.OrgUnitId
                            WHERE
                                ebgd.BusinessDate BETWEEN '{FromDate}' AND '{ToDate}'
                                AND
                                (
                                    @orgUnit IS NULL
                                    OR
                                    hov.ParentOrgUnitId IN ({GetSiteIdsByNames(controlDB, OrgUnit, "-1")})
                                )
                                AND ewa.EffectiveStart < COALESCE(hov.ChildEffectiveEnd, '2050-01-01') AND ewa.EffectiveEnd >= hov.ChildEffectiveStart
                                AND hov.ParentOrgLevelId = 999
                            ORDER BY
                                ebgd.EmployeeId,
                                ebgd.EmployeeBalanceId,
                                ebgd.BusinessDate,
                                ebgd.Value
                         ";
            return sql;
        }

        public string GetEmployeePayAdjustSql(ControlDB controlDB)
        {
            string orgUnit;
            if (String.IsNullOrEmpty(OrgUnit))
                orgUnit = "NULL";
            else
                orgUnit = $@"'{OrgUnit}'";

            string sql = $@"
                            DECLARE @orgUnit NVARCHAR(256) = {orgUnit};
                            SELECT
                                epa.EmployeeId,
                                epa.PayDate,
                                epa.BusinessDate,
                                epa.PayAdjCodeId,
                                epa.PayCategoryId,
                                epa.OrgUnitId,
                                epa.NetHours,
                                epa.MinuteDuration,
                                epa.TAFWId
                            FROM
                                EmployeePayAdjust epa WITH(NOLOCK)
                                INNER JOIN EmployeeWorkAssignment ewa WITH(NOLOCK) ON ewa.EmployeeId = epa.EmployeeId
                                INNER JOIN HierarchyOrgView hov WITH(NOLOCK) ON hov.ChildOrgUnitId = ewa.OrgUnitId
                            WHERE
                                epa.IsBalancePayout = 1
                                AND epa.PayDate BETWEEN '{FromDate}' AND '{ToDate}'
                                AND
                                (
                                    @orgUnit IS NULL
                                    OR
                                    hov.ParentOrgUnitId IN ({GetSiteIdsByNames(controlDB, OrgUnit, "-1")})
                                )
                                AND ewa.EffectiveStart < COALESCE(hov.ChildEffectiveEnd, '2050-01-01') AND ewa.EffectiveEnd >= hov.ChildEffectiveStart
                                AND hov.ParentOrgLevelId = 999
                            ORDER BY
                                epa.EmployeeId,
                                epa.PayDate,
                                epa.BusinessDate,
                                epa.PayAdjCodeId,
                                epa.PayCategoryId,
                                epa.OrgUnitId,
                                epa.NetHours,
                                epa.MinuteDuration
                         ";
            return sql;
        }

        public bool FetchRecords(ControlDB controlDB, string pairName, bool forceCompareOnly, string outputFolder)
        {
            Status = TaskStatus.ResultFetchFailed;

            if (!FetchEmployeeBalancePeriodRecords(controlDB, pairName, forceCompareOnly, outputFolder))
                return false;

            if (!FetchEmployeeBalanceTransactionRecords(controlDB, pairName, forceCompareOnly, outputFolder))
                return false;

            if (!FetchEmployeeBalanceGrantDateRecords(controlDB, pairName, forceCompareOnly, outputFolder))
                return false;

            if (!FetchEmployeePayAdjustRecords(controlDB, pairName, forceCompareOnly, outputFolder))
                return false;

            Status = TaskStatus.ResultFetched;
            return true;
        }

        public bool FetchEmployeeBalancePeriodRecords(ControlDB controlDB, string pairName, bool forceCompareOnly, string outputFolder)
        {
            if (!forceCompareOnly && !IsFetchResultRequired())
                return true;

            _m_Log.AppendLine($"Fetching employee balance period records for the instance '{Namespace}' from '{FromDate}' to '{ToDate}'...");

            try
            {
                string sql = GetEmployeeBalancePeriodSql(controlDB);
                var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));
                DataTable dt = sqlUtil.GetResultSet(sql);
                return SaveEmployeeBalancePeriodRecordsToFile(dt, pairName, outputFolder);
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($"FAILED to fetch employee balance period records.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        public bool FetchEmployeeBalanceTransactionRecords(ControlDB controlDB, string pairName, bool forceCompareOnly, string outputFolder)
        {
            if (!forceCompareOnly && !IsFetchResultRequired())
                return true;

            _m_Log.AppendLine($"Fetching employee balance transaction records for the instance '{Namespace}' from '{FromDate}' to '{ToDate}'...");

            try
            {
                string sql = GetEmployeeBalanceTransactionSql(controlDB);
                var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));
                DataTable dt = sqlUtil.GetResultSet(sql);
                return SaveEmployeeBalanceTransactionRecordsToFile(dt, pairName, outputFolder);
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($"FAILED to fetch employee balance transaction records.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        public bool FetchEmployeeBalanceGrantDateRecords(ControlDB controlDB, string pairName, bool forceCompareOnly, string outputFolder)
        {
            if (!forceCompareOnly && !IsFetchResultRequired())
                return true;

            _m_Log.AppendLine($"Fetching employee balance grant date records for the instance '{Namespace}' from '{FromDate}' to '{ToDate}'...");

            try
            {
                string sql = GetEmployeeBalanceGrantDateSql(controlDB);
                var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));
                DataTable dt = sqlUtil.GetResultSet(sql);
                return SaveEmployeeBalanceGrantDateRecordsToFile(dt, pairName, outputFolder);
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($"FAILED to fetch employee balance grant date records.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        public bool FetchEmployeePayAdjustRecords(ControlDB controlDB, string pairName, bool forceCompareOnly, string outputFolder)
        {
            if (!forceCompareOnly && !IsFetchResultRequired())
                return true;

            _m_Log.AppendLine($"Fetching employee pay adjust records for the instance '{Namespace}' from '{FromDate}' to '{ToDate}'...");

            try
            {
                string sql = GetEmployeePayAdjustSql(controlDB);
                var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));
                DataTable dt = sqlUtil.GetResultSet(sql);
                return SaveEmployeePayAdjRecordsToFile(dt, pairName, outputFolder);
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($"FAILED to fetch employee pay adjust records.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        private string GetFooter(long recCount)
            => $@"

No. of Records: {recCount}

";

        private bool SaveEmployeeBalancePeriodRecordsToFile(DataTable dt, string pairName, string outputFolder)
        {
            try
            {
                //if ((dt.Rows?.Count ?? 0) <= 0)
                //    return true;

                using (var sw = new StreamWriter(GetOutputFileName(outputFolder, pairName)))
                {
                    long recCount = 0;

                    sw.WriteLine(EmployeeBalancePeriodHeader);

                    if ((dt.Rows?.Count ?? 0) <= 0)
                    {
                        sw.WriteLine(NoRecords);
                        return true;
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        recCount++;
                        sw.WriteLine(BuildEmployeeBalancePeriodRecord(row));
                    }

                    sw.WriteLine(GetFooter(recCount));
                }

                return true;
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($@"FAILED to save employee balance period records to file for instance '{Namespace}'.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        private string BuildEmployeeBalancePeriodRecord(DataRow row)
        {
            StringBuilder sb = new StringBuilder();


            //EmployeeId,EmployeeBalanceId,StartDate,EndDate,Value,GrantValue,MinValue,MaxValue";

            if (row["EmployeeId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeId"])}");

            sb.Append(@",");
            if (row["EmployeeBalanceId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeBalanceId"])}");

            sb.Append(@",");
            if (row["StartDate"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["StartDate"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["EndDate"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["EndDate"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["Value"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["Value"])}");

            sb.Append(@",");
            if (row["GrantValue"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["GrantValue"])}");

            sb.Append(@",");
            if (row["MinValue"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["MinValue"])}");

            sb.Append(@",");
            if (row["MaxValue"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["MaxValue"])}");

            return sb.ToString();
        }

        private bool SaveEmployeeBalanceTransactionRecordsToFile(DataTable dt, string pairName, string outputFolder)
        {
            try
            {
                using (var sw = new StreamWriter(GetOutputFileName(outputFolder, pairName), true))
                {
                    long recCount = 0;

                    sw.WriteLine(SectionSeparator);
                    sw.WriteLine(EmployeeBalanceTransactionHeader);

                    if ((dt.Rows?.Count ?? 0) <= 0)
                    {
                        sw.WriteLine(NoRecords);
                        return true;
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        recCount++;
                        sw.WriteLine(BuildEmployeeBalanceTransactionRecord(row));
                    }

                    sw.WriteLine(GetFooter(recCount));
                }

                return true;
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($@"FAILED to save employee balance transaction records to file for instance '{Namespace}'.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        private string BuildEmployeeBalanceTransactionRecord(DataRow row)
        {
            StringBuilder sb = new StringBuilder();

            //  EmployeeId,EmployeeBalanceId,BalanceDate,BalanceTransactionType,Delta,GrantDelta,TAFWId,TransactionSource,Status,ExpiryDate,PayOutOriginalEntitlementDate,IsBalancePayout";

            if (row["EmployeeId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeId"])}");

            sb.Append(@",");
            if (row["EmployeeBalanceId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeBalanceId"])}");

            sb.Append(@",");
            if (row["BalanceDate"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["BalanceDate"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["BalanceTransactionType"] != DBNull.Value)
                sb.Append($"{Convert.ToString(row["BalanceTransactionType"])}");

            sb.Append(@",");
            if (row["Delta"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["Delta"])}");

            sb.Append(@",");
            if (row["GrantDelta"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["GrantDelta"])}");

            sb.Append(@",");
            if (row["TAFWId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["TAFWId"])}");

            sb.Append(@",");
            if (row["TransactionSource"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt32(row["TransactionSource"])}");

            sb.Append(@",");
            if (row["Status"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt32(row["Status"])}");

            sb.Append(@",");
            if (row["ExpiryDate"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["ExpiryDate"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["PayOutOriginalEntitlementDate"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["PayOutOriginalEntitlementDate"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["IsBalancePayout"] != DBNull.Value)
                sb.Append($"{Convert.ToInt16(row["IsBalancePayout"])}");

            return sb.ToString();
        }

        private bool SaveEmployeeBalanceGrantDateRecordsToFile(DataTable dt, string pairName, string outputFolder)
        {
            try
            {
                using (var sw = new StreamWriter(GetOutputFileName(outputFolder, pairName), true))
                {
                    long recCount = 0;

                    sw.WriteLine(SectionSeparator);
                    sw.WriteLine(EmployeeBalanceGrantDateHeader);

                    if ((dt.Rows?.Count ?? 0) <= 0)
                    {
                        sw.WriteLine(NoRecords);
                        return true;
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        recCount++;
                        sw.WriteLine(BuildEmployeeBalanceGrantDateRecord(row));
                    }

                    sw.WriteLine(GetFooter(recCount));
                }

                return true;
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($@"FAILED to save employee balance grant date records to file for instance '{Namespace}'.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        private string BuildEmployeeBalanceGrantDateRecord(DataRow row)
        {
            StringBuilder sb = new StringBuilder();

            //  EmployeeId,EmployeeBalanceId,BusinessDate,Value";

            if (row["EmployeeId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeId"])}");

            sb.Append(@",");
            if (row["EmployeeBalanceId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeBalanceId"])}");

            sb.Append(@",");
            if (row["BusinessDate"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["BusinessDate"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["Value"] != DBNull.Value)
                sb.Append($"{Convert.ToDecimal(row["Value"])}");

            return sb.ToString();
        }

        private bool SaveEmployeePayAdjRecordsToFile(DataTable dt, string pairName, string outputFolder)
        {
            try
            {
                using (var sw = new StreamWriter(GetOutputFileName(outputFolder, pairName), true))
                {
                    long recCount = 0;

                    sw.WriteLine(SectionSeparator);
                    sw.WriteLine(EmployeePayAdjustHeader);

                    if ((dt.Rows?.Count ?? 0) <= 0)
                    {
                        sw.WriteLine(NoRecords);
                        return true;
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        recCount++;
                        sw.WriteLine(BuildEmployeePayAdjRecord(row));
                    }

                    sw.WriteLine(GetFooter(recCount));
                }

                return true;
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($@"FAILED to save employee pay adjust records to file for instance '{Namespace}'.  Error:");
                _m_Log.AppendLine(e.Message);
                return false;
            }
        }

        private string BuildEmployeePayAdjRecord(DataRow row)
        {
            StringBuilder sb = new StringBuilder();

            //  EmployeeId,PayDate,BusinessDate,PayAdjCodeId,PayCategoryId,OrgUnitId,NetHours,MinuteDuration,TAFWId";

            if (row["EmployeeId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["EmployeeId"])}");

            sb.Append(@",");
            if (row["PayDate"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["PayDate"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["BusinessDate"] != DBNull.Value)
                sb.Append($"{Convert.ToDateTime(row["BusinessDate"]).ToString(@"yyyy-MM-dd")}");

            sb.Append(@",");
            if (row["PayAdjCodeId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["PayAdjCodeId"])}");

            sb.Append(@",");
            if (row["PayCategoryId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["PayCategoryId"])}");

            sb.Append(@",");
            if (row["OrgUnitId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["OrgUnitId"])}");

            sb.Append(@",");
            if (row["NetHours"] != DBNull.Value)
                sb.Append($@"{Convert.ToDecimal(row["NetHours"])}");

            sb.Append(@",");
            if (row["MinuteDuration"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["MinuteDuration"])}");

            sb.Append(@",");
            if (row["TAFWId"] != DBNull.Value)
                sb.Append($@"{Convert.ToInt64(row["TAFWId"])}");

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

        public bool CompareFiles(AwardEntitlementTask task2, string pairName, string outputFolder)
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
