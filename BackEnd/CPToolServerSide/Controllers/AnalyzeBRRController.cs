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

namespace CPToolServerSide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyzeBRRController : ControllerBase
    {
        private readonly CPTDBContext _context;

        public AnalyzeBRRController(CPTDBContext context)
        {
            _context = context;
        }

        // GET: api/AnalyzeBRR
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnalyzeBRR>>> GetAnalyzeBRR()
        {
            return await _context.AnalyzeBRR.ToListAsync();
        }

        // GET: api/AnalyzeBRR/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<AnalyzeBRR>>> GetAnalyzeBRR(int id)
        {
            // Get Input details using Comparison ID.
            var input = await _context.Input.FindAsync(id);

            if (input == null)
            {
                return NotFound();
            }

            // Get Analyzed Status for this Comparison ID
            int status = input.Analyzed;

            // Get Compared Status for this Comparison ID
            int compared = input.Compared;

            // Initialize Discrepacies List, which is to be returned after adding all discrepacies

            List<AnalyzeBRR> discrepacies = new List<AnalyzeBRR>();

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

                        // Create AnalyzeBRR Message - No Difference

                        AnalyzeBRR no_difference = new AnalyzeBRR
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            EmployeeId = "No Difference Causing Employee",
                            EmploymentStatusId = "No Difference Causing Employee Status ID",
                            EffectiveStart = "No Difference Causing Effective Start",
                            EffectiveEnd = "No Difference Causing Effective End",
                            Table = "No Difference Causing Table",
                            Discrepancy = "No Analyze Needed"
                        };

                        // Add No Analyze needed to AnalyzeBRR Table and API

                        _context.AnalyzeBRR.Add(no_difference);
                        await _context.SaveChangesAsync();

                        // Add in discrepacies list

                        discrepacies.Add(no_difference);


                    }
                    else if (version1_path == "" && version2_path == "" && results == "SUCCESS")
                    {

                        // No Difference - Exit with Result -> No Difference Reason

                        // Create AnalyzeBRR Message - No Difference Reason

                        AnalyzeBRR no_difference_reason = new AnalyzeBRR
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            EmployeeId = "No Employee in Both Versions",
                            EmploymentStatusId = "No Employee Status ID in Both Versions",
                            EffectiveStart = "No Effective Start in Both Version",
                            EffectiveEnd = "No Effective End in Both Version",
                            Table = "No Table in Both Version",
                            Discrepancy = "No Difference Reason: No Record exist for both Versions"
                        };

                        // Add No Difference Reason to AnalyzeBRR Table and API

                        _context.AnalyzeBRR.Add(no_difference_reason);
                        await _context.SaveChangesAsync();

                        // Add in discrepacies list

                        discrepacies.Add(no_difference_reason);

                    }

                    else if (version1_path == "" && version2_path == "" && results == "WARNING")
                    {

                        // No Difference - Exit with Result -> Difference Reason

                        // Create AnalyzeBRR Message - Difference Reason

                        AnalyzeBRR difference_reason = new AnalyzeBRR
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            EmployeeId = "Can't Find Employee",
                            EmploymentStatusId = "Can't Find Employee Status ID",
                            EffectiveStart = "Can't Find Effective Start",
                            EffectiveEnd = "Can't Find Effective End",
                            Table = "Can't Find Table",
                            Discrepancy = "Difference Reason: Can't Find Record for both Versions"
                        };

                        // Add Difference Reason to AnalyzeBRR Table and API

                        _context.AnalyzeBRR.Add(difference_reason);
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

                        // Create New Analyze File with Indexing

                        // Path to Analyze View File

                        string analyze_view = current_path + "\\Output\\" + client + "\\" + id.ToString() + "\\Analyze.html";

                        // Check Analyze file has all records aligned

                        FileInfo analyze_check = new FileInfo(analyze_file);

                        if (analyze_check.Length == 0)
                        {
                            // Can't Analyze - Exit with Result -> Large Record File

                            // Create AnalyzeBRR Message - Can't Analyze

                            AnalyzeBRR cant_analyze = new AnalyzeBRR
                            {
                                Comparison_ID = id,
                                Client = input.Client,
                                EmployeeId = "Can't Analyze",
                                EmploymentStatusId = "Can't Analyze",
                                EffectiveStart = "Can't Analyze",
                                EffectiveEnd = "Can't Analyze",
                                Table = "Can't Analyze",
                                Discrepancy = "Can't Analyze Reason: Large Record File"
                            };

                            // Add Difference Reason to Analyze Table and API

                            _context.AnalyzeBRR.Add(cant_analyze);
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

                        using (StreamWriter writer = new StreamWriter(analyze_view, true))
                        {
                            // HTML Body Initial
                            writer.WriteLine("<html>");
                            writer.WriteLine("<head>");
                            writer.WriteLine("<title> Analyze Comparison ID - " + input.ID.ToString() + "</title>");
                            writer.WriteLine("</head>");
                            writer.WriteLine("<body>");

                            // Index Tracking
                            int index = 1;

                            foreach (string line in System.IO.File.ReadLines(analyze_file))
                            {
                                string space = string.Empty;

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

                                    string[] version1_records = line.Split("|")[0].Trim().Split(",");
                                    string[] version2_records = line.Split("|")[1].Trim().Split(",");

                                    // If not same Table - Create Discrepancy with Message - Different Table

                                    if (version1_records.Length != version2_records.Length)
                                    {
                                        AnalyzeBRR diff_table = new AnalyzeBRR
                                        {
                                            Comparison_ID = id,
                                            Line = index,
                                            Client = input.Client,
                                            EmployeeId = "Different Table",
                                            EmploymentStatusId = "Different Table",
                                            EffectiveStart = "Different Table",
                                            EffectiveEnd = "Different Table",
                                            Table = "Different Table",
                                            Discrepancy = "Mannual Investigation - Different Table"
                                        };

                                        // Add Message to AnalyzeBRR Table and API

                                        _context.AnalyzeBRR.Add(diff_table);
                                        await _context.SaveChangesAsync();

                                        // Add in discrepacies list

                                        discrepacies.Add(diff_table);

                                        continue;
                                    }

                                    string table = string.Empty;

                                    // Check Table

                                    if (version1_records.Length == 9)
                                    {
                                        table = "Employee Applied Base Rates";
                                    }
                                    else if (version1_records.Length == 7)
                                    {
                                        table = "Employee Employment Status";
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    // Create Discrepancy Information

                                    string employee = version1_records[0];
                                    string employee_status = version1_records[1];
                                    string start = version1_records[2];
                                    string end = version1_records[3];
                                                                    
                                    // If Discrepancy Information is different for 2 Version, highlight difference field

                                    if(version1_records[0] != version2_records[0])
                                    {
                                        employee = version1_records[0] + " - " + version2_records[0];
                                    }
                                    if (version1_records[1] != version2_records[1])
                                    {
                                        employee_status = version1_records[1] + " - " + version2_records[1];
                                    }
                                    if (version1_records[2] != version2_records[2])
                                    {
                                        start = version1_records[2] + " - " + version2_records[2];
                                    }
                                    if (version1_records[3] != version2_records[3])
                                    {
                                        end = version1_records[3] + " - " + version2_records[3];
                                    }

                                    // Create AnalyzeBRR Message - Mannual Investigation

                                    AnalyzeBRR mannual = new AnalyzeBRR
                                    {
                                        Comparison_ID = id,
                                        Line = index,
                                        Client = input.Client,
                                        EmployeeId = employee,
                                        EmploymentStatusId = employee_status,
                                        EffectiveStart = start,
                                        EffectiveEnd = end,
                                        Table = table,
                                        Discrepancy = "Mannual Investigation"
                                    };

                                    // Add Message to AnalyzeBRR Table and API

                                    _context.AnalyzeBRR.Add(mannual);
                                    await _context.SaveChangesAsync();

                                    // Add in discrepacies list

                                    discrepacies.Add(mannual);
                                }
                                // Version 2 Record Doesn't exist
                                else if (line.Split("|")[0].Trim() != "" && line.Split("|")[1].Trim() == "")
                                {
                                    writer.WriteLine("<font color=\"red\"><pre>" + "[" + index_string + "]" + space + line + "</pre></font>");

                                    string[] version1_records = line.Split("|")[0].Trim().Split(",");

                                    string table = string.Empty;

                                    // Check Table

                                    if (version1_records.Length == 9)
                                    {
                                        table = "Employee Applied Base Rates";
                                    }
                                    else if (version1_records.Length == 7)
                                    {
                                        table = "Employee Employment Status";
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    // Create AnalyzeBRR Message - No Version 2 Record

                                    AnalyzeBRR no_version = new AnalyzeBRR
                                    {
                                        Comparison_ID = id,
                                        Line = index,
                                        Client = input.Client,
                                        EmployeeId =version1_records[0] + " - ",
                                        EmploymentStatusId = version1_records[1] + " - ",
                                        EffectiveStart = version1_records[2] + " - ",
                                        EffectiveEnd = version1_records[3] + " - ",
                                        Table = table,
                                        Discrepancy = "Mannual Investigation"
                                    };

                                    // Add Message to AnalyzeBRR Table and API

                                    _context.AnalyzeBRR.Add(no_version);
                                    await _context.SaveChangesAsync();

                                    // Add in discrepacies list

                                    discrepacies.Add(no_version);
                                }
                                // Version 1 Record Doesn't exist
                                else if (line.Split("|")[0].Trim() == "" && line.Split("|")[1].Trim() != "")
                                {
                                    writer.WriteLine("<font color=\"red\"><pre>" + "[" + index_string + "]" + space + line + "</pre></font>");

                                    string[] version2_records = line.Split("|")[1].Trim().Split(",");

                                    string table = string.Empty;

                                    // Check Table

                                    if (version2_records.Length == 9)
                                    {
                                        table = "Employee Applied Base Rates";
                                    }
                                    else if (version2_records.Length == 7)
                                    {
                                        table = "Employee Employment Status";
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    // Create AnalyzeBRR Message - No Version 1 Record

                                    AnalyzeBRR no_version = new AnalyzeBRR
                                    {
                                        Comparison_ID = id,
                                        Line = index,
                                        Client = input.Client,
                                        EmployeeId = " - " + version2_records[0],
                                        EmploymentStatusId = " - " + version2_records[1],
                                        EffectiveStart = " - " + version2_records[2],
                                        EffectiveEnd = " - " + version2_records[3],
                                        Table = table,
                                        Discrepancy = "Mannual Investigation"
                                    };

                                    // Add Message to AnalyzeBRR Table and API

                                    _context.AnalyzeBRR.Add(no_version);
                                    await _context.SaveChangesAsync();

                                    // Add in discrepacies list

                                    discrepacies.Add(no_version);
                                }

                                // Increment Index of Record
                                index++;

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

                        // Create AnalyzeBRR Message - Cant' Analyze

                        AnalyzeBRR cant_analyze = new AnalyzeBRR
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            EmployeeId = "Not Applicable",
                            EmploymentStatusId = "Not Applicable",
                            EffectiveStart = "Not Applicable",
                            EffectiveEnd = "Not Applicable",
                            Table = "Not Applicable",
                            Discrepancy = "Cant' Analyze - Difference Reason: No Record for Version 1"
                        };

                        // Add Message to AnalyzeBRR Table and API

                        _context.AnalyzeBRR.Add(cant_analyze);
                        await _context.SaveChangesAsync();

                        // Add in discrepacies list

                        discrepacies.Add(cant_analyze);


                    }
                    else if (version1_path != "" && version2_path == "" && results == "WARNING")
                    {
                        // Can't Analyze, because Version 2 has no Pay Summaries

                        // Create AnalyzeBRR Message - Cant' Analyze

                        AnalyzeBRR cant_analyze = new AnalyzeBRR
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            EmployeeId = "Not Applicable",
                            EmploymentStatusId = "Not Applicable",
                            EffectiveStart = "Not Applicable",
                            EffectiveEnd = "Not Applicable",
                            Table = "Not Applicable",
                            Discrepancy = "Cant' Analyze - Difference Reason: No Record for Version 2"
                        };

                        // Add Message to AnalyzeBRR Table and API

                        _context.AnalyzeBRR.Add(cant_analyze);
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
                List<AnalyzeBRR> already_discrepacies = await _context.AnalyzeBRR.Where(e => e.Comparison_ID == id).ToListAsync();

                // Then return already created Discrepacies
                discrepacies = already_discrepacies;
            }

            return discrepacies;

        }

        // PUT: api/AnalyzeBRR/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnalyzeBRR(int id, AnalyzeBRR analyzeBRR)
        {
            if (id != analyzeBRR.ID)
            {
                return BadRequest();
            }

            _context.Entry(analyzeBRR).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnalyzeBRRExists(id))
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

        // POST: api/AnalyzeBRR
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<AnalyzeBRR>> PostAnalyzeBRR(AnalyzeBRR analyzeBRR)
        {
            _context.AnalyzeBRR.Add(analyzeBRR);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAnalyzeBRR", new { id = analyzeBRR.ID }, analyzeBRR);
        }

        // DELETE: api/AnalyzeBRR/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<AnalyzeBRR>> DeleteAnalyzeBRR(int id)
        {
            var analyzeBRR = await _context.AnalyzeBRR.FindAsync(id);
            if (analyzeBRR == null)
            {
                return NotFound();
            }

            _context.AnalyzeBRR.Remove(analyzeBRR);
            await _context.SaveChangesAsync();

            return analyzeBRR;
        }

        private bool AnalyzeBRRExists(int id)
        {
            return _context.AnalyzeBRR.Any(e => e.ID == id);
        }
        private bool InputExists(int id)
        {
            return _context.Input.Any(e => e.ID == id);
        }
    }
}
