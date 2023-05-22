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
    public class AnalyzeEController : ControllerBase
    {
        private readonly CPTDBContext _context;

        public AnalyzeEController(CPTDBContext context)
        {
            _context = context;
        }

        // GET: api/AnalyzeE
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnalyzeE>>> GetAnalyzeE()
        {
            return await _context.AnalyzeE.ToListAsync();
        }

        // GET: api/AnalyzeE/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<AnalyzeE>>> GetAnalyzeE(int id)
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

            List<AnalyzeE> discrepacies = new List<AnalyzeE>();

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

                string comparison_file = current_path + "\\Output\\" + client + "\\" + id.ToString() + "\\CompareExportResult.txt";

                string comparison_result_file = current_path + "\\Output\\" + client + "\\" + id.ToString() + "\\ComparisonResult.txt";

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

                        // Create AnalyzeE Message - No Difference

                        AnalyzeE no_difference = new AnalyzeE
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            Discrepancy = "No Analyze Needed"
                        };

                        // Add No Analyze needed to AnalyzeJSR Table and API

                        _context.AnalyzeE.Add(no_difference);
                        await _context.SaveChangesAsync();

                        // Add in discrepacies list

                        discrepacies.Add(no_difference);


                    }
                    else if (version1_path == "" && version2_path == "" && results == "SUCCESS")
                    {

                        // No Difference - Exit with Result -> No Difference Reason

                        // Create AnalyzeE Message - No Difference Reason

                        AnalyzeE no_difference_reason = new AnalyzeE
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            Discrepancy = "No Difference Reason: No Record exist for both Versions"
                        };

                        // Add No Difference Reason to AnalyzeJSR Table and API

                        _context.AnalyzeE.Add(no_difference_reason);
                        await _context.SaveChangesAsync();

                        // Add in discrepacies list

                        discrepacies.Add(no_difference_reason);

                    }

                    else if (version1_path == "" && version2_path == "" && results == "WARNING")
                    {

                        // No Difference - Exit with Result -> Difference Reason

                        // Create AnalyzeE Message - Difference Reason

                        AnalyzeE difference_reason = new AnalyzeE
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            Discrepancy = "Difference Reason: Can't Find Record for both Versions"
                        };

                        // Add Difference Reason to AnalyzeJSR Table and API

                        _context.AnalyzeE.Add(difference_reason);
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

                            // Create AnalyzeE Message - Can't Analyze

                            AnalyzeE cant_analyze = new AnalyzeE
                            {
                                Comparison_ID = id,
                                Client = input.Client,
                                Discrepancy = "Can't Analyze Reason: Large Record File"
                            };

                            // Add Difference Reason to Analyze Table and API

                            _context.AnalyzeE.Add(cant_analyze);
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

                                    // Create AnalyzeE Message - Mannual Investigation

                                    AnalyzeE mannual = new AnalyzeE
                                    {
                                        Comparison_ID = id,
                                        Line = index,
                                        Client = input.Client,
                                        Discrepancy = "Mannual Investigation"
                                    };

                                    // Add Message to AnalyzeJSR Table and API

                                    _context.AnalyzeE.Add(mannual);
                                    await _context.SaveChangesAsync();

                                    // Add in discrepacies list

                                    discrepacies.Add(mannual);
                                }
                                // Version 2 Record Doesn't exist
                                else if (line.Split("|")[0].Trim() != "" && line.Split("|")[1].Trim() == "")
                                {
                                    writer.WriteLine("<font color=\"red\"><pre>" + "[" + index_string + "]" + space + line + "</pre></font>");

                                    // Create AnalyzeE Message - No Version 2 Record

                                    AnalyzeE no_version = new AnalyzeE
                                    {
                                        Comparison_ID = id,
                                        Line = index,
                                        Client = input.Client,
                                        Discrepancy = "Mannual Investigation"
                                    };

                                    // Add Message to AnalyzeJSR Table and API

                                    _context.AnalyzeE.Add(no_version);
                                    await _context.SaveChangesAsync();

                                    // Add in discrepacies list

                                    discrepacies.Add(no_version);
                                }
                                // Version 1 Record Doesn't exist
                                else if (line.Split("|")[0].Trim() == "" && line.Split("|")[1].Trim() != "")
                                {
                                    writer.WriteLine("<font color=\"red\"><pre>" + "[" + index_string + "]" + space + line + "</pre></font>");

                                    // Create AnalyzeE Message - No Version 1 Record

                                    AnalyzeE no_version = new AnalyzeE
                                    {
                                        Comparison_ID = id,
                                        Line = index,
                                        Client = input.Client,
                                        Discrepancy = "Mannual Investigation"
                                    };

                                    // Add Message to AnalyzeJSR Table and API

                                    _context.AnalyzeE.Add(no_version);
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

                        // Create AnalyzeE Message - Cant' Analyze

                        AnalyzeE cant_analyze = new AnalyzeE
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            Discrepancy = "Cant' Analyze - Difference Reason: No Record for Version 1"
                        };

                        // Add Message to AnalyzeJSR Table and API

                        _context.AnalyzeE.Add(cant_analyze);
                        await _context.SaveChangesAsync();

                        // Add in discrepacies list

                        discrepacies.Add(cant_analyze);


                    }
                    else if (version1_path != "" && version2_path == "" && results == "WARNING")
                    {
                        // Can't Analyze, because Version 2 has no Pay Summaries

                        // Create AnalyzeE Message - Cant' Analyze

                        AnalyzeE cant_analyze = new AnalyzeE
                        {
                            Comparison_ID = id,
                            Client = input.Client,
                            Discrepancy = "Cant' Analyze - Difference Reason: No Record for Version 2"
                        };

                        // Add Message to AnalyzeJSR Table and API

                        _context.AnalyzeE.Add(cant_analyze);
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
                        System.IO.File.Delete(comparison_result_file);
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
                List<AnalyzeE> already_discrepacies = await _context.AnalyzeE.Where(e => e.Comparison_ID == id).ToListAsync();

                // Then return already created Discrepacies
                discrepacies = already_discrepacies;
            }

            return discrepacies;
        }

        // PUT: api/AnalyzeE/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnalyzeE(int id, AnalyzeE analyzeE)
        {
            if (id != analyzeE.ID)
            {
                return BadRequest();
            }

            _context.Entry(analyzeE).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnalyzeEExists(id))
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

        // POST: api/AnalyzeE
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<AnalyzeE>> PostAnalyzeE(AnalyzeE analyzeE)
        {
            _context.AnalyzeE.Add(analyzeE);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAnalyzeE", new { id = analyzeE.ID }, analyzeE);
        }

        // DELETE: api/AnalyzeE/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<AnalyzeE>> DeleteAnalyzeE(int id)
        {
            var analyzeE = await _context.AnalyzeE.FindAsync(id);
            if (analyzeE == null)
            {
                return NotFound();
            }

            _context.AnalyzeE.Remove(analyzeE);
            await _context.SaveChangesAsync();

            return analyzeE;
        }

        private bool AnalyzeEExists(int id)
        {
            return _context.AnalyzeE.Any(e => e.ID == id);
        }
        private bool InputExists(int id)
        {
            return _context.Input.Any(e => e.ID == id);
        }
    }
}
