using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using MhLabs.PubSubExtensions.Model;
using Newtonsoft.Json;
using Xunit;

namespace MhLabs.PubSubExtensions.Tests
{
    public class MutationModelTest
    {
        [Fact]
        public void IdenticalModels()
        {
            var obj1 = new TestItem { Name = "TestName", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "TestName", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.Empty(model.Diff());
        }

        [Fact]
        public void NullProperties()
        {
            var obj1 = new TestItem { Name = null, Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = null, Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.Empty(model.Diff());
        }
        [Fact]
        public void LHSPropertyNull()
        {
            var obj1 = new TestItem { Name = null, Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.Single(model.Diff());
        }
        [Fact]
        public void RHSPropertiesNull()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = null, Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.Single(model.Diff());
        }

        [Fact]
        public void NullToLeft()
        {
            TestItem obj1 = null;
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.NotEmpty(model.Diff());
        }
        [Fact]
        public void NullToRight()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            TestItem obj2 = null;
            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.NotEmpty(model.Diff());
        }

        [Fact]
        public void RootItemChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test2", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.Single(model.Diff());
            Assert.Equal("Name", model.Diff()[0]);
        }

        [Fact]
        public void NestedItemChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(2, diff.Count);
            Assert.Contains(diff, p => p == "Address");
            Assert.Contains(diff, p => p == "Address.AddressRow1");
        }

        [Fact]
        public void NestedListNotChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();
            var minis = fixture.CreateMany<TestMiniItem>(10).ToList();

