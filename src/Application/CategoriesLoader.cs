using Application.Exceptions;
using Application.Settings;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public class CategoriesLoader
    {
        private ManualResetEventSlim loadingState = new ManualResetEventSlim(false);

        public async Task LoadAsync(ILogger<CategoriesLoader> logger,
                                    IConfiguration configuration,
                                    ICategoryRepository categoryRepository,
                                    IEntryRepository entryRepository)
        {
            logger.LogInformation("Starting categories loading...");
            loadingState.Reset();
            try
            {
                var dataSettings = DataSettings.Get(configuration);
                var desiredCategoriesJson = await File.ReadAllTextAsync(dataSettings.CategoriesListPath);
                var desiredCategoriesList = JsonConvert.DeserializeObject<List<Category>>(desiredCategoriesJson);
                if (desiredCategoriesList != null)
                {
                    for (int i = 0; i < desiredCategoriesList.Count; i++)
                    {
                        desiredCategoriesList[i].OrderOnList = i;
                    }
                    var desiredCategoriesKeywords = desiredCategoriesList.Select(c => c.Keyword).ToList();
                    if (desiredCategoriesKeywords.Contains("otherIncome") && desiredCategoriesKeywords.Contains("otherCost"))
                    {
                        var existingCategoriesList = await categoryRepository.GetAllAsync();
                        var otherIncomeCategory = existingCategoriesList.Where(c => c.Keyword.Equals("otherIncome")).FirstOrDefault();
                        var otherCostCategory = existingCategoriesList.Where(c => c.Keyword.Equals("otherCost")).FirstOrDefault();
                        var deletedCount = 0;
                        var updatedCount = 0;
                        var addedCount = 0;
                        foreach (var existingCategory in existingCategoriesList)
                        {
                            if (!desiredCategoriesKeywords.Contains(existingCategory.Keyword))
                            {
                                foreach (var entry in await entryRepository.GetAllByCategoryIdAsync(existingCategory.Id))
                                {
                                    entry.CategoryId = existingCategory.IsIncome ? otherIncomeCategory.Id : otherCostCategory.Id;
                                    await entryRepository.UpdateAsync(entry);
                                }
                                await categoryRepository.DeleteAsync(existingCategory);
                                deletedCount++;
                            }
                        }
                        existingCategoriesList = await categoryRepository.GetAllAsync();
                        foreach (var desiredCategory in desiredCategoriesList)
                        {
                            var existingCategory = existingCategoriesList.FirstOrDefault(c => c.Keyword.Equals(desiredCategory.Keyword));
                            if (existingCategory != null)
                            {
                                if (AreDifferent(desiredCategory, existingCategory))
                                {
                                    existingCategory.Keyword = desiredCategory.Keyword;
                                    existingCategory.IsIncome = desiredCategory.IsIncome;
                                    existingCategory.Icon = desiredCategory.Icon;
                                    existingCategory.IconColor = desiredCategory.IconColor;
                                    await categoryRepository.UpdateAsync(existingCategory);
                                    updatedCount++;
                                }
                            }
                            else
                            {
                                await categoryRepository.AddAsync(desiredCategory);
                                addedCount++;
                            }
                        }
                        logger.LogInformation($"Categories loading completed - deleted: {deletedCount}; updated: {updatedCount}; added: {addedCount}");
                    }
                    else
                    {
                        throw new CategoriesLoaderException("Categories with \"otherIncome\" and \"otherCost\" keywords not found");
                    }
                }
                else
                {
                    throw new CategoriesLoaderException($"No categories defined in {dataSettings.CategoriesListPath} file");
                }
            }
            finally
            {
                loadingState.Set();
            }
        }

        public void WaitForLoadingToFinish()
        {
            loadingState.Wait();
        }

        private bool AreDifferent(Category category1, Category category2)
        {
            return category1.OrderOnList != category2.OrderOnList ||
                !category1.Keyword.Equals(category2.Keyword) ||
                category1.IsIncome != category2.IsIncome ||
                !category1.Icon.Equals(category2.Icon) ||
                !category1.IconColor.Equals(category2.IconColor);

        }
    }
}
