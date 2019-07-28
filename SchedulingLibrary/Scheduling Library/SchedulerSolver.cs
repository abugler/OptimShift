using System;
using System.Collections.Generic;
using Employee = Scheduling_Library.Workweek.Employee;
using Shift = Scheduling_Library.Workweek.Shift;


namespace Scheduling_Library
{
    public class SchedulerSolver
    {
        private List<Shift> _shifts;
        private List<Employee> _employees;
        private Workweek _week;
        private SchedulingMatrixSolver _matrix;

        private string[,] _shiftIdentifiers;
        /**
         * Main function, assigns shifts
         */
        public Workweek AssignShifts(Workweek workweek)
        {
            //Generate shift, by parsing a .txt file. 
            _week = new Workweek();
            _shifts = _week.GenerateShifts();
            _employees = _week.PopulateEmployees("TestEmployees.txt");

            //Create a data structure to contain all combinations of shifts, employees
            //First param is the shift, second param is the employee, and the linked list is the slots for each shift and person.
            //
            _matrix = new SchedulingMatrixSolver(_shifts, _employees);
            for (int shiftIndex = 0; shiftIndex < _shifts.Count; shiftIndex++)
            {
                for (int employeeIndex = 0; employeeIndex < _employees.Count; employeeIndex++)
                {
                    _shiftIdentifiers[shiftIndex, employeeIndex] =
                        _shifts[shiftIndex].ToString() + "\n" + _employees[employeeIndex].GetName();
                }
            }

            throw new NotImplementedException();
        }
        
        class SchedulingMatrixSolver
        {
            private SchedulingMatrixNode _rootTrue;
            private SchedulingMatrixNode _rootFalse;
            private List<Shift> _shifts;
            private List<Employee> _employees;
            private int _heuristicIncrement = 1;
            
            /**
             * Constructor for the Solver
             * Take a shift and employee list, and creates a matrix for them
             * Also creates the first two nodes in the binary search tree
             */
            public SchedulingMatrixSolver(List<Shift> shifts, List<Employee> employees)
            {
                //Saves the shift and employee lists
                _shifts  = shifts;
                _employees = employees;
                
                //Initializes the first two nodes of the search tree
                _rootTrue = new SchedulingMatrixNode(0, 0);
                _rootTrue.working = true;
                _rootTrue.heuristic = 0;
                _rootFalse = new SchedulingMatrixNode(0, 0);
                _rootFalse.working = false;
                _rootFalse.heuristic = 0;
            }

            /**
             * Calculates the change in heuristic, changes heuristic, and outputs the change.
             *
             * If change results in heuristic = Int32.Min, it will output Int32.Min regardless of what the actual change it
             * If not, it outputs the change. 
             */
            private int VerifyNode(SchedulingMatrixNode node)
            {
                Employee currentEmployee = _employees[node.employeeChange];
                Shift currentShift = _shifts[node.shiftChange];
                int changeInHeuristic = 0;
                
                //Change heuristic from availability of employee
                switch (currentEmployee.LowestAvailabilityForShift(currentShift.StartTime, currentShift.EndTime)) {
                    case 0:
                        node.heuristic = Int32.MinValue;
                        return Int32.MinValue;
                    case 1:
                        node.heuristic -= 10 * _heuristicIncrement;
                        break;
                    case 2:
                        node.heuristic += _heuristicIncrement;
                        break;
                    case 3:
                        node.heuristic += 2 * _heuristicIncrement;
                        break;
                }

                
                if (!currentEmployee.IsAcceptableShiftLength(currentShift))
                {
                    node.heuristic = Int32.MinValue;
                    return Int32.MinValue;
                }
                
                //Change heuristic from Shifts per day
                
                
            }

            class SchedulingMatrixNode
            {
                public int employeeChange;
                public int shiftChange;
                public bool working;
                public int heuristic;
                private SchedulingMatrixNode parent;
                public SchedulingMatrixNode leftChild;
                public SchedulingMatrixNode rightChild;

                public SchedulingMatrixNode(int employeeChange, int shiftChange)
                {
                    this.employeeChange = employeeChange;
                    this.shiftChange = shiftChange;
                }
            }
        }
        
        
    }
}