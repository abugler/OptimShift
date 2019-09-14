using System;
using System.Collections;
using System.Collections.Generic;
using Google.OrTools.ConstraintSolver;
using Employee = Scheduling_Library.Workweek.Employee;
using Shift = Scheduling_Library.Workweek.Shift;


namespace Scheduling_Library
{
    public class SchedulerSolverGreedy : SchedulerSolver
    {
        private SchedulingMatrixSolver _matrix;
        private bool[,] _solution;

        /**
         * Main function, assigns shifts
         */
        public override void AssignShifts(string employeesAvailabilityFile)
        {
            //Generate shift, by parsing a .txt file. 
            _week = new Workweek();
            _shifts = _week.GenerateShifts();
            _employees = _week.PopulateEmployees(employeesAvailabilityFile); //"TestEmployees.txt"

            //Create a data structure to contain all combinations of shifts, employees
            //First param is the shift, second param is the employee
            _matrix = new SchedulingMatrixSolver(_shifts, _employees);

            _solution = _matrix.SolveGreedy();
        }

        
        public string toString()
        {
            string result = "";
            for (int i = 0; i < _solution.GetLength(0); i++)
            {
                result += _shifts[i] + "\n";
                for (int j = 0; j < _solution.GetLength(1); j++)
                {
                    if (_solution[i, j])
                        result += _employees[j].GetName() + "\n";
                }
                result += "\n";
            }
            return result;
        }
        
        class SchedulingMatrixSolver
        {
            private SchedulingMatrixNode _rootTrue;
            private SchedulingMatrixNode _rootFalse;
            private List<Shift> _shifts;
            private List<Employee> _employees;
            private int _heuristicIncrement = 1;
            private bool[,] _workingMatrix;
            
            private int MAX_WORK_TIME = 180;
            private int MIN_WORK_TIME = 60;
            
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
                _rootTrue = new SchedulingMatrixNode(0, 0, true, 0);
                _rootFalse = new SchedulingMatrixNode(0, 0, false, 0);
                _rootTrue.CurrentMatrix = new bool[shifts.Count, employees.Count];
                _rootFalse.CurrentMatrix = new bool[shifts.Count, employees.Count];
                _rootTrue.CurrentMatrix[0, 0] = true;
                _rootFalse.CurrentMatrix[0, 0] = false;
            }


            public bool[,] SolveGreedy()
            {
                NodePriorityQueue queue = new NodePriorityQueue();
                if (VerifyNode(_rootTrue) > Int32.MinValue)
                    queue.Insert(_rootTrue);
                queue.Insert(_rootFalse);
                SchedulingMatrixNode node;
                while (true)
                {
                    node = queue.RemoveHighest();
                    int nextShift;
                    int nextEmployee;
                    //If all employees have been looked at for this shift
                    if (node.employeeChange == _employees.Count - 1)
                    {
                        //Finds the next shift to work on
                        nextShift = node.shiftChange + 1;

                        //If no more shifts remain, break the loop, 
                        if (nextShift == _shifts.Count)
                            break;
                        nextEmployee = 0;
                    }
                    else
                    {
                        nextShift = node.shiftChange;
                        nextEmployee = node.employeeChange + 1;
                    }

                    CreateNodeChildren(node, nextShift, nextEmployee);
                    if (VerifyNode(node.rightChild) != Int32.MinValue)
                        queue.Insert(node.rightChild);
                                                                                                                                                               if (VerifyNode(node.leftChild) != Int32.MinValue)
                        queue.Insert(node.leftChild);
                }

                _workingMatrix = node.CurrentMatrix;
                return node.CurrentMatrix;
            }

