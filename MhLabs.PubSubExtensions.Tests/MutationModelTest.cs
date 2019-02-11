using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using MhLabs.PubSubExtensions.Model;
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
            Assert.Equal(3, diff.Count);
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
            Assert.True(diff.Any(p => p == "Minis"));
        }

        [Fact]
        public void NestedListContentChanged()
        {
            var obj1 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "B" } };
            var obj2 = new TestItem { Name = "Test", Age = 10, CreationDate = DateTime.Today, Address = new TestAddress { AddressRow1 = "C" } };

            var fixture = new Fixture();
            var minis = fixture.CreateMany<TestMiniItem>(10).ToList();

            obj1.Minis = minis;
            obj2.Minis = minis.Skip(1).ToList(); 
            obj2.Minis.Add(fixture.Create<TestMiniItem>());

            var model = new MutationModel<TestItem> { OldImage = obj1, NewImage = obj2 };
            var diff = model.Diff();
            Assert.Equal(3, diff.Count);
            Assert.True(diff.Any(p => p == "Address"));
            Assert.True(diff.Any(p => p == "Address.AddressRow1"));
            Assert.True(diff.Any(p => p == "Minis"));
        }
    }

    internal class TestItem
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime CreationDate { get; set; }
        public TestAddress Address { get; set; }
        public List<TestMiniItem> Minis { get; set; }
    }

    internal class TestMiniItem
    {
        public string Id { get; set; }
        public string Message { get; set; }
    }

    public class TestAddress
    {
        public string AddressRow1 { get; set; }
    }

}