using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ComparePay
{
    public class TaskParameter<T>
    {
        public Dictionary<string, List<string>/*namespace*/> ControlDBNamespace;
        public List<TaskPair<T>> Tasks;

        public string GetConnectionString(string instanceNamespace)
        {
            return (GetControlDB(instanceNamespace)?.ConnectionString) ?? "";
        }

        public ControlDB GetControlDB(string instanceNamespace)
        {
            return new ControlDB(ControlDBNamespace.FirstOrDefault(x => x.Value.Contains(instanceNamespace, StringComparer.InvariantCultureIgnoreCase)).Key);
        }
    }

    public static class TaskStatus
    {
        public const string JobQueued = "JobQueued";
        public const string JobQueueFailed = "JobQueueFailed";

        public const string JobInProgress = "JobInProgress";
        public const string JobCompleted = "JobCompleted";
        public const string JobFailed = "JobFailed";

        public const string ResultFetched = "ResultFetched";
        public const string ResultFetchFailed = "ResultFetchFailed";

        public const string CompareMismatch = "CompareMismatch";
        public const string CompareAllMatch = "CompareAllMatch";
        public const string CompareFailed = "CompareFailed";
    }

    public class TaskPair<T>
    {
        public string Name { get; set; }
        public bool ForceCompareOnly { get; set; }
        public T Task1 { get; set; }
        public T Task2 { get; set; }
    }

    public class Task
    {
        public virtual string GetNamespace() => "";

        public virtual void SetStatus(string status)
        {
            //NOTHING
        }

        public virtual bool GetIsRunTask() => false;
        public virtual string GetLogId() => "";

        public bool IsJobQueuedOrInProgress(bool IsRunTask, string Status)
            => ((IsRunTask) && (Status == TaskStatus.JobQueued || Status == TaskStatus.JobInProgress));

        public bool IsScheduleTaskRequired(bool IsRunTask, string Status)
            => ((IsRunTask) && (String.IsNullOrEmpty(Status) || Status == TaskStatus.JobQueueFailed || Status == TaskStatus.JobFailed));

        public bool IsFetchResultRequired(bool IsRunTask, string Status)
            => (IsRunTask) && (Status == TaskStatus.JobCompleted || Status == TaskStatus.ResultFetchFailed);

        public bool IsCompareTaskRequired(bool IsRunTask, string Status)
            => (IsRunTask) && (Status == TaskStatus.ResultFetched || Status == TaskStatus.CompareFailed);

        public virtual void SetLog(Log log)
        {
            // NOTHING
        }

        public virtual int GetClientId(ControlDB controlDB)
        {
            string sql = $@"SELECT ClientId FROM Namespace WITH(NOLOCK) WHERE Namespace = '{GetNamespace()}'";
            var sqlUtil = new SqlUtil(controlDB.ConnectionString);
            return sqlUtil.GetValue<int>(sql);
        }

        public virtual int GetClientId(ControlDB controlDB, string Namespace)
        {
            string sql = $@"SELECT ClientId FROM Namespace WITH(NOLOCK) WHERE Namespace = '{Namespace}'";
            var sqlUtil = new SqlUtil(controlDB.ConnectionString);
            return sqlUtil.GetValue<int>(sql);
        }

        public string GetSiteIdsByNames(ControlDB controlDB, string rawOrgUnit, string defaultValue = "")
        {
            string siteNameList = GetSiteNames(rawOrgUnit);
            if (String.IsNullOrEmpty(siteNameList))
                return defaultValue;

            string sql = $@"SELECT OrgUnitId FROM OrgUnit WITH(NOLOCK) WHERE ShortName IN ({siteNameList})";
            var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(GetNamespace()));

            return sqlUtil.GetValue<int>(sql).ToString();
        }

        public int GetJobIdForClient(ControlDB controlDB, string codeName)
        {
            string sql = $@"SELECT BackgroundJobId FROM BackgroundJob WITH(NOLOCK) WHERE CodeName = '{codeName}'";
            var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(GetNamespace()));

            return sqlUtil.GetValue<int>(sql);
        }

        public int GetJobDefinitionId(ControlDB controlDB, string codeName)
        {
            string sql = $@"SELECT BackgroundTaskDefinitionId FROM BackgroundTaskDefinition WITH(NOLOCK)
                            WHERE BackgroundJobId = (SELECT BackgroundJobId FROM BackgroundJob WITH(NOLOCK) WHERE CodeName = '{codeName}')";
            var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(GetNamespace()));

            return sqlUtil.GetValue<int>(sql);
        }

        public string GetSiteNames(string OrgUnit)
        {
            string siteNames = "";
            if (String.IsNullOrEmpty(OrgUnit))
                return siteNames;

            string[] orgUnits = OrgUnit.Split('~');
            foreach (var orgUnit in orgUnits)
            {
                if (!String.IsNullOrEmpty(siteNames))
                    siteNames += ",";

                siteNames += $"'{orgUnit}'";
            }

            return siteNames;
        }
    }

    class ParamXMLSerializer
    {
        public static string Serialize(IDictionary<string, object> data)
        {
            if (data == null || data.Count == 0 || data.Values.All(v => v == null))
            {
                return null;
            }

            var parameterXml = new XElement("Parameter");
            foreach (var e in data)
            {
                if (e.Value != null)
                {
                    parameterXml.Add(new XAttribute(e.Key, e.Value));
                }
            }

            return parameterXml.ToString(SaveOptions.DisableFormatting);
        }
    }
}
