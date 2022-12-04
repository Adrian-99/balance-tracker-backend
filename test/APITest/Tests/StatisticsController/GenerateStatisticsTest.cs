using Application;
using Application.Dtos.Ingoing.Statistics;
using Application.Dtos.Outgoing.Statistics;
using Application.Interfaces;
using Application.Utilities;
using Application.Utilities.Statistics;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APITest.Tests.StatisticsController
{
    public class GenerateStatisticsTest : AbstractTestClass
    {
        private static readonly string URL = "api/statistics";

        private User user;
        private JwtTokens tokens;

        protected override void PrepareTestData()
        {
            TestDataSeeder.SeedAll(GetService<CategoriesLoader>(), GetService<IConfiguration>(), databaseContext);
            user = databaseContext.Users.Where(u => u.IsEmailVerified).First();
            tokens = GetService<IJwtService>().GenerateTokens(user);
        }

        [Test]
        public async Task GenerateStatistics_WithAllPossibleSelectValues()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, null, null,
                new List<SelectValue> { SelectValue.Count, SelectValue.Min, SelectValue.Max, SelectValue.Sum, SelectValue.Average, SelectValue.Median }, 
                null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(6, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> { 
                SelectValue.Count, SelectValue.Min, SelectValue.Max, SelectValue.Sum, SelectValue.Average, SelectValue.Median
            }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.IsNull(responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(1, responseContent.Data.Rows.Count);
            Assert.IsNull(responseContent.Data.Rows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(6, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(6M, responseContent.Data.Rows[0].Values[SelectValue.Count]);
            Assert.AreEqual(-60.45M, responseContent.Data.Rows[0].Values[SelectValue.Min]);
            Assert.AreEqual(3200M, responseContent.Data.Rows[0].Values[SelectValue.Max]);
            Assert.AreEqual(3158.95M, responseContent.Data.Rows[0].Values[SelectValue.Sum]);
            Assert.AreEqual(526.49M, responseContent.Data.Rows[0].Values[SelectValue.Average]);
            Assert.AreEqual(-10.08M, responseContent.Data.Rows[0].Values[SelectValue.Median]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithDateRangeFilter()
        {
            var statisticsRequestDto = new StatisticsRequestDto(
                new StatisticsDateRangeFilterDto(new DateTime(2022, 6, 25, 0, 0, 0), new DateTime(2022, 7, 2, 23, 59, 59)),
                null, null, null, null, null, new List<SelectValue> { SelectValue.Sum }, null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(2, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> { SelectValue.Sum }, responseContent.Data.SelectValues);
            Assert.AreEqual(new DateTime(2022, 6, 25, 0, 0, 0, 0), responseContent.Data.DateFromFilter);
            Assert.AreEqual(new DateTime(2022, 7, 2, 23, 59, 59), responseContent.Data.DateToFilter);
            Assert.IsNull(responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(1, responseContent.Data.Rows.Count);
            Assert.IsNull(responseContent.Data.Rows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(1, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(-62.95M, responseContent.Data.Rows[0].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithEntryTypeFilter()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, EntryType.Cost, null, null, null, null,
                new List<SelectValue> { SelectValue.Count, SelectValue.Min, SelectValue.Max, SelectValue.Sum, SelectValue.Average, SelectValue.Median },
                null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(4, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> {
                SelectValue.Count, SelectValue.Min, SelectValue.Max, SelectValue.Sum, SelectValue.Average, SelectValue.Median
            }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.AreEqual(EntryType.Cost, responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(1, responseContent.Data.Rows.Count);
            Assert.IsNull(responseContent.Data.Rows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(6, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(4M, responseContent.Data.Rows[0].Values[SelectValue.Count]);
            Assert.AreEqual(2.5M, responseContent.Data.Rows[0].Values[SelectValue.Min]);
            Assert.AreEqual(60.45M, responseContent.Data.Rows[0].Values[SelectValue.Max]);
            Assert.AreEqual(141.05M, responseContent.Data.Rows[0].Values[SelectValue.Sum]);
            Assert.AreEqual(35.26M, responseContent.Data.Rows[0].Values[SelectValue.Average]);
            Assert.AreEqual(39.05M, responseContent.Data.Rows[0].Values[SelectValue.Median]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithCategoriesFilter()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, new List<string> { "otherIncome", "costCategory2" },
                null, null, null,new List<SelectValue> { SelectValue.Min, SelectValue.Max, SelectValue.Sum, SelectValue.Median },
                null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(3, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> {
                SelectValue.Min, SelectValue.Max, SelectValue.Sum, SelectValue.Median
            }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.IsNull(responseContent.Data.EntryTypeFilter);
            Assert.AreEqual(new List<string> { "otherIncome", "costCategory2" }, responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(1, responseContent.Data.Rows.Count);
            Assert.IsNull(responseContent.Data.Rows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(4, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(-17.65M, responseContent.Data.Rows[0].Values[SelectValue.Min]);
            Assert.AreEqual(100M, responseContent.Data.Rows[0].Values[SelectValue.Max]);
            Assert.AreEqual(79.85M, responseContent.Data.Rows[0].Values[SelectValue.Sum]);
            Assert.AreEqual(-2.5M, responseContent.Data.Rows[0].Values[SelectValue.Median]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithTagsFilter()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, new List<string> { "tag1", "secondTag" }, null,
                null, new List<SelectValue> { SelectValue.Min, SelectValue.Max }, null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(3, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> { SelectValue.Min, SelectValue.Max }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.IsNull(responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.AreEqual(new List<string> { "tag1", "secondTag" }, responseContent.Data.TagFilter);
            Assert.AreEqual(1, responseContent.Data.Rows.Count);
            Assert.IsNull(responseContent.Data.Rows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(-17.65M, responseContent.Data.Rows[0].Values[SelectValue.Min]);
            Assert.AreEqual(100M, responseContent.Data.Rows[0].Values[SelectValue.Max]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithGroupBy7DaysPeriod()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, new List<GroupBy> { GroupBy.TimeInterval },
                new StatisticsGroupByTimeIntervalDto(new DateTime(2022, 6, 1), 7, TimePeriodUnit.Day),
                new List<SelectValue> { SelectValue.Min, SelectValue.Max, SelectValue.Sum }, null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(6, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> {
                SelectValue.Min, SelectValue.Max, SelectValue.Sum
            }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.IsNull(responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(5, responseContent.Data.Rows.Count);
            Assert.AreEqual(new DateTime(2022, 5, 11, 0, 0, 0, 0), responseContent.Data.Rows[0].DateFrom);
            Assert.AreEqual(new DateTime(2022, 5, 17, 23, 59, 59, 999).AddTicks(9999), responseContent.Data.Rows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(3, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(-17.65M, responseContent.Data.Rows[0].Values[SelectValue.Min]);
            Assert.AreEqual(-17.65M, responseContent.Data.Rows[0].Values[SelectValue.Max]);
            Assert.AreEqual(-17.65M, responseContent.Data.Rows[0].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows);
            Assert.AreEqual(new DateTime(2022, 6, 1, 0, 0, 0, 0), responseContent.Data.Rows[1].DateFrom);
            Assert.AreEqual(new DateTime(2022, 6, 7, 23, 59, 59, 999).AddTicks(9999), responseContent.Data.Rows[1].DateTo);
            Assert.IsNull(responseContent.Data.Rows[1].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[1].TagName);
            Assert.AreEqual(3, responseContent.Data.Rows[1].Values.Count);
            Assert.AreEqual(-60.45M, responseContent.Data.Rows[1].Values[SelectValue.Min]);
            Assert.AreEqual(-60.45M, responseContent.Data.Rows[1].Values[SelectValue.Max]);
            Assert.AreEqual(-60.45M, responseContent.Data.Rows[1].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows);
            Assert.AreEqual(new DateTime(2022, 6, 8, 0, 0, 0, 0), responseContent.Data.Rows[2].DateFrom);
            Assert.AreEqual(new DateTime(2022, 6, 14, 23, 59, 59, 999).AddTicks(9999), responseContent.Data.Rows[2].DateTo);
            Assert.IsNull(responseContent.Data.Rows[2].EntryType);
            Assert.IsNull(responseContent.Data.Rows[2].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[2].TagName);
            Assert.AreEqual(3, responseContent.Data.Rows[2].Values.Count);
            Assert.AreEqual(100M, responseContent.Data.Rows[2].Values[SelectValue.Min]);
            Assert.AreEqual(3200M, responseContent.Data.Rows[2].Values[SelectValue.Max]);
            Assert.AreEqual(3300M, responseContent.Data.Rows[2].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[2].SubRows);
            Assert.AreEqual(new DateTime(2022, 6, 22, 0, 0, 0, 0), responseContent.Data.Rows[3].DateFrom);
            Assert.AreEqual(new DateTime(2022, 6, 28, 23, 59, 59, 999).AddTicks(9999), responseContent.Data.Rows[3].DateTo);
            Assert.IsNull(responseContent.Data.Rows[3].EntryType);
            Assert.IsNull(responseContent.Data.Rows[3].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[3].TagName);
            Assert.AreEqual(3, responseContent.Data.Rows[3].Values.Count);
            Assert.AreEqual(-2.5M, responseContent.Data.Rows[3].Values[SelectValue.Min]);
            Assert.AreEqual(-2.5M, responseContent.Data.Rows[3].Values[SelectValue.Max]);
            Assert.AreEqual(-2.5M, responseContent.Data.Rows[3].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[3].SubRows);
            Assert.AreEqual(new DateTime(2022, 6, 29, 0, 0, 0, 0), responseContent.Data.Rows[4].DateFrom);
            Assert.AreEqual(new DateTime(2022, 7, 5, 23, 59, 59, 999).AddTicks(9999), responseContent.Data.Rows[4].DateTo);
            Assert.IsNull(responseContent.Data.Rows[4].EntryType);
            Assert.IsNull(responseContent.Data.Rows[4].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[4].TagName);
            Assert.AreEqual(3, responseContent.Data.Rows[4].Values.Count);
            Assert.AreEqual(-60.45M, responseContent.Data.Rows[4].Values[SelectValue.Min]);
            Assert.AreEqual(-60.45M, responseContent.Data.Rows[4].Values[SelectValue.Max]);
            Assert.AreEqual(-60.45M, responseContent.Data.Rows[4].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[4].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithGroupBy2MonthsPeriod()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, new List<GroupBy> { GroupBy.TimeInterval },
                new StatisticsGroupByTimeIntervalDto(new DateTime(2022, 6, 1), 2, TimePeriodUnit.Month),
                new List<SelectValue> { SelectValue.Count, SelectValue.Sum }, null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(6, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> { SelectValue.Count, SelectValue.Sum }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.IsNull(responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(2, responseContent.Data.Rows.Count);
            Assert.AreEqual(new DateTime(2022, 4, 1, 0, 0, 0, 0), responseContent.Data.Rows[0].DateFrom);
            Assert.AreEqual(new DateTime(2022, 5, 31, 23, 59, 59, 999).AddTicks(9999), responseContent.Data.Rows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[0].Values[SelectValue.Count]);
            Assert.AreEqual(-17.65M, responseContent.Data.Rows[0].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows);
            Assert.AreEqual(new DateTime(2022, 6, 1, 0, 0, 0, 0), responseContent.Data.Rows[1].DateFrom);
            Assert.AreEqual(new DateTime(2022, 7, 31, 23, 59, 59, 999).AddTicks(9999), responseContent.Data.Rows[1].DateTo);
            Assert.IsNull(responseContent.Data.Rows[1].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[1].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].Values.Count);
            Assert.AreEqual(5M, responseContent.Data.Rows[1].Values[SelectValue.Count]);
            Assert.AreEqual(3176.6M, responseContent.Data.Rows[1].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithGroupBy1YearPeriod()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, new List<GroupBy> { GroupBy.TimeInterval },
                new StatisticsGroupByTimeIntervalDto(new DateTime(2022, 1, 1), 1, TimePeriodUnit.Year),
                new List<SelectValue> { SelectValue.Count, SelectValue.Sum }, null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(6, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> { SelectValue.Count, SelectValue.Sum }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.IsNull(responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(1, responseContent.Data.Rows.Count);
            Assert.AreEqual(new DateTime(2022, 1, 1), responseContent.Data.Rows[0].DateFrom);
            Assert.AreEqual(new DateTime(2022, 12, 31, 23, 59, 59, 999).AddTicks(9999), responseContent.Data.Rows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(6M, responseContent.Data.Rows[0].Values[SelectValue.Count]);
            Assert.AreEqual(3158.95M, responseContent.Data.Rows[0].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithGroupByEntryType()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, new List<GroupBy> { GroupBy.EntryType }, null,
                new List<SelectValue> { SelectValue.Count, SelectValue.Min, SelectValue.Max, SelectValue.Sum, SelectValue.Average, SelectValue.Median },
                null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(6, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> {
                SelectValue.Count, SelectValue.Min, SelectValue.Max, SelectValue.Sum, SelectValue.Average, SelectValue.Median
            }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.IsNull(responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(2, responseContent.Data.Rows.Count);
            Assert.IsNull(responseContent.Data.Rows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].DateTo);
            Assert.AreEqual(EntryType.Cost, responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(6, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(4M, responseContent.Data.Rows[0].Values[SelectValue.Count]);
            Assert.AreEqual(2.5M, responseContent.Data.Rows[0].Values[SelectValue.Min]);
            Assert.AreEqual(60.45M, responseContent.Data.Rows[0].Values[SelectValue.Max]);
            Assert.AreEqual(141.05M, responseContent.Data.Rows[0].Values[SelectValue.Sum]);
            Assert.AreEqual(35.26M, responseContent.Data.Rows[0].Values[SelectValue.Average]);
            Assert.AreEqual(39.05M, responseContent.Data.Rows[0].Values[SelectValue.Median]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows);
            Assert.IsNull(responseContent.Data.Rows[1].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].DateTo);
            Assert.AreEqual(EntryType.Income, responseContent.Data.Rows[1].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[1].TagName);
            Assert.AreEqual(6, responseContent.Data.Rows[1].Values.Count);
            Assert.AreEqual(2M, responseContent.Data.Rows[1].Values[SelectValue.Count]);
            Assert.AreEqual(100M, responseContent.Data.Rows[1].Values[SelectValue.Min]);
            Assert.AreEqual(3200M, responseContent.Data.Rows[1].Values[SelectValue.Max]);
            Assert.AreEqual(3300M, responseContent.Data.Rows[1].Values[SelectValue.Sum]);
            Assert.AreEqual(1650M, responseContent.Data.Rows[1].Values[SelectValue.Average]);
            Assert.AreEqual(1650M, responseContent.Data.Rows[1].Values[SelectValue.Median]);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithGroupByCategory()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, new List<GroupBy> { GroupBy.Category }, null,
                new List<SelectValue> { SelectValue.Count, SelectValue.Min, SelectValue.Max, SelectValue.Sum, SelectValue.Average, SelectValue.Median },
                null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(6, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> {
                SelectValue.Count, SelectValue.Min, SelectValue.Max, SelectValue.Sum, SelectValue.Average, SelectValue.Median
            }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.IsNull(responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(4, responseContent.Data.Rows.Count);
            Assert.IsNull(responseContent.Data.Rows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].EntryType);
            Assert.AreEqual("incomeCategory1", responseContent.Data.Rows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(6, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[0].Values[SelectValue.Count]);
            Assert.AreEqual(3200M, responseContent.Data.Rows[0].Values[SelectValue.Min]);
            Assert.AreEqual(3200M, responseContent.Data.Rows[0].Values[SelectValue.Max]);
            Assert.AreEqual(3200M, responseContent.Data.Rows[0].Values[SelectValue.Sum]);
            Assert.AreEqual(3200M, responseContent.Data.Rows[0].Values[SelectValue.Average]);
            Assert.AreEqual(3200M, responseContent.Data.Rows[0].Values[SelectValue.Median]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows);
            Assert.IsNull(responseContent.Data.Rows[1].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].DateTo);
            Assert.IsNull(responseContent.Data.Rows[1].EntryType);
            Assert.AreEqual("otherIncome", responseContent.Data.Rows[1].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[1].TagName);
            Assert.AreEqual(6, responseContent.Data.Rows[1].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[1].Values[SelectValue.Count]);
            Assert.AreEqual(100M, responseContent.Data.Rows[1].Values[SelectValue.Min]);
            Assert.AreEqual(100M, responseContent.Data.Rows[1].Values[SelectValue.Max]);
            Assert.AreEqual(100M, responseContent.Data.Rows[1].Values[SelectValue.Sum]);
            Assert.AreEqual(100M, responseContent.Data.Rows[1].Values[SelectValue.Average]);
            Assert.AreEqual(100M, responseContent.Data.Rows[1].Values[SelectValue.Median]);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows);
            Assert.IsNull(responseContent.Data.Rows[2].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[2].DateTo);
            Assert.IsNull(responseContent.Data.Rows[2].EntryType);
            Assert.AreEqual("costCategory2", responseContent.Data.Rows[2].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[2].TagName);
            Assert.AreEqual(6, responseContent.Data.Rows[2].Values.Count);
            Assert.AreEqual(2M, responseContent.Data.Rows[2].Values[SelectValue.Count]);
            Assert.AreEqual(2.5M, responseContent.Data.Rows[2].Values[SelectValue.Min]);
            Assert.AreEqual(17.65M, responseContent.Data.Rows[2].Values[SelectValue.Max]);
            Assert.AreEqual(20.15M, responseContent.Data.Rows[2].Values[SelectValue.Sum]);
            Assert.AreEqual(10.08M, responseContent.Data.Rows[2].Values[SelectValue.Average]);
            Assert.AreEqual(10.08M, responseContent.Data.Rows[2].Values[SelectValue.Median]);
            Assert.IsNull(responseContent.Data.Rows[2].SubRows);
            Assert.IsNull(responseContent.Data.Rows[3].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[3].DateTo);
            Assert.IsNull(responseContent.Data.Rows[3].EntryType);
            Assert.AreEqual("otherCost", responseContent.Data.Rows[3].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[3].TagName);
            Assert.AreEqual(6, responseContent.Data.Rows[3].Values.Count);
            Assert.AreEqual(2M, responseContent.Data.Rows[3].Values[SelectValue.Count]);
            Assert.AreEqual(60.45M, responseContent.Data.Rows[3].Values[SelectValue.Min]);
            Assert.AreEqual(60.45M, responseContent.Data.Rows[3].Values[SelectValue.Max]);
            Assert.AreEqual(120.9M, responseContent.Data.Rows[3].Values[SelectValue.Sum]);
            Assert.AreEqual(60.45M, responseContent.Data.Rows[3].Values[SelectValue.Average]);
            Assert.AreEqual(60.45M, responseContent.Data.Rows[3].Values[SelectValue.Median]);
            Assert.IsNull(responseContent.Data.Rows[3].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithGroupByTag()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, new List<GroupBy> { GroupBy.Tag }, null,
                new List<SelectValue> { SelectValue.Count, SelectValue.Sum }, null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(6, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> { SelectValue.Count, SelectValue.Sum }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.IsNull(responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(4, responseContent.Data.Rows.Count);
            Assert.IsNull(responseContent.Data.Rows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.AreEqual("secondTag", responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[0].Values[SelectValue.Count]);
            Assert.AreEqual(-2.5M, responseContent.Data.Rows[0].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows);
            Assert.IsNull(responseContent.Data.Rows[1].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].DateTo);
            Assert.IsNull(responseContent.Data.Rows[1].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].CategoryKeyword);
            Assert.AreEqual("Tag number 3", responseContent.Data.Rows[1].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].Values.Count);
            Assert.AreEqual(3M, responseContent.Data.Rows[1].Values[SelectValue.Count]);
            Assert.AreEqual(-138.55M, responseContent.Data.Rows[1].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows);
            Assert.IsNull(responseContent.Data.Rows[2].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[2].DateTo);
            Assert.IsNull(responseContent.Data.Rows[2].EntryType);
            Assert.IsNull(responseContent.Data.Rows[2].CategoryKeyword);
            Assert.AreEqual("tag1", responseContent.Data.Rows[2].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[2].Values.Count);
            Assert.AreEqual(2M, responseContent.Data.Rows[2].Values[SelectValue.Count]);
            Assert.AreEqual(82.35M, responseContent.Data.Rows[2].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[2].SubRows);
            Assert.IsNull(responseContent.Data.Rows[3].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[3].DateTo);
            Assert.IsNull(responseContent.Data.Rows[3].EntryType);
            Assert.IsNull(responseContent.Data.Rows[3].CategoryKeyword);
            Assert.AreEqual("", responseContent.Data.Rows[3].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[3].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[3].Values[SelectValue.Count]);
            Assert.AreEqual(3200M, responseContent.Data.Rows[3].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[3].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithMultipleGroupByAndSelectOnAllLevels()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null,
                new List<GroupBy> { GroupBy.TimeInterval, GroupBy.Tag, GroupBy.EntryType },
                new StatisticsGroupByTimeIntervalDto(new DateTime(2022, 6, 1), 2, TimePeriodUnit.Month),
                new List<SelectValue> { SelectValue.Count, SelectValue.Sum }, true);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(6, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> { SelectValue.Count, SelectValue.Sum }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.IsNull(responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(2, responseContent.Data.Rows.Count);

            Assert.AreEqual(new DateTime(2022, 4, 1, 0, 0, 0, 0), responseContent.Data.Rows[0].DateFrom);
            Assert.AreEqual(new DateTime(2022, 5, 31, 23, 59, 59, 999).AddTicks(9999), responseContent.Data.Rows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[0].Values[SelectValue.Count]);
            Assert.AreEqual(-17.65M, responseContent.Data.Rows[0].Values[SelectValue.Sum]);
            Assert.AreEqual(2, responseContent.Data.Rows[0].SubRows.Count);

            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].CategoryKeyword);
            Assert.AreEqual("Tag number 3", responseContent.Data.Rows[0].SubRows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[0].SubRows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[0].SubRows[0].Values[SelectValue.Count]);
            Assert.AreEqual(-17.65M, responseContent.Data.Rows[0].SubRows[0].Values[SelectValue.Sum]);
            Assert.AreEqual(1, responseContent.Data.Rows[0].SubRows[0].SubRows.Count);

            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].SubRows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].SubRows[0].DateTo);
            Assert.AreEqual(EntryType.Cost, responseContent.Data.Rows[0].SubRows[0].SubRows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].SubRows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].SubRows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[0].SubRows[0].SubRows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[0].SubRows[0].SubRows[0].Values[SelectValue.Count]);
            Assert.AreEqual(17.65M, responseContent.Data.Rows[0].SubRows[0].SubRows[0].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].SubRows[0].SubRows);

            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].CategoryKeyword);
            Assert.AreEqual("tag1", responseContent.Data.Rows[0].SubRows[1].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[0].SubRows[1].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[0].SubRows[1].Values[SelectValue.Count]);
            Assert.AreEqual(-17.65M, responseContent.Data.Rows[0].SubRows[1].Values[SelectValue.Sum]);
            Assert.AreEqual(1, responseContent.Data.Rows[0].SubRows[1].SubRows.Count);

            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].SubRows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].SubRows[0].DateTo);
            Assert.AreEqual(EntryType.Cost, responseContent.Data.Rows[0].SubRows[1].SubRows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].SubRows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].SubRows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[0].SubRows[1].SubRows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[0].SubRows[1].SubRows[0].Values[SelectValue.Count]);
            Assert.AreEqual(17.65M, responseContent.Data.Rows[0].SubRows[1].SubRows[0].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].SubRows[0].SubRows);

            Assert.AreEqual(new DateTime(2022, 6, 1, 0, 0, 0, 0), responseContent.Data.Rows[1].DateFrom);
            Assert.AreEqual(new DateTime(2022, 7, 31, 23, 59, 59, 999).AddTicks(9999), responseContent.Data.Rows[1].DateTo);
            Assert.IsNull(responseContent.Data.Rows[1].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[1].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].Values.Count);
            Assert.AreEqual(5M, responseContent.Data.Rows[1].Values[SelectValue.Count]);
            Assert.AreEqual(3176.6M, responseContent.Data.Rows[1].Values[SelectValue.Sum]);
            Assert.AreEqual(4, responseContent.Data.Rows[1].SubRows.Count);

            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].CategoryKeyword);
            Assert.AreEqual("secondTag", responseContent.Data.Rows[1].SubRows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].SubRows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[1].SubRows[0].Values[SelectValue.Count]);
            Assert.AreEqual(-2.5M, responseContent.Data.Rows[1].SubRows[0].Values[SelectValue.Sum]);
            Assert.AreEqual(1, responseContent.Data.Rows[1].SubRows[0].SubRows.Count);

            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].SubRows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].SubRows[0].DateTo);
            Assert.AreEqual(EntryType.Cost, responseContent.Data.Rows[1].SubRows[0].SubRows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].SubRows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].SubRows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].SubRows[0].SubRows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[1].SubRows[0].SubRows[0].Values[SelectValue.Count]);
            Assert.AreEqual(2.5M, responseContent.Data.Rows[1].SubRows[0].SubRows[0].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].SubRows[0].SubRows);

            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].DateTo);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].CategoryKeyword);
            Assert.AreEqual("Tag number 3", responseContent.Data.Rows[1].SubRows[1].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].SubRows[1].Values.Count);
            Assert.AreEqual(2M, responseContent.Data.Rows[1].SubRows[1].Values[SelectValue.Count]);
            Assert.AreEqual(-120.9M, responseContent.Data.Rows[1].SubRows[1].Values[SelectValue.Sum]);
            Assert.AreEqual(1, responseContent.Data.Rows[1].SubRows[1].SubRows.Count);

            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].SubRows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].SubRows[0].DateTo);
            Assert.AreEqual(EntryType.Cost, responseContent.Data.Rows[1].SubRows[1].SubRows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].SubRows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].SubRows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].SubRows[1].SubRows[0].Values.Count);
            Assert.AreEqual(2M, responseContent.Data.Rows[1].SubRows[1].SubRows[0].Values[SelectValue.Count]);
            Assert.AreEqual(120.9M, responseContent.Data.Rows[1].SubRows[1].SubRows[0].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].SubRows[0].SubRows);

            Assert.IsNull(responseContent.Data.Rows[1].SubRows[2].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[2].DateTo);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[2].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[2].CategoryKeyword);
            Assert.AreEqual("tag1", responseContent.Data.Rows[1].SubRows[2].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].SubRows[2].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[1].SubRows[2].Values[SelectValue.Count]);
            Assert.AreEqual(100M, responseContent.Data.Rows[1].SubRows[2].Values[SelectValue.Sum]);
            Assert.AreEqual(1, responseContent.Data.Rows[1].SubRows[2].SubRows.Count);

            Assert.IsNull(responseContent.Data.Rows[1].SubRows[2].SubRows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[2].SubRows[0].DateTo);
            Assert.AreEqual(EntryType.Income, responseContent.Data.Rows[1].SubRows[2].SubRows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[2].SubRows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[2].SubRows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].SubRows[2].SubRows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[1].SubRows[2].SubRows[0].Values[SelectValue.Count]);
            Assert.AreEqual(100M, responseContent.Data.Rows[1].SubRows[2].SubRows[0].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[2].SubRows[0].SubRows);

            Assert.IsNull(responseContent.Data.Rows[1].SubRows[3].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[3].DateTo);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[3].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[3].CategoryKeyword);
            Assert.AreEqual("", responseContent.Data.Rows[1].SubRows[3].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].SubRows[3].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[1].SubRows[3].Values[SelectValue.Count]);
            Assert.AreEqual(3200M, responseContent.Data.Rows[1].SubRows[3].Values[SelectValue.Sum]);
            Assert.AreEqual(1, responseContent.Data.Rows[1].SubRows[3].SubRows.Count);

            Assert.IsNull(responseContent.Data.Rows[1].SubRows[3].SubRows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[3].SubRows[0].DateTo);
            Assert.AreEqual(EntryType.Income, responseContent.Data.Rows[1].SubRows[3].SubRows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[3].SubRows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[3].SubRows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].SubRows[3].SubRows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[1].SubRows[3].SubRows[0].Values[SelectValue.Count]);
            Assert.AreEqual(3200M, responseContent.Data.Rows[1].SubRows[3].SubRows[0].Values[SelectValue.Sum]);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[3].SubRows[0].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithMultipleGroupBy()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null,
                new List<GroupBy> { GroupBy.EntryType, GroupBy.Tag }, null,
                new List<SelectValue> { SelectValue.Count, SelectValue.Average }, false);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(6, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> { SelectValue.Count, SelectValue.Average }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.IsNull(responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(2, responseContent.Data.Rows.Count);

            Assert.IsNull(responseContent.Data.Rows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].DateTo);
            Assert.AreEqual(EntryType.Cost, responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[0].TagName);
            Assert.IsNull(responseContent.Data.Rows[0].Values);
            Assert.AreEqual(3, responseContent.Data.Rows[0].SubRows.Count);

            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].CategoryKeyword);
            Assert.AreEqual("secondTag", responseContent.Data.Rows[0].SubRows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[0].SubRows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[0].SubRows[0].Values[SelectValue.Count]);
            Assert.AreEqual(2.5M, responseContent.Data.Rows[0].SubRows[0].Values[SelectValue.Average]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[0].SubRows);

            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].CategoryKeyword);
            Assert.AreEqual("Tag number 3", responseContent.Data.Rows[0].SubRows[1].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[0].SubRows[1].Values.Count);
            Assert.AreEqual(3M, responseContent.Data.Rows[0].SubRows[1].Values[SelectValue.Count]);
            Assert.AreEqual(46.18M, responseContent.Data.Rows[0].SubRows[1].Values[SelectValue.Average]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[1].SubRows);

            Assert.IsNull(responseContent.Data.Rows[0].SubRows[2].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[2].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[2].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[2].CategoryKeyword);
            Assert.AreEqual("tag1", responseContent.Data.Rows[0].SubRows[2].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[0].SubRows[2].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[0].SubRows[2].Values[SelectValue.Count]);
            Assert.AreEqual(17.65M, responseContent.Data.Rows[0].SubRows[2].Values[SelectValue.Average]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows[2].SubRows);

            Assert.IsNull(responseContent.Data.Rows[1].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].DateTo);
            Assert.AreEqual(EntryType.Income, responseContent.Data.Rows[1].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].CategoryKeyword);
            Assert.IsNull(responseContent.Data.Rows[1].TagName);
            Assert.IsNull(responseContent.Data.Rows[1].Values);
            Assert.AreEqual(2, responseContent.Data.Rows[1].SubRows.Count);

            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].CategoryKeyword);
            Assert.AreEqual("tag1", responseContent.Data.Rows[1].SubRows[0].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].SubRows[0].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[1].SubRows[0].Values[SelectValue.Count]);
            Assert.AreEqual(100M, responseContent.Data.Rows[1].SubRows[0].Values[SelectValue.Average]);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[0].SubRows);

            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].DateTo);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].CategoryKeyword);
            Assert.AreEqual("", responseContent.Data.Rows[1].SubRows[1].TagName);
            Assert.AreEqual(2, responseContent.Data.Rows[1].SubRows[1].Values.Count);
            Assert.AreEqual(1M, responseContent.Data.Rows[1].SubRows[1].Values[SelectValue.Count]);
            Assert.AreEqual(3200M, responseContent.Data.Rows[1].SubRows[1].Values[SelectValue.Average]);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows[1].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithGroupByAndEntryTypeFilter()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, EntryType.Cost, null, null, new List<GroupBy> { GroupBy.Tag }, null,
                new List<SelectValue> { SelectValue.Median }, null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(4, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> { SelectValue.Median }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.AreEqual(EntryType.Cost, responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.IsNull(responseContent.Data.TagFilter);
            Assert.AreEqual(3, responseContent.Data.Rows.Count);

            Assert.IsNull(responseContent.Data.Rows[0].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[0].DateTo);
            Assert.IsNull(responseContent.Data.Rows[0].EntryType);
            Assert.IsNull(responseContent.Data.Rows[0].CategoryKeyword);
            Assert.AreEqual("secondTag", responseContent.Data.Rows[0].TagName);
            Assert.AreEqual(1, responseContent.Data.Rows[0].Values.Count);
            Assert.AreEqual(2.5M, responseContent.Data.Rows[0].Values[SelectValue.Median]);
            Assert.IsNull(responseContent.Data.Rows[0].SubRows);

            Assert.IsNull(responseContent.Data.Rows[1].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[1].DateTo);
            Assert.IsNull(responseContent.Data.Rows[1].EntryType);
            Assert.IsNull(responseContent.Data.Rows[1].CategoryKeyword);
            Assert.AreEqual("Tag number 3", responseContent.Data.Rows[1].TagName);
            Assert.AreEqual(1, responseContent.Data.Rows[1].Values.Count);
            Assert.AreEqual(60.45M, responseContent.Data.Rows[1].Values[SelectValue.Median]);
            Assert.IsNull(responseContent.Data.Rows[1].SubRows);

            Assert.IsNull(responseContent.Data.Rows[2].DateFrom);
            Assert.IsNull(responseContent.Data.Rows[2].DateTo);
            Assert.IsNull(responseContent.Data.Rows[2].EntryType);
            Assert.IsNull(responseContent.Data.Rows[2].CategoryKeyword);
            Assert.AreEqual("tag1", responseContent.Data.Rows[2].TagName);
            Assert.AreEqual(1, responseContent.Data.Rows[2].Values.Count);
            Assert.AreEqual(17.65M, responseContent.Data.Rows[2].Values[SelectValue.Median]);
            Assert.IsNull(responseContent.Data.Rows[2].SubRows);
        }

        [Test]
        public async Task GenerateStatistics_WithNoEntriesAfterFiltering()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, EntryType.Income, null, new List<string> { "secondTag" }, null, null,
                new List<SelectValue> { SelectValue.Median }, null);

            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<StatisticsResponseDto>>(response);
            Assert.NotNull(responseContent);
            Assert.IsTrue(responseContent.Successful);

            Assert.AreEqual(0, responseContent.Data.EntriesCount);
            Assert.AreEqual(new List<SelectValue> {  SelectValue.Median }, responseContent.Data.SelectValues);
            Assert.IsNull(responseContent.Data.DateFromFilter);
            Assert.IsNull(responseContent.Data.DateToFilter);
            Assert.AreEqual(EntryType.Income, responseContent.Data.EntryTypeFilter);
            Assert.IsNull(responseContent.Data.CategoryFilter);
            Assert.AreEqual(new List<string> { "secondTag" }, responseContent.Data.TagFilter);
            Assert.AreEqual(0, responseContent.Data.Rows.Count);
        }

        [Test]
        public async Task GenerateStatistics_WithNoSelectValues()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, null, null, new List<SelectValue>(), null);
            await AssertUnsuccessfulActionAsync(statisticsRequestDto, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GenerateStatistics_WithIncorrectDateRangeFilter()
        {
            var statisticsRequestDto = new StatisticsRequestDto(
                new StatisticsDateRangeFilterDto(new DateTime(2022, 7, 1), new DateTime(2022, 6, 1)),
                null, null, null, null, null, new List<SelectValue> { SelectValue.Sum }, null);
            await AssertUnsuccessfulActionAsync(statisticsRequestDto, HttpStatusCode.BadRequest);
        }
        [Test]
        public async Task GenerateStatistics_WithDuplicateGroupBy()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null,
                new List<GroupBy> { GroupBy.EntryType, GroupBy.Category, GroupBy.EntryType }, null,
                new List<SelectValue> { SelectValue.Sum }, null);
            await AssertUnsuccessfulActionAsync(statisticsRequestDto, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GenerateStatistics_WithGroupByTimePeriodWithoutTimePeriodProperties()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, new List<GroupBy> { GroupBy.TimeInterval },
                null, new List<SelectValue> { SelectValue.Sum }, null);
            await AssertUnsuccessfulActionAsync(statisticsRequestDto, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GenerateStatistics_WithNegativeTimeIntervalValue()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, new List<GroupBy> { GroupBy.TimeInterval },
                new StatisticsGroupByTimeIntervalDto(new DateTime(2022, 6, 1), -7, TimePeriodUnit.Day),
                new List<SelectValue> { SelectValue.Sum }, null);
            await AssertUnsuccessfulActionAsync(statisticsRequestDto, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GenerateStatistics_WithDuplicateSelectValue()
        {
            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, null, null,
                new List<SelectValue> { SelectValue.Sum, SelectValue.Average, SelectValue.Sum }, null);
            await AssertUnsuccessfulActionAsync(statisticsRequestDto, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GenerateStatistics_ForUserWithUnverifiedEmail()
        {
            user = await databaseContext.Users.Where(u => !u.IsEmailVerified).FirstAsync();
            tokens = GetService<IJwtService>().GenerateTokens(user);

            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, null, null,
                new List<SelectValue> { SelectValue.Sum }, null);
            await AssertUnsuccessfulActionAsync(statisticsRequestDto, HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GenerateStatistics_Unauthorized()
        {
            tokens = new JwtTokens("someTotallyWrongAccessToken", "refreshTokenWrongAsWell");

            var statisticsRequestDto = new StatisticsRequestDto(null, null, null, null, null, null,
                new List<SelectValue> { SelectValue.Sum }, null);
            await AssertUnsuccessfulActionAsync(statisticsRequestDto, HttpStatusCode.Unauthorized);
        }

        private async Task AssertUnsuccessfulActionAsync(StatisticsRequestDto statisticsRequestDto, HttpStatusCode expectedStatusCode)
        {
            var response = await SendHttpRequestAsync(HttpMethod.Post, URL, tokens.AccessToken, statisticsRequestDto);
            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var responseContent = await GetResponseContentAsync<ApiResponse<string>>(response);
            Assert.NotNull(responseContent);
            Assert.IsFalse(responseContent.Successful);
        }
    }
}