            /**
             * Helper function for greedy search
             */
            private void CreateNodeChildren(SchedulingMatrixNode node, int nextShift, int nextEmployee)
            {
                node.leftChild = new SchedulingMatrixNode(nextShift, nextEmployee, false, node.heuristic);
                node.rightChild= new SchedulingMatrixNode(nextShift, nextEmployee, true, node.heuristic);
                node.leftChild.parent = node;
                node.rightChild.parent = node;
                node.leftChild.GenerateMatrix();
                node.rightChild.GenerateMatrix();
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

                
                //Change heuristic from number of cons working
                if (node.ConsWorkingThisShift(node.shiftChange) > currentShift.MaxEmployees())
                {
                    node.heuristic = Int32.MinValue;
                    return Int32.MinValue;
                }
                
                //Change heuristic from shift length 
                if (!AcceptableShiftLength(node))
                {
                    node.heuristic = Int32.MinValue;
                    return Int32.MinValue;
                }
                
                //If they aren't working, the rest of the heuristic function doesn't apply
                if (!node.working)
                    return 0;
                
                //Change heuristic from availability of employee
                switch (currentEmployee.LowestAvailabilityForShift(currentShift.StartTime, currentShift.EndTime)) {
                    case 0:
                        node.heuristic = Int32.MinValue;
                        return Int32.MinValue;
                    case 1:
                        changeInHeuristic -= 10 * _heuristicIncrement;
                        break;
                    case 2:
                        changeInHeuristic += _heuristicIncrement;
                        break;
                    case 3:
                        changeInHeuristic += 2 * _heuristicIncrement;
                        break;
                }

                //Change heuristic from number of shifts a day
                int numberOfShifts = NumberOfShiftsADay(currentShift.StartTime, node);
                if (numberOfShifts == 2)
                {
                    changeInHeuristic -= 5 * _heuristicIncrement;
                }
                else if (numberOfShifts > 2)
                {
                    node.heuristic = Int32.MinValue;
                    return Int32.MinValue;
                }

                node.heuristic += changeInHeuristic;
                return changeInHeuristic;
            }

            /**
             * Finds how many minutes the con is working given a location on the working matrix
             */
            private bool AcceptableShiftLength(SchedulingMatrixNode node)
            {
                // First, check for a possible short shift beforehand
                if (
                    // Check if it's not the first of its type
                    node.shiftChange != 0 
                    && _shifts[node.shiftChange - 1].EndTime == _shifts[node.shiftChange].StartTime
                    // Check that this node isn't set to work, and the employee works in this last shift
                    && !node.working
                    && node.CurrentMatrix[node.shiftChange - 1, node.employeeChange]
                    // Check that the previous node is not long enough
                    && _shifts[node.shiftChange - 1].WorkTime() < MIN_WORK_TIME
                    ){
                        int current = node.shiftChange - 1;
                        int time = _shifts[current].WorkTime();
                        // Now we have to backtrack through the matrix to see find other parts of the shift
                        while (current - 1 >= 0
                               && node.CurrentMatrix[current - 1, node.employeeChange]
                            && _shifts[current].StartTime == _shifts[current - 1].EndTime)
                            time += _shifts[--current].WorkTime();
                        // If the accumulated time is less than Min work time, return false
                        if (time < MIN_WORK_TIME)
                            return false;
                }
                
                // Long shifts are only checked if node.working is true.
                // There is no reason to check while false, unlike short shifts
                if (!node.working)
                    return true;
                
                // Second, check for a long shift
                // Backtrack through the matrix until false
                int before = node.shiftChange - 1;
                int timeWorked = _shifts[node.shiftChange].WorkTime();
                while (before >= 0
                       && node.CurrentMatrix[before, node.employeeChange]
                       && (_shifts[before].EndTime == _shifts[before + 1].StartTime))
                {
                    timeWorked += _shifts[before--].WorkTime();
                }

                return timeWorked < MAX_WORK_TIME;
            }

            /**
             * Find how many continuous shifts a con is working a day, given a time
             */
            private int NumberOfShiftsADay(int time, SchedulingMatrixNode node)
            {    
                //Calculate the day from the time, using integer division
                //Monday - Sunday is mapped to 0 - 6;
                int day = time / MINUTES_IN_A_DAY;
                int employeeIndex = node.employeeChange;
                
                //Calculates the midnight when the day begins, and the midnight when the day ends.
                //This code will not work if any shifts occur during midnight
                int dayBegins = day * MINUTES_IN_A_DAY;
                int dayEnds = (day + 1) * MINUTES_IN_A_DAY;
                int total = 0;

                for (int shiftIndex = 0; shiftIndex < _shifts.Count; shiftIndex++)
                {
                    if (node.CurrentMatrix[shiftIndex, employeeIndex] && _shifts[shiftIndex].StartTime > dayBegins && _shifts[shiftIndex].StartTime < dayEnds)
                    {
                        total++;
                        while (shiftIndex + 1 < _shifts.Count &&
                               node.CurrentMatrix[shiftIndex + 1, employeeIndex] &&
                               _shifts[shiftIndex].EndTime == _shifts[shiftIndex + 1].StartTime)
                            shiftIndex++;
                    }
                }
                return total;
            }

