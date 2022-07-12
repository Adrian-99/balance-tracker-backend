using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Utilities.Pagination
{
    public class Page<T> : ApiResponse<List<T>>
    {
        public int PageNumber { get; protected set; }
        public int PageSize { get; protected set; }
        public int TotalCount { get; protected set; }
        public bool IsLastPage { get; protected set; }

        public Page(int pageNumber, int pageSize, int totalCount, bool isLastPage, bool successful, string? translationKey, List<T> data)
            : base(successful, translationKey, data)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
            IsLastPage = isLastPage;
        }

        public static Page<T> New(Pageable pageable, List<T> allElements, string? translationKey = null)
        {
            return new Page<T>(
                pageable.PageNumber,
                pageable.PageSize,
                allElements.Count,
                pageable.PageNumber * pageable.PageSize >= allElements.Count,
                true,
                translationKey,
                allElements.Skip((pageable.PageNumber - 1) * pageable.PageSize).Take(pageable.PageSize).ToList()
                );
        }

        public Page<NewT> Map<NewT>(Func<T, NewT> mapperFunction)
        {
            var convertedData = Data.Select(element => mapperFunction.Invoke(element)).ToList();
            return new Page<NewT>(PageNumber, PageSize, TotalCount, IsLastPage, Successful, TranslationKey, convertedData);
        }
    }
}
