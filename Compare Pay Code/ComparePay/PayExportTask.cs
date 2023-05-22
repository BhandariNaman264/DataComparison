using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComparePay
{
    public class PayExportTask : Task
    {
        public bool IsRunTask { get; set; }
        public string Namespace { get; set; }
        public int PayGroupCalendarId { get; set; }
        public string ExportMode { get; set; }
        public bool MockTransmit { get; set; }

        public string ExportFileName { get; set; }

        public string LogId { get; set; }
        public string Status { get; set; }      // used to capture the TaskStatus

        [JsonIgnore]
        public DateTime RunTime { get; set; }

        public override bool GetIsRunTask() => IsRunTask;

        public override string GetNamespace() => Namespace;

        public override void SetStatus(string status)
        {
            Status = status;
        }

        public override string GetLogId() => LogId;

        private Log _m_Log = null;
        public override void SetLog(Log log)
        {
            _m_Log = log;
        }

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
            string sql = $@"SELECT
                                PayGroupId,
                                EffectiveStart,
                                EffectiveEnd,
                                TransmitByDate
                            FROM
                                PayGroupCalendar WITH(NOLOCK)
                            WHERE
                                PayGroupCalendarId = {PayGroupCalendarId}
                        ";

            var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));
            DataTable dt = sqlUtil.GetResultSet(sql);

            if ((dt?.Rows?.Count ?? 0) <= 0)
                return "";

            DataRow row = dt.Rows[0];


            var jobParam = new Dictionary<string, object>()
                            {
                                { "PayGroupId", Convert.ToInt64(row["PayGroupId"])},
                                { "ExportCalendarDate", Convert.ToDateTime(row["TransmitByDate"])},
                                { "MockTransmit", MockTransmit },
                                { "ExportMode", 0 },
                                { "PeriodStart", Convert.ToDateTime(row["EffectiveStart"]).Date},
                                { "PeriodEnd", Convert.ToDateTime(row["EffectiveEnd"]).Date}
                            };
            return ParamXMLSerializer.Serialize(jobParam);
        }

        //public override int GetClientId(ControlDB controlDB)
        //{
        //    return base.GetClientId(controlDB, Namespace);
        //}

        //public int GetJobDefinitionId(ControlDB controlDB)
        //{
        //    string sql = $@"SELECT BackgroundTaskDefinitionId FROM BackgroundTaskDefinition WITH(NOLOCK) WHERE BackgroundJobId = 12";
        //    var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));

        //    return sqlUtil.GetValue<int>(sql);
        //}

        public int GetTimeoutValue(ControlDB controlDB)
        {
            string sql = $@"SELECT TimeoutValue FROM BackgroundJob WITH(NOLOCK) WHERE BackgroundJobId = 12";
            var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));

            return sqlUtil.GetValue<int>(sql);
        }

        public string GetFolder(ControlDB controlDB)
        {
            string sql = $@"SELECT FolderName FROM BackgroundTaskDefinition WITH(NOLOCK) WHERE BackgroundJobId = 12";
            var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));

            return sqlUtil.GetValue<string>(sql);
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
                    'SharpTop.Engine.BackgroundJobs.PayrollExport',
                    {GetJobIdForClient(controlDB, "PayrollExport")},
                    NEWID(),
                    'Payroll Export',
                    'BackgroundJobs.dll',
                    'SharpTop.Engine.BackgroundJobs.PayrollExport',
                    'Execute',
                    '{ParamXml(controlDB)}',
                    NULL,
                    {GetJobDefinitionId(controlDB, "PayrollExport")},
                    'PayrollExport',
                    {GetClientId(controlDB)},
                    {GetTimeoutValue(controlDB)},
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
                    '{GetFolder(controlDB)}',
                    NULL,
                    NULL,
                    0,
                    15,
                    0
                )
            ";

            return insertSql;
        }

        public bool FetchFile(ControlDB controlDB, string pairName, string outputFolder)
        {
            string exportFileName = $@"{GetExportFolder(controlDB)}{Path.DirectorySeparatorChar}{ExportFileName}";
            string destFileName = GetPayExportFileName(outputFolder, pairName);

            try
            {
                FileInfo fi = new FileInfo(exportFileName);
                fi.CopyTo(destFileName);

                Status = TaskStatus.ResultFetched;
                return true;
            }
            catch (Exception e)
            {
                _m_Log.AppendLine("FAILED to fetch export file.  Error:");
                _m_Log.AppendLine(e.Message);

                Status = TaskStatus.ResultFetchFailed;
                return false;
            }
        }

        public bool CompareFiles(PayExportTask task2, string pairName, string outputFolder)
        {
            if (!IsCompareTaskRequired() && !task2.IsCompareTaskRequired())
                return true;

            string fileName1 = GetPayExportFileName(outputFolder, pairName);
            string fileName2 = task2.GetPayExportFileName(outputFolder, pairName);

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


        private string PayExportFileName => $"{RunTime.ToString("yyyyMMddHHmmss")}_{Namespace}_{LogId}_{ExportFileName}";

        private string GetPayExportFileName(string outputFolder, string pairName)
            => $"{outputFolder}{pairName}_{PayExportFileName}";

        private string GetExportFolder(ControlDB controlDB)
        {
            string siteFolder = GetExportSiteFolder(controlDB);
            string instanceFolder = GetExportInstanceFolder(controlDB);

            return $@"{siteFolder}{Path.DirectorySeparatorChar}{Namespace}{Path.DirectorySeparatorChar}Export{Path.DirectorySeparatorChar}{instanceFolder}{Path.DirectorySeparatorChar}archive";

        }

        private string GetExportSiteFolder(ControlDB controlDB)
        {
            string sql = $@"SELECT Value FROM SiteSetting WITH(NOLOCK) WHERE Name = 'BJE.ExportFolder'";

            var sqlUtil = new SqlUtil(controlDB.ConnectionString);
            return sqlUtil.GetValue<string>(sql);
        }

        private string GetExportInstanceFolder(ControlDB controlDB)
        {
            string sql = $@"SELECT FolderName
                            FROM
                                BackgroundTaskDefinition btd WITH(NOLOCK)
                                    JOIN BackgroundJob bj WITH(NOLOCK) ON btd.BackgroundJobId = bj.BackgroundJobId
                            WHERE
                                bj.CodeName = 'PayrollExport'
                        ";

            var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(Namespace));
            return sqlUtil.GetValue<string>(sql);
        }

    }
}
