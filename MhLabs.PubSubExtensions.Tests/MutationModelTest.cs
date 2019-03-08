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
            Assert.Equal(0, model.Diff().Count);
        }

        [Fact]
        public void NullProperties()
        {
            var obj1 = new TestItem { Name = null, Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = null, Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.Equal(0, model.Diff().Count);
        }
        [Fact]
        public void LHSPropertyNull()
        {
            var obj1 = new TestItem { Name = null, Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.Equal(1, model.Diff().Count);
        }
        [Fact]
        public void RHSPropertiesNull()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = null, Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.Equal(1, model.Diff().Count);
        }

        [Fact]
        public void NullToLeft()
        {
            TestItem obj1 = null;
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.NotEqual(0, model.Diff().Count);
        }
        [Fact]
        public void NullToRight()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            TestItem obj2 = null;
            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.NotEqual(0, model.Diff().Count);
        }

        [Fact]
        public void RootItemChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test2", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            Assert.Equal(1, model.Diff().Count);
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
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
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
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
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
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
            Assert.True(diff.Any(p => p == "Minis"));
            Assert.True(diff.Any(p => p == "Minis.Id"));
            Assert.True(diff.Any(p => p == "Minis.Message"));
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
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
            Assert.True(diff.Any(p => p == "Minis"));
            Assert.True(diff.Any(p => p == "Minis.Id"));
            Assert.True(diff.Any(p => p == "Minis.Message"));
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
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
            Assert.True(diff.Any(p => p == "Minis"));
            Assert.True(diff.Any(p => p == "Minis.Id"));
            Assert.True(diff.Any(p => p == "Minis.Message"));

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
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
            Assert.True(diff.Any(p => p == "Minis"));
            Assert.True(diff.Any(p => p == "Minis.Id"));
            Assert.True(diff.Any(p => p == "Minis.Message"));
        }

        [Fact]
        public void NestedDictionaryNotChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();
            var minisDictionary = new Dictionary<string, TestMiniItem>(fixture.CreateMany<KeyValuePair<string, TestMiniItem>>());

            obj1.MinisDictionary = minisDictionary;
            obj2.MinisDictionary = minisDictionary;

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(2, diff.Count);
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
        }

        [Fact]
        public void NestedDictionaryQuantityChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();
            var minisDictionary = new Dictionary<string, TestMiniItem>(fixture.CreateMany<KeyValuePair<string, TestMiniItem>>());

            obj1.MinisDictionary = minisDictionary;

            obj2.MinisDictionary = new Dictionary<string, TestMiniItem>(minisDictionary);
            obj2.MinisDictionary.Remove(obj2.MinisDictionary.First().Key);

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(5, diff.Count);
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
            Assert.True(diff.Any(p => p == "MinisDictionary"));
            Assert.True(diff.Any(p => p == "MinisDictionary.Id"));
            Assert.True(diff.Any(p => p == "MinisDictionary.Message"));
        }

        [Fact]
        public void NestedDictionaryContentChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();
            var minisDictionary = new Dictionary<string, TestMiniItem>(fixture.CreateMany<KeyValuePair<string, TestMiniItem>>());
            var minisDictionary2 = new Dictionary<string, TestMiniItem>(fixture.CreateMany<KeyValuePair<string, TestMiniItem>>());

            obj1.MinisDictionary = minisDictionary;
            obj2.MinisDictionary = minisDictionary2;

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(5, diff.Count);
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
            Assert.True(diff.Any(p => p == "MinisDictionary"));
            Assert.True(diff.Any(p => p == "MinisDictionary.Id"));
            Assert.True(diff.Any(p => p == "MinisDictionary.Message"));

        }

        [Fact]
        public void NestedDictionaryNullToLeft()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();


            obj1.MinisDictionary = null;
            obj2.MinisDictionary = new Dictionary<string, TestMiniItem>(fixture.CreateMany<KeyValuePair<string, TestMiniItem>>());

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(5, diff.Count);
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
            Assert.True(diff.Any(p => p == "MinisDictionary"));
            Assert.True(diff.Any(p => p == "MinisDictionary.Id"));
            Assert.True(diff.Any(p => p == "MinisDictionary.Message"));

        }
        [Fact]
        public void NestedDictionaryNullToRight()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();

            obj1.MinisDictionary = new Dictionary<string, TestMiniItem>(fixture.CreateMany<KeyValuePair<string, TestMiniItem>>());
            obj2.MinisDictionary = null;

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(5, diff.Count);
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
            Assert.True(diff.Any(p => p == "MinisDictionary"));
            Assert.True(diff.Any(p => p == "MinisDictionary.Id"));
            Assert.True(diff.Any(p => p == "MinisDictionary.Message"));

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
            Assert.Equal(1, diff.Count);
            Assert.True(diff.Any(p => p == "TestEnum"));
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

        [Fact(Skip = "See issue https://github.com/mhlabs/MhLabs.PubSubExtensions/issues/6")]
        public void EnumListTest()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItem>();
            item.Enums = fixture.Create<List<TestEnum>>();

            var json = JsonConvert.SerializeObject(item);
            var item2 = JsonConvert.DeserializeObject<TestItem>(json);
            
            item2.Enums = fixture.Create<List<TestEnum>>();

            var model = new MutationModel<TestItem> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();
            Assert.Equal(1, diff.Count);
            Assert.True(diff.Any(p => p == "TestEnum"));

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
            Assert.True(diff.Any(p => p == "DynamicItem"));

        }

        [Fact(Skip = ("See issue https://github.com/mhlabs/MhLabs.PubSubExtensions/issues/8"))]
        public void StringArrayTest()
        {
            var fixture = new Fixture();
            var item = fixture.Create<TestItemWithStringArray>();

            var json = JsonConvert.SerializeObject(item);
            var item2 = JsonConvert.DeserializeObject<TestItemWithStringArray>(json);

            var model = new MutationModel<TestItemWithStringArray> { OldImage = item, NewImage = item2 };
            var diff = model.Diff();

            Assert.Equal(0, diff.Count);
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
        public TestAddress Address { get; set; }
        public List<TestMiniItem> Minis { get; set; }
        public List<TestEnum> Enums { get; set; }
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