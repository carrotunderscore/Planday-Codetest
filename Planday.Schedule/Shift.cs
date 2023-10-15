using System;

namespace Planday.Schedule
{
    public class Shift
    {
        public Shift(long id, DateTime start, DateTime end, Employee? employee)
        {
            Id = id;
            Start = start;
            End = end;
            Employee = employee;
        }

		public long Id { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public Employee? Employee { get; set; }
	}    
}

