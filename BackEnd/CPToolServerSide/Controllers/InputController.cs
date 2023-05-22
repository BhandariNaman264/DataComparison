using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CPToolServerSide.Models;
using CPToolServerSide.JobInput;
using System.IO;
using System.Diagnostics;
using Serilog;

namespace CPToolServerSide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InputController : ControllerBase
    {
        private readonly CPTDBContext _context;

        public InputController(CPTDBContext context)
        {
            _context = context;
        }

        // GET: api/Input
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Input>>> GetInput()
        {

            List<Input> result = await _context.Input.ToListAsync();

            // Loop over all result, and return results which are initiated within last 30 days, and delete results which were initiated more than 30 days back, from API and Table

            for (int i = 0; i < result.Count; i++)
            {

                // Path Confriguation for Compare Pay Tool

                // If Running on Local

                // string current_path = Directory.GetCurrentDirectory();

                // If Running on Server

                string current_path = "E:\\ServerSide";

                // Input Values below are needed for checking and running right commands

                string client = result[i].Client.Replace(" ", string.Empty).Trim();
                string comparison_id = result[i].ID.ToString();
                string job = result[i].Job;


                // For each Job ID, this section will check weather 30 days has been past, since comparison was ran, if yes, Delete Input and Output folder for this Job ID,
                // and remove it from Input API and database, otherwise do nothing.


                // Create DateTime date of Job Initiated Date

                string month_job = new string(result[i].Date.Split("/")[0]);
                string date_job = new string(result[i].Date.Split("/")[1]);
                string year_job = new string(result[i].Date.Split("/")[2].Substring(0, 4));

                int job_month = int.Parse(month_job);
                int job_date = int.Parse(date_job);
                int job_year = int.Parse(year_job);

                DateTime job_datetime = new DateTime(job_year, job_month, job_date, 0, 0, 0);


                // Create DateTime date of Today's Date


                string month_today = new string(DateTime.Now.ToString("MM"));
                string date_today = new string(DateTime.Now.ToString("dd"));
                string year_today = new string(DateTime.Now.ToString("yyyy"));

                int today_month = int.Parse(month_today);
                int today_date = int.Parse(date_today);
                int today_year = int.Parse(year_today);

                DateTime today_datetime = new DateTime(today_year, today_month, today_date, 0, 0, 0);

                // Calculate Total Days Past

                double dayspast = (today_datetime - job_datetime).TotalDays;

                if (dayspast > 30)
                {

                    try
                    {
                        // If Input File still exist, Delete input folder also for this Comparison

                        if (result[i].Compared != 2 || result[i].Compared != 3)
                        {
                            // Comparison is not completed, Delete Input Folder also

                            // Path to Input Folder

                            string pathinputfolder = current_path + "\\Input\\" + client + "\\" + comparison_id;

                            // Delete the Input Folder

                            Directory.Delete(pathinputfolder, true);

                            // Delete the Client Folder also, if after removing this Job ID, Client Folder is empty

                            string client_folder = current_path + "\\Input\\" + client;

                            // Check number of Job IDs, in this Client Folder, if it is 0 delete Client Folder also, otherwise don't delete.

                            if (Directory.GetDirectories(client_folder).Length == 0 && Directory.GetFiles(client_folder, "*", SearchOption.AllDirectories).Length == 0)
                            {
                                // Delete the Client Folder
                                Directory.Delete(client_folder);

                            }


                        }

                        // Now, Delete Output Folder for this Comparison

                        // Path to Output Folder

                        string pathoutputfolder = current_path + "\\Output\\" + client + "\\" + comparison_id;

                        // Delete the Output Folder

                        Directory.Delete(pathoutputfolder, true);

                        // Delete the Client Folder also, if after removing this Job ID, Client Folder is empty

                        string client_folder_output = current_path + "\\Output\\" + client;

                        // Check number of Job IDs, in this Client Folder, if it is 0 delete Client Folder also, otherwise don't delete.

                        if (Directory.GetDirectories(client_folder_output).Length == 0 && Directory.GetFiles(client_folder_output, "*", SearchOption.AllDirectories).Length == 0)
                        {
                            // Delete the Client Folder
                            Directory.Delete(client_folder_output);

                        }


                        // Delete Discrepacies for this Comparison ID - Based on Job

                        if (result[i].Job == "PSR")
                        {
                            List<AnalyzePSR> comparison_discrepacies = await _context.AnalyzePSR.Where(e => e.Comparison_ID == result[i].ID).ToListAsync();

                            for (int j = 0; j < comparison_discrepacies.Count; j++)
                            {
                                var analyze = await _context.AnalyzePSR.FindAsync(comparison_discrepacies[j].ID);
                                if (analyze == null)
                                {
                                    return NotFound();
                                }

                                _context.AnalyzePSR.Remove(comparison_discrepacies[j]);
                                await _context.SaveChangesAsync();
                            }
                        }
                        else if (result[i].Job == "BRR")
                        {
                            List<AnalyzeBRR> comparison_discrepacies = await _context.AnalyzeBRR.Where(e => e.Comparison_ID == result[i].ID).ToListAsync();

                            for (int j = 0; j < comparison_discrepacies.Count; j++)
                            {
                                var analyze = await _context.AnalyzeBRR.FindAsync(comparison_discrepacies[j].ID);
                                if (analyze == null)
                                {
                                    return NotFound();
                                }

                                _context.AnalyzeBRR.Remove(comparison_discrepacies[j]);
                                await _context.SaveChangesAsync();
                            }
                        }
                        else if (result[i].Job == "JobStepRecalc")
                        {
                            List<AnalyzeJSR> comparison_discrepacies = await _context.AnalyzeJSR.Where(e => e.Comparison_ID == result[i].ID).ToListAsync();

                            for (int j = 0; j < comparison_discrepacies.Count; j++)
                            {
                                var analyze = await _context.AnalyzeJSR.FindAsync(comparison_discrepacies[j].ID);
                                if (analyze == null)
                                {
                                    return NotFound();
                                }

                                _context.AnalyzeJSR.Remove(comparison_discrepacies[j]);
                                await _context.SaveChangesAsync();
                            }
                        }
                        else if (result[i].Job == "SCR")
                        {
                            List<AnalyzeSCR> comparison_discrepacies = await _context.AnalyzeSCR.Where(e => e.Comparison_ID == result[i].ID).ToListAsync();

                            for (int j = 0; j < comparison_discrepacies.Count; j++)
                            {
                                var analyze = await _context.AnalyzeSCR.FindAsync(comparison_discrepacies[j].ID);
                                if (analyze == null)
                                {
                                    return NotFound();
                                }

                                _context.AnalyzeSCR.Remove(comparison_discrepacies[j]);
                                await _context.SaveChangesAsync();
                            }
                        }
                        else if (result[i].Job == "AE_Sample")
                        {
                            List<AnalyzeAE> comparison_discrepacies = await _context.AnalyzeAE.Where(e => e.Comparison_ID == result[i].ID).ToListAsync();

                            for (int j = 0; j < comparison_discrepacies.Count; j++)
                            {
                                var analyze = await _context.AnalyzeAE.FindAsync(comparison_discrepacies[j].ID);
                                if (analyze == null)
                                {
                                    return NotFound();
                                }

                                _context.AnalyzeAE.Remove(comparison_discrepacies[j]);
                                await _context.SaveChangesAsync();
                            }
                        }
                        else if (result[i].Job == "Export")
                        {
                            List<AnalyzeE> comparison_discrepacies = await _context.AnalyzeE.Where(e => e.Comparison_ID == result[i].ID).ToListAsync();

                            for (int j = 0; j < comparison_discrepacies.Count; j++)
                            {
                                var analyze = await _context.AnalyzeE.FindAsync(comparison_discrepacies[j].ID);
                                if (analyze == null)
                                {
                                    return NotFound();
                                }

                                _context.AnalyzeE.Remove(comparison_discrepacies[j]);
                                await _context.SaveChangesAsync();
                            }
                        }


                        // Delete this Job ID from Input API and Database

                        _context.Input.Remove(result[i]);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                }



            }


            // Reverse the result list, so that latest comparison appear on the Top in the results page.
            result.Reverse();

            return result;
        }

        // GET: api/Input/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Input>> GetInput(int id)
        {
            var input = await _context.Input.FindAsync(id);

            if (input == null)
            {
                return NotFound();
            }

            return input;
        }



        // PUT: api/Input/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInput(int id, Input input)
        {
            if (id != input.ID)
            {
                return BadRequest();
            }

            _context.Entry(input).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InputExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Input
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Input>> PostInput(Input input)
        {

            // Saving new Comparison ID Details to Database and InputAPI

            _context.Input.Add(input);
            await _context.SaveChangesAsync();

            CreatedAtActionResult result = CreatedAtAction("GetInput", new { id = input.ID }, input);

            // Create Input JSON string

            string input_json = string.Empty;

            // Job Command Code

            string job_code = string.Empty;

            // Create Input Job Object, so that we can use it to make Input JSON file for this Job.

            if (input.Job == "PSR")
            {
                InputPSR input_psr = new InputPSR();

                // Fill in ControlDBNamespace details of this Input Object

                string[] controlDBnamespace1 = new string[] { input.DBName_1 };

                string[] controlDBnamespace2 = new string[] { input.DBName_2 };

                input_psr.ControlDBNamespace.Add(input.ControlDBServer_Server1, controlDBnamespace1);

                input_psr.ControlDBNamespace.Add(input.ControlDBServer_Server2, controlDBnamespace2);

                // Fill in Tasks detaiks of this Input Object

                var t1 = new Dictionary<string, object>
            {
                { "Name", input.Task_Name },
                { "ForceCompareOnly", input.ForceCompareOnly }
            };

                var task1_details = new Dictionary<string, object>()
            {
                {"IsRunTask", input.RunTask_1 },
                {"Namespace", input.DBName_1 },
                {"FromDate", input.Start_Time },
                {"ToDate", input.End_Time },
                {"OrgUnit", input.Org }

            };

                var task2_details = new Dictionary<string, object>()
            {
                {"IsRunTask", input.RunTask_2 },
                {"Namespace", input.DBName_2 },
                {"FromDate", input.Start_Time },
                {"ToDate", input.End_Time },
                {"OrgUnit", input.Org }
            };

                t1.Add("Task1", task1_details);
                t1.Add("Task2", task2_details);

                input_psr.Tasks.Add(t1);

                input_json = Newtonsoft.Json.JsonConvert.SerializeObject(input_psr, Newtonsoft.Json.Formatting.Indented);

                job_code = "PSR";

            }
            else if (input.Job == "BRR")
            {
                InputBRR input_brr = new InputBRR();

                // Fill in ControlDBNamespace details of this Input Object

                string[] controlDBnamespace1 = new string[] { input.DBName_1 };

                string[] controlDBnamespace2 = new string[] { input.DBName_2 };

                input_brr.ControlDBNamespace.Add(input.ControlDBServer_Server1, controlDBnamespace1);

                input_brr.ControlDBNamespace.Add(input.ControlDBServer_Server2, controlDBnamespace2);

                // Fill in Tasks detaiks of this Input Object

                var t1 = new Dictionary<string, object>
            {
                { "Name", input.Task_Name },
                { "ForceCompareOnly", input.ForceCompareOnly }
            };

                var task1_details = new Dictionary<string, object>()
            {
                {"IsRunTask", input.RunTask_1 },
                {"Namespace", input.DBName_1 },
                {"FromDate", input.Start_Time },
                {"ToDate", input.End_Time },
                {"OrgUnit", input.Org }

            };

                var task2_details = new Dictionary<string, object>()
            {
                {"IsRunTask", input.RunTask_2 },
                {"Namespace", input.DBName_2 },
                {"FromDate", input.Start_Time },
                {"ToDate", input.End_Time },
                {"OrgUnit", input.Org }
            };

                t1.Add("Task1", task1_details);
                t1.Add("Task2", task2_details);

                input_brr.Tasks.Add(t1);

                input_json = Newtonsoft.Json.JsonConvert.SerializeObject(input_brr, Newtonsoft.Json.Formatting.Indented);

                job_code = "BaseRateRecalc";

            }
            else if (input.Job == "JobStepRecalc")
            {
                InputJobStepRecalc input_jsr = new InputJobStepRecalc();

                // Fill in ControlDBNamespace details of this Input Object

                string[] controlDBnamespace1 = new string[] { input.DBName_1 };

                string[] controlDBnamespace2 = new string[] { input.DBName_2 };

                input_jsr.ControlDBNamespace.Add(input.ControlDBServer_Server1, controlDBnamespace1);

                input_jsr.ControlDBNamespace.Add(input.ControlDBServer_Server2, controlDBnamespace2);

                // Fill in Tasks detaiks of this Input Object

                var t1 = new Dictionary<string, object>
            {
                { "Name", input.Task_Name },
                { "ForceCompareOnly", input.ForceCompareOnly }
            };

                var task1_details = new Dictionary<string, object>()
            {
                {"IsRunTask", input.RunTask_1 },
                {"Namespace", input.DBName_1 },
                {"FromDate", input.Start_Time },
                {"ToDate", input.End_Time },
                {"OrgUnit", input.Org }

            };

                var task2_details = new Dictionary<string, object>()
            {
                {"IsRunTask", input.RunTask_2 },
                {"Namespace", input.DBName_2 },
                {"FromDate", input.Start_Time },
                {"ToDate", input.End_Time },
                {"OrgUnit", input.Org }
            };

                t1.Add("Task1", task1_details);
                t1.Add("Task2", task2_details);

                input_jsr.Tasks.Add(t1);

                input_json = Newtonsoft.Json.JsonConvert.SerializeObject(input_jsr, Newtonsoft.Json.Formatting.Indented);

                job_code = "JOBSTEPRECALC";

            }
            else if (input.Job == "SCR")
            {
                InputSCR input_scr = new InputSCR();

                // Fill in ControlDBNamespace details of this Input Object

                string[] controlDBnamespace1 = new string[] { input.DBName_1 };

                string[] controlDBnamespace2 = new string[] { input.DBName_2 };

                input_scr.ControlDBNamespace.Add(input.ControlDBServer_Server1, controlDBnamespace1);

                input_scr.ControlDBNamespace.Add(input.ControlDBServer_Server2, controlDBnamespace2);

                // Fill in Tasks detaiks of this Input Object

                var t1 = new Dictionary<string, object>
            {
                { "Name", input.Task_Name },
                { "ForceCompareOnly", input.ForceCompareOnly }
            };

                var task1_details = new Dictionary<string, object>()
            {
                {"IsRunTask", input.RunTask_1 },
                {"Namespace", input.DBName_1 },
                {"DateRelativeToToday", input.Date_Relative_To_Today },
                {"OrgUnit", input.Org }

            };

                var task2_details = new Dictionary<string, object>()
            {
                {"IsRunTask", input.RunTask_2 },
                {"Namespace", input.DBName_2 },
                {"DateRelativeToToday", input.Date_Relative_To_Today },
                {"OrgUnit", input.Org }
            };

                t1.Add("Task1", task1_details);
                t1.Add("Task2", task2_details);

                input_scr.Tasks.Add(t1);

                input_json = Newtonsoft.Json.JsonConvert.SerializeObject(input_scr, Newtonsoft.Json.Formatting.Indented);

                job_code = "SCR";

            }
            else if (input.Job == "AE_Sample")
            {
                InputAE_Sample input_ae = new InputAE_Sample();


                // Fill in ControlDBNamespace details of this Input Object

                string[] controlDBnamespace1 = new string[] { input.DBName_1 };

                string[] controlDBnamespace2 = new string[] { input.DBName_2 };

                input_ae.ControlDBNamespace.Add(input.ControlDBServer_Server1, controlDBnamespace1);

                input_ae.ControlDBNamespace.Add(input.ControlDBServer_Server2, controlDBnamespace2);

                // Fill in Tasks detaiks of this Input Object

                var t1 = new Dictionary<string, object>
            {
                { "Name", input.Task_Name },
                { "ForceCompareOnly", input.ForceCompareOnly }
            };

                var task1_details = new Dictionary<string, object>()
            {
                {"IsRunTask", input.RunTask_1 },
                {"Namespace", input.DBName_1 },
                {"FromDate", input.Start_Time },
                {"ToDate", input.End_Time },
                {"OrgUnit", input.Org },
                {"Policy", input.Policy }

            };

                var task2_details = new Dictionary<string, object>()
            {
                {"IsRunTask", input.RunTask_2 },
                {"Namespace", input.DBName_2 },
                {"FromDate", input.Start_Time },
                {"ToDate", input.End_Time },
                {"OrgUnit", input.Org },
                {"Policy", input.Policy }
            };

                t1.Add("Task1", task1_details);
                t1.Add("Task2", task2_details);

                input_ae.Tasks.Add(t1);

                input_json = Newtonsoft.Json.JsonConvert.SerializeObject(input_ae, Newtonsoft.Json.Formatting.Indented);

                job_code = "AWARDENTITLEMENT";

            }
            else if (input.Job == "Export")
            {
                InputExport input_e = new InputExport();

                // Fill in ControlDBNamespace details of this Input Object

                string[] controlDBnamespace1 = new string[] { input.DBName_1 };

                string[] controlDBnamespace2 = new string[] { input.DBName_2 };

                input_e.ControlDBNamespace.Add(input.ControlDBServer_Server1, controlDBnamespace1);

                input_e.ControlDBNamespace.Add(input.ControlDBServer_Server2, controlDBnamespace2);

                // Fill in Tasks detaiks of this Input Object

                var t1 = new Dictionary<string, object>
            {
                { "Name", input.Task_Name },
                { "ForceCompareOnly", input.ForceCompareOnly }
            };

                if (input.Export_File_Name == "")
                {
                    var task1_details = new Dictionary<string, object>()
                {
                    {"IsRunTask", input.RunTask_1 },
                    {"Namespace", input.DBName_1 },
                    {"PayGroupCalendarId", input.Pay_Group_Calendar_Id },
                    {"ExportMode", input.Export_Mode },
                    {"MockTransmit", input.Mock_Transmit }

                };

                    var task2_details = new Dictionary<string, object>()
                {
                    {"IsRunTask", input.RunTask_2 },
                    {"Namespace", input.DBName_2 },
                    {"PayGroupCalendarId", input.Pay_Group_Calendar_Id },
                    {"ExportMode", input.Export_Mode },
                    {"MockTransmit", input.Mock_Transmit }
                };


                    t1.Add("Task1", task1_details);
                    t1.Add("Task2", task2_details);

                    input_e.Tasks.Add(t1);

                    input_json = Newtonsoft.Json.JsonConvert.SerializeObject(input_e, Newtonsoft.Json.Formatting.Indented);

                    job_code = "EXPORT";
                }
                else
                {
                    var task1_details = new Dictionary<string, object>()
                {
                    {"IsRunTask", input.RunTask_1 },
                    {"Namespace", input.DBName_1 },
                    {"PayGroupCalendarId", input.Pay_Group_Calendar_Id },
                    {"ExportMode", input.Export_Mode },
                    {"MockTransmit", input.Mock_Transmit },
                    {"ExportFileName", input.Export_File_Name}

                };

                    var task2_details = new Dictionary<string, object>()
                {
                    {"IsRunTask", input.RunTask_2 },
                    {"Namespace", input.DBName_2 },
                    {"PayGroupCalendarId", input.Pay_Group_Calendar_Id },
                    {"ExportMode", input.Export_Mode },
                    {"MockTransmit", input.Mock_Transmit },
                    {"ExportFileName", input.Export_File_Name}
                };

                    t1.Add("Task1", task1_details);
                    t1.Add("Task2", task2_details);

                    input_e.Tasks.Add(t1);

                    input_json = Newtonsoft.Json.JsonConvert.SerializeObject(input_e, Newtonsoft.Json.Formatting.Indented);

                    job_code = "EXPORT";

                }

            }


            try
            {

                // Path Confriguation for Compare Pay Tool

                // If Running on Local

                // string current_path = Directory.GetCurrentDirectory();

                // If Running on Server

                string current_path = "E:\\ServerSide";

                // Initialized ComparisonResults.txt file for this Job ID, to store comparison results information

                string comparison_result_text = "";
                string analyze_result = "";

                // Find Client Database use for this Job and save it in a string

                string client = input.Client.Replace(" ", string.Empty).Trim();
                string job = input.Job;
                string comparison_id = input.ID.ToString();

                // Create new folder for this Client Database if it doesn't exist then create a new folder for this specific Job ID. [INPUT]

                var directoryInfoInput = new DirectoryInfo(current_path + "\\Input\\");

                var clientfolderInfoInput = new DirectoryInfo(current_path + "\\Input\\" + client + "\\");


                if (directoryInfoInput.Exists && !clientfolderInfoInput.Exists)
                {
                    directoryInfoInput.CreateSubdirectory(client);
                    clientfolderInfoInput.CreateSubdirectory(comparison_id);
                }
                else if (directoryInfoInput.Exists && clientfolderInfoInput.Exists)
                {
                    clientfolderInfoInput.CreateSubdirectory(comparison_id);
                }

                // Create new folder for this Client Database if it doesn't exist then create a new folder for this specific Job ID. [INPUT]


                var directoryInfoOutput = new DirectoryInfo(current_path + "\\Output\\");

                var clientfolderInfoOutput = new DirectoryInfo(current_path + "\\Output\\" + client + "\\");


                if (directoryInfoOutput.Exists && !clientfolderInfoOutput.Exists)
                {
                    directoryInfoOutput.CreateSubdirectory(client);
                    clientfolderInfoOutput.CreateSubdirectory(comparison_id);
                }
                else if (directoryInfoOutput.Exists && clientfolderInfoOutput.Exists)
                {
                    clientfolderInfoOutput.CreateSubdirectory(comparison_id);
                }

                // Now, create Input JSON file and ComparisonResult TEXT file in the right placed based on the Client and Job ID

                string pathinput = current_path + "\\Input\\" + client + "\\" + comparison_id + "\\Input" + job + ".json";
                string pathoutput = current_path + "\\Output\\" + client + "\\" + comparison_id + "\\ComparisonResult.txt";
                string pathanalyze = current_path + "\\Output\\" + client + "\\" + comparison_id + "\\Analyze.txt";

                // And then write these files, with required details.

                System.IO.File.WriteAllText(pathinput, input_json);
                System.IO.File.WriteAllText(pathoutput, comparison_result_text);
                System.IO.File.WriteAllText(pathanalyze, analyze_result);

                if (input.Job == "Export")
                {
                    string analyze_result_export = "";
                    string pathoutput_export = current_path + "\\Output\\" + client + "\\" + comparison_id + "\\CompareResultExport.txt";
                    System.IO.File.WriteAllText(pathoutput_export, analyze_result_export);
                }

                if(input.Job != "Export")
                {
                    // If Force Compare Only Mode is Disabled then only Run Queue Command

                    if (input.ForceCompareOnly == false)
                    {

                        // Running Queue Command

                        string queue_command = ".\\ComparePay RunType=" + job_code + " ProcessType=QueueTask InputFile=.\\Input\\" + client + "\\" + comparison_id + "\\Input" + job + ".json OutputFolder=.\\Output\\" + client + "\\" + comparison_id;

                        Process queue_cmd = new Process();
                        queue_cmd.StartInfo.FileName = "cmd.exe";
                        queue_cmd.StartInfo.RedirectStandardInput = true;
                        queue_cmd.StartInfo.RedirectStandardOutput = true;
                        queue_cmd.StartInfo.CreateNoWindow = true;
                        queue_cmd.StartInfo.UseShellExecute = false;
                        queue_cmd.Start();

                        queue_cmd.StandardInput.WriteLine(queue_command);
                        queue_cmd.StandardInput.Flush();
                        queue_cmd.StandardInput.Close();
                        queue_cmd.WaitForExit();

                        //Console.WriteLine(queue_cmd.StandardOutput.ReadToEnd());
                        //Console.ReadKey();
                    }
                }
                else if(input.Job == "Export")
                {
                    // If Force Compare Only Mode is Disabled then only Run Queue Command

                    if (input.ForceCompareOnly == false)
                    {

                        // Running Queue Command

                        string queue_command = ".\\ComparePay RunType=" + job_code + " ProcessType=QueueTask InputFile=.\\Input\\" + client + "\\" + comparison_id + "\\Input" + job + ".json OutputFolder=.\\Output\\" + client + "\\" + comparison_id + " LogFile=CompareResultExport.txt";

                        Process queue_cmd = new Process();
                        queue_cmd.StartInfo.FileName = "cmd.exe";
                        queue_cmd.StartInfo.RedirectStandardInput = true;
                        queue_cmd.StartInfo.RedirectStandardOutput = true;
                        queue_cmd.StartInfo.CreateNoWindow = true;
                        queue_cmd.StartInfo.UseShellExecute = false;
                        queue_cmd.Start();

                        queue_cmd.StandardInput.WriteLine(queue_command);
                        queue_cmd.StandardInput.Flush();
                        queue_cmd.StandardInput.Close();
                        queue_cmd.WaitForExit();

                        //Console.WriteLine(queue_cmd.StandardOutput.ReadToEnd());
                        //Console.ReadKey();
                    }
                }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // Error - Log File
                var log = new LoggerConfiguration()
                    .WriteTo.RollingFile("log-{Date}.txt")
                    .CreateLogger();
                log.Fatal(ex, "Error in Command Line Tool - Compare Pay");
            }



            return result;
        }


        // DELETE: api/Input/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Input>> DeleteInput(int id)
        {

            try
            {

                var input = await _context.Input.FindAsync(id);
                if (input == null)
                {
                    return NotFound();
                }

                // Delete Discrepacies for this Comparison ID - Based on Job

                if (input.Job == "PSR")
                {
                    List<AnalyzePSR> comparison_discrepacies = await _context.AnalyzePSR.Where(e => e.Comparison_ID == input.ID).ToListAsync();

                    for (int j = 0; j < comparison_discrepacies.Count; j++)
                    {
                        var analyze = await _context.AnalyzePSR.FindAsync(comparison_discrepacies[j].ID);
                        if (analyze == null)
                        {
                            return NotFound();
                        }

                        _context.AnalyzePSR.Remove(comparison_discrepacies[j]);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (input.Job == "BRR")
                {
                    List<AnalyzeBRR> comparison_discrepacies = await _context.AnalyzeBRR.Where(e => e.Comparison_ID == input.ID).ToListAsync();

                    for (int j = 0; j < comparison_discrepacies.Count; j++)
                    {
                        var analyze = await _context.AnalyzeBRR.FindAsync(comparison_discrepacies[j].ID);
                        if (analyze == null)
                        {
                            return NotFound();
                        }

                        _context.AnalyzeBRR.Remove(comparison_discrepacies[j]);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (input.Job == "JobStepRecalc")
                {
                    List<AnalyzeJSR> comparison_discrepacies = await _context.AnalyzeJSR.Where(e => e.Comparison_ID == input.ID).ToListAsync();

                    for (int j = 0; j < comparison_discrepacies.Count; j++)
                    {
                        var analyze = await _context.AnalyzeJSR.FindAsync(comparison_discrepacies[j].ID);
                        if (analyze == null)
                        {
                            return NotFound();
                        }

                        _context.AnalyzeJSR.Remove(comparison_discrepacies[j]);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (input.Job == "SCR")
                {
                    List<AnalyzeSCR> comparison_discrepacies = await _context.AnalyzeSCR.Where(e => e.Comparison_ID == input.ID).ToListAsync();

                    for (int j = 0; j < comparison_discrepacies.Count; j++)
                    {
                        var analyze = await _context.AnalyzeSCR.FindAsync(comparison_discrepacies[j].ID);
                        if (analyze == null)
                        {
                            return NotFound();
                        }

                        _context.AnalyzeSCR.Remove(comparison_discrepacies[j]);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (input.Job == "AE_Sample")
                {
                    List<AnalyzeAE> comparison_discrepacies = await _context.AnalyzeAE.Where(e => e.Comparison_ID == input.ID).ToListAsync();

                    for (int j = 0; j < comparison_discrepacies.Count; j++)
                    {
                        var analyze = await _context.AnalyzeAE.FindAsync(comparison_discrepacies[j].ID);
                        if (analyze == null)
                        {
                            return NotFound();
                        }

                        _context.AnalyzeAE.Remove(comparison_discrepacies[j]);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (input.Job == "Export")
                {
                    List<AnalyzeE> comparison_discrepacies = await _context.AnalyzeE.Where(e => e.Comparison_ID == input.ID).ToListAsync();

                    for (int j = 0; j < comparison_discrepacies.Count; j++)
                    {
                        var analyze = await _context.AnalyzeE.FindAsync(comparison_discrepacies[j].ID);
                        if (analyze == null)
                        {
                            return NotFound();
                        }

                        _context.AnalyzeE.Remove(comparison_discrepacies[j]);
                        await _context.SaveChangesAsync();
                    }
                }

                //  Now Delete this Comparison ID and its details

                _context.Input.Remove(input);
                await _context.SaveChangesAsync();

                // Path Confriguation for Compare Pay Tool

                // If Running on Local

                // string current_path = Directory.GetCurrentDirectory();

                // If Running on Server

                string current_path = "E:\\ServerSide";

                // Input Values below are needed for deleting Output Folder

                string client = input.Client.Replace(" ", string.Empty).Trim();
                string comparison_id = input.ID.ToString();


                // Path to Output Folder

                string pathoutputfolder = current_path + "\\Output\\" + client + "\\" + comparison_id;

                // Delete the Output Folder

                Directory.Delete(pathoutputfolder, true);

                // Delete the Client Folder also, if after removing this Job ID, Client Folder is empty

                string client_folder_output = current_path + "\\Output\\" + client;

                // Check number of Job IDs, in this Client Folder, if it is 0 delete Client Folder also, otherwise don't delete.

                if (Directory.GetDirectories(client_folder_output).Length == 0 && Directory.GetFiles(client_folder_output, "*", SearchOption.AllDirectories).Length == 0)
                {
                    // Delete the Client Folder
                    Directory.Delete(client_folder_output);

                }

                return input;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }

        }

        private bool InputExists(int id)
        {
            return _context.Input.Any(e => e.ID == id);
        }
    }
}
