using Shift = Scheduling_Library.Workweek.Shift;
using Employee = Scheduling_Library.Workweek.Employee;
using System.Collections.Generic;
namespace Scheduling_Library
{
    public abstract class SchedulerSolver
    {
        protected List<Shift> _shifts;
        protected List<Employee> _employees;
        protected Workweek _week;
        protected static int MINUTES_IN_A_DAY = 1440;

        public abstract void AssignShifts(string file);
        
    }
}