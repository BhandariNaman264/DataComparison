using System;
using System.IO;
using System.Net.Mail;
using CPToolServerSide.Models;

namespace CPToolServerSide.Email
{
    public class Notification
    {
        public string CreateEmailBody(string userName, int result, int comparison_id, string task, string client, string version1, string version2, string job, string org, string start, string end, string initiated, string relativedate, string policy, string exportmode, string exportfile, int pgcalendarid, bool mocktransmit)
        {

            string body = string.Empty;

            // Path Confriguation for Compare Pay Tool

            // If Running on Local

            // string current_path = Directory.GetCurrentDirectory();

            // If Running on Server

            string current_path = "E:\\ServerSide";

            string html_path;
            if (result == 1)
            {
                html_path = current_path + "\\Email\\HtmlTemplateNoDifference.html";
            }
            else if (result == 2)
            {
                html_path = current_path + "\\Email\\HtmlTemplateDifference.html";
            }
            else if (result == 3)
            {
                html_path = current_path + "\\Email\\HtmlTemplateExport.html";
            }
            else
            {
                html_path = current_path + "\\Email\\HtmlTemplateFailed.html";
            }

            //using streamreader for reading my HtmlTemplate   
            using (StreamReader reader = new StreamReader(html_path))

            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{UserName}", userName);

            // URL Confriguation for Compare Pay Tool

            // If Running on Local

            // string api_url = "http://localhost:3000";

            // If Running on Server

            string api_url = "http://nan5dfc1web01.corpadds.com:8084";

            if (result == 1 || result == 2)
            {
                body = body.Replace("{Url}", api_url + "/comparison#/analyze/" + comparison_id.ToString());
            }
            else
            {
                body = body.Replace("{Url}", api_url + "/comparison");
            }

            body = body.Replace("{id}", comparison_id.ToString());
            body = body.Replace("{task}", task);
            body = body.Replace("{client}", client);
            body = body.Replace("{version1}", version1);
            body = body.Replace("{version2}", version2);
            body = body.Replace("{job}", job);
            if(org == "")
            {
                body = body.Replace("{org}", "Whole Organization");
            }
            else
            {
                body = body.Replace("{org}", org);
            }
            if(start == "")
            {
                body = body.Replace("{start}", " N.A. ");
            }
            else
            {
                body = body.Replace("{start}", start);
            }
            if(end == "")
            {
                body = body.Replace("{end}", " N.A. ");
            }
            else
            {
                body = body.Replace("{end}", end);
            }
            if(relativedate == "")
            {
                body = body.Replace("{relativedate}", " N.A. ");
            }
            else
            {
                body = body.Replace("{relativedate}", relativedate);
            }
            if (policy == "")
            {
                body = body.Replace("{policy}", " N.A. ");
            }
            else
            {
                body = body.Replace("{policy}", policy);
            }
            if (exportfile == "")
            {
                body = body.Replace("{exportfile}", " N.A. ");
            }
            else
            {
                body = body.Replace("{exportfile}", exportfile);
            }
            if (exportmode == "")
            {
                body = body.Replace("{exportmode}", " N.A. ");
                body = body.Replace("{mocktransmit}", " N.A. ");
            }
            else
            {
                body = body.Replace("{exportmode}", exportmode);
                body = body.Replace("{mocktransmit}", mocktransmit.ToString());
            }
            if (pgcalendarid == 0)
            {
                body = body.Replace("{pgcalendarid}", " N.A. ");
            }
            else
            {
                body = body.Replace("{pgcalendarid}", pgcalendarid.ToString());
            }

            body = body.Replace("{initiated}", initiated);

            return body;
        }

        public void SendEmail(Input input)
        {
            string EmailFrom = "comparepaytool@ceridian.com";
            string emailserver = "relay.ceridian.com";
            string EmailTo = input.User_Email;
            int result;


            string subject;
            if (input.Results == "SUCCESS")
            {
                result = 1;
                subject = "Results for Comparison ID: " + input.ID + " **SUCCESS**";
            }
            else if (input.Results == "WARNING")
            {
                result = 2;
                subject = "Results for Comparison ID: " + input.ID + " **WARNING**";
            }
            else if (input.Results == "MANUAL COMPARISON")
            {
                result = 3;
                subject = "Results for Comparison ID: " + input.ID + " **MANUAL COMPARISON**";
            }
            else
            {
                // When Job Queue Failed or Job Failed
                result = 0;
                subject = "Results for Comparison ID: " + input.ID + " **FAILED**";
            }

            string body = CreateEmailBody(input.User_Name.Split(",")[1].Trim() , result, input.ID, input.Task_Name, input.Client, input.DBName_1, input.DBName_2, input.Job, input.Org, input.Start_Time.Split("T")[0], input.End_Time.Split("T")[0], input.Date, input.Date_Relative_To_Today.Split("T")[0], input.Policy, input.Export_Mode, input.Export_File_Name, input.Pay_Group_Calendar_Id, input.Mock_Transmit);

            MailMessage message = new MailMessage(EmailFrom, EmailTo)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            // Send a BCC to Compare Pay Tool Mailbox

            MailAddress addressBCC = new MailAddress("comparepaytool@ceridian.com");
            message.Bcc.Add(addressBCC);

            SmtpClient email = new SmtpClient(emailserver)
            {
                // Credentials are necessary if the server requires the client  
                // to authenticate before it will send e-mail on the client's behalf.
                UseDefaultCredentials = true
            };

            try
            {
                email.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
