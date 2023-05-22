﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPToolServerSide.Models;

namespace CPToolServerSide.JobInput
{
    public class InputSCR
    {
        public Dictionary<string, string[]> ControlDBNamespace { get; set; }

        public List<Dictionary<string, object>> Tasks { get; set; }

        public InputSCR()
        {
            ControlDBNamespace = new Dictionary<string, string[]>();
            Tasks = new List<Dictionary<string, object>>();
        }
        public InputSCR(Input input)
        {
            ControlDBNamespace = new Dictionary<string, string[]>();
            Tasks = new List<Dictionary<string, object>>();

            // Fill in ControlDBNamespace details of this Input Object

            string[] controlDBnamespace1 = new string[] { input.DBName_1 };

            string[] controlDBnamespace2 = new string[] { input.DBName_2 };

            ControlDBNamespace.Add(input.ControlDBServer_Server1, controlDBnamespace1);

            ControlDBNamespace.Add(input.ControlDBServer_Server2, controlDBnamespace2);

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

            Tasks.Add(t1);
        }
    }
}
