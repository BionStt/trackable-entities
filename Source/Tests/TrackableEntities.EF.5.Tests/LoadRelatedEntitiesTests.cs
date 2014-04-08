﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using NUnit.Framework;
using TrackableEntities.EF.Tests.Contexts;
#if EF_6
using TrackableEntities.EF6;
#else
using TrackableEntities.EF5;
#endif
using TrackableEntities.EF.Tests;
using TrackableEntities.EF.Tests.Mocks;
using TrackableEntities.EF.Tests.NorthwindModels;

#if EF_6
namespace TrackableEntities.EF6.Tests
#else
namespace TrackableEntities.EF5.Tests
#endif
{
    [TestFixture]
    public class LoadRelatedEntitiesTests
    {
        private const string TestCustomerId1 = "AAAA";
        private const string TestCustomerId2 = "BBBB";
        private const string TestTerritoryId1 = "11111";
        private const string TestTerritoryId2 = "22222";
        private const string TestTerritoryId3 = "33333";
        private const CreateDbOptions CreateNorthwindDbOptions = CreateDbOptions.DropCreateDatabaseIfModelChanges;

        #region Setup

        [TestFixtureSetUp]
        public void Init()
        {
            // Make sure that reference data exists for tables with non-idendity keys
            using (var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions))
            {
                // Test Customers
                EnsureTestCustomer(context, TestCustomerId1, TestTerritoryId1);
                EnsureTestCustomer(context, TestCustomerId2, TestTerritoryId2);

                // Test Customer Settings
                EnsureTestCustomerSetting(context, TestCustomerId1);
                EnsureTestCustomerSetting(context, TestCustomerId2);

                // Test Territories
                EnsureTestTerritory(context, TestTerritoryId1);
                EnsureTestTerritory(context, TestTerritoryId2);
                EnsureTestTerritory(context, TestTerritoryId3);

                // Save changes
                context.SaveChanges();
            }
        }

        private static void EnsureTestTerritory(NorthwindDbContext context, string territoryId)
        {
            var territory = context.Territories
                .SingleOrDefault(t => t.TerritoryId == territoryId);
            if (territory == null)
            {
                territory = new Territory { TerritoryId = territoryId, TerritoryDescription = "Test Territory" };
                context.Territories.Add(territory);
            }
        }

        private static void EnsureTestCustomer(NorthwindDbContext context, string customerId, string territoryId)
        {
            var customer = context.Customers
                .SingleOrDefault(c => c.CustomerId == customerId);
            if (customer == null)
            {
                customer = new Customer
                {
                    CustomerId = customerId, 
                    CustomerName = "Test Customer " + customerId,
                    TerritoryId = territoryId
                };
                context.Customers.Add(customer);
            }
        }

        private static void EnsureTestCustomerSetting(NorthwindDbContext context, string customerId)
        {
            var setting = context.CustomerSettings
                .SingleOrDefault(c => c.CustomerId == customerId);
            if (setting == null)
            {
                setting = new CustomerSetting { CustomerId = customerId, Setting = "Setting1" };
                context.CustomerSettings.Add(setting);
            }
        }

        private List<Order> CreateTestOrders(NorthwindDbContext context)
        {
            // Create test entities
            var category1 = new Category
            {
                CategoryName = "Test Category 1"
            };
            var category2 = new Category
            {
                CategoryName = "Test Category 2"
            };
            var product1 = new Product
            {
                ProductName = "Test Product 1",
                UnitPrice = 10M,
                Category = category1
            };
            var product2 = new Product
            {
                ProductName = "Test Product 2",
                UnitPrice = 20M,
                Category = category2
            };
            var product3 = new Product
            {
                ProductName = "Test Product 3",
                UnitPrice = 30M,
                Category = category2
            };
            var customer1 = context.Customers
                .Include(c => c.Territory)
                .Include(c => c.CustomerSetting)
                .Single(c => c.CustomerId == TestCustomerId1);
            var customer2 = context.Customers
                .Include(c => c.Territory)
                .Include(c => c.CustomerSetting)
                .Single(c => c.CustomerId == TestCustomerId1);
            var detail1 = new OrderDetail {Product = product1, Quantity = 11, UnitPrice = 11M};
            var detail2 = new OrderDetail {Product = product2, Quantity = 12, UnitPrice = 12M};
            var detail3 = new OrderDetail {Product = product2, Quantity = 13, UnitPrice = 13M};
            var detail4 = new OrderDetail {Product = product3, Quantity = 14, UnitPrice = 14M};
            var order1 = new Order
            {
                OrderDate = DateTime.Today,
                Customer = customer1,
                OrderDetails = new List<OrderDetail>
                {
                    detail1,
                    detail2,
                }
            };
            var order2 = new Order
            {
                OrderDate = DateTime.Today,
                Customer = customer2,
                OrderDetails = new List<OrderDetail>
                {
                    detail3,
                    detail4,
                }
            };

            // Persist entities
            context.Orders.Add(order1);
            context.Orders.Add(order2);
            context.SaveChanges();

            // Detach entities
            var objContext = ((IObjectContextAdapter) context).ObjectContext;
            objContext.Detach(order1);
            objContext.Detach(order2);

            // Clear reference properties
            product1.Category = null;
            product2.Category = null;
            product3.Category = null;
            customer1.Territory = null;
            customer2.Territory = null;
            customer1.CustomerSetting = null;
            customer2.CustomerSetting = null;
            detail1.Product = null;
            detail2.Product = null;
            detail3.Product = null;
            detail4.Product = null;
            order1.OrderDetails = new List<OrderDetail> { detail1, detail2 };
            order1.OrderDetails = new List<OrderDetail> { detail3, detail4 };

            // Return orders
            return new List<Order> {order1, order2};
        }

        private List<Employee> CreateTestEmployees(NorthwindDbContext context)
        {
            // Create test entities
            var area1 = new Area { AreaName = "Northern" };
            var area2 = new Area { AreaName = "Southern" };
            var territory1 = context.Territories.Single(t => t.TerritoryId == TestTerritoryId1);
            var territory2 = context.Territories.Single(t => t.TerritoryId == TestTerritoryId2);
            var territory3 = context.Territories.Single(t => t.TerritoryId == TestTerritoryId3);
            territory1.Area = area1;
            territory2.Area = area2;
            territory3.Area = area2;
            var employee1 = new Employee
            {
                FirstName = "Test",
                LastName = "Employee1",
                City = "San Fransicso",
                Country = "USA",
                Territories = new List<Territory> { territory1, territory2 }
            };
            var employee2 = new Employee
            {
                FirstName = "Test",
                LastName = "Employee2",
                City = "Sacramento",
                Country = "USA",
                Territories = new List<Territory> { territory2, territory3 }
            };

            // Persist entities
            context.Employees.Add(employee1);
            context.Employees.Add(employee2);
            context.SaveChanges();

            // Detach entities
            var objContext = ((IObjectContextAdapter)context).ObjectContext;
            objContext.Detach(employee1);
            objContext.Detach(employee2);

            // Clear reference properties
            territory1.Area = null;
            territory2.Area = null;
            territory3.Area = null;
            employee1.Territories = new List<Territory> { territory1, territory2 };
            employee2.Territories = new List<Territory> { territory2, territory3 };

            // Return employees
            return new List<Employee> { employee1, employee2 };
        }

        #endregion

        #region Order-Customer: Many-to-One

        [Test]
        public void LoadRelatedEntities_Should_Populate_Order_With_Customer()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var order = CreateTestOrders(context)[0];

            // Act
            context.LoadRelatedEntities(order);

            // Assert
            Assert.IsNotNull(order.Customer);
            Assert.AreEqual(order.CustomerId, order.Customer.CustomerId);
        }

        [Test]
        public void LoadRelatedEntities_Should_Populate_Multiple_Orders_With_Customer()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var orders = CreateTestOrders(context);

            // Act
            context.LoadRelatedEntities(orders);

            // Assert
            Assert.IsFalse(orders.Any(o => o.Customer == null));
            Assert.IsFalse(orders.Any(o => o.Customer.CustomerId != o.CustomerId));
        }

        [Test]
        public void LoadRelatedEntities_Should_Populate_Order_With_Customer_With_Territory()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var order = CreateTestOrders(context)[0];

            // Act
            context.LoadRelatedEntities(order);

            // Assert
            Assert.IsNotNull(order.Customer.Territory);
            Assert.AreEqual(order.Customer.TerritoryId, order.Customer.Territory.TerritoryId);
        }

        [Test]
        public void LoadRelatedEntities_Should_Populate_Order_With_Customer_With_Setting()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var order = CreateTestOrders(context)[0];

            // Act
            context.LoadRelatedEntities(order);

            // Assert
            Assert.IsNotNull(order.Customer.CustomerSetting);
            Assert.AreEqual(order.Customer.CustomerId, order.Customer.CustomerSetting.CustomerId);
        }

