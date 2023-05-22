using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace ComparePay
{
    public enum RunType { PSR = 0, EXPORT = 1, JOBSTEPRECALC = 2, BASERATERECALC = 3, AWARDENTITLEMENT = 4, SCR = 5 };
    public enum ProcessType { QUEUETASK = 0, COMPARERESULT = 1, CHECKJOBSTATUS = 2 };

    public class Program
    {
        public const string RUNTYPE = "RunType";
        public const string PROCESSTYPE = "ProcessType";
        public const string INPUTFILE = "InputFile";
        public const string OUTPUTFOLDER = "OutputFolder";
        public const string LOGFILE = "LogFile";

        private const string DEFAULT_LOGFILE = "ComparisonResult.txt";

        // Run Types
        public const string PSR = "PSR";
        public const string EXPORT = "EXPORT";
        public const string JOBSTEPRECALC = "JOBSTEPRECALC";
        public const string BASERATERECALC = "BASERATERECALC";
        public const string AWARDENTITLEMENT = "AWARDENTITLEMENT";
        public const string SCR = "SCR";

        // Process Types
        public const string QUEUETASK = "QUEUETASK";
        public const string COMPARERESULT = "COMPARERESULT";
        public const string CHECKJOBSTATUS = "CHECKJOBSTATUS";


        static void Main(string[] args)
        {
            Dictionary<string, string> cmdArgs = ParseCmdLineArguments(args);

            //TestSerializer(GetRunType(cmdArgs));

            if (!ValidateArgs(cmdArgs))
                return;

            TaskScheduler psrScheduler = new TaskScheduler(cmdArgs);
            psrScheduler.Run();

        }

        private static string Usage()
        {
            string fileName = Assembly.GetExecutingAssembly().CodeBase;
            fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);

            return $"Usage: {fileName} {{{RUNTYPE}={{{PSR}}}|{SCR}|{EXPORT}|{JOBSTEPRECALC}|{BASERATERECALC}|{AWARDENTITLEMENT}}} {{{PROCESSTYPE}={{{QUEUETASK}}}|{COMPARERESULT}|{CHECKJOBSTATUS}}} {INPUTFILE}=input_path_name {OUTPUTFOLDER}=output_folder";
        }

        private static bool ValidateArgs(Dictionary<string, string> args)
        {
            if (!HasValidArg(args, INPUTFILE)
                || !HasValidArg(args, OUTPUTFOLDER))
            {
                Console.WriteLine(Usage());
                return false;
            }

            if (!ValidateFileFolder(args))
                return false;

            if (!args.ContainsKey(PROCESSTYPE))
                args.Add(PROCESSTYPE, null);

            if (String.IsNullOrEmpty(args[PROCESSTYPE])
                || (
                        args[PROCESSTYPE].ToUpperInvariant() != COMPARERESULT
                        && args[PROCESSTYPE].ToUpperInvariant() != CHECKJOBSTATUS
                   )
               )
                args[PROCESSTYPE] = QUEUETASK;

            return true;
        }

        private static bool ValidateFileFolder(Dictionary<string, string> cmdArgs)
        {
            bool result = true;

            if (!File.Exists(cmdArgs[INPUTFILE]))
            {
                Console.WriteLine($"The file '{cmdArgs[INPUTFILE]}' does not exist.");
                result &= false;
            }

            if (!Directory.Exists(cmdArgs[OUTPUTFOLDER]))
            {
                Console.WriteLine($"The folder '{cmdArgs[OUTPUTFOLDER]}' does not exist.");
                result &= false;
            }

            cmdArgs[OUTPUTFOLDER] = (cmdArgs[OUTPUTFOLDER].TrimEnd(Path.AltDirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar)) + Path.DirectorySeparatorChar;

            if (!cmdArgs.ContainsKey(LOGFILE))
            {
                cmdArgs.Add(LOGFILE, null);
            }

            if (String.IsNullOrEmpty(cmdArgs[LOGFILE]))
            {
                cmdArgs[LOGFILE] = DEFAULT_LOGFILE;
            }

            return result;
        }

        public static RunType GetRunType(Dictionary<string, string> cmdArgs)
        {
            RunType runType = RunType.PSR;
            if (cmdArgs.ContainsKey(RUNTYPE))
            {
                if (cmdArgs[RUNTYPE].ToUpperInvariant() == EXPORT)
                    runType = RunType.EXPORT;
                else if (cmdArgs[RUNTYPE].ToUpperInvariant() == JOBSTEPRECALC)
                    runType = RunType.JOBSTEPRECALC;
                else if (cmdArgs[RUNTYPE].ToUpperInvariant() == BASERATERECALC)
                    runType = RunType.BASERATERECALC;
                else if (cmdArgs[RUNTYPE].ToUpperInvariant() == AWARDENTITLEMENT)
                    runType = RunType.AWARDENTITLEMENT;
                else if (cmdArgs[RUNTYPE].ToUpperInvariant() == SCR)
                    runType = RunType.SCR;
            }

            return runType;
        }

        private static Dictionary<string, string> ParseCmdLineArguments(string[] args)
        {
            var cmdArgs = new Dictionary<string, string>();

            foreach (string arg in args)
            {
                string[] option = arg.Split('=');

                if (option.Length != 2)
                    continue;

                if (!cmdArgs.ContainsKey(option[0]))
                    cmdArgs.Add(option[0], option[1]);
            }

            return cmdArgs;
        }

        private static bool HasValidArg(Dictionary<string, string> cmdArgs, string option)
        {
            string optionValue;

            if (!cmdArgs.TryGetValue(option, out optionValue) || String.IsNullOrEmpty(optionValue))
                return false;

            return true;
        }

        private static void TestSerializer(RunType runType)
        {
            if (runType == RunType.PSR)
                TestSerializerPSR();
            else if (runType == RunType.EXPORT)
                TestSerializerExport();
        }

        private static void TestSerializerPSR()
        {
            var taskParam = new TaskParameter<PSRTask>()
            {
                ControlDBNamespace = new Dictionary<string, List<string>>()
                                                        {
                                                            {
                                                                "Host1;TestDB",
                                                                new List<string>() { "Test11_8425", "Test12_8425" }
                                                            },
                                                            {
                                                                "Host2;TestDB2",
                                                                new List<string>() { "Test11_1000", "Test12_1000" }
                                                            }
                                                        },
                Tasks = new List<TaskPair<PSRTask>>()
                                        {
                                            new TaskPair<PSRTask>()
                                            {
                                                Name = "TestPSR_1",
                                                Task1 = new PSRTask()
                                                {
                                                    IsRunTask = true,
                                                    Namespace = "Test11_8425",
                                                    FromDate = new DateTime(2017, 9, 1),
                                                    ToDate = new DateTime(2018, 2, 28),
                                                    OrgUnit = "USA"
                                                },
                                                Task2 = new PSRTask()
                                                {
                                                    IsRunTask = true,
                                                    Namespace = "Test12_8425",
                                                    FromDate = new DateTime(2017, 9, 1),
                                                    ToDate = new DateTime(2018, 2, 28),
                                                    OrgUnit = "USA"
                                                }
                                            },
                                            new TaskPair<PSRTask>()
                                            {
                                                Name = "TestPSR_2",
                                                Task1 = new PSRTask()
                                                {
                                                    IsRunTask = true,
                                                    Namespace = "Test11_8425",
                                                    FromDate = new DateTime(2017, 9, 1),
                                                    ToDate = new DateTime(2018, 2, 28),
                                                    OrgUnit = "CANADA"
                                                },
                                                Task2 = new PSRTask()
                                                {
                                                    IsRunTask = true,
                                                    Namespace = "Test12_8425",
                                                    FromDate = new DateTime(2017, 9, 1),
                                                    ToDate = new DateTime(2018, 2, 28),
                                                    OrgUnit = "CANADA"
                                                }
                                            }
                                        }
            };

            SerializeToFile(@"D:\Development\ComparePay\TestInputPSR.json", taskParam);

        }

        private static void TestSerializerExport()
        {
            var taskParam = new TaskParameter<PayExportTask>()
            {
                ControlDBNamespace = new Dictionary<string, List<string>>()
                                        {
                                            {
                                                "Host1;TestDB",
                                                new List<string>() { "Test11_8425", "Test12_8425" }
                                            },
                                            {
                                                "Host2;TestDB2",
                                                new List<string>() { "Test11_1000", "Test12_1000" }
                                            }
                                        },
                Tasks = new List<TaskPair<PayExportTask>>()
                        {
                            new TaskPair<PayExportTask>()
                            {
                                Name = "TestExport_1",
                                Task1 = new PayExportTask()
                                {
                                    IsRunTask = true,
                                    Namespace = "Test11_8425",
                                    PayGroupCalendarId = 111,
                                    ExportMode = "Regular",
                                    MockTransmit = false
                                },
                                Task2 = new PayExportTask()
                                {
                                    IsRunTask = true,
                                    Namespace = "Test12_8425",
                                    PayGroupCalendarId = 111,
                                    ExportMode = "Regular",
                                    MockTransmit = false
                                }
                            },
                            new TaskPair<PayExportTask>()
                            {
                                Name = "TestExport_2",
                                Task1 = new PayExportTask()
                                {
                                    IsRunTask = true,
                                    Namespace = "Test11_1000",
                                    PayGroupCalendarId = 111,
                                    ExportMode = "Regular",
                                    MockTransmit = false
                                },
                                Task2 = new PayExportTask()
                                {
                                    IsRunTask = true,
                                    Namespace = "Test12_1000",
                                    PayGroupCalendarId = 111,
                                    ExportMode = "Regular",
                                    MockTransmit = false
                                }
                            }
                        }
            };

            SerializeToFile(@"D:\Development\ComparePay\TestInputExport.json", taskParam);

        }

        public static void SerializeToFile(string file, TaskParameter<PSRTask> taskParam)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(file))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, taskParam);
                }
            }
        }

        public static void SerializeToFile(string file, TaskParameter<PayExportTask> taskParam)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(file))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, taskParam);
                }
            }
        }

        public static void SerializeToFile(string file, TaskParameter<JobStepRecalcTask> taskParam)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(file))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, taskParam);
                }
            }
        }

        public static void SerializeToFile(string file, TaskParameter<BaseRateRecalcTask> taskParam)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(file))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, taskParam);
                }
            }
        }

        public static void SerializeToFile(string file, TaskParameter<AwardEntitlementTask> taskParam)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(file))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, taskParam);
                }
            }
        }

        public static void SerializeToFile(string file, TaskParameter<SCRTask> taskParam)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(file))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, taskParam);
                }
            }
        }

        // Members
    }

}
