using System;
using System.Linq;
using System.Threading;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Utility.Containers;

namespace UnitTests
{
    [TestFixture]
    public class LinkedListTests: TestBase
    {
        [Test]
        public void Should__enumerate_empty_list()
        {
            var linkedList = new LinkedList<TestData>();
            Assert.AreEqual(0, linkedList.Count());

            var enumerator = linkedList.EnumerateFrom(null);
            Assert.IsNull(enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator = linkedList.EnumerateFrom(null, false);
            Assert.IsNull(enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void Should_behave_like_a_queue()
        {
            var linkedList = new LinkedList<TestData>();
            linkedList.Prepend(new TestData { Field1 = "A" });
            linkedList.Prepend(new TestData { Field1 = "B" });
            linkedList.Prepend(new TestData { Field1 = "C" });

            Assert.AreEqual("A", linkedList.PopLast().Field1);
            Assert.AreEqual("B", linkedList.PopLast().Field1);
            Assert.AreEqual("C", linkedList.PopLast().Field1);
        }

        [Test]
        public void Should_behave_like_a_stack()
        {
            var linkedList = new LinkedList<TestData>();
            linkedList.Prepend(new TestData { Field1 = "A" });
            linkedList.Prepend(new TestData { Field1 = "B" });
            linkedList.Prepend(new TestData { Field1 = "C" });

            Assert.AreEqual("C", linkedList.PopFirst().Field1);
            Assert.AreEqual("B", linkedList.PopFirst().Field1);
            Assert.AreEqual("A", linkedList.PopFirst().Field1);
        }

        [Test]
        public void Should_build_list()
        {
            var linkedList = new LinkedList<TestData>();

            var dataA = new TestData { Field1 = "A" };
            var dataB = new TestData { Field1 = "B" };
            var dataC = new TestData { Field1 = "C" };

            var elementA = linkedList.Append(dataA);
            var elementB = linkedList.Append(dataB);
            var elementC = linkedList.Append(dataC);

            Assert.AreEqual("A", elementA.Data.Field1);
            Assert.AreEqual("B", elementB.Data.Field1);
            Assert.AreEqual("C", elementC.Data.Field1);

            var list2 = linkedList.ToList();
            Assert.AreEqual(3, list2.Count);
            Assert.AreEqual("A", list2[0].Field1);
            Assert.AreEqual("B", list2[1].Field1);
            Assert.AreEqual("C", list2[2].Field1);
        }

        [Test]
        public void Should_delete_from_head()
        {
            var linkedList = new LinkedList<TestData>();

            var dataA = new TestData { Field1 = "A" };
            var dataB = new TestData { Field1 = "B" };
            var dataC = new TestData { Field1 = "C" };

            var elementA = linkedList.Append(dataA);
            var elementB = linkedList.Append(dataB);
            var elementC = linkedList.Append(dataC);

            linkedList.Delete(elementA);

            var list = linkedList.ToList();
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("B", list[0].Field1);
            Assert.AreEqual("C", list[1].Field1);
        }

        [Test]
        public void Should_delete_from_tail()
        {
            var linkedList = new LinkedList<TestData>();

            var dataA = new TestData { Field1 = "A" };
            var dataB = new TestData { Field1 = "B" };
            var dataC = new TestData { Field1 = "C" };

            var elementA = linkedList.Append(dataA);
            var elementB = linkedList.Append(dataB);
            var elementC = linkedList.Append(dataC);

            linkedList.Delete(elementC);

            var list = linkedList.ToList();
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("A", list[0].Field1);
            Assert.AreEqual("B", list[1].Field1);
        }

        [Test]
        public void Should_delete_from_middle()
        {
            var linkedList = new LinkedList<TestData>();

            var dataA = new TestData { Field1 = "A" };
            var dataB = new TestData { Field1 = "B" };
            var dataC = new TestData { Field1 = "C" };

            var elementA = linkedList.Append(dataA);
            var elementB = linkedList.Append(dataB);
            var elementC = linkedList.Append(dataC);

            linkedList.Delete(elementB);

            var list = linkedList.ToList();
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("A", list[0].Field1);
            Assert.AreEqual("C", list[1].Field1);
        }

        [Test]
        public void Should_delete_from_multiple_lists()
        {
            var linkedList1 = new LinkedList<TestData>();
            var linkedList2 = new LinkedList<TestData>();

            var dataA = new TestData { Field1 = "A" };
            var dataB = new TestData { Field1 = "B" };
            var dataC = new TestData { Field1 = "C" };

            dataA.Element1 = linkedList1.Append(dataA);
            dataB.Element1 = linkedList1.Append(dataB);
            dataC.Element1 = linkedList1.Append(dataC);

            dataB.Element2 = linkedList2.Append(dataB);
            dataA.Element2 = linkedList2.Append(dataA);
            dataC.Element2 = linkedList2.Append(dataC);

            var list1 = linkedList1.ToList();
            var list2 = linkedList2.ToList();

            Assert.AreEqual(3, list1.Count);
            Assert.AreEqual("A", list1[0].Field1);
            Assert.AreEqual("B", list1[1].Field1);
            Assert.AreEqual("C", list1[2].Field1);

            Assert.AreEqual(3, list2.Count);
            Assert.AreEqual("B", list2[0].Field1);
            Assert.AreEqual("A", list2[1].Field1);
            Assert.AreEqual("C", list2[2].Field1);

            linkedList1.Delete(dataA.Element1);
            linkedList2.Delete(dataA.Element2);

            list1 = linkedList1.ToList();
            list2 = linkedList2.ToList();

            Assert.AreEqual(2, list1.Count);
            Assert.AreEqual("B", list1[0].Field1);
            Assert.AreEqual("C", list1[1].Field1);

            Assert.AreEqual(2, list2.Count);
            Assert.AreEqual("B", list2[0].Field1);
            Assert.AreEqual("C", list2[1].Field1);
        }

        [Test]
        public void Should_enumerate_forwards()
        {
            var linkedList = new LinkedList<TestData>();

            var dataA = new TestData { Field1 = "A" };
            var dataB = new TestData { Field1 = "B" };
            var dataC = new TestData { Field1 = "C" };
            var dataD = new TestData { Field1 = "D" };
            var dataE = new TestData { Field1 = "E" };
            var dataF = new TestData { Field1 = "F" };

            dataA.Element1 = linkedList.Append(dataA);
            dataB.Element1 = linkedList.Append(dataB);
            dataC.Element1 = linkedList.Append(dataC);
            dataD.Element1 = linkedList.Append(dataD);
            dataE.Element1 = linkedList.Append(dataE);
            dataF.Element1 = linkedList.Append(dataF);

            var list1 = ToList(linkedList.EnumerateFrom(null));

            Assert.AreEqual(6, list1.Count);
            Assert.AreEqual("A", list1[0].Field1);
            Assert.AreEqual("B", list1[1].Field1);
            Assert.AreEqual("F", list1[5].Field1);

            var list2 = ToList(linkedList.EnumerateFrom(dataA.Element1));

            Assert.AreEqual(5, list2.Count);
            Assert.AreEqual("B", list2[0].Field1);
            Assert.AreEqual("C", list2[1].Field1);
            Assert.AreEqual("F", list2[4].Field1);

            var list3 = ToList(linkedList.EnumerateFrom(dataC.Element1));

            Assert.AreEqual(3, list3.Count);
            Assert.AreEqual("D", list3[0].Field1);
            Assert.AreEqual("E", list3[1].Field1);
            Assert.AreEqual("F", list3[2].Field1);
        }

        [Test]
        public void Should_enumerate_backwards()
        {
            var linkedList = new LinkedList<TestData>();

            var dataA = new TestData { Field1 = "A" };
            var dataB = new TestData { Field1 = "B" };
            var dataC = new TestData { Field1 = "C" };
            var dataD = new TestData { Field1 = "D" };
            var dataE = new TestData { Field1 = "E" };
            var dataF = new TestData { Field1 = "F" };

            dataA.Element1 = linkedList.Append(dataA);
            dataB.Element1 = linkedList.Append(dataB);
            dataC.Element1 = linkedList.Append(dataC);
            dataD.Element1 = linkedList.Append(dataD);
            dataE.Element1 = linkedList.Append(dataE);
            dataF.Element1 = linkedList.Append(dataF);

            var list1 = ToList(linkedList.EnumerateFrom(null, false));

            Assert.AreEqual(6, list1.Count);
            Assert.AreEqual("F", list1[0].Field1);
            Assert.AreEqual("E", list1[1].Field1);
            Assert.AreEqual("A", list1[5].Field1);

            var list2 = ToList(linkedList.EnumerateFrom(dataA.Element1, false));

            Assert.AreEqual(0, list2.Count);

            var list3 = ToList(linkedList.EnumerateFrom(dataC.Element1, false));

            Assert.AreEqual(2, list3.Count);
            Assert.AreEqual("B", list3[0].Field1);
            Assert.AreEqual("A", list3[1].Field1);
        }

        [Test]
        public void Should_be_thread_safe()
        {
            var linkedList = new LinkedList<TestData>();

            var threads = new System.Collections.Generic.List<Thread>();
            var itemsProcessed = new System.Collections.Generic.HashSet<string>();
            var hasDuplicates = false;
            var exceptions = false;

            var stopThreads = false;
            for (var i = 0; i < 10; i++)
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        while (!stopThreads)
                        {
                            Thread.Sleep(0);
                            var testData = linkedList.PopFirst();
                            if (testData != null)
                            {
                                lock (itemsProcessed)
                                {
                                    if (!itemsProcessed.Add(testData.Field1))
                                        hasDuplicates = true;
                                }
                            }
                        }
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception)
                    {
                        exceptions = true;
                    }
                });
                threads.Add(thread);
                thread.Start();
            }

            for (var i = 0; i < 10000; i++)
            {
                var testData = new TestData { Field1 = i.ToString() };
                linkedList.Append(testData);
            }

            while (linkedList.ToList().Count > 0)
                Thread.Sleep(10);

            stopThreads = true;
            Thread.Sleep(50);

            Assert.IsFalse(exceptions);
            Assert.IsFalse(hasDuplicates);
            Assert.AreEqual(10000, itemsProcessed.Count);
        }

        private System.Collections.Generic.List<TestData> ToList(System.Collections.Generic.IEnumerator<TestData> list)
        {
            var result = new System.Collections.Generic.List<TestData>();
            while (list.MoveNext())
                result.Add(list.Current);
            return result;
        }

        private class TestData
        {
            public LinkedList<TestData>.ListElement Element1 { get; set; }
            public LinkedList<TestData>.ListElement Element2 { get; set; }

            public string Field1 { get; set; }
            public string Field2 { get; set; }
            public string Field3 { get; set; }
        }

    }
}
