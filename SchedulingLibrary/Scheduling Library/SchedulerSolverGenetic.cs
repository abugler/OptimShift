using Shift = Scheduling_Library.Workweek.Shift;
using Employee = Scheduling_Library.Workweek.Employee;
using System.Collections.Generic;

namespace Scheduling_Library
{
    /**
     * A solver for the scheduler using a genetic algorithm
     *
     * Requirements:
     * 
     * A data structure for a single schedule which has the values:
     *     Which Employees will work which shifts represented by a 2D bool array
     *     Heuristic
     *
     * A function which sorts the Employees by Heuristic
     *
     * A function that evaluates the heuristic for a 
     *
     */
    public class SchedulerSolverGenetic : SchedulerSolver
    {
        private const int NumberOfGenerations = 100;
        private const int SchedulesPerGeneration = 1000;
        public override void AssignShifts(string employeesAvailabilityFile)
        {
            
        }
    }
}