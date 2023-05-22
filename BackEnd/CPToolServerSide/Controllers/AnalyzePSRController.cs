using CPToolServerSide.Database;
using CPToolServerSide.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CPToolServerSide.Email;
using System.Diagnostics;
using Serilog;

namespace CPToolServerSide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyzePSRController : ControllerBase
    {
        private readonly CPTDBContext _context;

        public AnalyzePSRController(CPTDBContext context)
        {
            _context = context;
        }

        // GET: api/AnalyzePSR
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnalyzePSR>>> GetAnalyzePSR()
        {
            return await _context.AnalyzePSR.ToListAsync();
        }

        // GET: api/AnalyzePSR/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<AnalyzePSR>>> GetAnalyzePSR(int id)
        {
            // Get Input details using Comparison ID.
            var input = await _context.Input.FindAsync(id);

            var log = new LoggerConfiguration()
            .WriteTo.RollingFile("E:\\ServerSide\\Logs\\{Date}\\log.txt")
            .CreateLogger();
            log.Information("Reached Analyzing");

            if (input == null)
            {
                return NotFound();
            }

            // Get Analyzed Status for this Comparison ID
            int status = input.Analyzed;

            // Get Compared Status for this Comparison ID
            int compared = input.Compared;

            // Initialize Discrepacies List, which is to be returned after adding all discrepacies

            List<AnalyzePSR> discrepacies = new List<AnalyzePSR>();

            if (status == 0 && compared == 2)
            {

                // Set Analyzed to 1, since Analyze for this Comparison ID has started
                input.Analyzed = 1;

                // And update this input
                _context.Entry(input).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InputExists(input.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }


                // Get Client for this Input Parameters
                string client = input.Client.Replace(" ", string.Empty).Trim();

                // Get Versions for this Client
                string version1 = input.DBName_1;
                string version2 = input.DBName_2;

                // Get Comparison Results

                string results = input.Results;

                // Path Confriguation for Compare Pay Tool

                // If Running on Local

                // string current_path = Directory.GetCurrentDirectory();

                // If Running on Server

                string current_path = "E:\\ServerSide";

                // Path to Output Folder

                string pathoutputfolder = current_path + "\\Output\\" + client + "\\" + id.ToString();

                // Path of Analyze File

                string analyze_file = current_path + "\\Output\\" + client + "\\" + id.ToString() + "\\Analyze.txt";

                // Path of Comparison Result File

                string comparison_file = current_path + "\\Output\\" + client + "\\" + id.ToString() + "\\ComparisonResult.txt";

                // Get Folder using path(Output Folder)

                var directoryInfoOutput = new DirectoryInfo(pathoutputfolder);

                if (directoryInfoOutput.Exists)
                {
                    string[] output_files = Directory.GetFiles(pathoutputfolder);


                    string version1_path = new string("");
                    string version2_path = new string("");

                    for (int i = 0; i < output_files.Length; i++)
                    {
                        if (output_files[i].Contains(version1))
                        {
                            // Get Version 1 Results Path
                            version1_path = output_files[i];
                        }
                        else if (output_files[i].Contains(version2))
                        {
                            // Get Version 2 Results Path
                            version2_path = output_files[i];
                        }
                    }

                    // Link Output Version Path to Input (For Local Host)

                    if (version1_path != "")
                    {
                        input.Version1_Path = new Uri(version1_path).AbsoluteUri;
                    }
                    if (version2_path != "")
                    {
                        input.Version2_Path = new Uri(version2_path).AbsoluteUri;
                    }

                    // Update this input
                    _context.Entry(input).State = EntityState.Modified;

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!InputExists(input.ID))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }


                    if (version1_path != "" && version2_path != "" && results == "SUCCESS")
                    {
                        // No Difference - Exit with Result -> No Analyze Needed

                        // Create AnalyzePSR Message - No Difference

                        AnalyzePSR no_difference = new AnalyzePSR
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            Employee_ID = "No Difference Causing Employee",
                            Buisness_Date = "No Difference Causing Buisness Date",
                            Employee_Pay_AdjustID = "No Difference Causing Employee Pay AdjustID",
                            Discrepancy = "No Analyze Needed"
                        };

                        // Add No Analyze needed to Analyze Table and API

                        _context.AnalyzePSR.Add(no_difference);
                        await _context.SaveChangesAsync();

                        // Add in discrepacies list

                        discrepacies.Add(no_difference);


                    }
                    else if (version1_path == "" && version2_path == "" && results == "SUCCESS")
                    {

                        // No Difference - Exit with Result -> No Difference Reason

                        // Create AnalyzePSR Message - No Difference Reason

                        AnalyzePSR no_difference_reason = new AnalyzePSR
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            Employee_ID = "No Employee in Both Versions",
                            Buisness_Date = "No Buisness Date in Both Versions",
                            Employee_Pay_AdjustID = "No Employee Pay AdjustID in Both Versions",
                            Discrepancy = "No Difference Reason: No Record exist for both Versions"
                        };

                        // Add No Difference Reason to Analyze Table and API

                        _context.AnalyzePSR.Add(no_difference_reason);
                        await _context.SaveChangesAsync();

                        // Add in discrepacies list

                        discrepacies.Add(no_difference_reason);

                    }

                    else if (version1_path == "" && version2_path == "" && results == "WARNING")
                    {

                        // No Difference - Exit with Result -> Difference Reason

                        // Create AnalyzePSR Message - Difference Reason

                        AnalyzePSR difference_reason = new AnalyzePSR
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            Employee_ID = "Can't Find Employee",
                            Buisness_Date = "Can't Find Buisness Date",
                            Employee_Pay_AdjustID = "Can't Find Employee Pay AdjustID", 
                            Discrepancy = "Difference Reason: Can't Find Record for both Versions"
                        };

                        // Add Difference Reason to Analyze Table and API

                        _context.AnalyzePSR.Add(difference_reason);
                        await _context.SaveChangesAsync();

                        // Add in discrepacies list

                        discrepacies.Add(difference_reason);

                    }

                    // Check for Difference and Analyze Results
                    else if (version1_path != "" && version2_path != "" && results == "WARNING")
                    {
                        // Get Relative Path to Output Files for both Versions

                        string version1_file = version1_path.Split("\\" + id + "\\")[1];
                        string version2_file = version2_path.Split("\\" + id + "\\")[1];

                        // Analyzing Difference using LCS and Alignment Algorithm

                        string analyze_command = ".\\diff" + " " + ".\\Output\\" + client + "\\" + id + "\\" + version1_file + " " + ".\\Output\\" + client + "\\" + id + "\\" + version2_file + " " + ".\\Output\\" + client + "\\" + id + "\\Analyze.txt";

                        Process analyze_cmd = new Process();
                        analyze_cmd.StartInfo.FileName = "cmd.exe";
                        analyze_cmd.StartInfo.RedirectStandardInput = true;
                        analyze_cmd.StartInfo.RedirectStandardOutput = true;
                        analyze_cmd.StartInfo.CreateNoWindow = true;
                        analyze_cmd.StartInfo.UseShellExecute = false;
                        analyze_cmd.Start();

                        analyze_cmd.StandardInput.WriteLine(analyze_command);
                        analyze_cmd.StandardInput.Flush();
                        analyze_cmd.StandardInput.Close();
                        analyze_cmd.WaitForExit();

                        //Console.WriteLine(analyze_cmd.StandardOutput.ReadToEnd());
                        //Console.ReadKey();

                        // Wait for Analyzed to complete
                        StreamReader ouput = analyze_cmd.StandardOutput;
                        string analyzed_status = await ouput.ReadToEndAsync();

                        //Delete the Output Files for Both the Version, since Analyze Result already have Align View of Both Version Records or Results

                        // Line Index

                        int line_index = 0;

                        // Check Analyze file has all records aligned

                        FileInfo analyze_check = new FileInfo(analyze_file);

                        if (analyze_check.Length == 0)
                        {
                            // Can't Analyze - Exit with Result -> Large Record File

                            // Create AnalyzePSR Message - Can't Analyze

                            AnalyzePSR cant_analyze = new AnalyzePSR
                            {
                                Comparison_ID = id,
                                Client = input.Client,
                                Employee_ID = "Can't Analyze",
                                Buisness_Date = "Can't Analyze",
                                Employee_Pay_AdjustID = "Can't Analyze",
                                Discrepancy = "Can't Analyze Reason: Large Record File"
                            };

                            // Add Difference Reason to Analyze Table and API

                            _context.AnalyzePSR.Add(cant_analyze);
                            await _context.SaveChangesAsync();

                            // Add in discrepacies list

                            discrepacies.Add(cant_analyze);

                            // Delete Analyze Result (Without Indexing)

                            try
                            {
                                System.IO.File.Delete(analyze_file);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                            // Delete Comparison Result

                            try
                            {
                                System.IO.File.Delete(comparison_file);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                            // Set Analyzed to 2, since Analyze for this Comparison ID has Completed
                            input.Analyzed = 2;

                            // And update this input
                            _context.Entry(input).State = EntityState.Modified;

                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                if (!InputExists(input.ID))
                                {
                                    return NotFound();
                                }
                                else
                                {
                                    throw;
                                }
                            }

                            // Send Email to User with Comparison Result and notify them -> Analyze is Completed

                            Notification cant_email = new Notification();

                            cant_email.SendEmail(input);

                            return discrepacies;

                        }

                        // Get Analyze Results - Aligned and Contain Differences

                        // Finding Discrepacies and Difference

                        foreach (string line in System.IO.File.ReadLines(analyze_file))
                        {
                            if (line.Split("|")[0].Trim() == "Pay Summary Header")
                            {
                                // This is the first line of Analyze File, skip this iteration and continue with next line which have records from both version.
                                line_index++;
                                continue;
                            }
                            else if (line.Trim() == "|")
                            {
                                // Reading all Records from Both versions is done, break this iteration
                                break;
                            }
                            else
                            {
                                if (line.Split("|")[0].Trim() != "" && line.Split("|")[1].Trim() != "" && line.Split("|")[0].Trim() == line.Split("|")[1].Trim())
                                {
                                    // No need to check for any difference, since row are matching exactly
                                    line_index++;
                                    continue;
                                }
                                else if (line.Split("|")[0].Trim() != "" && line.Split("|")[1].Trim() != "" && line.Split("|")[0].Trim() != line.Split("|")[1].Trim())
                                {
                                    string[] version1_records = line.Split("|")[0].Trim().Split(",");
                                    string[] version2_records = line.Split("|")[1].Trim().Split(",");

                                    // Make Null Date for Comparison

                                    DateTime null_date = DateTime.ParseExact("0001-01-01 00:00:00:000", "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.InvariantCulture);

                                    // Duplicate Tardy Case OR EmployeePayAdjustID values are  Different OR EmployeePayAdjustID OR PayAdjCodeId values are Different
                                    if ((version1_records[8] != "") && (version2_records[8] != "") && (version1_records[7] != "") && (version2_records[7] != "") && (version1_records[0] == version2_records[0]) && (version1_records[1] == version2_records[1]) && (version1_records[2] == version2_records[2]) && (version1_records[3] == version2_records[3]) && (version1_records[4] == version2_records[4]) && (version1_records[5] == version2_records[5]) && (version1_records[6] == version2_records[6]) && (version1_records[8] != version2_records[8]) && (version1_records[9] == version2_records[9]) && (version1_records[10] == version2_records[10]) && (version1_records[11] == version2_records[11]) && (version1_records[12] == version2_records[12]) && (version1_records[13] == version2_records[13]))
                                    {
                                        // Get Employee ID from any one of the versions

                                        string employee_ID = version1_records[0];

                                        // Get Buisness Date from any one of the versions

                                        string buisness_date = version1_records[2];

                                        // Get EmployeePayAdjustId for both Versions.

                                        string EmployeePayAdjustId1 = version1_records[8];
                                        string EmployeePayAdjustId2 = version2_records[8];

                                        // Create ControlDB instance for this Client 2 Versions
                                        ControlDB controlDBNamespace1 = new ControlDB(input.ControlDBServer_Server1);
                                        ControlDB controlDBNamespace2 = new ControlDB(input.ControlDBServer_Server2);

                                        // Try Finding Tardy Discrepacy

                                        try
                                        {
                                            // Query - Database Version 1

                                            // Get Anchor Time for Both Version EmployeePayAdjustID

                                            // Get Anchor Time in Database Version 1, for EmployeePayAdjustId 1
                                            string sql1_v1 = $@"SELECT anchortime FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {EmployeePayAdjustId1} AND IsDeleted = 0 ";

                                            var sqlUtil1_v1 = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                            DateTime anchortime1_v1 = sqlUtil1_v1.GetValue<DateTime>(sql1_v1);


                                            // Get Anchor Time in Database Version 1, for EmployeePayAdjustId 2
                                            string sql2_v1 = $@"SELECT anchortime FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {EmployeePayAdjustId2} AND IsDeleted = 0 ";

                                            var sqlUtil2_v1 = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                            DateTime anchortime2_v1 = sqlUtil2_v1.GetValue<DateTime>(sql2_v1);


                                            // The anchortime of the two pay adjustments are the same and not empty or null and Is Deleted is false then duplicate tardy exists

                                            if (DateTime.Compare(anchortime1_v1, null_date) != 0 && DateTime.Compare(anchortime2_v1, null_date) != 0 && anchortime1_v1 != null && anchortime2_v1 != null && DateTime.Compare(anchortime1_v1, anchortime2_v1) == 0)
                                            {
                                                // Duplicate Tardy Exist - Exit with Result -> Duplicate tardy

                                                // Create a new Discrepacy - Duplicate tardy

                                                AnalyzePSR duplicate_tardy = new AnalyzePSR
                                                {
                                                    Comparison_ID = id,
                                                    Line = line_index,
                                                    Client = input.Client,
                                                    Employee_ID = employee_ID,
                                                    Buisness_Date = buisness_date,
                                                    Employee_Pay_AdjustID = version1_records[8] + " - " + version2_records[8],
                                                    Discrepancy = "Duplicate Tardy"
                                                };

                                                // Add Duplicate tardy Discrepacy to Analyze Table and API

                                                _context.AnalyzePSR.Add(duplicate_tardy);
                                                await _context.SaveChangesAsync();

                                                // Add in discrepacies list

                                                discrepacies.Add(duplicate_tardy);

                                                //  No Need to Check for other Version - Duplicate Tardy Already Found
                                                line_index++;
                                                continue;


                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                        }


                                        try
                                        {
                                            // Query - Database Version 2

                                            // Check if Anchor Time exist for Both Version EmployeePayAdjustID

                                            // Get Anchor Time in Database Version 2, for EmployeePayAdjustId 1
                                            string sql1_v2 = $@"SELECT anchortime FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {EmployeePayAdjustId1} AND IsDeleted = 0 ";

                                            var sqlUtil1_v2 = new SqlUtil(controlDBNamespace2.GetInstanceDBConnectionString(version2));
                                            DateTime anchortime1_v2 = sqlUtil1_v2.GetValue<DateTime>(sql1_v2);

                                            // Get Anchor Time in Database Version 2, for EmployeePayAdjustId 2
                                            string sql2_v2 = $@"SELECT anchortime FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {EmployeePayAdjustId2} AND IsDeleted = 0 ";

                                            var sqlUtil2_v2 = new SqlUtil(controlDBNamespace2.GetInstanceDBConnectionString(version2));
                                            DateTime anchortime2_v2 = sqlUtil2_v2.GetValue<DateTime>(sql2_v2);

                                            // The anchortime of the two pay adjustments are the same and not empty or null and Is Deleted is false then duplicate tardy exists

                                            if (DateTime.Compare(anchortime1_v2, null_date) != 0 && DateTime.Compare(anchortime2_v2, null_date) != 0 && anchortime1_v2 != null && anchortime2_v2 != null && DateTime.Compare(anchortime1_v2, anchortime2_v2) == 0)
                                            {
                                                // Duplicate Tardy Exist - No Need to Check for other Version - Exit with Result -> Duplicate tardy

                                                // Create a new Discrepacy - Duplicate tardy

                                                AnalyzePSR duplicate_tardy = new AnalyzePSR
                                                {
                                                    Comparison_ID = id,
                                                    Line = line_index,
                                                    Client = input.Client,
                                                    Employee_ID = employee_ID,
                                                    Buisness_Date = buisness_date,
                                                    Employee_Pay_AdjustID = version1_records[8] + " - " + version2_records[8],
                                                    Discrepancy = "Duplicate Tardy"
                                                };

                                                // Add Duplicate tardy Discrepacy to Analyze Table and API

                                                _context.AnalyzePSR.Add(duplicate_tardy);
                                                await _context.SaveChangesAsync();

                                                // Add in discrepacies list

                                                discrepacies.Add(duplicate_tardy);

                                                // Exit with results - Duplicate Tardy Found
                                                line_index++;
                                                continue;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                        }

                                        // Since Duplicate Tardy and no other known issue or discrepancy is found for this Record, Tell User to Mannual Investigate with some details highlighting difference which might be useful for User.

                                        // In this case, difference is a well know Difference - EmployeePayAdjustID values are  Different
                                        if (version1_records[7] == version2_records[7])
                                        {

                                            AnalyzePSR different_id = new AnalyzePSR
                                            {
                                                Comparison_ID = id,
                                                Line = line_index,
                                                Client = input.Client,
                                                Employee_ID = employee_ID,
                                                Buisness_Date = buisness_date,
                                                Employee_Pay_AdjustID = version1_records[8] + " - " + version2_records[8],
                                                Discrepancy = "Only EmployeePayAdjustId Mismatch"
                                            };

                                            // Add difference to Analyze Table and API

                                            _context.AnalyzePSR.Add(different_id);
                                            await _context.SaveChangesAsync();

                                            // Add in discrepacies list

                                            discrepacies.Add(different_id);

                                            line_index++;
                                            continue;
                                        }
                                        // In this case, difference is a well know Difference - EmployeePayAdjustID and PayAdjCodeId values are Different
                                        else
                                        {
                                            AnalyzePSR different_id = new AnalyzePSR
                                            {
                                                Comparison_ID = id,
                                                Line = line_index,
                                                Client = input.Client,
                                                Employee_ID = employee_ID,
                                                Buisness_Date = buisness_date,
                                                Employee_Pay_AdjustID = version1_records[8] + " - " + version2_records[8],
                                                Discrepancy = "Only EmployeePayAdjustId and PayAdjCodeId Mismatch"
                                            };

                                            // Add difference to Analyze Table and API

                                            _context.AnalyzePSR.Add(different_id);
                                            await _context.SaveChangesAsync();

                                            // Add in discrepacies list

                                            discrepacies.Add(different_id);

                                            line_index++;
                                            continue;
                                        }

                                    }

                                    // Overlapping Tardy Case OR Total Minutes and Pay Amount is different - Mannual Investigation Needed
                                    else if ((version1_records[8] != "") && (version2_records[8] != "") && (version1_records[0] == version2_records[0]) && (version1_records[1] == version2_records[1]) && (version1_records[2] == version2_records[2]) && (version1_records[3] == version2_records[3]) && (version1_records[4] == version2_records[4]) && (version1_records[5] == version2_records[5]) && (version1_records[6] == version2_records[6]) && (version1_records[7] == version2_records[7]) && (version1_records[9] == version2_records[9]) && (version1_records[10] == version2_records[10]) && (version1_records[11] != version2_records[11]))
                                    {
                                        // Get Employee ID from any one of the versions

                                        string employee_ID = version1_records[0];

                                        // Get Buisness Date from any one of the versions

                                        string buisness_date = version1_records[2];

                                        // Convert Minutes to Integer
                                        int version1_minutes = int.Parse(version1_records[11]);
                                        int version2_minutes = int.Parse(version2_records[11]);

                                        // Get EmployeePayAdjustID from Record which have more Total Time (Minutes)

                                        int equal_id;

                                        if (version1_minutes > version2_minutes)
                                        {
                                            equal_id = int.Parse(version1_records[8]);
                                        }
                                        else
                                        {
                                            equal_id = int.Parse(version2_records[8]);
                                        }


                                        // This EmployeeAdjustmentPay id has TimeStart and TimeEnd equal to the shift

                                        // Create ControlDB instance for this Client 2 Versions
                                        ControlDB controlDBNamespace1 = new ControlDB(input.ControlDBServer_Server1);
                                        ControlDB controlDBNamespace2 = new ControlDB(input.ControlDBServer_Server2);

                                        try
                                        {
                                            // Get TimeStart and TimeEnd for this EmployeeAdjustmentPay id

                                            // TimeStart

                                            string sql_timestart = $@"SELECT timestart FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {equal_id} AND IsDeleted = 0 ";

                                            var sqlUtil_timestart = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                            DateTime time_start = sqlUtil_timestart.GetValue<DateTime>(sql_timestart);

                                            // TimeEnd

                                            string sql_timeend = $@"SELECT timeend FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {equal_id} AND IsDeleted = 0";

                                            var sqlUtil_timeend = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                            DateTime time_end = sqlUtil_timeend.GetValue<DateTime>(sql_timeend);

                                            // Now, check is there Tardy generated when TimeStart is Same as Shift, and TimeEnd is between TimeStart and TimeEnd of the shift

                                            string sql_endtime_endtime = $@"SELECT timeend FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND timestart = '{time_start}' AND timeend BETWEEN '{time_start}' AND '{time_end}'";

                                            var sqlUtil_anchor_endtime = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                            DateTime endtime_endtime = sqlUtil_anchor_endtime.GetValue<DateTime>(sql_endtime_endtime);

                                            // Now, check is there Tardy generated when TimeEnd is Same as Shift, and TimeStart is between TimeStart and TimeEnd of the shift

                                            string sql_starttime_starttime = $@"SELECT timestart FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND timeend = '{time_end}' AND timestart BETWEEN '{time_start}' AND '{time_end}'";

                                            var sqlUtil_anchor_starttime = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                            DateTime starttime_starttime = sqlUtil_anchor_starttime.GetValue<DateTime>(sql_starttime_starttime);

                                            // Now, there can be 3 Scenarios
                                            // 1. Tardy Generated when there is Early Out or Late Out & Tardy Generated when there is Early In or Late In
                                            // 2. Tardy Generated when there is Early Out or Late Out
                                            // 3. Tardy Generated when there is Early In or Late In


                                            // Scenario 1

                                            // If such Tardy(row) exist, Then we have Overlapping Tardy Case - When Employee has Start Punch after Start Time.
                                            // and
                                            // If such Tardy(row) exist, Then we have Overlapping Tardy Case - When Employee has End Punch before End Time.



                                            if (DateTime.Compare(endtime_endtime, null_date) != 0 && endtime_endtime != null && DateTime.Compare(endtime_endtime, time_end) != 0 && DateTime.Compare(starttime_starttime, null_date) != 0 && starttime_starttime != null && DateTime.Compare(starttime_starttime, time_start) != 0)
                                            {
                                                string employee_pay_adjust_id = string.Empty;
                                                if (version1_records[8] == version2_records[8])
                                                {
                                                    string sql_adjustid1 = $@"SELECT EmployeePayAdjustId FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND timestart = '{time_start}' AND timeend BETWEEN '{time_start}' AND '{time_end}'";

                                                    var sqlUtil_adjustid1 = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                                    string adjustid1 = sqlUtil_adjustid1.GetValue<int>(sql_adjustid1).ToString();


                                                    string sql_adjustid2 = $@"SELECT EmployeePayAdjustId FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} ANDBusinessDate = '{buisness_date}' AND timeend = '{time_end}' AND timestart BETWEEN '{time_start}' AND '{time_end}'";

                                                    var sqlUtil_adjustid2 = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                                    string adjustid2 = sqlUtil_adjustid2.GetValue<int>(sql_adjustid2).ToString();

                                                    if (version1_minutes > version2_minutes)
                                                    {
                                                        employee_pay_adjust_id = version1_records[8] + " - ( " + adjustid1 + " - " + adjustid2 + " )";
                                                    }
                                                    else
                                                    {
                                                        employee_pay_adjust_id = "( " + adjustid1 + " - " + adjustid2 + " ) - " + version1_records[8];
                                                    }

                                                }
                                                else
                                                {
                                                    employee_pay_adjust_id = version1_records[8] + " - " + version2_records[8];
                                                }

                                                // Create a new Discrepacy - Overlapping tardy

                                                AnalyzePSR overlapping_tardy = new AnalyzePSR
                                                {
                                                    Comparison_ID = id,
                                                    Line = line_index,
                                                    Client = input.Client,
                                                    Employee_ID = employee_ID,
                                                    Buisness_Date = buisness_date,
                                                    Employee_Pay_AdjustID = employee_pay_adjust_id,
                                                    Discrepancy = "Overlapping Tardy"
                                                };

                                                // Add Overlapping tardy Discrepacy to Analyze Table and API

                                                _context.AnalyzePSR.Add(overlapping_tardy);
                                                await _context.SaveChangesAsync();

                                                // Add in discrepacies list

                                                discrepacies.Add(overlapping_tardy);

                                                // Exit With Results - Overlapping Tardy

                                                line_index++;
                                                continue;
                                            }


                                            // Scenario 2

                                            // If such Tardy(row) exist, Then we have Overlapping Tardy Case - Early Out or Late Out.

                                            else if (DateTime.Compare(endtime_endtime, null_date) != 0 && endtime_endtime != null && DateTime.Compare(endtime_endtime, time_end) != 0)
                                            {
                                                string employee_pay_adjust_id = string.Empty;
                                                if (version1_records[8] == version2_records[8])
                                                {
                                                    string sql_adjustid = $@"SELECT EmployeePayAdjustId FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND timestart = '{time_start}' AND timeend BETWEEN '{time_start}' AND '{time_end}'";

                                                    var sqlUtil_adjustid = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                                    string adjustid = sqlUtil_adjustid.GetValue<int>(sql_adjustid).ToString();

                                                    if (version1_minutes > version2_minutes)
                                                    {
                                                        employee_pay_adjust_id = version1_records[8] + " - " + adjustid;
                                                    }
                                                    else
                                                    {
                                                        employee_pay_adjust_id = adjustid + " - " + version1_records[8];
                                                    }

                                                }
                                                else
                                                {
                                                    employee_pay_adjust_id = version1_records[8] + " - " + version2_records[8];
                                                }
                                                // Create a new Discrepacy - Overlapping tardy

                                                AnalyzePSR overlapping_tardy = new AnalyzePSR
                                                {
                                                    Comparison_ID = id,
                                                    Line = line_index,
                                                    Client = input.Client,
                                                    Employee_ID = employee_ID,
                                                    Buisness_Date = buisness_date,
                                                    Employee_Pay_AdjustID = employee_pay_adjust_id,
                                                    Discrepancy = "Overlapping Tardy"
                                                };

                                                // Add Overlapping tardy Discrepacy to Analyze Table and API

                                                _context.AnalyzePSR.Add(overlapping_tardy);
                                                await _context.SaveChangesAsync();

                                                // Add in discrepacies list

                                                discrepacies.Add(overlapping_tardy);

                                                // Exit With Results - Overlapping Tardy
                                                line_index++;
                                                continue;
                                            }

                                            // Scenario 3

                                            // If such Tardy(row) exist, Then we have Overlapping Tardy Case - When Employee has End Punch before End Time.


                                            else if (DateTime.Compare(starttime_starttime, null_date) != 0 && starttime_starttime != null && DateTime.Compare(starttime_starttime, time_start) != 0)
                                            {
                                                string employee_pay_adjust_id = string.Empty;
                                                if (version1_records[8] == version2_records[8])
                                                {
                                                    string sql_adjustid = $@"SELECT EmployeePayAdjustId FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} ANDBusinessDate = '{buisness_date}' AND timeend = '{time_end}' AND timestart BETWEEN '{time_start}' AND '{time_end}'";

                                                    var sqlUtil_adjustid = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                                    string adjustid = sqlUtil_adjustid.GetValue<int>(sql_adjustid).ToString();

                                                    if (version1_minutes > version2_minutes)
                                                    {
                                                        employee_pay_adjust_id = version1_records[8] + " - " + adjustid;
                                                    }
                                                    else
                                                    {
                                                        employee_pay_adjust_id = adjustid + " - " + version1_records[8];
                                                    }
                                                }
                                                else
                                                {
                                                    employee_pay_adjust_id = version1_records[8] + " - " + version2_records[8];
                                                }

                                                // Create a new Discrepacy - Overlapping tardy

                                                AnalyzePSR overlapping_tardy = new AnalyzePSR
                                                {
                                                    Comparison_ID = id,
                                                    Line = line_index,
                                                    Client = input.Client,
                                                    Employee_ID = employee_ID,
                                                    Buisness_Date = buisness_date,
                                                    Employee_Pay_AdjustID = employee_pay_adjust_id,
                                                    Discrepancy = "Overlapping Tardy"
                                                };

                                                // Add Overlapping tardy Discrepacy to Analyze Table and API

                                                _context.AnalyzePSR.Add(overlapping_tardy);
                                                await _context.SaveChangesAsync();

                                                // Add in discrepacies list

                                                discrepacies.Add(overlapping_tardy);

                                                // Exit With Results - Overlapping Tardy
                                                line_index++;
                                                continue;
                                            }

                                            // Since Overlapping Tardy and no other known issue or discrepancy is found for this Record, Tell User to Mannual Investigate with some details highlighting difference which might be useful for User.
                                            // It might be the case when Pay Amount is broken down to more than one records, in some situtation

                                            else
                                            {
                                                // Create a new difference - Pay Amount is broken down to more than one records

                                                AnalyzePSR different_pay = new AnalyzePSR
                                                {
                                                    Comparison_ID = id,
                                                    Line = line_index,
                                                    Client = input.Client,
                                                    Employee_ID = employee_ID,
                                                    Buisness_Date = buisness_date,
                                                    Employee_Pay_AdjustID = version1_records[8] + " - " + version2_records[8],
                                                    Discrepancy = "Total Minutes and Pay Amount is different - Mannual Investigation Needed"
                                                };

                                                // Add Difference to Analyze Table and API

                                                _context.AnalyzePSR.Add(different_pay);
                                                await _context.SaveChangesAsync();

                                                // Add in discrepacies list

                                                discrepacies.Add(different_pay);

                                                line_index++;
                                                continue;

                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                        }
                                    }
                                    // Duplicate/Overlapping Work Assignment with Different Rate Case OR Only Rate and Pay Amount is Different - Mannual Investigation Needed
                                    else if ((version1_records[10] != "") && (version2_records[10] != "") && (version1_records[13] != "") && (version2_records[13] != "") && (version1_records[0] == version2_records[0]) && (version1_records[1] == version2_records[1]) && (version1_records[2] == version2_records[2]) && (version1_records[3] == version2_records[3]) && (version1_records[4] == version2_records[4]) && (version1_records[5] == version2_records[5]) && (version1_records[6] == version2_records[6]) && (version1_records[7] == version2_records[7]) && (version1_records[8] == version2_records[8]) && (version1_records[9] == version2_records[9]) && (version1_records[10] != version2_records[10]) && (version1_records[11] == version2_records[11] && (version1_records[12] == version2_records[12]) && (version1_records[13] != version2_records[13])))
                                    {
                                        // Get Employee ID from any one of the versions

                                        string employee_ID = version1_records[0];

                                        // Get Buisness Date from any one of the versions

                                        string buisness_date = version1_records[2];

                                        // Get Rates from both the Versions
                                        double rate1 = double.Parse(version1_records[10]);
                                        double rate2 = double.Parse(version2_records[10]);

                                        // Get Dept ID from any of the Versions, since it is same
                                        int dept_id = int.Parse(version1_records[5]);

                                        // Create ControlDB instance for this Client 2 Versions
                                        ControlDB controlDBNamespace1 = new ControlDB(input.ControlDBServer_Server1);
                                        ControlDB controlDBNamespace2 = new ControlDB(input.ControlDBServer_Server2);

                                        // For Version 1 - Employee Work Assignment Table, search if we have 2 Work Assignment for same Department (DeptJobId) with rate equal to
                                        // rate1 and rate2, such that Business Date is between Effective From and Effective Date for both these Work Assignment
                                        try
                                        {
                                            // Find Effective From - Date for Rate 1, Work Assignment

                                            string effective_start1_v1 = $@"SELECT EffectiveStart FROM EmployeeWorkAssignment WITH(NOLOCK) WHERE employeeid = {employee_ID} AND DeptJobId = '{dept_id}' AND Rate = {rate1}";

                                            var sqlUtil_effective_start1_v1 = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                            DateTime effective_from1_v1 = sqlUtil_effective_start1_v1.GetValue<DateTime>(effective_start1_v1);

                                            // Find Effective From - Date for Rate 2, Work Assignment

                                            string effective_start2_v1 = $@"SELECT EffectiveStart FROM EmployeeWorkAssignment WITH(NOLOCK) WHERE employeeid = {employee_ID} AND DeptJobId = '{dept_id}' AND Rate = {rate2}";

                                            var sqlUtil_effective_start2_v1 = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                            DateTime effective_from2_v1 = sqlUtil_effective_start2_v1.GetValue<DateTime>(effective_start2_v1);



                                            if (effective_from1_v1 != null && effective_from2_v1 != null && DateTime.Compare(effective_from1_v1, null_date) != 0 && DateTime.Compare(effective_from2_v1, null_date) != 0)
                                            {
                                                // Find Effective To - Date for Rate 1, Work Assignment

                                                string effective_end1_v1 = $@"SELECT EffectiveEnd FROM EmployeeWorkAssignment WITH(NOLOCK) WHERE employeeid = {employee_ID} AND DeptJobId = '{dept_id}' AND Rate = {rate1}";

                                                var sqlUtil_effective_end1_v1 = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                                DateTime effective_to1_v1 = sqlUtil_effective_end1_v1.GetValue<DateTime>(effective_end1_v1);

                                                // Find Effective To - Date for Rate 2, Work Assignment

                                                string effective_end2_v1 = $@"SELECT EffectiveEnd FROM EmployeeWorkAssignment WITH(NOLOCK) WHERE employeeid = {employee_ID} AND DeptJobId = '{dept_id}' AND Rate = {rate2}";

                                                var sqlUtil_effective_end2_v1 = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                                DateTime effective_to2_v1 = sqlUtil_effective_end1_v1.GetValue<DateTime>(effective_end2_v1);

                                                // Now, using Effective Start Date and Effective End Date for each Work Assignment, check if Business Date is between the Effective Time Period

                                                // Convert Business Date to DateTime format

                                                DateTime date = DateTime.ParseExact(buisness_date + " 00:00:00:000", "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.InvariantCulture);

                                                bool in_period1_v1 = false;
                                                bool in_period2_v1 = false;

                                                // For 1st Work Assignment with rate1

                                                if (DateTime.Compare(effective_to1_v1, null_date) != 0)
                                                {
                                                    if ((DateTime.Compare(effective_from1_v1, date) < 0 || DateTime.Compare(effective_from1_v1, date) == 0) && (DateTime.Compare(date, effective_to1_v1) < 0 || DateTime.Compare(date, effective_to1_v1) == 0))
                                                    {
                                                        in_period1_v1 = true;
                                                    }
                                                }
                                                else
                                                {
                                                    if ((DateTime.Compare(effective_from1_v1, date) < 0 || DateTime.Compare(effective_from1_v1, date) == 0))
                                                    {
                                                        in_period1_v1 = true;
                                                    }
                                                }

                                                // For 2st Work Assignment with rate2

                                                if (DateTime.Compare(effective_to2_v1, null_date) != 0)
                                                {
                                                    if ((DateTime.Compare(effective_from2_v1, date) < 0 || DateTime.Compare(effective_from2_v1, date) == 0) && (DateTime.Compare(date, effective_to2_v1) < 0 || DateTime.Compare(date, effective_to2_v1) == 0))
                                                    {
                                                        in_period2_v1 = true;
                                                    }
                                                }
                                                else
                                                {
                                                    if ((DateTime.Compare(effective_from2_v1, date) < 0 || DateTime.Compare(effective_from2_v1, date) == 0))
                                                    {
                                                        in_period2_v1 = true;
                                                    }
                                                }

                                                // Now, if Buisness Date lies in Both Work Assignment Period, it is a Duplicate Work Assignment with Different Rates Discrepancy

                                                if (in_period1_v1 && in_period2_v1)
                                                {
                                                    // Create a new Discrepacy - Duplicate Work Assignment with Different Rates

                                                    AnalyzePSR duplicate_wa = new AnalyzePSR
                                                    {
                                                        Comparison_ID = id,
                                                        Line = line_index,
                                                        Client = input.Client,
                                                        Employee_ID = employee_ID,
                                                        Buisness_Date = buisness_date,
                                                        Employee_Pay_AdjustID = version1_records[8] + " - " + version2_records[8],
                                                        Discrepancy = "Duplicate Work Assignment with Different Rates"
                                                    };

                                                    // Add Duplicate Work Assignment with Different Rates Discrepacy to Analyze Table and API

                                                    _context.AnalyzePSR.Add(duplicate_wa);
                                                    await _context.SaveChangesAsync();

                                                    // Add in discrepacies list

                                                    discrepacies.Add(duplicate_wa);

                                                    // Exit With Results - Duplicate Work Assignment with Different Rates. No need to check in other version

                                                    line_index++;
                                                    continue;
                                                }

                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                        }

                                        // For Version 2 - Employee Work Assignment Table, search if we have 2 Work Assignment for same Department (DeptJobId) with rate equal to
                                        // rate1 and rate2, such that Business Date is between Effective From and Effective Date for both these Work Assignment
                                        try
                                        {
                                            // Find Effective From - Date for Rate 1, Work Assignment

                                            string effective_start1_v2 = $@"SELECT EffectiveStart FROM EmployeeWorkAssignment WITH(NOLOCK) WHERE employeeid = {employee_ID} AND DeptJobId = '{dept_id}' AND Rate = {rate1}";

                                            var sqlUtil_effective_start1_v2 = new SqlUtil(controlDBNamespace2.GetInstanceDBConnectionString(version2));
                                            DateTime effective_from1_v2 = sqlUtil_effective_start1_v2.GetValue<DateTime>(effective_start1_v2);

                                            // Find Effective From - Date for Rate 2, Work Assignment

                                            string effective_start2_v2 = $@"SELECT EffectiveStart FROM EmployeeWorkAssignment WITH(NOLOCK) WHERE employeeid = {employee_ID} AND DeptJobId = '{dept_id}' AND Rate = {rate2}";

                                            var sqlUtil_effective_start2_v2 = new SqlUtil(controlDBNamespace2.GetInstanceDBConnectionString(version2));
                                            DateTime effective_from2_v2 = sqlUtil_effective_start2_v2.GetValue<DateTime>(effective_start2_v2);

                                            if (effective_from1_v2 != null && effective_from2_v2 != null && DateTime.Compare(effective_from1_v2, null_date) != 0 && DateTime.Compare(effective_from2_v2, null_date) != 0)
                                            {
                                                // Find Effective To - Date for Rate 1, Work Assignment

                                                string effective_end1_v2 = $@"SELECT EffectiveEnd FROM EmployeeWorkAssignment WITH(NOLOCK) WHERE employeeid = {employee_ID} AND DeptJobId = '{dept_id}' AND Rate = {rate1}";

                                                var sqlUtil_effective_end1_v2 = new SqlUtil(controlDBNamespace2.GetInstanceDBConnectionString(version2));
                                                DateTime effective_to1_v2 = sqlUtil_effective_end1_v2.GetValue<DateTime>(effective_end1_v2);

                                                // Find Effective To - Date for Rate 2, Work Assignment

                                                string effective_end2_v2 = $@"SELECT EffectiveEnd FROM EmployeeWorkAssignment WITH(NOLOCK) WHERE employeeid = {employee_ID} AND DeptJobId = '{dept_id}' AND Rate = {rate2}";

                                                var sqlUtil_effective_end2_v2 = new SqlUtil(controlDBNamespace2.GetInstanceDBConnectionString(version2));
                                                DateTime effective_to2_v2 = sqlUtil_effective_end1_v2.GetValue<DateTime>(effective_end2_v2);

                                                // Now, using Effective Start Date and Effective End Date for each Work Assignment, check if Business Date is between the Effective Time Period

                                                // Convert Business Date to DateTime format

                                                DateTime date = DateTime.ParseExact(buisness_date + " 00:00:00:000", "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.InvariantCulture);

                                                bool in_period1_v2 = false;
                                                bool in_period2_v2 = false;

                                                // For 1st Work Assignment with rate1

                                                if (DateTime.Compare(effective_to1_v2, null_date) != 0)
                                                {
                                                    if ((DateTime.Compare(effective_from1_v2, date) < 0 || DateTime.Compare(effective_from1_v2, date) == 0) && (DateTime.Compare(date, effective_to1_v2) < 0 || DateTime.Compare(date, effective_to1_v2) == 0))
                                                    {
                                                        in_period1_v2 = true;
                                                    }
                                                }
                                                else
                                                {
                                                    if ((DateTime.Compare(effective_from1_v2, date) < 0 || DateTime.Compare(effective_from1_v2, date) == 0))
                                                    {
                                                        in_period1_v2 = true;
                                                    }
                                                }

                                                // For 2st Work Assignment with rate2

                                                if (DateTime.Compare(effective_to2_v2, null_date) != 0)
                                                {
                                                    if ((DateTime.Compare(effective_from2_v2, date) < 0 || DateTime.Compare(effective_from2_v2, date) == 0) && (DateTime.Compare(date, effective_to2_v2) < 0 || DateTime.Compare(date, effective_to2_v2) == 0))
                                                    {
                                                        in_period2_v2 = true;
                                                    }
                                                }
                                                else
                                                {
                                                    if ((DateTime.Compare(effective_from2_v2, date) < 0 || DateTime.Compare(effective_from2_v2, date) == 0))
                                                    {
                                                        in_period2_v2 = true;
                                                    }
                                                }

                                                // Now, if Buisness Date lies in Both Work Assignment Period, it is a Duplicate Work Assignment with Different Rates Discrepancy

                                                if (in_period1_v2 && in_period2_v2)
                                                {
                                                    // Create a new Discrepacy - Duplicate Work Assignment with Different Rates

                                                    AnalyzePSR duplicate_wa = new AnalyzePSR
                                                    {
                                                        Comparison_ID = id,
                                                        Line = line_index,
                                                        Client = input.Client,
                                                        Employee_ID = employee_ID,
                                                        Buisness_Date = buisness_date,
                                                        Employee_Pay_AdjustID = version1_records[8] + " - " + version2_records[8],
                                                        Discrepancy = "Duplicate Work Assignment with Different Rates"
                                                    };

                                                    // Add Duplicate Work Assignment with Different Rates Discrepacy to Analyze Table and API

                                                    _context.AnalyzePSR.Add(duplicate_wa);
                                                    await _context.SaveChangesAsync();

                                                    // Add in discrepacies list

                                                    discrepacies.Add(duplicate_wa);

                                                    // Exit With Results - Duplicate Work Assignment with Different Rates

                                                    line_index++;
                                                    continue;
                                                }
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                        }

                                        // Since Duplicate Work Assignment with Different Rates and no other known issue or discrepancy is found for this Record, Tell User to Mannual Investigate with some details highlighting difference which might be useful for User.

                                        AnalyzePSR mannual_rate = new AnalyzePSR
                                        {
                                            Comparison_ID = id,
                                            Line = line_index,
                                            Client = input.Client,
                                            Employee_ID = employee_ID,
                                            Buisness_Date = buisness_date,
                                            Employee_Pay_AdjustID = version1_records[8] + " - " + version2_records[8],
                                            Discrepancy = "Only Rate and Pay Amount is Different - Mannual Investigation Needed"
                                        };

                                        // Add Difference to Analyze Table and API

                                        _context.AnalyzePSR.Add(mannual_rate);
                                        await _context.SaveChangesAsync();

                                        // Add in discrepacies list

                                        discrepacies.Add(mannual_rate);

                                        line_index++;
                                        continue;
                                    }

                                    // If reached here, no known issue or discrepancy is found for this Record, other than that there is also no additional information
                                    // which can be useful for User therefore tell User to Mannually investigate this record.

                                    // Add a check for the scenerio, if Employee ID or Buisness Date is different but still records are aligned

                                    string emp_id = string.Empty;
                                    string buis_date = string.Empty;

                                    if (version1_records[0] == version2_records[0])
                                    {
                                        emp_id = version1_records[0];
                                    }
                                    else
                                    {
                                        emp_id = version1_records[0] + " - " + version2_records[0];
                                    }

                                    if (version1_records[2] == version2_records[2])
                                    {
                                        buis_date = version1_records[2];
                                    }
                                    else
                                    {
                                        buis_date = version1_records[2] + " - " + version2_records[2];
                                    }

                                    AnalyzePSR mannual = new AnalyzePSR
                                    {
                                        Comparison_ID = id,
                                        Line = line_index,
                                        Client = input.Client,
                                        Employee_ID = emp_id,
                                        Buisness_Date = buis_date,
                                        Employee_Pay_AdjustID = version1_records[8] + " - " + version2_records[8],
                                        Discrepancy = "Mannual Investigation Needed"
                                    };

                                    // Add Difference to Analyze Table and API

                                    _context.AnalyzePSR.Add(mannual);
                                    await _context.SaveChangesAsync();

                                    // Add in discrepacies list

                                    discrepacies.Add(mannual);

                                    line_index++;
                                    continue;

                                }
                                else if (line.Split("|")[0].Trim() != "" && line.Split("|")[1].Trim() == "")
                                {
                                    string[] version1_records = line.Split("|")[0].Trim().Split(",");

                                    // Get Employee ID from any one of the versions

                                    string employee_ID = version1_records[0];

                                    // Get Buisness Date from any one of the versions

                                    string buisness_date = version1_records[2];

                                    // Make Null Date for Comparison

                                    DateTime null_date = DateTime.ParseExact("0001-01-01 00:00:00:000", "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.InvariantCulture);

                                    string discrepancy = "Version 2 has no corresponding record - Mannual Investigation Needed";
                                    string payadjust_id = version1_records[8] + " -";

                                    // Check if Overlapping Tardy is causing this difference
                                    List<AnalyzePSR> overlapping_tardy = await _context.AnalyzePSR.Where(e => e.Employee_Pay_AdjustID.Contains(version1_records[8]) && e.Employee_ID == employee_ID && e.Comparison_ID == id && e.Buisness_Date == buisness_date && e.Discrepancy == "Overlapping Tardy").ToListAsync();

                                    if (overlapping_tardy.Count == 1)
                                    {
                                        discrepancy = "Version 2 has no corresponding record - Reason - Overlapping Tardy";
                                        payadjust_id = version1_records[8] + " - " + overlapping_tardy[0].Employee_Pay_AdjustID.Split(" - ")[1];
                                    }

                                    // Check if Pay Amount is broken down to more than one records is causing this difference
                                    List<AnalyzePSR> broken_pay = await _context.AnalyzePSR.Where(e => e.Employee_ID == employee_ID && e.Comparison_ID == id && e.Buisness_Date == buisness_date && e.Discrepancy == "Total Minutes and Pay Amount is different - Mannual Investigation Needed").ToListAsync();

                                    if (broken_pay.Count == 1)
                                    {
                                        string version1_id = broken_pay[0].Employee_Pay_AdjustID.Split(" - ")[0];
                                        string version2_id = broken_pay[0].Employee_Pay_AdjustID.Split(" - ")[1];

                                        if (version1_id != version2_id)
                                        {
                                            // Pay Amount is broken down to more than one records is causing this difference, since this record Employee Pay Adjust ID is in broken Pay Amount
                                            if (broken_pay[0].Employee_Pay_AdjustID.Contains(version1_records[8]))
                                            {
                                                discrepancy = "Version 2 has no corresponding record - Reason - Pay Amount is broken down to more than one records";
                                                payadjust_id = version1_records[8] + " - " + broken_pay[0].Employee_Pay_AdjustID.Split(" - ")[1];
                                            }
                                        }
                                        else
                                        {
                                            foreach (string broken_line in System.IO.File.ReadLines(analyze_file))
                                            {
                                                if (broken_line.Split("|")[0].Trim() == "Pay Summary Header")
                                                {
                                                    // This is the first line of Analyze File, skip this iteration and continue with next line which have records from both version.
                                                    continue;
                                                }
                                                else if (broken_line.Trim() == "|")
                                                {
                                                    // Reading all Records from Both versions is done, break this iteration
                                                    break;
                                                }

                                                else if (broken_line.Split("|")[0].Trim().Contains("," + version1_id + ","))
                                                {
                                                    // Get Total Minutes for these records, and if sum of Total Worked Minutes for this record(line) and version2 record(broken line) = version2 record(broken line)
                                                    // Then Pay Amount is broken down to more than one records is causing this difference

                                                    int version1_minutes = int.Parse(version1_records[11]);
                                                    int version1_brokenminutes = int.Parse(broken_line.Split("|")[0].Trim().Split(",")[11]);
                                                    int version2_brokenminutes = int.Parse(broken_line.Split("|")[1].Trim().Split(",")[11]);

                                                    int sum = version1_minutes + version1_brokenminutes;

                                                    if (sum == version2_brokenminutes)
                                                    {
                                                        discrepancy = "Version 2 has no corresponding record - Reason - Pay Amount is broken down to more than one records";
                                                        payadjust_id = version1_records[8] + " - " + broken_pay[0].Employee_Pay_AdjustID.Split(" - ")[1];

                                                        // Exit this search, since Pay Amount is broken down to more than one records, is detected
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // Check if Duplicate Tardy is caused by this difference, such that we have another similar record in other version which have no corresponding record and these two records are Duplicate Tardy
                                    List<AnalyzePSR> duplicate_tardy_align = await _context.AnalyzePSR.Where(e => e.Employee_Pay_AdjustID.Contains(version1_records[8]) && e.Employee_ID == employee_ID && e.Comparison_ID == id && e.Buisness_Date == buisness_date && e.Discrepancy == "Version 1 and 2 has no corresponding aligned record, but these records are similar and exist as Duplicate Tardy").ToListAsync();

                                    if (duplicate_tardy_align.Count == 1)
                                    {
                                        // Just Continue, since this issue is already detected
                                        line_index++;
                                        continue;
                                    }

                                    if (discrepancy != "Version 2 has no corresponding record - Reason - Pay Amount is broken down to more than one records" && discrepancy != "Version 2 has no corresponding record - Reason - Overlapping Tardy")
                                    {
                                        // Count Number of Similar Records in Version 1, such that thier corresponding Version 2 records doesn't exist
                                        int similar = 0;

                                        // Create a Temperory variables to store similar record type
                                        string discrepancy_temp = string.Empty;
                                        string payadjust_id_temp = string.Empty;

                                        // To check If Duplicate Tardy Exist
                                        bool duplicate_exact_exist = false;

                                        foreach (string position in System.IO.File.ReadLines(analyze_file))
                                        {
                                            if (position.Split("|")[0].Trim() == "Pay Summary Header")
                                            {
                                                // This is the first line of Analyze File, skip this iteration and continue with next line which have records from both version.
                                                continue;
                                            }
                                            else if (position.Trim() == "|")
                                            {
                                                // Reading all Records from Both versions is done, break this iteration
                                                break;
                                            }

                                            else if ((position.Split("|")[0].Trim() == "") && (position.Split("|")[1].Trim() != "") && (position.Split("|")[1].Trim().Split(",")[0] == version1_records[0]) && (position.Split("|")[1].Trim().Split(",")[1] == version1_records[1]) && (position.Split("|")[1].Trim().Split(",")[2] == version1_records[2]) && (position.Split("|")[1].Trim().Split(",")[3] == version1_records[3]) && (position.Split("|")[1].Trim().Split(",")[4] == version1_records[4]) && (position.Split("|")[1].Trim().Split(",")[5] == version1_records[5]) && (position.Split("|")[1].Trim().Split(",")[6] == version1_records[6]) && (position.Split("|")[1].Trim().Split(",")[9] == version1_records[9]) && (position.Split("|")[1].Trim().Split(",")[10] == version1_records[10]) && (position.Split("|")[1].Trim().Split(",")[11] == version1_records[11]) && (position.Split("|")[1].Trim().Split(",")[12] == version1_records[12]) && (position.Split("|")[1].Trim().Split(",")[13] == version1_records[13]))
                                            {
                                                // Get EmployeePayAdjustID for these records
                                                string version1_id = version1_records[8];
                                                string version2_id = position.Split("|")[1].Trim().Split(",")[8];

                                                // Get PayAdjCodeId for these records
                                                string version1_pay = version1_records[7];
                                                string version2_pay = position.Split("|")[1].Trim().Split(",")[7];


                                                // Reaching here indicates, we found a similar record in Version 1, such that thier corresponding Version 2 records doesn't exist
                                                similar++;

                                                if ((version1_id == version2_id) && (version1_pay == version2_pay) && (version1_records[8] != ""))
                                                {
                                                    discrepancy = "Version 2 has no corresponding aligned record, but Version 2 contain exactly same record which has no corresponding Version 1 Record and EmployeePayAdjustId is not empty";
                                                    payadjust_id = version1_records[8] + " - " + version2_id;
                                                    duplicate_exact_exist = true;

                                                    // As, we found exactly same record, where EmployeePayAdjustID is not empty
                                                    break;
                                                }
                                                else if ((version1_id == version2_id) && (version1_pay == version2_pay) && (version1_records[8] == ""))
                                                {
                                                    discrepancy_temp = "Version 2 has no corresponding aligned record, but Version 2 contain exactly same record which has no corresponding Version 1 Record and EmployeePayAdjustId is empty";
                                                    payadjust_id_temp = version1_records[8] + " - " + version2_id;
                                                    continue;

                                                }
                                                //Extra Check - Not a Case
                                                else if ((version1_id == version2_id) && (version1_pay != version2_pay))
                                                {
                                                    discrepancy_temp = "Version 2 has no corresponding aligned record, but Version 2 contain a similar record with PayAdjCodeId as only difference which has no corresponding Version 1 Record";
                                                    payadjust_id_temp = version1_records[8] + " - " + version2_id;
                                                    continue;
                                                }

                                                // If none of the above case is True, Check for Duplicate Tardy, it may exist in this scenario as it matches precondition for Duplicate Tardy to exist

                                                // Create ControlDB instance for this Client 2 Versions
                                                ControlDB controlDBNamespace1 = new ControlDB(input.ControlDBServer_Server1);
                                                ControlDB controlDBNamespace2 = new ControlDB(input.ControlDBServer_Server2);

                                                // Try Finding Tardy Discrepacy

                                                try
                                                {
                                                    // Query - Database Version 1

                                                    // Get Anchor Time for Both Version EmployeePayAdjustID

                                                    // Get Anchor Time in Database Version 1, for EmployeePayAdjustId 1
                                                    string sql1_v1 = $@"SELECT anchortime FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {version1_id} AND IsDeleted = 0 ";

                                                    var sqlUtil1_v1 = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                                    DateTime anchortime1_v1 = sqlUtil1_v1.GetValue<DateTime>(sql1_v1);


                                                    // Get Anchor Time in Database Version 1, for EmployeePayAdjustId 2
                                                    string sql2_v1 = $@"SELECT anchortime FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {version2_id} AND IsDeleted = 0 ";

                                                    var sqlUtil2_v1 = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                                    DateTime anchortime2_v1 = sqlUtil2_v1.GetValue<DateTime>(sql2_v1);



                                                    // The anchortime of the two pay adjustments are the same and not empty or null and Is Deleted is false then duplicate tardy exists

                                                    if (DateTime.Compare(anchortime1_v1, null_date) != 0 && DateTime.Compare(anchortime2_v1, null_date) != 0 && anchortime1_v1 != null && anchortime1_v1 != null && DateTime.Compare(anchortime1_v1, anchortime2_v1) == 0)
                                                    {
                                                        //  No Need to Check for other Version - Exit this loop and Result -> Duplicate tardy

                                                        discrepancy = "Version 1 and 2 has no corresponding aligned record, but these records are similar and exist as Duplicate Tardy";
                                                        payadjust_id = version1_records[8] + " - " + version2_id;

                                                        duplicate_exact_exist = true;

                                                        // As we found Duplicate Tardy, exit
                                                        break;

                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine(e.Message);
                                                }


                                                try
                                                {
                                                    // Query - Database Version 2

                                                    // Check if Anchor Time exist for Both Version EmployeePayAdjustID


                                                    // Get Anchor Time in Database Version 2, for EmployeePayAdjustId 1
                                                    string sql1_v2 = $@"SELECT anchortime FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {version1_id} AND IsDeleted = 0 ";

                                                    var sqlUtil1_v2 = new SqlUtil(controlDBNamespace2.GetInstanceDBConnectionString(version2));
                                                    DateTime anchortime1_v2 = sqlUtil1_v2.GetValue<DateTime>(sql1_v2);

                                                    // Get Anchor Time in Database Version 2, for EmployeePayAdjustId 2
                                                    string sql2_v2 = $@"SELECT anchortime FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {version2_id} AND IsDeleted = 0 ";

                                                    var sqlUtil2_v2 = new SqlUtil(controlDBNamespace2.GetInstanceDBConnectionString(version2));
                                                    DateTime anchortime2_v2 = sqlUtil2_v2.GetValue<DateTime>(sql2_v2);

                                                    // The anchortime of the two pay adjustments are the same and not empty or null and Is Deleted is false then duplicate tardy exists

                                                    if (DateTime.Compare(anchortime1_v2, null_date) != 0 && DateTime.Compare(anchortime2_v2, null_date) != 0 && anchortime1_v2 != null && anchortime2_v2 != null && DateTime.Compare(anchortime1_v2, anchortime2_v2) == 0)
                                                    {
                                                        //  Exit this loop and Result -> Duplicate tardy

                                                        discrepancy = "Version 1 and 2 has no corresponding aligned record, but these records are similar and exist as Duplicate Tardy";
                                                        payadjust_id = version1_records[8] + " - " + version2_id;

                                                        duplicate_exact_exist = true;

                                                        // As we found Duplicate Tardy, exit
                                                        break;
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine(e.Message);
                                                }

                                                if ((version1_id != version2_id) && (version1_pay == version2_pay))
                                                {
                                                    discrepancy_temp = "Version 2 has no corresponding aligned record, but Version 2 contain a similar record with EmployeePayAdjustID as only difference which has no corresponding Version 1 Record";
                                                    payadjust_id_temp = version1_records[8] + " - " + version2_id;
                                                }
                                                else if ((version1_id != version2_id) && (version1_pay != version2_pay))
                                                {
                                                    discrepancy_temp = "Version 2 has no corresponding aligned record, but Version 2 contain a similar record with EmployeePayAdjustID and PayAdjCodeId as only difference which has no corresponding Version 1 Record";
                                                    payadjust_id_temp = version1_records[8] + " - " + version2_id;
                                                }
                                            }
                                        }

                                        // If, only one similar record - Discrepancy and EmployeePayAdjustID will be equal to its corresponding value and Duplicate Tardy doesn't exist
                                        if (!duplicate_exact_exist && similar == 1)
                                        {
                                            discrepancy = discrepancy_temp;
                                            payadjust_id = payadjust_id_temp;
                                        }
                                        // If, more than one such that record is found and Duplicate Tardy doesn't exist
                                        else if (!duplicate_exact_exist && similar > 1)
                                        {
                                            discrepancy = "Version 2 has no corresponding aligned record, but Version 2 contain " + similar.ToString() + " records with (EmployeePayAdjustID/PayAdjCodeId-Both-None) as only difference and has no corresponding Version 1 Record";
                                            payadjust_id = version1_records[8] + " - ( )";
                                        }

                                    }



                                    AnalyzePSR no_adjacent = new AnalyzePSR
                                    {
                                        Comparison_ID = id,
                                        Line = line_index,
                                        Client = input.Client,
                                        Employee_ID = employee_ID,
                                        Buisness_Date = buisness_date,
                                        Employee_Pay_AdjustID = payadjust_id,
                                        Discrepancy = discrepancy
                                    };

                                    // Add Difference to Analyze Table and API

                                    _context.AnalyzePSR.Add(no_adjacent);
                                    await _context.SaveChangesAsync();

                                    // Add in discrepacies list

                                    discrepacies.Add(no_adjacent);

                                    line_index++;
                                    continue;

                                }
                                else if (line.Split("|")[0].Trim() == "" && line.Split("|")[1].Trim() != "")
                                {
                                    string[] version2_records = line.Split("|")[1].Trim().Split(",");

                                    // Get Employee ID from any one of the versions

                                    string employee_ID = version2_records[0];

                                    // Get Buisness Date from any one of the versions

                                    string buisness_date = version2_records[2];

                                    // Make Null Date for Comparison

                                    DateTime null_date = DateTime.ParseExact("0001-01-01 00:00:00:000", "yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.InvariantCulture);

                                    string discrepancy = "Version 1 has no corresponding record - Mannual Investigation Needed";
                                    string payadjust_id = "- " + version2_records[8];

                                    // Check if Overlapping Tardy is causing this difference
                                    List<AnalyzePSR> overlapping_tardy = await _context.AnalyzePSR.Where(e => e.Employee_Pay_AdjustID.Contains(version2_records[8]) && e.Employee_ID == employee_ID && e.Comparison_ID == id && e.Buisness_Date == buisness_date && e.Discrepancy == "Overlapping Tardy").ToListAsync();

                                    if (overlapping_tardy.Count == 1)
                                    {
                                        discrepancy = "Version 1 has no corresponding record - Reason - Overlapping Tardy";
                                        payadjust_id = overlapping_tardy[0].Employee_Pay_AdjustID.Split(" - ")[0] + " - " + version2_records[8];
                                    }

                                    // Check if Pay Amount is broken down to more than one records is causing this difference
                                    List<AnalyzePSR> broken_pay = await _context.AnalyzePSR.Where(e => e.Employee_ID == employee_ID && e.Comparison_ID == id && e.Buisness_Date == buisness_date && e.Discrepancy == "Total Minutes and Pay Amount is different - Mannual Investigation Needed").ToListAsync();

                                    if (broken_pay.Count == 1)
                                    {
                                        string version1_id = broken_pay[0].Employee_Pay_AdjustID.Split(" - ")[0];
                                        string version2_id = broken_pay[0].Employee_Pay_AdjustID.Split(" - ")[1];

                                        if (version1_id != version2_id)
                                        {
                                            // Pay Amount is broken down to more than one records is causing this difference, since this record Employee Pay Adjust ID is in broken Pay Amount
                                            if (broken_pay[0].Employee_Pay_AdjustID.Contains(version2_records[8]))
                                            {
                                                discrepancy = "Version 1 has no corresponding record - Reason - Pay Amount is broken down to more than one records";
                                                payadjust_id = broken_pay[0].Employee_Pay_AdjustID.Split(" - ")[0] + " - " + version2_records[8];
                                            }
                                        }
                                        else
                                        {
                                            foreach (string broken_line in System.IO.File.ReadLines(analyze_file))
                                            {
                                                if (broken_line.Split("|")[0].Trim() == "Pay Summary Header")
                                                {
                                                    // This is the first line of Analyze File, skip this iteration and continue with next line which have records from both version.
                                                    continue;
                                                }
                                                else if (broken_line.Trim() == "|")
                                                {
                                                    // Reading all Records from Both versions is done, break this iteration
                                                    break;
                                                }

                                                else if (broken_line.Split("|")[0].Trim().Contains("," + version1_id + ","))
                                                {
                                                    // Get Total Minutes for these records, and if sum of Total Worked Minutes for this record(line) and version2 record(broken line) = version2 record(broken line)
                                                    // Then Pay Amount is broken down to more than one records is causing this difference

                                                    int version1_minutes = int.Parse(version2_records[11]);
                                                    int version1_brokenminutes = int.Parse(broken_line.Split("|")[0].Trim().Split(",")[11]);
                                                    int version2_brokenminutes = int.Parse(broken_line.Split("|")[1].Trim().Split(",")[11]);

                                                    int sum = version1_minutes + version1_brokenminutes;

                                                    if (sum == version2_brokenminutes)
                                                    {
                                                        discrepancy = "Version 1 has no corresponding record - Reason - Pay Amount is broken down to more than one records";
                                                        payadjust_id = broken_pay[0].Employee_Pay_AdjustID.Split(" - ")[0] + " - " + version2_records[8];

                                                        // Exit this search, since Pay Amount is broken down to more than one records, is detected
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // Check if Duplicate Tardy is caused by this difference, such that we have another similar record in other version which have no corresponding record and these two records are Duplicate Tardy
                                    List<AnalyzePSR> duplicate_tardy_align = await _context.AnalyzePSR.Where(e => e.Employee_Pay_AdjustID.Contains(version2_records[8]) && e.Employee_ID == employee_ID && e.Comparison_ID == id && e.Buisness_Date == buisness_date && e.Discrepancy == "Version 1 and 2 has no corresponding aligned record, but these records are similar and exist as Duplicate Tardy").ToListAsync();

                                    if (duplicate_tardy_align.Count == 1)
                                    {
                                        // Just Continue, since this issue is already detected
                                        line_index++;
                                        continue;
                                    }

                                    if (discrepancy != "Version 1 has no corresponding record - Reason - Pay Amount is broken down to more than one records" && discrepancy != "Version 1 has no corresponding record - Reason - Overlapping Tardy")
                                    {
                                        // Count Number of Similar Records in Version 1, such that thier corresponding Version 2 records doesn't exist
                                        int similar = 0;

                                        // Create a Temperory variables to store similar record type
                                        string discrepancy_temp = string.Empty;
                                        string payadjust_id_temp = string.Empty;

                                        // To check If Duplicate Tardy Exist
                                        bool duplicate_exact_exist = false;

                                        foreach (string position in System.IO.File.ReadLines(analyze_file))
                                        {

                                            if (position.Split("|")[0].Trim() == "Pay Summary Header")
                                            {
                                                // This is the first line of Analyze File, skip this iteration and continue with next line which have records from both version.
                                                continue;
                                            }
                                            else if (position.Trim() == "|")
                                            {
                                                // Reading all Records from Both versions is done, break this iteration
                                                break;
                                            }

                                            else if ((position.Split("|")[1].Trim() == "") && (position.Split("|")[0].Trim() != "") && (position.Split("|")[0].Trim().Split(",")[0] == version2_records[0]) && (position.Split("|")[0].Trim().Split(",")[1] == version2_records[1]) && (position.Split("|")[0].Trim().Split(",")[2] == version2_records[2]) && (position.Split("|")[0].Trim().Split(",")[3] == version2_records[3]) && (position.Split("|")[0].Trim().Split(",")[4] == version2_records[4]) && (position.Split("|")[0].Trim().Split(",")[5] == version2_records[5]) && (position.Split("|")[0].Trim().Split(",")[6] == version2_records[6]) && (position.Split("|")[0].Trim().Split(",")[9] == version2_records[9]) && (position.Split("|")[0].Trim().Split(",")[10] == version2_records[10]) && (position.Split("|")[0].Trim().Split(",")[11] == version2_records[11]) && (position.Split("|")[0].Trim().Split(",")[12] == version2_records[12]) && (position.Split("|")[0].Trim().Split(",")[13] == version2_records[13]))
                                            {
                                                // Get EmployeePayAdjustID for these records
                                                string version2_id = version2_records[8];
                                                string version1_id = position.Split("|")[0].Trim().Split(",")[8];

                                                // Get PayAdjCodeId for these records
                                                string version2_pay = version2_records[7];
                                                string version1_pay = position.Split("|")[0].Trim().Split(",")[7];

                                                // Reaching here indicates, we found a similar record in Version 1, such that thier corresponding Version 2 records doesn't exist
                                                similar++;

                                                if ((version1_id == version2_id) && (version1_pay == version2_pay) && (version2_records[8] != ""))
                                                {
                                                    discrepancy = "Version 1 has no corresponding aligned record, but Version 1 contain exactly same record which has no corresponding Version 2 Record and EmployeePayAdjustId is not empty";
                                                    payadjust_id = version2_id + " - " + version2_records[8];
                                                    duplicate_exact_exist = true;

                                                    // As, we found exactly same record, where EmployeePayAdjustID is not empty
                                                    break;
                                                }
                                                else if ((version1_id == version2_id) && (version1_pay == version2_pay) && (version2_records[8] == ""))
                                                {
                                                    discrepancy_temp = "Version 1 has no corresponding aligned record, but Version 1 contain exactly same record which has no corresponding Version 2 Record and EmployeePayAdjustId is empty";
                                                    payadjust_id = version2_id + " - " + version2_records[8];
                                                    continue;

                                                }
                                                //Extra Check - Not a Case
                                                else if ((version1_id == version2_id) && (version1_pay != version2_pay))
                                                {
                                                    discrepancy_temp = "Version 1 has no corresponding aligned record, but Version 1 contain a similar record with PayAdjCodeId as only difference which has no corresponding Version 2 Record";
                                                    payadjust_id = version2_id + " - " + version2_records[8];
                                                    continue;
                                                }


                                                // If none of the above case is True, Check for Duplicate Tardy, it may exist in this scenario as it matches precondition for Duplicate Tardy to exist

                                                // Create ControlDB instance for this Client 2 Versions
                                                ControlDB controlDBNamespace1 = new ControlDB(input.ControlDBServer_Server1);
                                                ControlDB controlDBNamespace2 = new ControlDB(input.ControlDBServer_Server2);

                                                // Try Finding Tardy Discrepacy

                                                try
                                                {
                                                    // Query - Database Version 1

                                                    // Get Anchor Time for Both Version EmployeePayAdjustID

                                                    // Get Anchor Time in Database Version 1, for EmployeePayAdjustId 1
                                                    string sql1_v1 = $@"SELECT anchortime FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {version1_id} AND IsDeleted = 0 ";

                                                    var sqlUtil1_v1 = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                                    DateTime anchortime1_v1 = sqlUtil1_v1.GetValue<DateTime>(sql1_v1);


                                                    // Get Anchor Time in Database Version 1, for EmployeePayAdjustId 2
                                                    string sql2_v1 = $@"SELECT anchortime FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {version2_id} AND IsDeleted = 0 ";

                                                    var sqlUtil2_v1 = new SqlUtil(controlDBNamespace1.GetInstanceDBConnectionString(version1));
                                                    DateTime anchortime2_v1 = sqlUtil2_v1.GetValue<DateTime>(sql2_v1);


                                                    // The anchortime of the two pay adjustments are the same and not empty or null and Is Deleted is false then duplicate tardy exists

                                                    if (DateTime.Compare(anchortime1_v1, null_date) != 0 && DateTime.Compare(anchortime2_v1, null_date) != 0 && anchortime1_v1 != null && anchortime2_v1 != null && DateTime.Compare(anchortime1_v1, anchortime2_v1) == 0)
                                                    {
                                                        //  No Need to Check for other Version - Exit this loop and Result -> Duplicate tardy

                                                        discrepancy = "Version 1 and 2 has no corresponding aligned record, but these records are similar and exist as Duplicate Tardy";
                                                        payadjust_id = version1_id + " - " + version2_records[8];

                                                        duplicate_exact_exist = true;

                                                        // As we found Duplicate Tardy, exit
                                                        break;
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine(e.Message);
                                                }


                                                try
                                                {
                                                    // Query - Database Version 2

                                                    // Check if Anchor Time exist for Both Version EmployeePayAdjustID

                                                    // Get Anchor Time in Database Version 2, for EmployeePayAdjustId 1
                                                    string sql1_v2 = $@"SELECT anchortime FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {version1_id} AND IsDeleted = 0 ";

                                                    var sqlUtil1_v2 = new SqlUtil(controlDBNamespace2.GetInstanceDBConnectionString(version2));
                                                    DateTime anchortime1_v2 = sqlUtil1_v2.GetValue<DateTime>(sql1_v2);

                                                    // Get Anchor Time in Database Version 2, for EmployeePayAdjustId 2
                                                    string sql2_v2 = $@"SELECT anchortime FROM EmployeePayAdjust WITH(NOLOCK) WHERE employeeid = {employee_ID} AND BusinessDate = '{buisness_date}' AND EmployeePayAdjustId = {version2_id} AND IsDeleted = 0 ";

                                                    var sqlUtil2_v2 = new SqlUtil(controlDBNamespace2.GetInstanceDBConnectionString(version2));
                                                    DateTime anchortime2_v2 = sqlUtil2_v2.GetValue<DateTime>(sql2_v2);

                                                    // The anchortime of the two pay adjustments are the same and not empty or null and Is Deleted is false then duplicate tardy exists

                                                    if (DateTime.Compare(anchortime1_v2, null_date) != 0 && DateTime.Compare(anchortime2_v2, null_date) != 0 && anchortime1_v2 != null && anchortime2_v2 != null && DateTime.Compare(anchortime1_v2, anchortime2_v2) == 0)
                                                    {
                                                        //  Exit this loop and Result -> Duplicate tardy

                                                        discrepancy = "Version 1 and 2 has no corresponding aligned record, but these records are similar and exist as Duplicate Tardy";
                                                        payadjust_id = version1_id + " - " + version2_records[8];

                                                        duplicate_exact_exist = true;

                                                        // As we found Duplicate Tardy, exit
                                                        break;
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine(e.Message);
                                                }


                                                if ((version1_id != version2_id) && (version1_pay == version2_pay))
                                                {
                                                    discrepancy_temp = "Version 1 has no corresponding aligned record, but Version 1 contain a similar record with EmployeePayAdjustID as only difference which has no corresponding Version 2 Record";
                                                    payadjust_id_temp = version1_id + " - " + version2_records[8];
                                                }
                                                else if ((version1_id != version2_id) && (version1_pay != version2_pay))
                                                {
                                                    discrepancy_temp = "Version 1 has no corresponding aligned record, but Version 1 contain a similar record with EmployeePayAdjustID and PayAdjCodeId as only difference which has no corresponding Version 2 Record";
                                                    payadjust_id_temp = version1_id + " - " + version2_records[8];
                                                }

                                            }
                                        }

                                        // If, only one similar record - Discrepancy and EmployeePayAdjustID will be equal to its corresponding value and Duplicate Tardy doesn't exist
                                        if (!duplicate_exact_exist && similar == 1)
                                        {
                                            discrepancy = discrepancy_temp;
                                            payadjust_id = payadjust_id_temp;
                                        }
                                        // If, more than one such that record is found and Duplicate Tardy doesn't exist
                                        else if (!duplicate_exact_exist && similar > 1)
                                        {
                                            discrepancy = "Version 1 has no corresponding aligned record, but Version 1 contain " + similar.ToString() + " records with (EmployeePayAdjustID/PayAdjCodeId-Both-None) as only difference and has no corresponding Version 2 Record";
                                            payadjust_id = "( ) - " + version2_records[8];
                                        }
                                    }

                                    AnalyzePSR no_adjacent = new AnalyzePSR
                                    {
                                        Comparison_ID = id,
                                        Line = line_index,
                                        Client = input.Client,
                                        Employee_ID = employee_ID,
                                        Buisness_Date = buisness_date,
                                        Employee_Pay_AdjustID = payadjust_id,
                                        Discrepancy = discrepancy
                                    };

                                    // Add Difference to Analyze Table and API

                                    _context.AnalyzePSR.Add(no_adjacent);
                                    await _context.SaveChangesAsync();

                                    // Add in discrepacies list

                                    discrepacies.Add(no_adjacent);

                                    line_index++;
                                    continue;
                                }
                            }

                        }

                        // Create new AnalyzePSR File with Indexing

                        // Path to Analyze View File

                        string analyze_view = current_path + "\\Output\\" + client + "\\" + id.ToString() + "\\Analyze.html";

                        using (StreamWriter writer = new StreamWriter(analyze_view, true))
                        {
                            // HTML Body Initial
                            writer.WriteLine("<html>");
                            writer.WriteLine("<head>");
                            writer.WriteLine("<title> Analyze Comparison ID - " + input.ID.ToString() + "</title>");
                            writer.WriteLine("</head>");
                            writer.WriteLine("<body>");

                            // To keep track of Record Index
                            int index = 0;

                            // To check if end of records is reached

                            bool reached = false;

                            foreach (string line in System.IO.File.ReadLines(analyze_file))
                            {
                                string space = string.Empty;

                                if (reached)
                                {
                                    // Since we have read all records, only add spacing without index

                                    // Give Total Space i.e. 25
                                    space = new string(' ', 25);

                                    // Empty Line
                                    if (line.Trim() == "|")
                                    {
                                        writer.WriteLine("<font color=\"black\"><pre>" + space + line + "</pre></font>");
                                    }
                                    // Records are matching
                                    else if (line.Split("|")[0].Trim() != "" && line.Split("|")[1].Trim() != "" && line.Split("|")[0].Trim() == line.Split("|")[1].Trim())
                                    {
                                        writer.WriteLine("<font color=\"black\"><pre>" + space + line + "</pre></font>");
                                    }
                                    // Both Records exist and are different
                                    else if (line.Split("|")[0].Trim() != "" && line.Split("|")[1].Trim() != "" && line.Split("|")[0].Trim() != line.Split("|")[1].Trim())
                                    {
                                        writer.WriteLine("<font color=\"red\"><pre>" + space + line + "</pre></font>");
                                    }

                                }
                                else if (line.Trim() == "|")
                                {
                                    // This is the first line after reading all records, only add spacing without index

                                    // Give Total Space i.e. 25
                                    space = new string(' ', 25);

                                    writer.WriteLine("<font color=\"black\"><pre>" + space + line + "</pre></font>");

                                    // Set reached to True
                                    reached = true;
                                }
                                else if (line.Split("|")[0].Trim() == "Pay Summary Header")
                                {
                                    // This is the first line of Analyze File, only add spacing without index

                                    // Give Total Space i.e. 25
                                    space = new string(' ', 25);

                                    writer.WriteLine("<font color=\"black\"><pre>" + space + line + "</pre></font>");

                                    // Increment Index of Record
                                    index++;
                                }
                                else
                                {
                                    string index_string = index.ToString();

                                    // Give relavent space with indexing such that Total Space is 40
                                    space = new string(' ', 25 - index_string.Length - 2);

                                    // Records are matching
                                    if (line.Split("|")[0].Trim() != "" && line.Split("|")[1].Trim() != "" && line.Split("|")[0].Trim() == line.Split("|")[1].Trim())
                                    {
                                        writer.WriteLine("<font color=\"black\"><pre>" + "[" + index_string + "]" + space + line + "</pre></font>");
                                    }
                                    // Both Records exist and are different
                                    else if (line.Split("|")[0].Trim() != "" && line.Split("|")[1].Trim() != "" && line.Split("|")[0].Trim() != line.Split("|")[1].Trim())
                                    {
                                        writer.WriteLine("<font color=\"red\"><pre>" + "[" + index_string + "]" + space + line + "</pre></font>");
                                    }
                                    // Version 2 Record Doesn't exist
                                    else if (line.Split("|")[0].Trim() != "" && line.Split("|")[1].Trim() == "")
                                    {
                                        writer.WriteLine("<font color=\"red\"><pre>" + "[" + index_string + "]" + space + line + "</pre></font>");
                                    }
                                    // Version 1 Record Doesn't exist
                                    else if (line.Split("|")[0].Trim() == "" && line.Split("|")[1].Trim() != "")
                                    {
                                        writer.WriteLine("<font color=\"red\"><pre>" + "[" + index_string + "]" + space + line + "</pre></font>");
                                    }

                                    // Increment Index of Record
                                    index++;
                                }
                            }

                            // HTML Body Final
                            writer.WriteLine("</body>");
                            writer.WriteLine("</html>");
                        }

                        // Link To Analyze View Result

                        input.Analyze_Path = new Uri(analyze_view).AbsoluteUri;

                    }
                    else if (version1_path == "" && version2_path != "" && results == "WARNING")
                    {
                        // Can't Analyze, because Version 1 has no Pay Summaries

                        // Create AnalyzePSR Message - Cant' Analyze

                        AnalyzePSR cant_analyze = new AnalyzePSR
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            Employee_ID = "Not Applicable",
                            Buisness_Date = "Not Applicable",
                            Employee_Pay_AdjustID = "Not Applicable",
                            Discrepancy = "Cant' Analyze - Difference Reason: No Record for Version 1"
                        };

                        // Add Message to Analyze Table and API

                        _context.AnalyzePSR.Add(cant_analyze);
                        await _context.SaveChangesAsync();

                        // Add in discrepacies list

                        discrepacies.Add(cant_analyze);


                    }
                    else if (version1_path != "" && version2_path == "" && results == "WARNING")
                    {
                        // Can't Analyze, because Version 2 has no Pay Summaries

                        // Create AnalyzePSR Message - Cant' Analyze

                        AnalyzePSR cant_analyze = new AnalyzePSR
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            Employee_ID = "Not Applicable",
                            Buisness_Date = "Not Applicable",
                            Employee_Pay_AdjustID = "Not Applicable",
                            Discrepancy = "Cant' Analyze - Difference Reason: No Record for Version 2"
                        };

                        // Add Message to Analyze Table and API

                        _context.AnalyzePSR.Add(cant_analyze);
                        await _context.SaveChangesAsync();

                        // Add in discrepacies list

                        discrepacies.Add(cant_analyze);


                    }

                    // Delete Analyze Result (Without Indexing)

                    try
                    {
                        System.IO.File.Delete(analyze_file);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

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

                // Set Analyzed to 2, since Analyze for this Comparison ID has Completed
                input.Analyzed = 2;

                // And update this input
                _context.Entry(input).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InputExists(input.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Send Email to User with Comparison Result and notify them -> Analyze is Completed

                Notification email = new Notification();

                email.SendEmail(input);

            }
            else
            {
                // Get already created discrepacies
                List<AnalyzePSR> already_discrepacies = await _context.AnalyzePSR.Where(e => e.Comparison_ID == id).ToListAsync();

                // Then return already created Discrepacies
                discrepacies = already_discrepacies;
            }

            return discrepacies;
        }

        // PUT: api/Analyze/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnalyze(int id, AnalyzePSR analyze)
        {
            if (id != analyze.ID)
            {
                return BadRequest();
            }

            _context.Entry(analyze).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnalyzeExists(id))
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

        // POST: api/Analyze
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<AnalyzePSR>> PostAnalyze(AnalyzePSR analyze)
        {
            _context.AnalyzePSR.Add(analyze);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAnalyze", new { id = analyze.ID }, analyze);
        }

        // DELETE: api/Analyze/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<AnalyzePSR>> DeleteAnalyze(int id)
        {
            var analyze = await _context.AnalyzePSR.FindAsync(id);
            if (analyze == null)
            {
                return NotFound();
            }

            _context.AnalyzePSR.Remove(analyze);
            await _context.SaveChangesAsync();

            return analyze;
        }

        private bool AnalyzeExists(int id)
        {
            return _context.AnalyzePSR.Any(e => e.ID == id);
        }
        private bool InputExists(int id)
        {
            return _context.Input.Any(e => e.ID == id);
        }
    }
}
