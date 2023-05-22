using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CPToolServerSide.Models;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using CPToolServerSide.JobInput;
using System.Net;
using CPToolServerSide.Email;
using Serilog;

namespace CPToolServerSide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly CPTDBContext _context;

        public ServerController(CPTDBContext context)
        {
            _context = context;
        }

        // GET: api/Server/access
        [HttpGet("{password}")]
        public async Task<ActionResult<IEnumerable<Input>>> GetInput(string password)
        {

            // Check Server Access - Password
            if (password != "jn2a6mh4n")
            {
                return NotFound();
            }

            // Keeping Running Server to Update Job Status and Result
            while (true)
            {

                // Path Confriguation for Compare Pay Tool

                // If Running on Local

                // string current_path = Directory.GetCurrentDirectory();

                // If Running on Server

                string current_path = "E:\\ServerSide";

                // Get List of Comparisons which are not Completed or Failed

                List<Input> comparison = await _context.Input.Where(e => e.Compared != 2 && e.Compared != 3).ToListAsync();

                for (int i = 0; i < comparison.Count; i++)
                {
                    // Input Values below are needed for checking and running right commands


                    string client = comparison[i].Client.Replace(" ", string.Empty).Trim();
                    string comparison_id = comparison[i].ID.ToString();
                    string job = comparison[i].Job;

                    string job_code = string.Empty;

                    if (job == "PSR")
                    {
                        job_code = "PSR";
                    }
                    else if (job == "BRR")
                    {
                        job_code = "BaseRateRecalc";
                    }
                    else if (job == "JobStepRecalc")
                    {
                        job_code = "JOBSTEPRECALC";
                    }
                    else if (job == "SCR")
                    {
                        job_code = "SCR";
                    }
                    else if (job == "AE_Sample")
                    {
                        job_code = "AWARDENTITLEMENT";
                    }
                    else if (job == "Export")
                    {
                        job_code = "EXPORT";
                    }
                    // If Compare=0 keep running status command until there is an update in Compare value to 1 or 3.
                    // Note: Run Staus Command only if Force Compare Mode is Disabled.

                    if ((comparison[i].Compared == 0) && (comparison[i].ForceCompareOnly == false))
                    {

                        // Input File Location

                        string pathinput = current_path + "\\Input\\" + client + "\\" + comparison_id + "\\Input" + job + ".json";

                        if (comparison[i].Job != "Export")
                        {
                            // Check Status Command, based on Job, and Job ID.

                            string check_status_command = ".\\ComparePay RunType=" + job_code + " ProcessType=CheckJobStatus InputFile=.\\Input\\" + client + "\\" + comparison_id + "\\Input" + job + ".json OutputFolder=.\\Output\\" + client + "\\" + comparison_id;

                            Process check_status_cmd = new Process();
                            check_status_cmd.StartInfo.FileName = "cmd.exe";
                            check_status_cmd.StartInfo.RedirectStandardInput = true;
                            check_status_cmd.StartInfo.RedirectStandardOutput = true;
                            check_status_cmd.StartInfo.CreateNoWindow = true;
                            check_status_cmd.StartInfo.UseShellExecute = false;
                            check_status_cmd.Start();

                            check_status_cmd.StandardInput.WriteLine(check_status_command);
                            check_status_cmd.StandardInput.Flush();
                            check_status_cmd.StandardInput.Close();
                            check_status_cmd.WaitForExit();

                            //Console.WriteLine(check_status_cmd.StandardOutput.ReadToEnd());
                            //Console.ReadKey();

                        }
                        else if (comparison[i].Job == "Export")
                        {
                            // Check Status Command, based on Job, and Job ID.

                            string check_status_command = ".\\ComparePay RunType=" + job_code + " ProcessType=CheckJobStatus InputFile=.\\Input\\" + client + "\\" + comparison_id + "\\Input" + job + ".json OutputFolder=.\\Output\\" + client + "\\" + comparison_id + " LogFile=CompareResultExport.txt";

                            Process check_status_cmd = new Process();
                            check_status_cmd.StartInfo.FileName = "cmd.exe";
                            check_status_cmd.StartInfo.RedirectStandardInput = true;
                            check_status_cmd.StartInfo.RedirectStandardOutput = true;
                            check_status_cmd.StartInfo.CreateNoWindow = true;
                            check_status_cmd.StartInfo.UseShellExecute = false;
                            check_status_cmd.Start();

                            check_status_cmd.StandardInput.WriteLine(check_status_command);
                            check_status_cmd.StandardInput.Flush();
                            check_status_cmd.StandardInput.Close();
                            check_status_cmd.WaitForExit();

                            //Console.WriteLine(check_status_cmd.StandardOutput.ReadToEnd());
                            //Console.ReadKey();

                        }
                        try
                        {
                            // Read the updated Input File, to get updated Status and Result

                            string jsonString = System.IO.File.ReadAllText(pathinput);

                            List<Dictionary<string, object>> tasks = new List<Dictionary<string, object>>();

                            Dictionary<string, object> task_one;

                            if (comparison[i].Job == "PSR")
                            {
                                // Fetching values from InputPSR and storing them in thier relative data type, so that values retrieved can be used and stored in InputAPI and Table

                                InputPSR inJob = System.Text.Json.JsonSerializer.Deserialize<InputPSR>(jsonString);

                                tasks = inJob.Tasks;
                                task_one = tasks[0];


                                if (!task_one.TryGetValue("Task1", out object task1_obj))
                                {
                                    Console.WriteLine("Error in getting Task 1 from Task");
                                }

                                if (!task_one.TryGetValue("Task2", out object task2_obj))
                                {
                                    Console.WriteLine("Error in getting Task 2 from Task");
                                }


                                string task1_obj_string = Convert.ToString(task1_obj);

                                var task1_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(task1_obj_string);



                                string task2_obj_string = Convert.ToString(task2_obj);

                                var task2_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(task2_obj_string);



                                if (!task1_dict.TryGetValue("LogId", out string logid1))
                                {
                                    Console.WriteLine("Error in getting logid1 from Task 1");
                                }
                                if (!task2_dict.TryGetValue("LogId", out string logid2))
                                {
                                    Console.WriteLine("Error in getting logid2 from Task 2");
                                }


                                if (!task1_dict.TryGetValue("Status", out string status1))
                                {
                                    Console.WriteLine("Error in getting Status 1 from Task 1");
                                }
                                if (!task2_dict.TryGetValue("Status", out string status2))
                                {
                                    Console.WriteLine("Error in getting Status 2 from Task 2");
                                }



                                // If fetched values are not null, update the current status with these values.

                                if (logid1 != null)
                                {
                                    comparison[i].LogId1 = logid1;
                                }
                                if (logid2 != null)
                                {
                                    comparison[i].LogId2 = logid2;
                                }
                                if (status1 != null)
                                {
                                    comparison[i].Status1 = status1;
                                }
                                if (status2 != null)
                                {
                                    comparison[i].Status2 = status2;
                                }

                            }
                            else if (comparison[i].Job == "BRR")
                            {
                                // Fetching values from InputPSR and storing them in thier relative data type, so that values retrieved can be used and stored in InputAPI and Table

                                InputBRR inJob = System.Text.Json.JsonSerializer.Deserialize<InputBRR>(jsonString);

                                tasks = inJob.Tasks;
                                task_one = tasks[0];


                                if (!task_one.TryGetValue("Task1", out object task1_obj))
                                {
                                    Console.WriteLine("Error in getting Task 1 from Task");
                                }

                                if (!task_one.TryGetValue("Task2", out object task2_obj))
                                {
                                    Console.WriteLine("Error in getting Task 2 from Task");
                                }


                                string task1_obj_string = Convert.ToString(task1_obj);

                                var task1_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(task1_obj_string);



                                string task2_obj_string = Convert.ToString(task2_obj);

                                var task2_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(task2_obj_string);



                                if (!task1_dict.TryGetValue("LogId", out string logid1))
                                {
                                    Console.WriteLine("Error in getting logid1 from Task 1");
                                }
                                if (!task2_dict.TryGetValue("LogId", out string logid2))
                                {
                                    Console.WriteLine("Error in getting logid2 from Task 2");
                                }


                                if (!task1_dict.TryGetValue("Status", out string status1))
                                {
                                    Console.WriteLine("Error in getting Status 1 from Task 1");
                                }
                                if (!task2_dict.TryGetValue("Status", out string status2))
                                {
                                    Console.WriteLine("Error in getting Status 2 from Task 2");
                                }



                                // If fetched values are not null, update the current status with these values.

                                if (logid1 != null)
                                {
                                    comparison[i].LogId1 = logid1;
                                }
                                if (logid2 != null)
                                {
                                    comparison[i].LogId2 = logid2;
                                }
                                if (status1 != null)
                                {
                                    comparison[i].Status1 = status1;
                                }
                                if (status2 != null)
                                {
                                    comparison[i].Status2 = status2;
                                }

                            }
                            else if (comparison[i].Job == "JobStepRecalc")
                            {
                                // Fetching values from InputPSR and storing them in thier relative data type, so that values retrieved can be used and stored in InputAPI and Table

                                InputJobStepRecalc inJob = System.Text.Json.JsonSerializer.Deserialize<InputJobStepRecalc>(jsonString);

                                tasks = inJob.Tasks;
                                task_one = tasks[0];


                                if (!task_one.TryGetValue("Task1", out object task1_obj))
                                {
                                    Console.WriteLine("Error in getting Task 1 from Task");
                                }

                                if (!task_one.TryGetValue("Task2", out object task2_obj))
                                {
                                    Console.WriteLine("Error in getting Task 2 from Task");
                                }


                                string task1_obj_string = Convert.ToString(task1_obj);

                                var task1_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(task1_obj_string);



                                string task2_obj_string = Convert.ToString(task2_obj);

                                var task2_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(task2_obj_string);



                                if (!task1_dict.TryGetValue("LogId", out string logid1))
                                {
                                    Console.WriteLine("Error in getting logid1 from Task 1");
                                }
                                if (!task2_dict.TryGetValue("LogId", out string logid2))
                                {
                                    Console.WriteLine("Error in getting logid2 from Task 2");
                                }


                                if (!task1_dict.TryGetValue("Status", out string status1))
                                {
                                    Console.WriteLine("Error in getting Status 1 from Task 1");
                                }
                                if (!task2_dict.TryGetValue("Status", out string status2))
                                {
                                    Console.WriteLine("Error in getting Status 2 from Task 2");
                                }



                                // If fetched values are not null, update the current status with these values.

                                if (logid1 != null)
                                {
                                    comparison[i].LogId1 = logid1;
                                }
                                if (logid2 != null)
                                {
                                    comparison[i].LogId2 = logid2;
                                }
                                if (status1 != null)
                                {
                                    comparison[i].Status1 = status1;
                                }
                                if (status2 != null)
                                {
                                    comparison[i].Status2 = status2;
                                }

                            }
                            else if (comparison[i].Job == "SCR")
                            {
                                // Fetching values from InputPSR and storing them in thier relative data type, so that values retrieved can be used and stored in InputAPI and Table

                                InputSCR inJob = System.Text.Json.JsonSerializer.Deserialize<InputSCR>(jsonString);

                                tasks = inJob.Tasks;
                                task_one = tasks[0];


                                if (!task_one.TryGetValue("Task1", out object task1_obj))
                                {
                                    Console.WriteLine("Error in getting Task 1 from Task");
                                }

                                if (!task_one.TryGetValue("Task2", out object task2_obj))
                                {
                                    Console.WriteLine("Error in getting Task 2 from Task");
                                }


                                string task1_obj_string = Convert.ToString(task1_obj);

                                var task1_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(task1_obj_string);



                                string task2_obj_string = Convert.ToString(task2_obj);

                                var task2_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(task2_obj_string);



                                if (!task1_dict.TryGetValue("LogId", out string logid1))
                                {
                                    Console.WriteLine("Error in getting logid1 from Task 1");
                                }
                                if (!task2_dict.TryGetValue("LogId", out string logid2))
                                {
                                    Console.WriteLine("Error in getting logid2 from Task 2");
                                }


                                if (!task1_dict.TryGetValue("Status", out string status1))
                                {
                                    Console.WriteLine("Error in getting Status 1 from Task 1");
                                }
                                if (!task2_dict.TryGetValue("Status", out string status2))
                                {
                                    Console.WriteLine("Error in getting Status 2 from Task 2");
                                }



                                // If fetched values are not null, update the current status with these values.

                                if (logid1 != null)
                                {
                                    comparison[i].LogId1 = logid1;
                                }
                                if (logid2 != null)
                                {
                                    comparison[i].LogId2 = logid2;
                                }
                                if (status1 != null)
                                {
                                    comparison[i].Status1 = status1;
                                }
                                if (status2 != null)
                                {
                                    comparison[i].Status2 = status2;
                                }

                            }
                            else if (comparison[i].Job == "AE_Sample")
                            {
                                // Fetching values from InputPSR and storing them in thier relative data type, so that values retrieved can be used and stored in InputAPI and Table

                                InputAE_Sample inJob = System.Text.Json.JsonSerializer.Deserialize<InputAE_Sample>(jsonString);

                                tasks = inJob.Tasks;
                                task_one = tasks[0];


                                if (!task_one.TryGetValue("Task1", out object task1_obj))
                                {
                                    Console.WriteLine("Error in getting Task 1 from Task");
                                }

                                if (!task_one.TryGetValue("Task2", out object task2_obj))
                                {
                                    Console.WriteLine("Error in getting Task 2 from Task");
                                }


                                string task1_obj_string = Convert.ToString(task1_obj);

                                var task1_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(task1_obj_string);



                                string task2_obj_string = Convert.ToString(task2_obj);

                                var task2_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(task2_obj_string);



                                if (!task1_dict.TryGetValue("LogId", out string logid1))
                                {
                                    Console.WriteLine("Error in getting logid1 from Task 1");
                                }
                                if (!task2_dict.TryGetValue("LogId", out string logid2))
                                {
                                    Console.WriteLine("Error in getting logid2 from Task 2");
                                }


                                if (!task1_dict.TryGetValue("Status", out string status1))
                                {
                                    Console.WriteLine("Error in getting Status 1 from Task 1");
                                }
                                if (!task2_dict.TryGetValue("Status", out string status2))
                                {
                                    Console.WriteLine("Error in getting Status 2 from Task 2");
                                }



                                // If fetched values are not null, update the current status with these values.

                                if (logid1 != null)
                                {
                                    comparison[i].LogId1 = logid1;
                                }
                                if (logid2 != null)
                                {
                                    comparison[i].LogId2 = logid2;
                                }
                                if (status1 != null)
                                {
                                    comparison[i].Status1 = status1;
                                }
                                if (status2 != null)
                                {
                                    comparison[i].Status2 = status2;
                                }

                            }
                            else if (comparison[i].Job == "Export")
                            {
                                // Fetching values from InputPSR and storing them in thier relative data type, so that values retrieved can be used and stored in InputAPI and Table

                                InputExport inJob = System.Text.Json.JsonSerializer.Deserialize<InputExport>(jsonString);

                                tasks = inJob.Tasks;
                                task_one = tasks[0];


                                if (!task_one.TryGetValue("Task1", out object task1_obj))
                                {
                                    Console.WriteLine("Error in getting Task 1 from Task");
                                }

                                if (!task_one.TryGetValue("Task2", out object task2_obj))
                                {
                                    Console.WriteLine("Error in getting Task 2 from Task");
                                }


                                string task1_obj_string = Convert.ToString(task1_obj);

                                var task1_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(task1_obj_string);



                                string task2_obj_string = Convert.ToString(task2_obj);

                                var task2_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(task2_obj_string);



                                if (!task1_dict.TryGetValue("LogId", out string logid1))
                                {
                                    Console.WriteLine("Error in getting logid1 from Task 1");
                                }
                                if (!task2_dict.TryGetValue("LogId", out string logid2))
                                {
                                    Console.WriteLine("Error in getting logid2 from Task 2");
                                }


                                if (!task1_dict.TryGetValue("Status", out string status1))
                                {
                                    Console.WriteLine("Error in getting Status 1 from Task 1");
                                }
                                if (!task2_dict.TryGetValue("Status", out string status2))
                                {
                                    Console.WriteLine("Error in getting Status 2 from Task 2");
                                }



                                // If fetched values are not null, update the current status with these values.

                                if (logid1 != null)
                                {
                                    comparison[i].LogId1 = logid1;
                                }
                                if (logid2 != null)
                                {
                                    comparison[i].LogId2 = logid2;
                                }
                                if (status1 != null)
                                {
                                    comparison[i].Status1 = status1;
                                }
                                if (status2 != null)
                                {
                                    comparison[i].Status2 = status2;
                                }

                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }



                        // If Both Job Failed 

                        if ((comparison[i].Status1 == "JobFailed") && (comparison[i].Status2 == "JobFailed"))
                        {
                            // (Set Compare to 3 and no need to run commands for this JobID again)
                            comparison[i].Compared = 3;
                            comparison[i].Results = "No Results-Job Failed";
                        }

                        // If one of the Job Failed and other is Completed

                        if ((comparison[i].Status1 == "JobFailed") && (comparison[i].Status2 == "JobCompleted"))
                        {
                            // (Set Compare to 3 and no need to run commands for this JobID again)
                            comparison[i].Compared = 3;
                            comparison[i].Results = "No Results-Job Failed";
                        }

                        // If one of the Job Failed and other is Completed

                        if ((comparison[i].Status1 == "JobCompleted") && (comparison[i].Status2 == "JobFailed"))
                        {
                            // (Set Compare to 3 and no need to run commands for this JobID again)
                            comparison[i].Compared = 3;
                            comparison[i].Results = "No Results-Job Failed";
                        }


                        // If Both Queue Failed 

                        if ((comparison[i].Status1 == "JobQueueFailed") && (comparison[i].Status2 == "JobQueueFailed"))
                        {
                            // (Set Compare to 3 and no need to run commands for this JobID again)
                            comparison[i].Compared = 3;
                            comparison[i].Results = "No Results-Queue Failed";
                        }

                        // If one of the Queue Failed and other is Completed

                        if ((comparison[i].Status1 == "JobQueueFailed") && (comparison[i].Status2 == "JobCompleted"))
                        {
                            // (Set Compare to 3 and no need to run commands for this JobID again)
                            comparison[i].Compared = 3;
                            comparison[i].Results = "No Results-Queue Failed";
                        }

                        // If one of the Queue Failed and other is Completed

                        if ((comparison[i].Status1 == "JobCompleted") && (comparison[i].Status2 == "JobQueueFailed"))
                        {
                            // (Set Compare to 3 and no need to run commands for this JobID again)
                            comparison[i].Compared = 3;
                            comparison[i].Results = "No Results-Queue Failed";
                        }


                        // Update this input

                        _context.Entry(comparison[i]).State = EntityState.Modified;

                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!InputExists(comparison[i].ID))
                            {
                                Console.WriteLine("Comparison Not Found");
                            }
                            else
                            {
                                throw;
                            }
                        }

                        // If both Job are completed and Background Job is Pay Export, skip Comparison until Comparison for Pay Export is Fixed
                        if (((comparison[i].Job == "Export") && (comparison[i].Status1 == "JobCompleted") && (comparison[i].Status2 == "JobCompleted")))
                        {
                            // Set Compared to 1 and change results status to comparison started, after Running Compare Command, so that we never run comparison again.
                            comparison[i].Compared = 2;
                            comparison[i].Results = "MANUAL COMPARISON";

                            // Update this input

                            _context.Entry(comparison[i]).State = EntityState.Modified;

                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                if (!InputExists(comparison[i].ID))
                                {
                                    Console.WriteLine("Comparison Not Found");
                                }
                                else
                                {
                                    throw;
                                }
                            }

                            // Delete Log File for Pay Export Job

                            try
                            {
                                System.IO.File.Delete(current_path + "\\Output\\" + client + "\\" + comparison_id + "\\CompareResultExport.txt");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                            string comparison_file = current_path + "\\Output\\" + client + "\\" + comparison_id + "\\ComparisonResult.txt";

                            // Delete Comparison Result

                            try
                            {
                                System.IO.File.Delete(comparison_file);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                            // Send Email to User: Mannual Compare and Analyze until Pay Export Comparison is Fixed
                            Notification email = new Notification();
                            email.SendEmail(comparison[i]);

                        }

                        // If both Job are completed, then Run comparison OR If ForceCompareOnly is Enabled directly run the compare commands without checking status.
                        else if (((comparison[i].Status1 == "JobCompleted") && (comparison[i].Status2 == "JobCompleted")))
                        {
                            // Set Compared to 1 and change results status to comparison started, after Running Compare Command, so that we never run comparison again.
                            comparison[i].Compared = 1;
                            comparison[i].Results = "Comparison has started";

                            // Update this input

                            _context.Entry(comparison[i]).State = EntityState.Modified;

                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                if (!InputExists(comparison[i].ID))
                                {
                                    Console.WriteLine("Comparison Not Found");
                                }
                                else
                                {
                                    throw;
                                }
                            }

                            string compare_command = ".\\ComparePay RunType=" + job_code + " ProcessType=CompareResult InputFile=.\\Input\\" + client + "\\" + comparison_id + "\\Input" + job + ".json OutputFolder=.\\Output\\" + client + "\\" + comparison_id;

                            Process compare_cmd = new Process();
                            compare_cmd.StartInfo.FileName = "cmd.exe";
                            compare_cmd.StartInfo.RedirectStandardInput = true;
                            compare_cmd.StartInfo.RedirectStandardOutput = true;
                            compare_cmd.StartInfo.CreateNoWindow = true;
                            compare_cmd.StartInfo.UseShellExecute = false;
                            compare_cmd.Start();

                            compare_cmd.StandardInput.WriteLine(compare_command);
                            compare_cmd.StandardInput.Flush();
                            compare_cmd.StandardInput.Close();
                            // Don't wait for Compare Command
                            //compare_cmd.WaitForExit();

                            //Console.WriteLine(compare_cmd.StandardOutput.ReadToEnd());
                            //Console.ReadKey();


                        }



                    }


                    // If Force Compare Mode is Enabled, directly Run Comparison without any need to check status of Jobs.

                    else if ((comparison[i].Compared == 0) && (comparison[i].ForceCompareOnly == true))
                    {

                        // Set Compared to 1 and change results status to comparison started, after Running Compare Command, so that we never run comparison again.
                        comparison[i].Compared = 1;
                        comparison[i].Results = "Comparison has started";

                        // Update this input

                        _context.Entry(comparison[i]).State = EntityState.Modified;

                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!InputExists(comparison[i].ID))
                            {
                                Console.WriteLine("Comparison Not Found");
                            }
                            else
                            {
                                throw;
                            }
                        }


                        string compare_command = ".\\ComparePay RunType=" + job_code + " ProcessType=CompareResult InputFile=.\\Input\\" + client + "\\" + comparison_id + "\\Input" + job + ".json OutputFolder=.\\Output\\" + client + "\\" + comparison_id;

                        Process compare_cmd = new Process();
                        compare_cmd.StartInfo.FileName = "cmd.exe";
                        compare_cmd.StartInfo.RedirectStandardInput = true;
                        compare_cmd.StartInfo.RedirectStandardOutput = true;
                        compare_cmd.StartInfo.CreateNoWindow = true;
                        compare_cmd.StartInfo.UseShellExecute = false;
                        compare_cmd.Start();

                        compare_cmd.StandardInput.WriteLine(compare_command);
                        compare_cmd.StandardInput.Flush();
                        compare_cmd.StandardInput.Close();
                        // Don't wait for Compare Command
                        //compare_cmd.WaitForExit();

                        //Console.WriteLine(compare_cmd.StandardOutput.ReadToEnd());
                        //Console.ReadKey();



                    }

                    // Check for Result everytime, if Compared is 1, which highlights 3rd Command(Comparison) has been executed and now until we get either "Success" or "Warning" we can check for Results
                    // Also note: Comparison is 1, when Both Jobs are Completed or ForceCompare is Enabled.
                    // Keep running this command, until Compared changes to 2.

                    if (comparison[i].Compared == 1)
                    {
                        string pathoutput;

                        if (comparison[i].Job == "Export")
                        {
                            pathoutput = current_path + "\\Output\\" + client + "\\" + comparison_id + "\\CompareResultExport.txt";
                        }
                        else
                        {
                            pathoutput = current_path + "\\Output\\" + client + "\\" + comparison_id + "\\ComparisonResult.txt";
                        }


                        string[] lines = System.IO.File.ReadAllLines(pathoutput);

                        // Alternate: Can read only Last Line to increase speed

                        if (lines.Length > 0)
                        {
                            for (int j = lines.Length - 1; j >= 0; j--)
                            {
                                if (lines[j].Length > 9)
                                {
                                    if (lines[j].Substring(2, 7) == "SUCCESS")
                                    {
                                        comparison[i].Results = "SUCCESS";
                                        comparison[i].Compared = 2;

                                        // Update this input

                                        _context.Entry(comparison[i]).State = EntityState.Modified;

                                        try
                                        {
                                            await _context.SaveChangesAsync();
                                        }
                                        catch (DbUpdateConcurrencyException)
                                        {
                                            if (!InputExists(comparison[i].ID))
                                            {
                                                Console.WriteLine("Comparison Not Found");
                                            }
                                            else
                                            {
                                                throw;
                                            }
                                        }

                                        break;
                                    }
                                    else if (lines[j].Substring(2, 7) == "WARNING")
                                    {
                                        comparison[i].Results = "WARNING";
                                        comparison[i].Compared = 2;

                                        // Update this input

                                        _context.Entry(comparison[i]).State = EntityState.Modified;

                                        try
                                        {
                                            await _context.SaveChangesAsync();
                                        }
                                        catch (DbUpdateConcurrencyException)
                                        {
                                            if (!InputExists(comparison[i].ID))
                                            {
                                                Console.WriteLine("Comparison Not Found");
                                            }
                                            else
                                            {
                                                throw;
                                            }
                                        }

                                        break;
                                    }
                                    else if (lines[j].Substring(0, 5) == "Failed")
                                    {
                                        comparison[i].Results = "Failed";
                                        comparison[i].Compared = 3;

                                        // Update this input

                                        _context.Entry(comparison[i]).State = EntityState.Modified;

                                        try
                                        {
                                            await _context.SaveChangesAsync();
                                        }
                                        catch (DbUpdateConcurrencyException)
                                        {
                                            if (!InputExists(comparison[i].ID))
                                            {
                                                Console.WriteLine("Comparison Not Found");
                                            }
                                            else
                                            {
                                                throw;
                                            }
                                        }

                                        break;
                                    }

                                }

                            }
                        }
                    }

                    // Delete Input when Force Compare is Enabled

                    if ((comparison[i].Compared == 2) && (comparison[i].ForceCompareOnly == true))
                    {
                        // Delete Input Folder for this Comparison ID

                        // Path to Input Folder

                        string pathinputfolder = current_path + "\\Input\\" + client + "\\" + comparison_id;

                        try
                        {
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
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }


                    }


                    // Delete Input Folder, and all JSON File in it after getting Result or when atleast one Job has failed, and Comparison can't be done.
                    // Status, Log ID and Results are already stored in Database and Input files are of no use and is using a lot of memory.
                    // This is when Force Compare is Disabled

                    else if ((comparison[i].Compared == 2 && comparison[i].ForceCompareOnly == false) || (comparison[i].Compared == 3 && comparison[i].ForceCompareOnly == false))
                    {

                        // Path to Input Folder

                        string pathinputfolder = current_path + "\\Input\\" + client + "\\" + comparison_id;

                        try
                        {
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
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }

                    }

                    var log = new LoggerConfiguration()
                    .WriteTo.RollingFile("E:\\ServerSide\\Logs\\{Date}\\log.txt")
                    .CreateLogger();
                    log.Information("Starting Analyzing");

                    // Start Analyzing for this Comparison if we have Results

                    try
                    {

                        // URL Confriguation for Compare Pay Tool

                        // If Running on Local

                        // string api_url = "http://localhost:5000";

                        // If Running on Server

                        string api_url = "http://nan5dfc1web01.corpadds.com:8084/server";

                        if (comparison[i].Compared == 2 && comparison[i].Job == "PSR")
                        {
                            // Start Analyzing for this Comparison

                            string url = api_url + "/api/analyzepsr/" + comparison[i].ID.ToString();

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                            // Stop Blocking of Code, and continue without getting response
                            var response = request.GetResponseAsync().ConfigureAwait(false);
                        }
                        else if (comparison[i].Compared == 2 && comparison[i].Job == "BRR")
                        {
                            // Start Analyzing for this Comparison

                            string url = api_url + "/api/analyzebrr/" + comparison[i].ID.ToString();

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                            // Stop Blocking of Code, and continue without getting response
                            var response = request.GetResponseAsync().ConfigureAwait(false);
                        }
                        else if (comparison[i].Compared == 2 && comparison[i].Job == "JobStepRecalc")
                        {
                            // Start Analyzing for this Comparison

                            string url = api_url + "/api/analyzejsr/" + comparison[i].ID.ToString();

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                            // Stop Blocking of Code, and continue without getting response
                            var response = request.GetResponseAsync().ConfigureAwait(false);
                        }
                        else if (comparison[i].Compared == 2 && comparison[i].Job == "SCR")
                        {
                            // Start Analyzing for this Comparison

                            string url = api_url + "/api/analyzescr/" + comparison[i].ID.ToString();

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                            // Stop Blocking of Code, and continue without getting response
                            var response = request.GetResponseAsync().ConfigureAwait(false);
                        }
                        else if (comparison[i].Compared == 2 && comparison[i].Job == "AE_Sample")
                        {
                            // Start Analyzing for this Comparison

                            string url = api_url + "/api/analyzeae/" + comparison[i].ID.ToString();

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                            // Stop Blocking of Code, and continue without getting response
                            var response = request.GetResponseAsync().ConfigureAwait(false);
                        }
                        // When Pay Export Comparison is Fixed, add ability to Analyze Records
                        //else if (comparison[i].Compared == 2 && comparison[i].Job == "Export")
                        //{
                        //    // Start Analyzing for this Comparison

                        //    string url = api_url + "/api/analyzee/" + comparison[i].ID.ToString();

                        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                        //    // Stop Blocking of Code, and continue without getting response
                        //    var response = request.GetResponseAsync().ConfigureAwait(false);
                        //}

                    }
                    catch (Exception ex)
                    {
                        log.Fatal(ex, "Error in Analyze API Calling");
                    }

                    log.Information("Analyzed");
                    
                    // If Job Queue Failed or Job Failed, send Email to notify them, so that they can start Job Again.

                    if ((comparison[i].Compared == 3))
                    {
                        Notification email = new Notification();
                        email.SendEmail(comparison[i]);

                        string comparison_file = current_path + "\\Output\\" + client + "\\" + comparison_id + "\\ComparisonResult.txt";

                        // Delete Comparison Result

                        try
                        {
                            System.IO.File.Delete(comparison_file);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }

                    }

                    // After running all the required commands for this iteration of results,
                    // Update Table and Input API, so that all changes in "input" gets saved and be reflected.


                    _context.Entry(comparison[i]).State = EntityState.Modified;

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!InputExists(comparison[i].ID))
                        {
                            Console.WriteLine("Comparison Not Found");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                // Path Confriguation for Compare Pay Tool

                // If Running on Local - 1 Minute

                int time = 5000;

                // If Running on Server - 30 Minutes

                // int time = 1800000; 

                // Wait for specified time and then continue to Check Status and Results
                Thread.Sleep(time);
            }

        }

        private bool InputExists(int id)
        {
            return _context.Input.Any(e => e.ID == id);
        }
    }
}
