using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Utilities
{
    public class ApiResponse<T>
    {
        public bool Successful { get; protected set; }
        public string? TranslationKey { get; protected set; }
        public T Data { get; protected set; }

        public ApiResponse(bool successful, string? translationKey, T data)
        {
            Successful = successful;
            TranslationKey = translationKey;
            Data = data;
        }

        public static ApiResponse<T> Success(T data, string? translationKey = null)
        {
            return new ApiResponse<T>(true, translationKey, data);
        }

        public static ApiResponse<T> Error(T data, string? translationKey = null)
        {
            return new ApiResponse<T>(false, translationKey, data);
        }
    }
}
