using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using Google.OrTools.ConstraintSolver;
namespace Scheduling_Library
{
    public class Workweek
    {
        private List<Shift> shifts;
        private List<Employee> employees;
        private int MaxCons = 2;
        
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
            /**
             * Add an availability at the end of the list. 
             */
            public void UpdateAvailability(int status, int time)
            {
                Availabilities.Add(new Tuple<int, int>(status, time));
            }
            
            /**
             * Is the employee available during the times given?
             */
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
             * Returns lowest availability given a time of the shift
             */
            public int LowestAvailabilityForShift(int StartTime, int EndTime)
            {
                int time = 0;
                int avail = 3;
                foreach (Tuple<int, int> AvailAndTime in Availabilities)
                {
                    time += AvailAndTime.Item2;
                    if (time < StartTime)
                        continue;
                    if (AvailAndTime.Item1 < avail || time > StartTime)
                        avail = AvailAndTime.Item1;
                    if (time >= EndTime)
                        return avail;
                    
                }
                throw new Exception("EndTime exceeds employee availability times");
            }
            /**
             * Getter method, returns name
             */
            public string GetName()
            {
                return name;
            }
        }

        public class Shift
        {
            //Start Time and End Time are measured in minutes after Monday Midnight 
            public int StartTime; 
            public int EndTime;
            //TODO: Add Shift Type
            private int MaxCons;
            
            public Shift(int start, int end, int max)
            {
                this.StartTime = start;
                this.EndTime = end;
                MaxCons = max;
            }
            public override string ToString()
            {
                return "Start: " + StartTime + "\n End: " + EndTime;
            }

            public int WorkTime()
            {
                return EndTime - StartTime;
            }

            public int MaxEmployees()
            {
                return MaxCons;
            }
        }
        
        //Populates the shifts object, and returns the result
        public List<Shift> GenerateShifts()
        {
            shifts = new List<Shift>();
            shifts.Add(new Shift(465, 540, MaxCons));
            for (int i = 540; i < 930; i = i + 30)
            {
                shifts.Add(new Shift(i, i + 30, MaxCons));
            }
            shifts.Add(new Shift(930, 1020, MaxCons));
            return shifts;
        }
        
        /**
         * Takes a properly formatted TXT file, and populates
         */
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

        public Employee FindEmployee(string name)
        {
            foreach (Employee employee in employees)
            {
                if (employee.GetName().Equals(name))
                    return employee;
            }
            throw new ArgumentException("Employee not found");
        }
    }
}