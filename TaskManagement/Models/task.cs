using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Models
{
    public class task
    {
        public int taskID { get; set; }
        public String taskDescription { get; set; }

        public String taskPriority { get; set; }
        public String taskStatus { get; set; }
        public String customerID { get; set; }

        public static implicit operator string(task v)
        {
            throw new NotImplementedException();
        }
    }
}
