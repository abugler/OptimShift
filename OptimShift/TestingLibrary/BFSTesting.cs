using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scheduling_Library;
using System.Collections.Generic;

namespace TestingLibrary
{
    [TestClass]
    public class BFSTesting
    {
        private string _employeesFile = "TestEmployees.txt";
        [TestMethod]
        public void DoesBFSCrash()
        {
            SchedulerSolverGreedy solverGreedy = new SchedulerSolverGreedy();
            solverGreedy.AssignShifts(_employeesFile);
            Console.WriteLine(solverGreedy.toString());
        }
    }
}