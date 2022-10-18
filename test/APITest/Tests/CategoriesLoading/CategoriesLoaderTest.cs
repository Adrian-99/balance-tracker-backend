using Application;
using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITest.Tests.CategoriesLoading
{
    public class CategoriesLoaderTest
    {
        private Mock<ILogger<CategoriesLoader>> loggerMock;
        private IConfiguration configuration;
        private Mock<ICategoryRepository> categoryRepositoryMock;
        private Mock<IEntryRepository> entryRepositoryMock;
        private CategoriesLoader categoriesLoader;

        private static readonly Category incomeCategory1;
        private static readonly Category incomeCategory2;
        private static readonly Category otherIncomeCategory;
        private static readonly Category costCategory1;
        private static readonly Category costCategory2;
        private static readonly Category otherCostCategory;
        
        private static readonly Category existingIncomeCategory1;
        private static readonly Category existingIncomeCategory2;
        private static readonly Category existingOtherIncomeCategory;
        private static readonly Category existingCostCategory1;
        private static readonly Category existingCostCategory2;
        private static readonly Category existingOtherCostCategory;

        static CategoriesLoaderTest()
        {
            incomeCategory1 = new Category();
            incomeCategory1.Keyword = "incomeCategory1";
            incomeCategory1.IsIncome = true;
            incomeCategory1.Icon = "icon data 1";
            incomeCategory1.IconColor = "#654321";

            incomeCategory2 = new Category();
            incomeCategory2.Keyword = "incomeCategory2";
            incomeCategory2.IsIncome = true;
            incomeCategory2.Icon = "icon data 2";
            incomeCategory2.IconColor = "#654321";

            otherIncomeCategory = new Category();
            otherIncomeCategory.Keyword = "otherIncome";
            otherIncomeCategory.IsIncome = true;
            otherIncomeCategory.Icon = "icon data 3";
            otherIncomeCategory.IconColor = "#654321";

            costCategory1 = new Category();
            costCategory1.Keyword = "costCategory1";
            costCategory1.IsIncome = false;
            costCategory1.Icon = "icon data 4";
            costCategory1.IconColor = "#123456";

            costCategory2 = new Category();
            costCategory2.Keyword = "costCategory2";
            costCategory2.IsIncome = false;
            costCategory2.Icon = "icon data 5";
            costCategory2.IconColor = "#123456";

            otherCostCategory = new Category();
            otherCostCategory.Keyword = "otherCost";
            otherCostCategory.IsIncome = false;
            otherCostCategory.Icon = "icon data 6";
            otherCostCategory.IconColor = "#123456";

            existingIncomeCategory1 = CopyCategory(incomeCategory1);
            existingIncomeCategory1.Id = Guid.NewGuid();
            existingIncomeCategory1.OrderOnList = 0;

            existingIncomeCategory2 = CopyCategory(incomeCategory2);
            existingIncomeCategory2.Id = Guid.NewGuid();
            existingIncomeCategory2.OrderOnList = 1;

            existingOtherIncomeCategory = CopyCategory(otherIncomeCategory);
            existingOtherIncomeCategory.Id = Guid.NewGuid();
            existingOtherIncomeCategory.OrderOnList = 2;

            existingCostCategory1 = CopyCategory(costCategory1);
            existingCostCategory1.Id = Guid.NewGuid();
            existingCostCategory1.OrderOnList = 3;

            existingCostCategory2 = CopyCategory(costCategory2);
            existingCostCategory2.Id = Guid.NewGuid();
            existingCostCategory2.OrderOnList = 4;

            existingOtherCostCategory = CopyCategory(otherCostCategory);
            existingOtherCostCategory.Id = Guid.NewGuid();
            existingOtherCostCategory.OrderOnList = 5;
        }

        [SetUp]
        public void Setup()
        {
            loggerMock = new Mock<ILogger<CategoriesLoader>>();
            categoryRepositoryMock = new Mock<ICategoryRepository>();
            entryRepositoryMock = new Mock<IEntryRepository>();
            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();
            categoriesLoader = new CategoriesLoader();
        }

        [Test]
        public async Task LoadAsync_WithNewData()
        {
            categoryRepositoryMock.Setup(cr => cr.GetAllAsync()).ReturnsAsync(new List<Category>());
            entryRepositoryMock.Setup(er => er.GetAllByCategoryIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Entry>());

            await categoriesLoader.LoadAsync(
                loggerMock.Object,
                configuration,
                categoryRepositoryMock.Object,
                entryRepositoryMock.Object
                );

            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, incomeCategory1))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, incomeCategory2))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, otherIncomeCategory))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, costCategory1))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, costCategory2))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, otherCostCategory))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.UpdateAsync(It.IsAny<Category>()), Times.Never);
            categoryRepositoryMock.Verify(cr => cr.DeleteAsync(It.IsAny<Category>()), Times.Never);
        }

        [Test]
        public async Task LoadAsync_WithTheSameData()
        {
            categoryRepositoryMock.Setup(cr => cr.GetAllAsync()).ReturnsAsync(new List<Category>()
            {
                existingIncomeCategory1,
                existingIncomeCategory2,
                existingOtherIncomeCategory,
                existingCostCategory1,
                existingCostCategory2,
                existingOtherCostCategory
            });
            entryRepositoryMock.Setup(er => er.GetAllByCategoryIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Entry>());

            await categoriesLoader.LoadAsync(
                loggerMock.Object,
                configuration,
                categoryRepositoryMock.Object,
                entryRepositoryMock.Object
                );

            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.IsAny<Category>()), Times.Never);
            categoryRepositoryMock.Verify(cr => cr.UpdateAsync(It.IsAny<Category>()), Times.Never);
            categoryRepositoryMock.Verify(cr => cr.DeleteAsync(It.IsAny<Category>()), Times.Never);
        }

        [Test]
        public async Task LoadAsync_WithUpdatedData()
        {
            var existingIncomeCategory3 = new Category();
            existingIncomeCategory3.Id = Guid.NewGuid();
            existingIncomeCategory3.OrderOnList = 1;
            existingIncomeCategory3.Keyword = "incomeCategory3";
            existingIncomeCategory3.IsIncome = true;
            existingIncomeCategory3.Icon = "icon data 7";
            existingIncomeCategory3.IconColor = "#654321";

            var existingCostCategory3 = new Category();
            existingCostCategory3.Id = Guid.NewGuid();
            existingCostCategory3.OrderOnList = 4;
            existingCostCategory3.Keyword = "costCategory3";
            existingCostCategory3.IsIncome = false;
            existingCostCategory3.Icon = "icon data 8";
            existingCostCategory3.IconColor = "#123456";

            var changedExistingIncomeCategory1 = CopyCategory(existingIncomeCategory1);
            changedExistingIncomeCategory1.IconColor = "#987654";
            var changedExistingCostCategory1 = CopyCategory(existingCostCategory1);
            changedExistingCostCategory1.OrderOnList = 4;

            var existingEntry1 = new Entry();
            existingEntry1.Id = Guid.NewGuid();
            existingEntry1.CategoryId = existingIncomeCategory3.Id;
            existingEntry1.UserId = Guid.NewGuid();
            existingEntry1.Date = DateTime.UtcNow;
            existingEntry1.Name = "entry name 1";
            existingEntry1.Value = 15;

            var existingEntry2 = new Entry();
            existingEntry2.Id = Guid.NewGuid();
            existingEntry2.CategoryId = existingCostCategory3.Id;
            existingEntry2.UserId = Guid.NewGuid();
            existingEntry2.Date = DateTime.UtcNow;
            existingEntry2.Name = "entry name 2";
            existingEntry2.Value = 15;

            categoryRepositoryMock.Setup(cr => cr.GetAllAsync()).ReturnsAsync(new List<Category>()
            {
                changedExistingIncomeCategory1,
                existingIncomeCategory3,
                existingOtherIncomeCategory,
                existingCostCategory3,
                changedExistingCostCategory1,
                existingOtherCostCategory
            });
            entryRepositoryMock.Setup(er => er.GetAllByCategoryIdAsync(existingIncomeCategory3.Id)).ReturnsAsync(new List<Entry>()
            {
                existingEntry1
            });
            entryRepositoryMock.Setup(er => er.GetAllByCategoryIdAsync(existingCostCategory3.Id)).ReturnsAsync(new List<Entry>()
            {
                existingEntry2
            });

            await categoriesLoader.LoadAsync(
                loggerMock.Object,
                configuration,
                categoryRepositoryMock.Object,
                entryRepositoryMock.Object
                );

            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, incomeCategory2))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, costCategory2))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.UpdateAsync(It.Is<Category>(c => CategoriesMatcher(c, incomeCategory1))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.UpdateAsync(It.Is<Category>(c => CategoriesMatcher(c, costCategory1))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.DeleteAsync(existingIncomeCategory3), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.DeleteAsync(existingCostCategory3), Times.Once);

            entryRepositoryMock.Verify(er => er.UpdateAsync(It.Is<Entry>(
                e => e.Id.Equals(existingEntry1.Id) && e.CategoryId.Equals(existingOtherIncomeCategory.Id)
                )), Times.Once);
            entryRepositoryMock.Verify(er => er.UpdateAsync(It.Is<Entry>(
                e => e.Id.Equals(existingEntry2.Id) && e.CategoryId.Equals(existingOtherCostCategory.Id)
                )), Times.Once);
        }

        [Test]
        public void LoadAsync_WithIncorrectData()
        {
            configuration["Data:CategoriesListPath"] = "TestResources/Data/CategoriesIncorrect.json";

            categoryRepositoryMock.Setup(cr => cr.GetAllAsync()).ReturnsAsync(new List<Category>());
            entryRepositoryMock.Setup(er => er.GetAllByCategoryIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Entry>());

            Assert.ThrowsAsync<CategoriesLoaderException>( 
                () => categoriesLoader.LoadAsync(
                    loggerMock.Object,
                    configuration,
                    categoryRepositoryMock.Object,
                    entryRepositoryMock.Object
                    ));

            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.IsAny<Category>()), Times.Never);
            categoryRepositoryMock.Verify(cr => cr.UpdateAsync(It.IsAny<Category>()), Times.Never);
            categoryRepositoryMock.Verify(cr => cr.DeleteAsync(It.IsAny<Category>()), Times.Never);
        }

        private bool CategoriesMatcher(Category category1, Category category2)
        {
            return category1.Keyword.Equals(category2.Keyword) &&
                category1.IsIncome == category2.IsIncome &&
                category1.Icon.Equals(category2.Icon) &&
                category1.IconColor.Equals(category2.IconColor);
        }

        private static Category CopyCategory(Category toCopy)
        {
            var copy = new Category();
            copy.Id = toCopy.Id;
            copy.OrderOnList = toCopy.OrderOnList;
            copy.Keyword = toCopy.Keyword;
            copy.IsIncome = toCopy.IsIncome;
            copy.Icon = toCopy.Icon;
            copy.IconColor = toCopy.IconColor;
            return copy;
        }
    }
}