#if EF_6
        
        [Test]
        public async void LoadRelatedEntitiesAsync_Should_Populate_Order_With_Customer()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var order = CreateTestOrders(context)[0];

            // Act
            await context.LoadRelatedEntitiesAsync(order);

            // Assert
            Assert.IsNotNull(order.Customer);
            Assert.AreEqual(order.CustomerId, order.Customer.CustomerId);
        }

        [Test]
        public async void LoadRelatedEntitiesAsync_Should_Populate_Multiple_Orders_With_Customer()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var orders = CreateTestOrders(context);

            // Act
            await context.LoadRelatedEntitiesAsync(orders);

            // Assert
            Assert.IsFalse(orders.Any(o => o.Customer == null));
            Assert.IsFalse(orders.Any(o => o.Customer.CustomerId != o.CustomerId));
        }

        [Test]
        public async void LoadRelatedEntitiesAsync_Should_Populate_Order_With_Customer_With_Territory()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var order = CreateTestOrders(context)[0];

            // Act
            await context.LoadRelatedEntitiesAsync(order);

            // Assert
            Assert.IsNotNull(order.Customer.Territory);
            Assert.AreEqual(order.Customer.TerritoryId, order.Customer.Territory.TerritoryId);
        }

        [Test]
        public async void LoadRelatedEntitiesAsync_Should_Populate_Order_With_Customer_With_Setting()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var order = CreateTestOrders(context)[0];

            // Act
            await context.LoadRelatedEntitiesAsync(order);

            // Assert
            Assert.IsNotNull(order.Customer.CustomerSetting);
            Assert.AreEqual(order.Customer.CustomerId, order.Customer.CustomerSetting.CustomerId);
        }