            obj1.Minis = minis;
            obj2.Minis = minis;

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(2, diff.Count);
            Assert.Contains(diff, p => p == "Address");
            Assert.Contains(diff, p => p == "Address.AddressRow1");
        }

        [Fact]
        public void NestedListQuantityChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();
            var minis = fixture.CreateMany<TestMiniItem>(10).ToList();

            obj1.Minis = minis;
            obj2.Minis = minis.Skip(1).ToList();

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(5, diff.Count);
            Assert.Contains(diff, p => p == "Address");
            Assert.Contains(diff, p => p == "Address.AddressRow1");
            Assert.Contains(diff, p => p == "Minis");
            Assert.Contains(diff, p => p == "Minis.Id");
            Assert.Contains(diff, p => p == "Minis.Message");
        }

        [Fact]
        public void NestedListContentChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();

            obj1.Minis = fixture.CreateMany<TestMiniItem>(10).ToList();
            obj2.Minis = fixture.CreateMany<TestMiniItem>(10).ToList();

            // obj2.Minis.Add(fixture.Create<TestMiniItem>());

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(5, diff.Count);
            Assert.Contains(diff, p => p == "Address");
            Assert.Contains(diff, p => p == "Address.AddressRow1");
            Assert.Contains(diff, p => p == "Minis");
            Assert.Contains(diff, p => p == "Minis.Id");
            Assert.Contains(diff, p => p == "Minis.Message");
        }



        [Fact]
        public void NestedListNullToLeft()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();

            obj1.Minis = null;
            obj2.Minis = fixture.CreateMany<TestMiniItem>(10).ToList();

            // obj2.Minis.Add(fixture.Create<TestMiniItem>());

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(5, diff.Count);
            Assert.Contains(diff, p => p == "Address");
            Assert.Contains(diff, p => p == "Address.AddressRow1");
            Assert.Contains(diff, p => p == "Minis");
            Assert.Contains(diff, p => p == "Minis.Id");
            Assert.Contains(diff, p => p == "Minis.Message");
        }

        [Fact]
        public void NestedListNullToRight()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();

            obj1.Minis = fixture.CreateMany<TestMiniItem>(10).ToList();
            obj2.Minis = null;

            // obj2.Minis.Add(fixture.Create<TestMiniItem>());

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(5, diff.Count);
            Assert.Contains(diff, p => p == "Address");
            Assert.Contains(diff, p => p == "Address.AddressRow1");
            Assert.Contains(diff, p => p == "Minis");
            Assert.Contains(diff, p => p == "Minis.Id");
            Assert.Contains(diff, p => p == "Minis.Message");
        }

        [Fact]
        public void NestedListObjectNullToRight()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };

            var fixture = new Fixture();
            obj1.Enums = new List<TestEnum>() {
                TestEnum.One,
                TestEnum.Two,
                TestEnum.Three
            };

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = null };
            var diff = model.Diff();
            Assert.Equal(7, diff.Count);
            Assert.Contains(diff, p => p == "Address");
            Assert.Contains(diff, p => p == "Address.AddressRow1");
            Assert.Contains(diff, p => p == "Enums");
        }

        [Fact]
        public void NestedDictionaryNotChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();

            var minisDictionary = new Dictionary<string, TestMiniItem>();
            fixture.CreateMany<KeyValuePair<string, TestMiniItem>>().ToList().ForEach(kvp => minisDictionary.Add(kvp.Key, kvp.Value));

            obj1.MinisDictionary = minisDictionary;
            obj2.MinisDictionary = minisDictionary;

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(2, diff.Count);
            Assert.Contains(diff, p => p == "Address");
            Assert.Contains(diff, p => p == "Address.AddressRow1");
        }

        [Fact]
        public void NestedDictionaryQuantityChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();

            var minisDictionary = new Dictionary<string, TestMiniItem>();
            fixture.CreateMany<KeyValuePair<string, TestMiniItem>>().ToList().ForEach(kvp => minisDictionary.Add(kvp.Key, kvp.Value));

            obj1.MinisDictionary = minisDictionary;

            obj2.MinisDictionary = new Dictionary<string, TestMiniItem>(minisDictionary);
            obj2.MinisDictionary.Remove(obj2.MinisDictionary.First().Key);

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(5, diff.Count);
            Assert.Contains(diff, p => p == "Address");
            Assert.Contains(diff, p => p == "Address.AddressRow1");
            Assert.Contains(diff, p => p == "MinisDictionary");
            Assert.Contains(diff, p => p == "MinisDictionary.Id");
            Assert.Contains(diff, p => p == "MinisDictionary.Message");
        }

        [Fact]
        public void NestedDictionaryContentChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();
            var minisDictionary = new Dictionary<string, TestMiniItem>();
            fixture.CreateMany<KeyValuePair<string, TestMiniItem>>().ToList().ForEach(kvp => minisDictionary.Add(kvp.Key, kvp.Value));

            var minisDictionary2 = new Dictionary<string, TestMiniItem>();
            fixture.CreateMany<KeyValuePair<string, TestMiniItem>>().ToList().ForEach(kvp => minisDictionary2.Add(kvp.Key, kvp.Value));

            obj1.MinisDictionary = minisDictionary;
            obj2.MinisDictionary = minisDictionary2;

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(5, diff.Count);
            Assert.Contains(diff, p => p == "Address");
            Assert.Contains(diff, p => p == "Address.AddressRow1");
            Assert.Contains(diff, p => p == "MinisDictionary");
            Assert.Contains(diff, p => p == "MinisDictionary.Id");
            Assert.Contains(diff, p => p == "MinisDictionary.Message");
        }

        [Fact]
        public void NestedDictionaryNullToLeft()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();


            obj1.MinisDictionary = null;

            var minisDictionary = new Dictionary<string, TestMiniItem>();
            fixture.CreateMany<KeyValuePair<string, TestMiniItem>>().ToList().ForEach(kvp => minisDictionary.Add(kvp.Key, kvp.Value));

            obj2.MinisDictionary = minisDictionary;

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(5, diff.Count);
            Assert.Contains(diff, p => p == "Address");
            Assert.Contains(diff, p => p == "Address.AddressRow1");
            Assert.Contains(diff, p => p == "MinisDictionary");
            Assert.Contains(diff, p => p == "MinisDictionary.Id");
            Assert.Contains(diff, p => p == "MinisDictionary.Message");
        }
        [Fact]
        public void NestedDictionaryNullToRight()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();

            var minisDictionary = new Dictionary<string, TestMiniItem>();
            fixture.CreateMany<KeyValuePair<string, TestMiniItem>>().ToList().ForEach(kvp => minisDictionary.Add(kvp.Key, kvp.Value));

            obj1.MinisDictionary = minisDictionary;
            obj2.MinisDictionary = null;

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(5, diff.Count);
            Assert.Contains(diff, p => p == "Address");
            Assert.Contains(diff, p => p == "Address.AddressRow1");
            Assert.Contains(diff, p => p == "MinisDictionary");
            Assert.Contains(diff, p => p == "MinisDictionary.Id");
            Assert.Contains(diff, p => p == "MinisDictionary.Message");
        }

        [Fact]
        public void EnumTest()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItem>();
            var json = JsonConvert.SerializeObject(item);
            var item2 = JsonConvert.DeserializeObject<TestItem>(json);

            item2.TestEnum = TestEnum.Three;

            var model = new MutationModel<TestItem> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();
            Assert.Single(diff);
            Assert.Contains(diff, p => p == "TestEnum");
        }
        [Fact]
        public void DateTimeTest()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestDateTime>();
            var item2 = fixture.Create<TestDateTime>();
            var model = new MutationModel<TestDateTime> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();
            Assert.Equal(2, diff.Count);
        }

        [Fact]
        public void EnumListTest()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItem>();
            item.Enums = fixture.Create<List<TestEnum>>();

            var json = JsonConvert.SerializeObject(item);
            var item2 = JsonConvert.DeserializeObject<TestItem>(json);

            item2.Enums.Reverse();

            var model = new MutationModel<TestItem> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();
            Assert.Single(diff);
            Assert.Contains(diff, p => p == "Enums");
        }

        [Fact]
        public void IntListTest_RightSideNullShouldDiff()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItem>();
            item.IntList = fixture.Create<List<int>>();

            var model = new MutationModel<TestItem> { OldImage = item, NewImage = null };
            var diff = model.Diff();
            Assert.Contains(diff, p => p == "IntList");
        }

        [Fact]
        public void IntListTest_ShouldNotDiff()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItem>();
            item.IntList = fixture.Create<List<int>>();

            var json = JsonConvert.SerializeObject(item);
            var item2 = JsonConvert.DeserializeObject<TestItem>(json);

            var model = new MutationModel<TestItem> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();
            Assert.DoesNotContain(diff, p => p == "IntList");
        }

        [Fact]
        public void IntListTest_ShouldDiff()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItem>();
            item.IntList = fixture.Create<List<int>>();

            var json = JsonConvert.SerializeObject(item);
            var item2 = JsonConvert.DeserializeObject<TestItem>(json);
            item2.IntList = fixture.Create<List<int>>();

            var model = new MutationModel<TestItem> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();
            Assert.Single(diff);
            Assert.Contains(diff, p => p == "IntList");
        }

        [Fact]
        public void DoubleListTest_RightSideNullShouldDiff()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItem>();
            item.DoubleList = fixture.Create<List<double>>();

            var model = new MutationModel<TestItem> { OldImage = item, NewImage = null };
            var diff = model.Diff();
            Assert.Contains(diff, p => p == "DoubleList");
        }

        [Fact]
        public void DoubleListTest_ShouldNotDiff()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItem>();
            item.DoubleList = fixture.Create<List<double>>();

            var json = JsonConvert.SerializeObject(item);
            var item2 = JsonConvert.DeserializeObject<TestItem>(json);

            var model = new MutationModel<TestItem> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();
            Assert.DoesNotContain(diff, p => p == "DoubleList");
        }

        [Fact]
        public void DoubleListTest_ShouldDiff()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItem>();
            item.DoubleList = fixture.Create<List<double>>();

            var json = JsonConvert.SerializeObject(item);
            var item2 = JsonConvert.DeserializeObject<TestItem>(json);
            item2.DoubleList = fixture.Create<List<double>>();

            var model = new MutationModel<TestItem> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();
            Assert.Single(diff);
            Assert.Contains(diff, p => p == "DoubleList");
        }

        [Fact]
        public void DateTimeListTest_RightSideNullShouldDiff()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItem>();
            item.DateTimeList = fixture.Create<List<DateTime>>();

            var model = new MutationModel<TestItem> { OldImage = item, NewImage = null };
            var diff = model.Diff();
            Assert.Contains(diff, p => p == "DateTimeList");
        }

        [Fact]
        public void DateTimeListTest_ShouldNotDiff()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItem>();
            item.DateTimeList = fixture.Create<List<DateTime>>();

            var json = JsonConvert.SerializeObject(item);
            var item2 = JsonConvert.DeserializeObject<TestItem>(json);

            var model = new MutationModel<TestItem> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();
            Assert.DoesNotContain(diff, p => p == "DateTimeList");
        }

        [Fact]
        public void DateTimeListTest_ShouldDiff()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItem>();
            item.DateTimeList = fixture.Create<List<DateTime>>();

            var json = JsonConvert.SerializeObject(item);
            var item2 = JsonConvert.DeserializeObject<TestItem>(json);
            item2.DateTimeList = fixture.Create<List<DateTime>>();

            var model = new MutationModel<TestItem> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();
            Assert.Single(diff);
            Assert.Contains(diff, p => p == "DateTimeList");
        }

        [Fact(Skip = "See issue https://github.com/mhlabs/MhLabs.PubSubExtensions/issues/4")]
        public void DynamicTest()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItemDynamic>();
            var json = JsonConvert.SerializeObject(item);
            var item2 = JsonConvert.DeserializeObject<TestItemDynamic>(json);

            item2.DynamicItem = fixture.Create<TestAddress>();

            var model = new MutationModel<TestItemDynamic> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();
            Assert.Equal(3, diff.Count);
            Assert.Contains(diff, p => p == "DynamicItem");
        }

        [Fact]
        public void StringArrayTest()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItemWithStringArray>();

            var json = JsonConvert.SerializeObject(item);
            var item2 = JsonConvert.DeserializeObject<TestItemWithStringArray>(json);

            var model = new MutationModel<TestItemWithStringArray> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();

            Assert.Empty(diff);
        }
    }

    internal class TestItemDynamic
    {
        public dynamic DynamicItem { get; set; }
    }

    internal class TestItem
    {
        public TestEnum TestEnum { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? RemovedDate { get; set; }
        public TestAddress Address { get; set; }
        public List<TestMiniItem> Minis { get; set; }
        public List<TestEnum> Enums { get; set; }
        public List<int> IntList { get; set; }
        public List<double> DoubleList { get; set; }
        public List<DateTime> DateTimeList { get; set; }
        public Dictionary<string, TestMiniItem> MinisDictionary { get; set; }
    }

    internal class TestItemWithStringArray
    {
        public List<string> Strings { get; set; } = new List<string>();
    }

    internal enum TestEnum
    {
        One,
        Two,
        Three
    }

    internal struct TestStruct
    {
        public string Id { get; set; }
        public string Message { get; set; }
    }

    internal class TestMiniItem
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public TestExtraMiniItem[] ExtraMinis { get; set; }
    }

    internal class TestExtraMiniItem
    {
        public string Id { get; set; }
        public string Message { get; set; }
    }

    public class TestAddress
    {
        public string AddressRow1 { get; set; }
    }

    public class TestDateTime
    {
        public DateTime DateTime { get; set; }
        public DateTime? NullableDateTime { get; set; }
    }

}