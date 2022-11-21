using NUnit.Framework;
using System;
using System.Linq;

namespace Higo.Mobx.Tests
{
    public static class ListTest
    {
        [Test]
        public static void AddTest()
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

        [Test]
        public static void RemoveAtTest()
        {
            var list = Store32.AsRoot<ObservableList<int>>();
            list.Add(0);
            list.Add(1);
            list.Add(2);

            var testNum = 0;
            var count = 0;

            testNum = 2;
            list.AutoRun(() =>
            {
                count++;
                if (list.TryGetValue(2, out var ret))
                    Assert.AreEqual(testNum, ret);
            });
            Assert.AreEqual(count, 1);

            testNum = 22222;
            using (list.CreateActionScope())
            {
                list.RemoveAt(0);
            }
            Assert.AreEqual(count, 2);
        }


        [Test]
        public static void RemoveTest()
        {
            var list = Store32.AsRoot<ObservableList<int>>();
            list.Add(0);
            list.Add(1);
            list.Add(2);

            var testNum = 0;
            var count = 0;

            testNum = 2;
            list.AutoRun(() =>
            {
                count++;
                if (list.TryGetValue(2, out var ret))
                    Assert.AreEqual(testNum, ret);
            });
            Assert.AreEqual(count, 1);

            testNum = 22222;
            using (list.CreateActionScope())
            {
                list.Remove(0);
            }
            Assert.AreEqual(count, 2);
        }

        [Test]
        public static void ClearTest()
        {
            var list = Store32.AsRoot<ObservableList<int>>();
            list.Add(0);
            list.Add(1);
            list.Add(2);

            var count = 0;
            list.AutoRun(() =>
            {
                if (list.TryGetValue(0, out var ret))
                    Assert.AreEqual(ret, 0);
                count++;

            });
            list.AutoRun(() =>
            {
                if (list.TryGetValue(1, out var ret))
                    Assert.AreEqual(ret, 1);
                count++;
            });
            list.AutoRun(() =>
            {
                if (list.TryGetValue(2, out var ret))
                    Assert.AreEqual(ret, 2);
                count++;
            });
            list.AutoRun(() =>
            {
                Assert.False(list.TryGetValue(3, out _));
                count++;
            });
            Assert.AreEqual(count, 4);

            count = 0;
            using (list.CreateActionScope())
            {
                list.Clear();
            }
            Assert.AreEqual(count, 3);
        }

        [Test]
        public static void ContainsTest()
        {
            var list = Store32.AsRoot<ObservableList<int>>();
            list.Add(0);
            list.Add(1);
            list.Add(2);

            Assert.True(list.Contains(0));
            Assert.True(list.Contains(1));
            Assert.True(list.Contains(2));

            list.Remove(1);
            Assert.False(list.Contains(1));
        }

        [Test]
        public static void ForeachTest()
        {
            var list = Store32.AsRoot<ObservableList<int>>();
            list.Add(0);
            list.Add(1);
            list.Add(2);

            var targetSum = 3;
            var count = 0;
            list.AutoRun(() =>
            {
                foreach (var num in list)
                {
                    count += num;
                }
            });
            Assert.AreEqual(count, targetSum);

            targetSum = 4;
            count = 0;
            using (list.CreateActionScope())
                list[1] = 2;
            Assert.AreEqual(count, targetSum);
        }

        [Test]
        public static void SumTest()
        {
            var list = Store32.AsRoot<ObservableList<int>>();
            list.Add(0);
            list.Add(1);
            list.Add(2);

            var targetSum = 3;
            var count = 0;
            list.AutoRun(() =>
            {
                count = list.Sum();
            });
            Assert.AreEqual(count, targetSum);

            targetSum = 4;
            count = 0;
            using (list.CreateActionScope())
                list[1] = 2;
            Assert.AreEqual(count, targetSum);

            targetSum = 2;
            count = 0;
            using (list.CreateActionScope())
                list.RemoveAt(1);
            Assert.AreEqual(count, targetSum);
        }
    }

}