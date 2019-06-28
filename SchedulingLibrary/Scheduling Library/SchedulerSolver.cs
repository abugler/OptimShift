using System;
using System.Collections.Generic;
using Google.OrTools.ConstraintSolver;
using Google.OrTools.Sat;
using Constraint = Google.OrTools.ConstraintSolver.Constraint;


namespace Scheduling_Library
{
    /**
     * This is the meat of the algorithm.
     * We will use Google's OR tools to resolve this problem.
     *
     * The following are the constraints of which these problems are to be solved:
     * 
     * No Shifts under 60 minutes, nor over 180 minutes
     * A person should not be scheduled when he is not available 
     */
    public class SchedulerSolver
    {
        
        public Workweek AssignShifts(Workweek workweek)
        {
            Workweek week = new Workweek();
            week.GenerateShifts();
            List<Workweek.Employee> employees =  week.PopulateEmployees("TestEmployees.txt");
            
            CpModel model = new CpModel();
            
            
            Solver solver = new Solver("Scheduler");
            
            Constraint isAvailable = new Constraint(solver);
            Constraint appropriateLength = new Constraint(solver);
                
            
            throw new NotImplementedException();
        }
    }
}