#endif

        #endregion

        #region Order-OrderDetail-Product-Category: One-to-Many-to-One

        [Test]
        public void LoadRelatedEntities_Should_Populate_Order_Details_With_Product_With_Category()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var order = CreateTestOrders(context)[0];

            // Act
            context.LoadRelatedEntities(order);

            // Assert
            var details = order.OrderDetails;
            Assert.IsFalse(details.Any(d => d.Product == null));
            Assert.IsFalse(details.Any(d => d.Product.ProductId != d.ProductId));
            Assert.IsFalse(details.Any(d => d.Product.Category == null));
            Assert.IsFalse(details.Any(d => d.Product.Category.CategoryId != d.Product.CategoryId));
        }

        [Test]
        public void LoadRelatedEntities_Should_Populate_Multiple_Orders_Details_With_Product_With_Category()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var orders = CreateTestOrders(context);

            // Act
            context.LoadRelatedEntities(orders);

            // Assert
            var details1 = orders[0].OrderDetails;
            var details2 = orders[1].OrderDetails;
            var allDetails = orders.SelectMany(o => o.OrderDetails).ToList();
            Assert.IsFalse(allDetails.Any(d => d.Product == null));
            Assert.IsFalse(allDetails.Any(d => d.Product.Category == null));
            Assert.IsFalse(details1.Any(d => d.Product.ProductId != d.ProductId));
            Assert.IsFalse(details2.Any(d => d.Product.Category.CategoryId != d.Product.CategoryId));
        }