            class SchedulingMatrixNode
            {
                public int employeeChange;
                public int shiftChange;
                public bool working;
                public int heuristic;
                public SchedulingMatrixNode parent;
                public SchedulingMatrixNode leftChild;
                public SchedulingMatrixNode rightChild;
                public bool[,] CurrentMatrix;

                public SchedulingMatrixNode(int shiftChange, int employeeChange, bool working)
                {
                    this.employeeChange = employeeChange;
                    this.shiftChange = shiftChange;
                    this.working = working;
                }

                public SchedulingMatrixNode(int shiftChange, int employeeChange, bool working, int heuristic)
                {
                    this.employeeChange = employeeChange;
                    this.shiftChange = shiftChange;
                    this.working = working;
                    this.heuristic = heuristic;
                }

                public void GenerateMatrix()
                {
                    CurrentMatrix = (bool[,])parent.CurrentMatrix.Clone();
                    CurrentMatrix[shiftChange, employeeChange] = working;
                }
                /**
                 * Given a Shift index, checks whether or not the shift has the correct amount of cons.
                */
                public int ConsWorkingThisShift(int shiftIndex)
                {
                    int numberOfCons = 0;
                    for (int employeeIndex = 0; employeeIndex < CurrentMatrix.GetLength(1); employeeIndex++)
                    {
                        if (CurrentMatrix[shiftIndex, employeeIndex])
                        {
                            numberOfCons++;
                        }
                    }
                    return numberOfCons;
                }
            }

            /**
             * A Priority Queue using the heuristic of the node.
             */
            class NodePriorityQueue
            {
                public SchedulingMatrixNode[] nodes;
                public int numberOfNodes;
                public readonly int START_ARRAY_SIZE = 16;

                public NodePriorityQueue()
                {
                    nodes = new SchedulingMatrixNode[START_ARRAY_SIZE];
                    numberOfNodes = 0;
                }

                public void Insert(SchedulingMatrixNode node)
                {
                    if (numberOfNodes == nodes.Length)
                        expandArray();
                    nodes[numberOfNodes] = node;
                    heapifyUp(numberOfNodes++);
                    if (numberOfNodes > nodes.Length)
                        expandArray();
                }

                public SchedulingMatrixNode RemoveHighest()
                {
                    SchedulingMatrixNode temp = nodes[0];
                    nodes[0] = nodes[--numberOfNodes];
                    heapifyDown(0);
                    return temp;
                }

                private void heapifyDown(int i)
                {
                    int right = RightChild(i);
                    int left = LeftChild(i);
                    if (right < numberOfNodes)
                    {
                        int bigger = nodes[right].heuristic > nodes[left].heuristic ? right : left;
                        if (nodes[i].heuristic < nodes[bigger].heuristic)
                        {
                            SchedulingMatrixNode temp = nodes[i];
                            nodes[i] = nodes[bigger];
                            nodes[bigger] = temp;
                            heapifyDown(bigger);
                        }
                    }
                    else if (left < numberOfNodes)
                    {
                        if (nodes[i].heuristic < nodes[left].heuristic)
                        {
                            SchedulingMatrixNode temp = nodes[i];
                            nodes[i] = nodes[left];
                            nodes[left] = temp;
                            heapifyDown(left);
                        }
                    }
                }

                private void heapifyUp(int i)
                {
                    if (i == 0)
                        return;
                    int parent = Parent(i);
                    if (nodes[parent].heuristic < nodes[i].heuristic)
                    {
                        SchedulingMatrixNode temp = nodes[i];
                        nodes[i] = nodes[parent];
                        nodes[parent] = temp;
                        heapifyUp(parent);
                    }
                }

                private void expandArray()
                {
                    SchedulingMatrixNode[] newNodes = new SchedulingMatrixNode[(int)Math.Pow(nodes.Length, 2)];
                    for(int i = 0; i < nodes.Length; i++)
                    {
                        newNodes[i] = nodes[i];
                    }
                    nodes = newNodes;
                }

                private int Parent(int i)
                {
                    return (i - 1) / 2;
                }
                public int LeftChild(int i)
                {
                    return 2 * i + 1;
                }
                public int RightChild(int i)
                {
                    return 2 * i + 2;
                }
            }
        }
    }
}