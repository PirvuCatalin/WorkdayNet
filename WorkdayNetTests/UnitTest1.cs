using WorkdayNet;

namespace WorkdayNetTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestIncrementConversion()
        {
            WorkdayCalendar testCalendar = new WorkdayCalendar();
            testCalendar.SetWorkdayStartAndStop(8, 0, 16, 0);

            TimeSpan time = testCalendar.ConvertIncrementToTimeSpan(44.723656m);

            Assert.That(time.Days, Is.EqualTo(44));
            Assert.That(time.Hours, Is.EqualTo(5));
            Assert.That(time.Minutes, Is.EqualTo(47));
        }

        [Test]
        public void SimpleTests()
        {
            IWorkdayCalendar calendar = new WorkdayCalendar();
            calendar.SetWorkdayStartAndStop(8, 0, 16, 0);

            var start = new DateTime(2004, 5, 24, 9, 5, 0);
            decimal increment = 0.25m;
            var incrementedDate = calendar.GetWorkdayIncrement(start, increment);
            Assert.That(incrementedDate, Is.EqualTo(new DateTime(2004, 5, 24, 11, 5, 0)));

            start = new DateTime(2004, 5, 24, 9, 5, 0);
            increment = 0.27273m;
            incrementedDate = calendar.GetWorkdayIncrement(start, increment);
            Assert.That(incrementedDate, Is.EqualTo(new DateTime(2004, 5, 24, 11, 15, 0)));
        }

        [Test]
        public void VariableWorkdayHoursTest()
        {
            IWorkdayCalendar sixHourCalendar = new WorkdayCalendar();
            sixHourCalendar.SetWorkdayStartAndStop(9, 15, 15, 15);

            var start = new DateTime(2004, 5, 24, 9, 25, 0);
            var increment = 0.6459m;
            var incrementedDate = sixHourCalendar.GetWorkdayIncrement(start, increment);

            Assert.That(incrementedDate, Is.EqualTo(new DateTime(2004, 5, 24, 13, 17, 0)));
        }

        [Test]
        public void NightWorkingTest()
        {
            IWorkdayCalendar sixHourCalendar = new WorkdayCalendar();
            sixHourCalendar.SetWorkdayStartAndStop(22, 10, 3, 10);

            var start = new DateTime(2004, 5, 24, 23, 10, 0);
            var increment = 0.5m;
            var incrementedDate = sixHourCalendar.GetWorkdayIncrement(start, increment);

            Assert.That(incrementedDate, Is.EqualTo(new DateTime(2004, 5, 25, 1, 40, 0)));
        }

        // all tests above assume that there is no need to go to the next day, meaning that the "job" can be done in the same workday
        // also, tests above don't test for holidays!

        [Test]
        public void MultidayIncrementsTest()
        {
            IWorkdayCalendar calendar = new WorkdayCalendar();
            calendar.SetWorkdayStartAndStop(8, 0, 16, 0);

            var start = new DateTime(2004, 5, 24, 12, 3, 0);
            decimal increment = 1.25m;
            var incrementedDate = calendar.GetWorkdayIncrement(start, increment);

            Assert.That(incrementedDate, Is.EqualTo(new DateTime(2004, 5, 25, 14, 3, 0)));
        }

        [Test]
        public void OutsideOfBusinessHoursTest()
        {
            IWorkdayCalendar calendar = new WorkdayCalendar();
            calendar.SetWorkdayStartAndStop(8, 0, 16, 0);

            var start = new DateTime(2004, 5, 24, 15, 30, 0);
            decimal increment = 1.25m;
            var incrementedDate = calendar.GetWorkdayIncrement(start, increment);

            Assert.That(incrementedDate, Is.EqualTo(new DateTime(2004, 5, 26, 9, 30, 0)));
        }

        [Test]
        public void OutsideOfBusinessHoursSpecialCaseTest()
        {
            IWorkdayCalendar calendar = new WorkdayCalendar();
            calendar.SetWorkdayStartAndStop(21, 0, 3, 0);

            var start = new DateTime(2004, 5, 24, 23, 0, 0);
            decimal increment = 1m;
            var incrementedDate = calendar.GetWorkdayIncrement(start, increment);

            Assert.That(incrementedDate, Is.EqualTo(new DateTime(2004, 5, 25, 23, 0, 0)));
        }

        [Test]
        public void StartAfterStopHoursTest()
        {
            IWorkdayCalendar calendar = new WorkdayCalendar();
            calendar.SetWorkdayStartAndStop(8, 0, 16, 0);

            var start = new DateTime(2004, 5, 24, 17, 0, 0);
            decimal increment = 0.25m;
            var incrementedDate = calendar.GetWorkdayIncrement(start, increment);

            Assert.That(incrementedDate, Is.EqualTo(new DateTime(2004, 5, 25, 10, 0, 0)));
        }

        [Test]
        public void StartBeforeStartHoursTest()
        {
            IWorkdayCalendar calendar = new WorkdayCalendar();
            calendar.SetWorkdayStartAndStop(8, 0, 16, 0);

            var start = new DateTime(2004, 5, 24, 7, 0, 0);
            decimal increment = 0.25m;
            var incrementedDate = calendar.GetWorkdayIncrement(start, increment);

            Assert.That(incrementedDate, Is.EqualTo(new DateTime(2004, 5, 24, 10, 0, 0)));
        }

        [Test]
        public void StartDateWeekendTest()
        {
            IWorkdayCalendar calendar = new WorkdayCalendar();
            calendar.SetWorkdayStartAndStop(8, 0, 16, 0);

            var start = new DateTime(2022, 5, 21, 12, 0, 0);
            decimal increment = 0.25m;
            var incrementedDate = calendar.GetWorkdayIncrement(start, increment);

            Assert.That(incrementedDate, Is.EqualTo(new DateTime(2022, 5, 23, 10, 0, 0)));
        }

        [Test]
        public void IncomingWeekendTest()
        {
            IWorkdayCalendar calendar = new WorkdayCalendar();
            calendar.SetWorkdayStartAndStop(8, 0, 16, 0);

            var start = new DateTime(2022, 5, 20, 12, 0, 0);
            decimal increment = 1.25m;
            var incrementedDate = calendar.GetWorkdayIncrement(start, increment);

            Assert.That(incrementedDate, Is.EqualTo(new DateTime(2022, 5, 23, 14, 0, 0)));
        }


        //[Test]
        //public void NegativeTest1()
        //{
        //    IWorkdayCalendar calendar = new WorkdayCalendar();
        //    calendar.SetWorkdayStartAndStop(8, 0, 16, 0);
        //    calendar.SetRecurringHoliday(5, 17);
        //    calendar.SetHoliday(new DateTime(2004, 5, 27));

        //    var start = new DateTime(2004, 5, 24, 18, 5, 0);
        //    decimal increment = -5.5m;
        //    var incrementedDate = calendar.GetWorkdayIncrement(start, increment);

        //    Assert.That(incrementedDate, Is.EqualTo(new DateTime(1, 01, 01, 0, 0, 0)));
        //}
    }
}