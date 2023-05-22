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
    public class TaskScheduler
    {
        private RunType TaskType { get; }
        public ProcessType ProcessType { get; }
        public string InputFile { get; }
        public string OutputFolder { get; }

        private Log _m_Log;

        public TaskScheduler(Dictionary<string, string> args)
        {
            TaskType = Program.GetRunType(args);
            InputFile = args[Program.INPUTFILE];
            OutputFolder = args[Program.OUTPUTFOLDER];
            ProcessType = (args[Program.PROCESSTYPE].ToUpperInvariant() == Program.COMPARERESULT)
                            ? ProcessType.COMPARERESULT
                            : ((args[Program.PROCESSTYPE].ToUpperInvariant() == Program.CHECKJOBSTATUS)
                                ? ProcessType.CHECKJOBSTATUS
                                : ProcessType.QUEUETASK);
            _m_Log = new Log($"{OutputFolder}{args[Program.LOGFILE]}");

            _m_Log.AppendLine(Environment.NewLine + "====================================================================");
            _m_Log.AppendLine($"Command line parameters: RunType={TaskType} ProcessType={ProcessType} InputFile={InputFile} OutputFolder={OutputFolder} LogFile={args[Program.LOGFILE]}");
        }

        public void Run()
        {
            if (ProcessType == ProcessType.QUEUETASK)
            {
                if (TaskType == RunType.EXPORT)
                {
                    _m_Log.AppendLine("Queueing Export tasks...");
                    ScheduleTasks(LoadInputFile<PayExportTask>());
                }
                else if (TaskType == RunType.PSR)
                {
                    _m_Log.AppendLine("Queueing PSR tasks...");
                    ScheduleTasks(LoadInputFile<PSRTask>());
                }
                else if (TaskType == RunType.JOBSTEPRECALC)
                {
                    _m_Log.AppendLine("Queueing Job Step Recalc tasks...");
                    ScheduleTasks(LoadInputFile<JobStepRecalcTask>());
                }
                else if (TaskType == RunType.BASERATERECALC)
                {
                    _m_Log.AppendLine("Queueing Base Rate Recalc tasks...");
                    ScheduleTasks(LoadInputFile<BaseRateRecalcTask>());
                }
                else if (TaskType == RunType.AWARDENTITLEMENT)
                {
                    _m_Log.AppendLine("Queueing Award Entitlement tasks...");
                    ScheduleTasks(LoadInputFile<AwardEntitlementTask>());
                }
                else if (TaskType == RunType.SCR)
                {
                    _m_Log.AppendLine("Queueing Schedule Cost Recalc (SCR) tasks...");
                    ScheduleTasks(LoadInputFile<SCRTask>());
                }
            }
            else if (ProcessType == ProcessType.COMPARERESULT)
            {
                if (TaskType == RunType.EXPORT)
                {
                    _m_Log.AppendLine("Comparing Export results...");
                    CompareResults(LoadInputFile<PayExportTask>());
                }
                else if (TaskType == RunType.PSR)
                {
                    _m_Log.AppendLine("Comparing PSR results...");
                    CompareResults(LoadInputFile<PSRTask>());
                }
                else if (TaskType == RunType.JOBSTEPRECALC)
                {
                    _m_Log.AppendLine("Comparing Job Step Recalc results...");
                    CompareResults(LoadInputFile<JobStepRecalcTask>());
                }
                else if (TaskType == RunType.BASERATERECALC)
                {
                    _m_Log.AppendLine("Comparing Base Rate Recalc results...");
                    CompareResults(LoadInputFile<BaseRateRecalcTask>());
                }
                else if (TaskType == RunType.AWARDENTITLEMENT)
                {
                    _m_Log.AppendLine("Comparing Award Entitlement results...");
                    CompareResults(LoadInputFile<AwardEntitlementTask>());
                }
                else if (TaskType == RunType.SCR)
                {
                    _m_Log.AppendLine("Comparing SCR results...");
                    CompareResults(LoadInputFile<SCRTask>());
                }

            }
            else if (ProcessType == ProcessType.CHECKJOBSTATUS)
            {
                if (TaskType == RunType.EXPORT)
                {
                    _m_Log.AppendLine("Checking Export job status...");
                    CheckJobStatus(LoadInputFile<PayExportTask>());
                }
                else if (TaskType == RunType.PSR)
                {
                    _m_Log.AppendLine("Checking PSR job status...");
                    CheckJobStatus(LoadInputFile<PSRTask>());
                }
                else if (TaskType == RunType.JOBSTEPRECALC)
                {
                    _m_Log.AppendLine("Checking Job Step Recalc job status...");
                    CheckJobStatus(LoadInputFile<JobStepRecalcTask>());
                }
                else if (TaskType == RunType.BASERATERECALC)
                {
                    _m_Log.AppendLine("Checking Base Rate Recalc job status...");
                    CheckJobStatus(LoadInputFile<BaseRateRecalcTask>());
                }
                else if (TaskType == RunType.AWARDENTITLEMENT)
                {
                    _m_Log.AppendLine("Checking Award Entitlement job status...");
                    CheckJobStatus(LoadInputFile<AwardEntitlementTask>());
                }
                else if (TaskType == RunType.SCR)
                {
                    _m_Log.AppendLine("Checking SCR job status...");
                    CheckJobStatus(LoadInputFile<SCRTask>());
                }

            }
        }

        private TaskParameter<T> LoadInputFile<T>()
        {
            string jsonText = ReadInputFile();
            return JsonConvert.DeserializeObject<TaskParameter<T>>(jsonText);
        }

        private string ReadInputFile()
        {
            using (var reader = new StreamReader(InputFile))
            {
                return reader.ReadToEnd();
            }
        }

        private void SetLogForTasks(Task task1, Task task2)
        {
            task1?.SetLog(_m_Log);
            task2?.SetLog(_m_Log);
        }

        private void ScheduleTasks(TaskParameter<PSRTask> taskParam)
        {
            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                if (task.ForceCompareOnly)
                {
                    _m_Log.AppendLine($"No tasks were queued for pair {task.Name} because ForceCompareOnly mode = \"true\"...");
                    continue;
                }

                if (task.Task1?.IsScheduleTaskRequired() ?? false)
                {
                    _m_Log.AppendLine($"Queueing task {nameof(task.Task1)} for instance '{task.Task1.Namespace}'...");
                    ScheduleTask(taskParam.GetControlDB(task.Task1.Namespace), task.Task1);
                }

                if (task.Task2?.IsScheduleTaskRequired() ?? false)
                {
                    _m_Log.AppendLine($"Queueing task {nameof(task.Task2)} for instance '{task.Task2.Namespace}'...");
                    ScheduleTask(taskParam.GetControlDB(task.Task2.Namespace), task.Task2);
                }
            }

            SaveTasksStatus(taskParam);
        }

        private void ScheduleTasks(TaskParameter<PayExportTask> taskParam)
        {
            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                if (task.ForceCompareOnly)
                {
                    _m_Log.AppendLine($"No tasks were queued for pair {task.Name} because ForceCompareOnly mode = \"true\"...");
                    continue;
                }

                if (task.Task1?.IsScheduleTaskRequired() ?? false)
                {
                    _m_Log.AppendLine($"Queueing task {nameof(task.Task1)} for instance '{task.Task1.Namespace}'...");
                    ScheduleTask(taskParam.GetControlDB(task.Task1.Namespace), task.Task1);
                }

                if (task.Task2?.IsScheduleTaskRequired() ?? false)
                {
                    _m_Log.AppendLine($"Queueing task {nameof(task.Task2)} for instance '{task.Task2.Namespace}'...");
                    ScheduleTask(taskParam.GetControlDB(task.Task2.Namespace), task.Task2);
                }
            }

            SaveTasksStatus(taskParam);
        }

        private void ScheduleTasks(TaskParameter<JobStepRecalcTask> taskParam)
        {
            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                if (task.ForceCompareOnly)
                {
                    _m_Log.AppendLine($"No tasks were queued for pair {task.Name} because ForceCompareOnly mode = \"true\"...");
                    continue;
                }

                if (task.Task1?.IsScheduleTaskRequired() ?? false)
                {
                    _m_Log.AppendLine($"Queueing task {nameof(task.Task1)} for instance '{task.Task1.Namespace}'...");
                    ScheduleTask(taskParam.GetControlDB(task.Task1.Namespace), task.Task1);
                }

                if (task.Task2?.IsScheduleTaskRequired() ?? false)
                {
                    _m_Log.AppendLine($"Queueing task {nameof(task.Task2)} for instance '{task.Task2.Namespace}'...");
                    ScheduleTask(taskParam.GetControlDB(task.Task2.Namespace), task.Task2);
                }
            }

            SaveTasksStatus(taskParam);
        }

        private void ScheduleTasks(TaskParameter<BaseRateRecalcTask> taskParam)
        {
            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                if (task.ForceCompareOnly)
                {
                    _m_Log.AppendLine($"No tasks were queued for pair {task.Name} because ForceCompareOnly mode = \"true\"...");
                    continue;
                }

                if (task.Task1?.IsScheduleTaskRequired() ?? false)
                {
                    _m_Log.AppendLine($"Queueing task {nameof(task.Task1)} for instance '{task.Task1.Namespace}'...");
                    ScheduleTask(taskParam.GetControlDB(task.Task1.Namespace), task.Task1);
                }

                if (task.Task2?.IsScheduleTaskRequired() ?? false)
                {
                    _m_Log.AppendLine($"Queueing task {nameof(task.Task2)} for instance '{task.Task2.Namespace}'...");
                    ScheduleTask(taskParam.GetControlDB(task.Task2.Namespace), task.Task2);
                }
            }

            SaveTasksStatus(taskParam);
        }

        private void ScheduleTasks(TaskParameter<AwardEntitlementTask> taskParam)
        {
            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                if (task.ForceCompareOnly)
                {
                    _m_Log.AppendLine($"No tasks were queued for pair {task.Name} because ForceCompareOnly mode = \"true\"...");
                    continue;
                }

                if (task.Task1?.IsScheduleTaskRequired() ?? false)
                {
                    _m_Log.AppendLine($"Queueing task {nameof(task.Task1)} for instance '{task.Task1.Namespace}'...");
                    ScheduleTask(taskParam.GetControlDB(task.Task1.Namespace), task.Task1);
                }

                if (task.Task2?.IsScheduleTaskRequired() ?? false)
                {
                    _m_Log.AppendLine($"Queueing task {nameof(task.Task2)} for instance '{task.Task2.Namespace}'...");
                    ScheduleTask(taskParam.GetControlDB(task.Task2.Namespace), task.Task2);
                }
            }

            SaveTasksStatus(taskParam);
        }

        private void ScheduleTasks(TaskParameter<SCRTask> taskParam)
        {
            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                if (task.ForceCompareOnly)
                {
                    _m_Log.AppendLine($"No tasks were queued for pair {task.Name} because ForceCompareOnly mode = \"true\"...");
                    continue;
                }

                if (task.Task1?.IsScheduleTaskRequired() ?? false)
                {
                    _m_Log.AppendLine($"Queueing task {nameof(task.Task1)} for instance '{task.Task1.Namespace}'...");
                    ScheduleTask(taskParam.GetControlDB(task.Task1.Namespace), task.Task1);
                }

                if (task.Task2?.IsScheduleTaskRequired() ?? false)
                {
                    _m_Log.AppendLine($"Queueing task {nameof(task.Task2)} for instance '{task.Task2.Namespace}'...");
                    ScheduleTask(taskParam.GetControlDB(task.Task2.Namespace), task.Task2);
                }
            }

            SaveTasksStatus(taskParam);
        }

        private void SaveTasksStatus(TaskParameter<PSRTask> taskParam)
        {
            BackupInputFile();
            Program.SerializeToFile(InputFile, taskParam);
        }

        private void SaveTasksStatus(TaskParameter<PayExportTask> taskParam)
        {
            BackupInputFile();
            Program.SerializeToFile(InputFile, taskParam);
        }

        private void SaveTasksStatus(TaskParameter<JobStepRecalcTask> taskParam)
        {
            BackupInputFile();
            Program.SerializeToFile(InputFile, taskParam);
        }

        private void SaveTasksStatus(TaskParameter<BaseRateRecalcTask> taskParam)
        {
            BackupInputFile();
            Program.SerializeToFile(InputFile, taskParam);
        }

        private void SaveTasksStatus(TaskParameter<AwardEntitlementTask> taskParam)
        {
            BackupInputFile();
            Program.SerializeToFile(InputFile, taskParam);
        }

        private void SaveTasksStatus(TaskParameter<SCRTask> taskParam)
        {
            BackupInputFile();
            Program.SerializeToFile(InputFile, taskParam);
        }

        private bool BackupInputFile()
        {
            FileInfo fi = new FileInfo(InputFile);

            try
            {
                string fileName = fi.Name;
                fileName = fileName.Replace(fi.Extension, null);
                fileName = $@"{fi.DirectoryName}{Path.DirectorySeparatorChar}{fileName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}{fi.Extension}";

                fi.CopyTo(fileName, true);
                return true;
            }
            catch (Exception e)
            {
                _m_Log.WriteLine($"FAILED to backup file '{fi.FullName}'.  Error:");
                _m_Log.WriteLine(e.Message);
                return false;
            }

        }


        private void ScheduleTask(ControlDB controlDB, PSRTask task)
        {
            string sql = task.GetJobSql(controlDB);

            try
            {
                var sqlUtil = new SqlUtil(controlDB.ConnectionString);
                task.LogId = sqlUtil.GetValue<string>(sql);

                if (!String.IsNullOrEmpty(task.LogId))
                {
                    task.Status = TaskStatus.JobQueued;
                    _m_Log.AppendLine($"Task for instance '{task.Namespace}' is successfully queued.  LogId = '{task.LogId}'");
                }
                else
                {
                    task.Status = TaskStatus.JobQueueFailed;
                    _m_Log.AppendLine($"FAILED to queue task for instance '{task.Namespace}'.");
                }

            }
            catch (Exception e)
            {
                task.Status = TaskStatus.JobQueueFailed;
                _m_Log.AppendLine($"FAILED to queue task for instance '{task.Namespace}'.");
                _m_Log.AppendLine(e.Message);
            }
        }

        private void ScheduleTask(ControlDB controlDB, PayExportTask task)
        {
            string sql = task.GetJobSql(controlDB);

            try
            {
                var sqlUtil = new SqlUtil(controlDB.ConnectionString);
                task.LogId = sqlUtil.GetValue<string>(sql);

                if (!String.IsNullOrEmpty(task.LogId))
                {
                    task.Status = TaskStatus.JobQueued;
                    _m_Log.AppendLine($"Task for instance '{task.Namespace}' is successfully queued.  LogId = '{task.LogId}'");
                }
                else
                {
                    task.Status = TaskStatus.JobQueueFailed;
                    _m_Log.AppendLine($"FAILED to queue task for instance '{task.Namespace}'.");
                }

            }
            catch (Exception e)
            {
                task.Status = TaskStatus.JobQueueFailed;
                _m_Log.AppendLine($"FAILED to queue task for instance '{task.Namespace}'.");
                _m_Log.AppendLine(e.Message);
            }
        }

        private void ScheduleTask(ControlDB controlDB, JobStepRecalcTask task)
        {
            string sql = task.GetJobSql(controlDB);

            try
            {
                var sqlUtil = new SqlUtil(controlDB.ConnectionString);
                task.LogId = sqlUtil.GetValue<string>(sql);

                if (!String.IsNullOrEmpty(task.LogId))
                {
                    task.Status = TaskStatus.JobQueued;
                    _m_Log.AppendLine($"Task for instance '{task.Namespace}' is successfully queued.  LogId = '{task.LogId}'");
                }
                else
                {
                    task.Status = TaskStatus.JobQueueFailed;
                    _m_Log.AppendLine($"FAILED to queue task for instance '{task.Namespace}'.");
                }

            }
            catch (Exception e)
            {
                task.Status = TaskStatus.JobQueueFailed;
                _m_Log.AppendLine($"FAILED to queue task for instance '{task.Namespace}'.");
                _m_Log.AppendLine(e.Message);
            }
        }

        private void ScheduleTask(ControlDB controlDB, BaseRateRecalcTask task)
        {
            string sql = task.GetJobSql(controlDB);

            try
            {
                var sqlUtil = new SqlUtil(controlDB.ConnectionString);
                task.LogId = sqlUtil.GetValue<string>(sql);

                if (!String.IsNullOrEmpty(task.LogId))
                {
                    task.Status = TaskStatus.JobQueued;
                    _m_Log.AppendLine($"Task for instance '{task.Namespace}' is successfully queued.  LogId = '{task.LogId}'");
                }
                else
                {
                    task.Status = TaskStatus.JobQueueFailed;
                    _m_Log.AppendLine($"FAILED to queue task for instance '{task.Namespace}'.");
                }

            }
            catch (Exception e)
            {
                task.Status = TaskStatus.JobQueueFailed;
                _m_Log.AppendLine($"FAILED to queue task for instance '{task.Namespace}'.");
                _m_Log.AppendLine(e.Message);
            }
        }

        private void ScheduleTask(ControlDB controlDB, AwardEntitlementTask task)
        {
            string sql = task.GetJobSql(controlDB);

            try
            {
                var sqlUtil = new SqlUtil(controlDB.ConnectionString);
                task.LogId = sqlUtil.GetValue<string>(sql);

                if (!String.IsNullOrEmpty(task.LogId))
                {
                    task.Status = TaskStatus.JobQueued;
                    _m_Log.AppendLine($"Task for instance '{task.Namespace}' is successfully queued.  LogId = '{task.LogId}'");
                }
                else
                {
                    task.Status = TaskStatus.JobQueueFailed;
                    _m_Log.AppendLine($"FAILED to queue task for instance '{task.Namespace}'.");
                }

            }
            catch (Exception e)
            {
                task.Status = TaskStatus.JobQueueFailed;
                _m_Log.AppendLine($"FAILED to queue task for instance '{task.Namespace}'.");
                _m_Log.AppendLine(e.Message);
            }
        }

        private void ScheduleTask(ControlDB controlDB, SCRTask task)
        {
            string sql = task.GetJobSql(controlDB);

            try
            {
                var sqlUtil = new SqlUtil(controlDB.ConnectionString);
                task.LogId = sqlUtil.GetValue<string>(sql);

                if (!String.IsNullOrEmpty(task.LogId))
                {
                    task.Status = TaskStatus.JobQueued;
                    _m_Log.AppendLine($"Task for instance '{task.Namespace}' is successfully queued.  LogId = '{task.LogId}'");
                }
                else
                {
                    task.Status = TaskStatus.JobQueueFailed;
                    _m_Log.AppendLine($"FAILED to queue task for instance '{task.Namespace}'.");
                }

            }
            catch (Exception e)
            {
                task.Status = TaskStatus.JobQueueFailed;
                _m_Log.AppendLine($"FAILED to queue task for instance '{task.Namespace}'.");
                _m_Log.AppendLine(e.Message);
            }
        }


        private void CheckJobStatus(TaskParameter<PSRTask> taskParam)
        {
            bool saveStatus = false;

            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                ControlDB controlDB1 = null;
                ControlDB controlDB2 = null;

                if (task.Task1?.IsJobQueuedOrInProgress() ?? false)
                {
                    IsJobCompletedSuccessfully(controlDB1 = taskParam.GetControlDB(task.Task1.Namespace), task.Task1);
                    saveStatus |= true;
                }

                if (task.Task2?.IsJobQueuedOrInProgress() ?? false)
                {
                    IsJobCompletedSuccessfully(controlDB2 = taskParam.GetControlDB(task.Task2.Namespace), task.Task2);
                    saveStatus |= true;
                }
            }

            if (saveStatus)
                SaveTasksStatus(taskParam);
        }

        private void CheckJobStatus(TaskParameter<PayExportTask> taskParam)
        {
            bool saveStatus = false;

            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                ControlDB controlDB1 = null;
                ControlDB controlDB2 = null;

                if (task.Task1?.IsJobQueuedOrInProgress() ?? false)
                {
                    IsJobCompletedSuccessfully(controlDB1 = taskParam.GetControlDB(task.Task1.Namespace), task.Task1);
                    saveStatus |= true;
                }

                if (task.Task2?.IsJobQueuedOrInProgress() ?? false)
                {
                    IsJobCompletedSuccessfully(controlDB2 = taskParam.GetControlDB(task.Task2.Namespace), task.Task2);
                    saveStatus |= true;
                }
            }

            if (saveStatus)
                SaveTasksStatus(taskParam);
        }

        private void CheckJobStatus(TaskParameter<JobStepRecalcTask> taskParam)
        {
            bool saveStatus = false;

            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                ControlDB controlDB1 = null;
                ControlDB controlDB2 = null;

                if (task.Task1?.IsJobQueuedOrInProgress() ?? false)
                {
                    IsJobCompletedSuccessfully(controlDB1 = taskParam.GetControlDB(task.Task1.Namespace), task.Task1);
                    saveStatus |= true;
                }

                if (task.Task2?.IsJobQueuedOrInProgress() ?? false)
                {
                    IsJobCompletedSuccessfully(controlDB2 = taskParam.GetControlDB(task.Task2.Namespace), task.Task2);
                    saveStatus |= true;
                }
            }

            if (saveStatus)
                SaveTasksStatus(taskParam);
        }

        private void CheckJobStatus(TaskParameter<BaseRateRecalcTask> taskParam)
        {
            bool saveStatus = false;

            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                ControlDB controlDB1 = null;
                ControlDB controlDB2 = null;

                if (task.Task1?.IsJobQueuedOrInProgress() ?? false)
                {
                    IsJobCompletedSuccessfully(controlDB1 = taskParam.GetControlDB(task.Task1.Namespace), task.Task1);
                    saveStatus |= true;
                }

                if (task.Task2?.IsJobQueuedOrInProgress() ?? false)
                {
                    IsJobCompletedSuccessfully(controlDB2 = taskParam.GetControlDB(task.Task2.Namespace), task.Task2);
                    saveStatus |= true;
                }
            }

            if (saveStatus)
                SaveTasksStatus(taskParam);
        }

        private void CheckJobStatus(TaskParameter<AwardEntitlementTask> taskParam)
        {
            bool saveStatus = false;

            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                ControlDB controlDB1 = null;
                ControlDB controlDB2 = null;

                if (task.Task1?.IsJobQueuedOrInProgress() ?? false)
                {
                    IsJobCompletedSuccessfully(controlDB1 = taskParam.GetControlDB(task.Task1.Namespace), task.Task1);
                    saveStatus |= true;
                }

                if (task.Task2?.IsJobQueuedOrInProgress() ?? false)
                {
                    IsJobCompletedSuccessfully(controlDB2 = taskParam.GetControlDB(task.Task2.Namespace), task.Task2);
                    saveStatus |= true;
                }
            }

            if (saveStatus)
                SaveTasksStatus(taskParam);
        }

        private void CheckJobStatus(TaskParameter<SCRTask> taskParam)
        {
            bool saveStatus = false;

            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                ControlDB controlDB1 = null;
                ControlDB controlDB2 = null;

                if (task.Task1?.IsJobQueuedOrInProgress() ?? false)
                {
                    IsJobCompletedSuccessfully(controlDB1 = taskParam.GetControlDB(task.Task1.Namespace), task.Task1);
                    saveStatus |= true;
                }

                if (task.Task2?.IsJobQueuedOrInProgress() ?? false)
                {
                    IsJobCompletedSuccessfully(controlDB2 = taskParam.GetControlDB(task.Task2.Namespace), task.Task2);
                    saveStatus |= true;
                }
            }

            if (saveStatus)
                SaveTasksStatus(taskParam);
        }


        private void CompareResults(TaskParameter<PSRTask> taskParam)
        {
            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                string forceText = task.ForceCompareOnly ? "*forcely* " : "";
                _m_Log.AppendLine($"Comparing {forceText}results for task pair {task.Name}...");

                task.Task1.RunTime = task.Task2.RunTime = DateTime.Now;

                ControlDB controlDB1 = null;
                ControlDB controlDB2 = null;

                if (!IsOkToCompare(task.Name, task.Task1, task.Task2, task.ForceCompareOnly)
                    || !IsJobCompletedSuccessfully(controlDB1 = taskParam.GetControlDB(task.Task1.Namespace), task.Task1, task.ForceCompareOnly)
                    || !IsJobCompletedSuccessfully(controlDB2 = taskParam.GetControlDB(task.Task2.Namespace), task.Task2, task.ForceCompareOnly))
                    continue;

                if ((task.Task1?.FetchRecords(controlDB1, task.Name, task.ForceCompareOnly, OutputFolder) ?? false)
                    && (task.Task2?.FetchRecords(controlDB2, task.Name, task.ForceCompareOnly, OutputFolder) ?? false))
                {
                    if (task.Task1?.CompareFiles(task.Task2, task.Name, OutputFolder) ?? false)
                        _m_Log.AppendLine($@"**SUCCESS: Pay summary results are the same for instances '{task.Task1.Namespace}' and '{task.Task2.Namespace}'.");
                    else
                        _m_Log.AppendLine($@"**WARNING: Pay summary results are different for instances '{task.Task1.Namespace}' and '{task.Task2.Namespace}'.");
                }
            }

            SaveTasksStatus(taskParam);
        }

        private void CompareResults(TaskParameter<PayExportTask> taskParam)
        {
            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                string forceText = task.ForceCompareOnly ? "*forcely* " : "";
                _m_Log.AppendLine($"Comparing {forceText}results for task pair {task.Name}...");

                task.Task1.RunTime = task.Task2.RunTime = DateTime.Now;

                ControlDB controlDB1 = null;
                ControlDB controlDB2 = null;

                if (!IsOkToCompare(task.Name, task.Task1, task.Task2, task.ForceCompareOnly)
                    || !IsJobCompletedSuccessfully(controlDB1 = taskParam.GetControlDB(task.Task1.Namespace), task.Task1, task.ForceCompareOnly)
                    || !IsJobCompletedSuccessfully(controlDB2 = taskParam.GetControlDB(task.Task2.Namespace), task.Task2, task.ForceCompareOnly))
                    continue;

                if ((task.Task1?.FetchFile(controlDB1, task.Name, OutputFolder) ?? false)
                    && (task.Task2?.FetchFile(controlDB2, task.Name, OutputFolder) ?? false))
                {
                    if ((task.Task1?.CompareFiles(task.Task2, task.Name, OutputFolder)) ?? false)
                        _m_Log.AppendLine($@"**SUCCESS: Pay export results are the same for instances '{task.Task1.Namespace}' and '{task.Task2.Namespace}'.");
                    else
                        _m_Log.AppendLine($@"**WARNING: Pay export results are different for instances '{task.Task1.Namespace}' and '{task.Task2.Namespace}'.");
                }
            }

            SaveTasksStatus(taskParam);

        }

        private void CompareResults(TaskParameter<JobStepRecalcTask> taskParam)
        {
            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                string forceText = task.ForceCompareOnly ? "*forcely* " : "";
                _m_Log.AppendLine($"Comparing {forceText}results for task pair {task.Name}...");

                task.Task1.RunTime = task.Task2.RunTime = DateTime.Now;

                ControlDB controlDB1 = null;
                ControlDB controlDB2 = null;

                if (!IsOkToCompare(task.Name, task.Task1, task.Task2, task.ForceCompareOnly)
                    || !IsJobCompletedSuccessfully(controlDB1 = taskParam.GetControlDB(task.Task1.Namespace), task.Task1, task.ForceCompareOnly)
                    || !IsJobCompletedSuccessfully(controlDB2 = taskParam.GetControlDB(task.Task2.Namespace), task.Task2, task.ForceCompareOnly))
                    continue;

                if ((task.Task1?.FetchRecords(controlDB1, task.Name, task.ForceCompareOnly, OutputFolder) ?? false)
                    && (task.Task2?.FetchRecords(controlDB2, task.Name, task.ForceCompareOnly, OutputFolder) ?? false))
                {
                    if (task.Task1?.CompareFiles(task.Task2, task.Name, OutputFolder) ?? false)
                        _m_Log.AppendLine($@"**SUCCESS: Job Step results are the same for instances '{task.Task1.Namespace}' and '{task.Task2.Namespace}'.");
                    else
                        _m_Log.AppendLine($@"**WARNING: Job Step results are different for instances '{task.Task1.Namespace}' and '{task.Task2.Namespace}'.");
                }
            }

            SaveTasksStatus(taskParam);
        }

        private void CompareResults(TaskParameter<BaseRateRecalcTask> taskParam)
        {
            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                string forceText = task.ForceCompareOnly ? "*forcely* " : "";
                _m_Log.AppendLine($"Comparing {forceText}results for task pair {task.Name}...");

                task.Task1.RunTime = task.Task2.RunTime = DateTime.Now;

                ControlDB controlDB1 = null;
                ControlDB controlDB2 = null;

                if (!IsOkToCompare(task.Name, task.Task1, task.Task2, task.ForceCompareOnly)
                    || !IsJobCompletedSuccessfully(controlDB1 = taskParam.GetControlDB(task.Task1.Namespace), task.Task1, task.ForceCompareOnly)
                    || !IsJobCompletedSuccessfully(controlDB2 = taskParam.GetControlDB(task.Task2.Namespace), task.Task2, task.ForceCompareOnly))
                    continue;

                if ((task.Task1?.FetchRecords(controlDB1, task.Name, task.ForceCompareOnly, OutputFolder) ?? false)
                    && (task.Task2?.FetchRecords(controlDB2, task.Name, task.ForceCompareOnly, OutputFolder) ?? false))
                {
                    if (task.Task1?.CompareFiles(task.Task2, task.Name, OutputFolder) ?? false)
                        _m_Log.AppendLine($@"**SUCCESS: Base Rate results are the same for instances '{task.Task1.Namespace}' and '{task.Task2.Namespace}'.");
                    else
                        _m_Log.AppendLine($@"**WARNING: Base Rate results are different for instances '{task.Task1.Namespace}' and '{task.Task2.Namespace}'.");
                }
            }

            SaveTasksStatus(taskParam);
        }

        private void CompareResults(TaskParameter<AwardEntitlementTask> taskParam)
        {
            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                string forceText = task.ForceCompareOnly ? "*forcely* " : "";
                _m_Log.AppendLine($"Comparing {forceText}results for task pair {task.Name}...");

                task.Task1.RunTime = task.Task2.RunTime = DateTime.Now;

                ControlDB controlDB1 = null;
                ControlDB controlDB2 = null;

                if (!IsOkToCompare(task.Name, task.Task1, task.Task2, task.ForceCompareOnly)
                    || !IsJobCompletedSuccessfully(controlDB1 = taskParam.GetControlDB(task.Task1.Namespace), task.Task1, task.ForceCompareOnly)
                    || !IsJobCompletedSuccessfully(controlDB2 = taskParam.GetControlDB(task.Task2.Namespace), task.Task2, task.ForceCompareOnly))
                    continue;

                if ((task.Task1?.FetchRecords(controlDB1, task.Name, task.ForceCompareOnly, OutputFolder) ?? false)
                    && (task.Task2?.FetchRecords(controlDB2, task.Name, task.ForceCompareOnly, OutputFolder) ?? false))
                {
                    if (task.Task1?.CompareFiles(task.Task2, task.Name, OutputFolder) ?? false)
                        _m_Log.AppendLine($@"**SUCCESS: Award Entitlement results are the same for instances '{task.Task1.Namespace}' and '{task.Task2.Namespace}'.");
                    else
                        _m_Log.AppendLine($@"**WARNING: Award Entitlement results are different for instances '{task.Task1.Namespace}' and '{task.Task2.Namespace}'.");
                }
            }

            SaveTasksStatus(taskParam);
        }

        private void CompareResults(TaskParameter<SCRTask> taskParam)
        {
            foreach (var task in taskParam.Tasks)
            {
                SetLogForTasks(task.Task1, task.Task2);

                string forceText = task.ForceCompareOnly ? "*forcely* " : "";
                _m_Log.AppendLine($"Comparing {forceText}results for task pair {task.Name}...");

                task.Task1.RunTime = task.Task2.RunTime = DateTime.Now;

                ControlDB controlDB1 = null;
                ControlDB controlDB2 = null;

                if (!IsOkToCompare(task.Name, task.Task1, task.Task2, task.ForceCompareOnly)
                    || !IsJobCompletedSuccessfully(controlDB1 = taskParam.GetControlDB(task.Task1.Namespace), task.Task1, task.ForceCompareOnly)
                    || !IsJobCompletedSuccessfully(controlDB2 = taskParam.GetControlDB(task.Task2.Namespace), task.Task2, task.ForceCompareOnly))
                    continue;

                if ((task.Task1?.FetchRecords(controlDB1, task.Name, task.ForceCompareOnly, OutputFolder) ?? false)
                    && (task.Task2?.FetchRecords(controlDB2, task.Name, task.ForceCompareOnly, OutputFolder) ?? false))
                {
                    if (task.Task1?.CompareFiles(task.Task2, task.Name, OutputFolder) ?? false)
                        _m_Log.AppendLine($@"**SUCCESS: Schedule cost results are the same for instances '{task.Task1.Namespace}' and '{task.Task2.Namespace}'.");
                    else
                        _m_Log.AppendLine($@"**WARNING: Schedule cost results are different for instances '{task.Task1.Namespace}' and '{task.Task2.Namespace}'.");
                }
            }

            SaveTasksStatus(taskParam);
        }

        private bool IsOkToCompare(string pairName, Task task1, Task task2, bool forceCompareOnly = false)
        {
            if ((task1?.GetIsRunTask() ?? false) != (task2?.GetIsRunTask() ?? false))
            {
                string instanceName = "NO INSTANCE";

                if (!(task1?.GetIsRunTask() ?? false))
                    instanceName = task1?.GetNamespace() ?? "NO INSTANCE";
                else if (!(task2?.GetIsRunTask() ?? false))
                    instanceName = task2?.GetNamespace() ?? "NO INSTANCE";

                _m_Log.AppendLine($"Cannot compare files for the PSR task pair {pairName} because the instance '{instanceName}' is not runnable.");
                return false;
            }

            if (!(task1?.GetIsRunTask() ?? false))
            {
                _m_Log.AppendLine($"Comparison is skipped for the task pair {pairName} because both instances are not runnable.");
                return false;
            }

            if (forceCompareOnly)
                return true;

            if (String.IsNullOrEmpty(task1?.GetLogId()))
            {
                _m_Log.AppendLine($"Comparison is skipped for the task pair {pairName} because the PSR task for instance {task1.GetNamespace()} was not queued.");
                return false;
            }

            if (String.IsNullOrEmpty(task2?.GetLogId()))
            {
                _m_Log.AppendLine($"Comparison is skipped for the task pair {pairName} because the PSR task for instance {task2.GetNamespace()} was not queued.");
                return false;
            }

            return true;

        }

        private bool IsJobCompletedSuccessfully(ControlDB controlDB, Task task, bool forceCompareOnly = false)
        {
            if (forceCompareOnly)
                return true;

            int status = 0;

            try
            {
                string sql = $@"SELECT BackgroundJobStatusId, ExecStartTime, ExecEndTime FROM BackgroundJobLog WITH(NOLOCK) WHERE LogId = '{task.GetLogId()}'";
                var sqlUtil = new SqlUtil(controlDB.GetInstanceDBConnectionString(task.GetNamespace()));
                DataTable dt = sqlUtil.GetResultSet(sql);

                status = GetJobStatus(dt, task);
            }
            catch (Exception e)
            {
                _m_Log.AppendLine($"FAILED to check job status for instance '{task.GetNamespace()}'.  LogId={task.GetLogId()}");
                _m_Log.AppendLine(e.Message);
            }

            return status == 1;
        }

        private int GetJobStatus(DataTable dt, Task task)
        {
            if ((dt?.Rows?.Count ?? 0) <= 0)
            {
                task.SetStatus(TaskStatus.JobQueued);
                _m_Log.AppendLine($"Cannot determine the job status for the task for instance '{task.GetNamespace()}'.");
                return 0;
            }

            DataRow row = dt.Rows[0];
            int status = (row["BackgroundJobStatusId"] == DBNull.Value) ? 0 : Convert.ToInt32(row["BackgroundJobStatusId"]);
            DateTime startTime = (row["ExecStartTime"] == DBNull.Value) ? DateTime.MinValue : Convert.ToDateTime(row["ExecStartTime"]);
            DateTime endTime = (row["ExecEndTime"] == DBNull.Value) ? DateTime.MinValue : Convert.ToDateTime(row["ExecEndTime"]);

            if (status == 1)    // completed
            {
                task.SetStatus(TaskStatus.JobCompleted);
                string durationText = (endTime - startTime).ToString(@"dd\.hh\:mm\:ss");
                _m_Log.AppendLine($"Task for instance '{task.GetNamespace()}' is completed successfully.  LogId={task.GetLogId()}  Start={startTime}  End={endTime}  Elapse={durationText}");
            }
            else if (status == 2)
            {
                task.SetStatus(TaskStatus.JobQueued);
                _m_Log.AppendLine($"Task for instance '{task.GetNamespace()}' is in the queue.");
            }
            else if (status == 3)
            {
                task.SetStatus(TaskStatus.JobFailed);
                _m_Log.AppendLine($"Task for instance '{task.GetNamespace()}' completed with errors.");
            }
            else if (status == 4) // In progress
            {
                task.SetStatus(TaskStatus.JobInProgress);
                _m_Log.AppendLine($"Task for instance '{task.GetNamespace()}' is in progress...");
            }
            else
            {
                task.SetStatus(TaskStatus.JobFailed);
                _m_Log.AppendLine($"Task for instance '{task.GetNamespace()}' failed with unknown reasons.");
            }

            return status;
        }

        static public int CompareFiles(string fileName1, string fileName2)
        {
            try
            {
                using (FileStream fs1 = new FileStream(fileName1, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (FileStream fs2 = new FileStream(fileName2, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader reader1 = new BinaryReader(fs1))
                using (BinaryReader reader2 = new BinaryReader(fs2))
                {
                    int bufferSize = 64 * 1024;

                    while (true)
                    {
                        byte[] data1 = reader1.ReadBytes(bufferSize);
                        byte[] data2 = reader2.ReadBytes(bufferSize);

                        if (data1.Length != data2.Length)
                            return 0;

                        if (data1.Length == 0)
                            return 1;

                        if (!data1.SequenceEqual(data2))
                            return 0;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