#if EF_6
        
        [Test]
        public async void LoadRelatedEntitiesAsync_Should_Populate_Order_Details_With_Product_With_Category()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var order = CreateTestOrders(context)[0];

            // Act
            await context.LoadRelatedEntitiesAsync(order);

            // Assert
            var details = order.OrderDetails;
            Assert.IsFalse(details.Any(d => d.Product == null));
            Assert.IsFalse(details.Any(d => d.Product.ProductId != d.ProductId));
            Assert.IsFalse(details.Any(d => d.Product.Category == null));
            Assert.IsFalse(details.Any(d => d.Product.Category.CategoryId != d.Product.CategoryId));
        }

        [Test]
        public async void LoadRelatedEntitiesAsync_Should_Populate_Multiple_Orders_Details_With_Product_With_Category()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var orders = CreateTestOrders(context);

            // Act
            await context.LoadRelatedEntitiesAsync(orders);

            // Assert
            var details1 = orders[0].OrderDetails;
            var details2 = orders[1].OrderDetails;
            var allDetails = orders.SelectMany(o => o.OrderDetails).ToList();
            Assert.IsFalse(allDetails.Any(d => d.Product == null));
            Assert.IsFalse(allDetails.Any(d => d.Product.Category == null));
            Assert.IsFalse(details1.Any(d => d.Product.ProductId != d.ProductId));
            Assert.IsFalse(details2.Any(d => d.Product.Category.CategoryId != d.Product.CategoryId));
        }

#endif
        
        #endregion

        #region Employee-Territory-Area: Many-to-Many-to-One

        [Test]
        public void LoadRelatedEntities_Should_Populate_Employee_Territories_With_Area()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var employee = CreateTestEmployees(context)[0];

            // Act
            context.LoadRelatedEntities(employee);

            // Assert
            Assert.IsFalse(employee.Territories.Any(t => t.Area == null));
            Assert.IsFalse(employee.Territories.Any(t => t.AreaId != t.Area.AreaId));
        }

        [Test]
        public void LoadRelatedEntities_Should_Populate_Multiple_Employees_Territories_With_Area()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var employees = CreateTestEmployees(context);

            // Act
            context.LoadRelatedEntities(employees);

            // Assert
            Assert.IsFalse(employees[0].Territories.Any(t => t.Area == null));
            Assert.IsFalse(employees[0].Territories.Any(t => t.AreaId != t.Area.AreaId));
            Assert.IsFalse(employees[1].Territories.Any(t => t.Area == null));
            Assert.IsFalse(employees[1].Territories.Any(t => t.AreaId != t.Area.AreaId));
        }

#if EF_6
        
        [Test]
        public async void LoadRelatedEntitiesAsync_Should_Populate_Employee_Territories_With_Area()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var employee = CreateTestEmployees(context)[0];

            // Act
            await context.LoadRelatedEntitiesAsync(employee);

            // Assert
            Assert.IsFalse(employee.Territories.Any(t => t.Area == null));
            Assert.IsFalse(employee.Territories.Any(t => t.AreaId != t.Area.AreaId));
        }

        [Test]
        public async void LoadRelatedEntitiesAsync_Should_Populate_Multiple_Employees_Territories_With_Area()
        {
            // Arrange
            var context = TestsHelper.CreateNorthwindDbContext(CreateNorthwindDbOptions);
            var employees = CreateTestEmployees(context);

            // Act
            await context.LoadRelatedEntitiesAsync(employees);

            // Assert
            Assert.IsFalse(employees[0].Territories.Any(t => t.Area == null));
            Assert.IsFalse(employees[0].Territories.Any(t => t.AreaId != t.Area.AreaId));
            Assert.IsFalse(employees[1].Territories.Any(t => t.Area == null));
            Assert.IsFalse(employees[1].Territories.Any(t => t.AreaId != t.Area.AreaId));
        }

#endif

        #endregion
    }
}
