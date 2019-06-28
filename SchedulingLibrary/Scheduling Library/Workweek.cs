using System;
using System.Collections.Generic;
using System.IO;
using Google.OrTools.ConstraintSolver;
namespace Scheduling_Library
{
    public class Workweek
    {
        private List<Shift> shifts;
        private List<Employee> employees;
        public class Employee
        {
            private string name;
            private List<Tuple<int, int>> Availabilities;
            private int minutesWorking;
            private List<Shift> shifts;

            private int MinShiftLength = 60;
            private int MaxShiftLength = 180;
            //First int is availability
            //0 is Red, 1 is Pink, 2 is White, 3 is Green
            //Second int is time for each availability, in minutes
            public Employee(string name)
            {
                this.name = name;
                Availabilities = new List<Tuple<int, int>>();
                minutesWorking = 0;
            }

            public void UpdateAvailability(int status, int time)
            {
                Availabilities.Add(new Tuple<int, int>(status, time));
            }

            public bool IsAvailable(int StartTime, int EndTime)
            {
                int time = 0;
                bool AvailableLastTime;
                foreach (Tuple<int, int> AvailAndTime in Availabilities)
                {
                    time += AvailAndTime.Item2;
                    AvailableLastTime = AvailAndTime.Item1 != 0;
                    if (time < StartTime)
                        continue;
                    if (AvailAndTime.Item1 == 0 || (!AvailableLastTime && time > StartTime))
                        return false;
                    if (time >= EndTime)
                        return true;
                    
                }
                throw new Exception("EndTime exceeds employee availability times");
            }
            
            /**
             * This function should only be called by Shift.AddEmployee();
             * Adds a Shift
             */
            public void AddShift(Shift shift)
            {
                shifts.Add(shift);
                minutesWorking += shift.WorkTime();
                ShiftInsertionSort();
                
            }
            /**
             * Helper function for AddShift. Keeps Shifts sorted for easy
             * InsertionSort is Preferred over QuickSort, as an Item will be placed at the end of the list,
             * and only that item will be out of place. This make InsertionSort O(n), as opposed to QuickSorts O(log(n))
             */
            private void ShiftInsertionSort()
            {
                if (shifts.Count < 2)
                    return;
                int CurrentComparison = shifts.Count - 1;
                Shift last = shifts[shifts.Count - 1];
                while (shifts[CurrentComparison - 1].StartTime > last.StartTime)
                {
                    CurrentComparison--;
                }

                Shift temp = last;
                shifts[shifts.Count - 1] = shifts[CurrentComparison];
                shifts[CurrentComparison] = temp;
            }
            /**
             * Given a Shift Object, how long is the employee working continuously?
             */
            public int ShiftLength(Shift shift)
            {
                //Perform Binary Search to find the shift
                int currentSearch = shifts.Count / 2;
                int currentIncrement = currentSearch / 2;
                while (shifts[currentSearch] != shift)
                {
                    if(currentIncrement == 0)
                        throw new ArgumentException("Shift " + shift + " not found for employee " + name + "." );
                    else if (shifts[currentSearch].StartTime < shift.StartTime)
                    {
                        currentSearch = currentSearch + currentIncrement;
                        currentIncrement /= 2;
                    }
                    else if (shifts[currentSearch].StartTime > shift.StartTime)
                    {
                        currentSearch = currentSearch - currentIncrement;
                        currentIncrement /= 2;
                    }
                }
                return ShiftLengthHelper(currentSearch);
            }

            private int ShiftLengthHelper(int i)
            {
                int time = shifts[i].WorkTime();
                if (i != 0 && shifts[i - 1].EndTime == shifts[i].StartTime)
                    time = ShiftLengthHelper(i - 1);
                if (i < shifts.Count - 1 && shifts[i + 1].StartTime == shifts[i].EndTime)
                    time = ShiftLengthHelper(i + 1);
                return time;
            }

            private bool IsAcceptableShift(Shift shift)
            {
                int shiftLength = ShiftLength(shift);
                return shiftLength >= MinShiftLength && shiftLength <= MaxShiftLength;
            }
        }

        public class Shift
        {
            public Employee[] WorkingEmployees;
            public int NumberOfEmployees;
            public int StartTime; //Start Time and End Time are measured in minutes after Monday Midnight 
            public int EndTime;

            public Shift(int start, int end, int max)
            {
                this.StartTime = start;
                this.EndTime = end;
                this.WorkingEmployees = new Employee[max];
                this.NumberOfEmployees = 0;
            }
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public void AddEmployee(Employee employee)
            {
                if (NumberOfEmployees <= WorkingEmployees.Length)
                {
                    WorkingEmployees[NumberOfEmployees] = employee;
                    NumberOfEmployees++;
                    employee.AddShift(this);
                }
                else
                {
                    Console.WriteLine("Shift is full");
                }
            }

            public int WorkTime()
            {
                return EndTime - StartTime;
            }
        }
        
        //Populates the shifts object, and returns the result
        public List<Shift> GenerateShifts()
        {
            shifts = new List<Shift>();
            int MaxCons = 2;
            shifts.Add(new Shift(465, 540, MaxCons));
            for (int i = 540; i < 930; i = i + 30)
            {
                shifts.Add(new Shift(i, i + 30, MaxCons));
            }
            shifts.Add(new Shift(930, 1020, MaxCons));
            return shifts;
        }
        
        //takes a file, and populates the employees list
        public List<Employee> PopulateEmployees(string filename)
        {
            string[] Lines = File.ReadAllLines(filename);
            employees = new List<Employee>();
            int currentLine = 0;
            if (!Lines[currentLine].Equals("EMPLOYEES"))
            {
                throw new ArgumentException();
            }

            currentLine++;
            while (currentLine < Lines.Length)
            {
                if (!Lines[currentLine].Substring(0, 6).Equals("NAME: "))
                    throw new ArgumentException();
                Employee NewEmployee = new Employee(Lines[currentLine].Substring(6));
                currentLine++;
                if(!Lines[currentLine].Equals("AVAILABILITY"))
                    throw new ArgumentException();
                currentLine++;
                while (currentLine < Lines.Length && Lines[currentLine].Length != 0)
                {
                    string[] TimeAndStatus = Lines[currentLine].Split(' ');
                    NewEmployee.UpdateAvailability(Int32.Parse(TimeAndStatus[1]),
                        Int32.Parse(TimeAndStatus[0]));
                    currentLine++;
                }
                employees.Add(NewEmployee);
                currentLine++;
            }
            return employees;
        } 
    }
}