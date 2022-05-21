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

            TimeSpan time = testCalendar.IncrementToTimeSpan(44.723656m);

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

            var start = new DateTime(2004, 5, 24, 9, 5, 0);
            var increment = 0.6459m;
            var incrementedDate = sixHourCalendar.GetWorkdayIncrement(start, increment);

            Assert.That(incrementedDate, Is.EqualTo(new DateTime(2004, 5, 24, 12, 57, 0)));
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