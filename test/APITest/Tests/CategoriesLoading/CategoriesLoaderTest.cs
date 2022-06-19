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

        private static readonly Category category1;
        private static readonly Category category2;
        private static readonly Category otherIncomeCategory;
        private static readonly Category category4;
        private static readonly Category category5;
        private static readonly Category otherCostCategory;
        
        private static readonly Category existingCategory1;
        private static readonly Category existingCategory2;
        private static readonly Category existingOtherIncomeCategory;
        private static readonly Category existingCategory4;
        private static readonly Category existingCategory5;
        private static readonly Category existingOtherCostCategory;

        static CategoriesLoaderTest()
        {
            category1 = new Category();
            category1.Keyword = "category1";
            category1.IsIncome = true;
            category1.Icon = "icon data 1";
            category1.IconColor = "#654321";

            category2 = new Category();
            category2.Keyword = "category2";
            category2.IsIncome = true;
            category2.Icon = "icon data 2";
            category2.IconColor = "#654321";

            otherIncomeCategory = new Category();
            otherIncomeCategory.Keyword = "otherIncome";
            otherIncomeCategory.IsIncome = true;
            otherIncomeCategory.Icon = "icon data 3";
            otherIncomeCategory.IconColor = "#654321";

            category4 = new Category();
            category4.Keyword = "category4";
            category4.IsIncome = false;
            category4.Icon = "icon data 4";
            category4.IconColor = "#123456";

            category5 = new Category();
            category5.Keyword = "category5";
            category5.IsIncome = false;
            category5.Icon = "icon data 5";
            category5.IconColor = "#123456";

            otherCostCategory = new Category();
            otherCostCategory.Keyword = "otherCost";
            otherCostCategory.IsIncome = false;
            otherCostCategory.Icon = "icon data 6";
            otherCostCategory.IconColor = "#123456";

            existingCategory1 = CopyCategory(category1);
            existingCategory1.Id = Guid.NewGuid();
            existingCategory1.OrderOnList = 0;

            existingCategory2 = CopyCategory(category2);
            existingCategory2.Id = Guid.NewGuid();
            existingCategory2.OrderOnList = 1;

            existingOtherIncomeCategory = CopyCategory(otherIncomeCategory);
            existingOtherIncomeCategory.Id = Guid.NewGuid();
            existingOtherIncomeCategory.OrderOnList = 2;

            existingCategory4 = CopyCategory(category4);
            existingCategory4.Id = Guid.NewGuid();
            existingCategory4.OrderOnList = 3;

            existingCategory5 = CopyCategory(category5);
            existingCategory5.Id = Guid.NewGuid();
            existingCategory5.OrderOnList = 4;

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

            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, category1))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, category2))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, otherIncomeCategory))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, category4))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, category5))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, otherCostCategory))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.UpdateAsync(It.IsAny<Category>()), Times.Never);
            categoryRepositoryMock.Verify(cr => cr.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task LoadAsync_WithTheSameData()
        {
            categoryRepositoryMock.Setup(cr => cr.GetAllAsync()).ReturnsAsync(new List<Category>()
            {
                existingCategory1,
                existingCategory2,
                existingOtherIncomeCategory,
                existingCategory4,
                existingCategory5,
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
            categoryRepositoryMock.Verify(cr => cr.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task LoadAsync_WithUpdatedData()
        {
            var existingCategory7 = new Category();
            existingCategory7.Id = Guid.NewGuid();
            existingCategory7.OrderOnList = 1;
            existingCategory7.Keyword = "category7";
            existingCategory7.IsIncome = true;
            existingCategory7.Icon = "icon data 7";
            existingCategory7.IconColor = "#654321";

            var existingCategory8 = new Category();
            existingCategory8.Id = Guid.NewGuid();
            existingCategory8.OrderOnList = 4;
            existingCategory8.Keyword = "category8";
            existingCategory8.IsIncome = false;
            existingCategory8.Icon = "icon data 8";
            existingCategory8.IconColor = "#123456";

            var changedExistingCategory1 = CopyCategory(existingCategory1);
            changedExistingCategory1.IconColor = "#987654";
            var changedExistingCategory4 = CopyCategory(existingCategory4);
            changedExistingCategory4.OrderOnList = 4;

            var existingEntry1 = new Entry();
            existingEntry1.Id = Guid.NewGuid();
            existingEntry1.CategoryId = existingCategory7.Id;
            existingEntry1.UserId = Guid.NewGuid();
            existingEntry1.Date = DateTime.UtcNow;
            existingEntry1.Name = "entry name 1";
            existingEntry1.Value = 15;

            var existingEntry2 = new Entry();
            existingEntry2.Id = Guid.NewGuid();
            existingEntry2.CategoryId = existingCategory8.Id;
            existingEntry2.UserId = Guid.NewGuid();
            existingEntry2.Date = DateTime.UtcNow;
            existingEntry2.Name = "entry name 2";
            existingEntry2.Value = 15;

            categoryRepositoryMock.Setup(cr => cr.GetAllAsync()).ReturnsAsync(new List<Category>()
            {
                changedExistingCategory1,
                existingCategory7,
                existingOtherIncomeCategory,
                existingCategory8,
                changedExistingCategory4,
                existingOtherCostCategory
            });
            entryRepositoryMock.Setup(er => er.GetAllByCategoryIdAsync(existingCategory7.Id)).ReturnsAsync(new List<Entry>()
            {
                existingEntry1
            });
            entryRepositoryMock.Setup(er => er.GetAllByCategoryIdAsync(existingCategory8.Id)).ReturnsAsync(new List<Entry>()
            {
                existingEntry2
            });

            await categoriesLoader.LoadAsync(
                loggerMock.Object,
                configuration,
                categoryRepositoryMock.Object,
                entryRepositoryMock.Object
                );

            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, category2))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.AddAsync(It.Is<Category>(c => CategoriesMatcher(c, category5))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.UpdateAsync(It.Is<Category>(c => CategoriesMatcher(c, category1))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.UpdateAsync(It.Is<Category>(c => CategoriesMatcher(c, category4))), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.DeleteAsync(existingCategory7.Id), Times.Once);
            categoryRepositoryMock.Verify(cr => cr.DeleteAsync(existingCategory8.Id), Times.Once);

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
            categoryRepositoryMock.Verify(cr => cr.DeleteAsync(It.IsAny<Guid>()), Times.Never);
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
