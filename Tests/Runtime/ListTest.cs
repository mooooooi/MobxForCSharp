using NUnit.Framework;
using System;

namespace Higo.Mobx.Tests
{
    public static class ListTest
    {
        [Test]
        public static void NormalTest()
        {
            var list = Store32.AsRoot<ObservableList<int>>();
            var testNum = 0;
            var testNum1 = 0;
            list.AutoRun(() =>
            {
                if (list.TryGetValue(0, out var i))
                    testNum = i;
            });
            list.AutoRun(() =>
            {
                if (list.TryGetValue(1, out var i))
                    testNum1 = i;
            });
            Assert.AreEqual(testNum, 0);
            Assert.AreEqual(testNum1, 0);

            using (list.CreateActionScope())
            {
                list.Add(2);
            }
            Assert.AreEqual(testNum, 2);


            using (list.CreateActionScope())
            {
                list.Add(3);
            }
            Assert.AreEqual(testNum1, 3);
        }
    }

}