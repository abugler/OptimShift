using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scheduling_Library;
using System.Collections.Generic;

namespace TestingLibrary
{
    [TestClass]
    public class BFSTesting
    {
        private string _employeesFakeFile = "TestEmployees.txt";
        private string _realDataSet = "AVAILABILITY_2019-09-16 20:45:50.086897.txt";
        [TestMethod]
        public void DoesBFSCrash()
        {
            SchedulerSolverGreedy solverGreedy = new SchedulerSolverGreedy();
            solverGreedy.AssignShifts(_employeesFakeFile);
            Console.WriteLine(solverGreedy.toString());
        }

        [TestMethod]
        public void RealDataSet()
        {
            SchedulerSolverGreedy solverGreedy = new SchedulerSolverGreedy();
            solverGreedy.AssignShifts(_realDataSet);
            Console.WriteLine(solverGreedy.toString());
        }
    }
}