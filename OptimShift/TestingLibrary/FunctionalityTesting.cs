using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scheduling_Library;
using System.Collections.Generic;
namespace TestingLibrary
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void DoesItNotCrash()
        {
            Workweek week = new Workweek();
            week.GenerateShifts();
            week.PopulateEmployees("TestEmployees.txt");
        }

        [TestMethod]
        public void DoesAvailabilityFunctionWork()
        {
            Workweek week = new Workweek();
            week.GenerateShifts();
            List<Workweek.Employee> employees =  week.PopulateEmployees("TestEmployees.txt");
            //This is Seoyeon, who is free throughout the Monday
            Assert.IsTrue(employees[0].IsAvailable(400, 700));
            //This is Hakan, who is pinch hitting, and is never free
            Assert.IsFalse(employees[2].IsAvailable(400, 900));
            //This is Andie, who has a more complex schedule on Mondays. 
            //Sometimes she is free, sometimes, she is not.
            Assert.IsTrue(employees[1].IsAvailable(560, 810));
            //The end of this shift is red, so she cannot work it
            Assert.IsFalse(employees[1].IsAvailable(600, 840));
            //The start of this shift is red, so she cannot work it
            Assert.IsFalse(employees[1].IsAvailable(900, 1200));
            //In the middle of this shift, there is a red section, so she cannot work it
            Assert.IsFalse(employees[1].IsAvailable(600, 1000));
        } 
    }
}