using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Common.Helpers
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Message { get; set; }
        public IReadOnlyCollection<string> Errors { get; }

        protected internal Result(bool isSuccess, string? message, IEnumerable<string>? errors = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors?.ToList() ?? new List<string>();
        }

        public static Result Success(string message = "Proceso exitoso") => new(true, message);
        public static Result Failure(string error) => new(false, error);
        public static Result Failure(IEnumerable<string> errors) => new(false, null, errors);

        public static Result<T> Success<T>(T value, string message = "Proceso exitoso") => new(value, true, message);
        public static Result<T> Failure<T>(string error) => new(default, false, error);
        public static Result<T> Failure<T>(IEnumerable<string> errors) => new(default, false, null, errors);

    }
    public class Result<T> : Result
    {
        public T? Value { get; }
        protected internal Result(T? value, bool isSuccess, string? message, IEnumerable<string>? errors = null) : base(isSuccess, message, errors)
        {
            Value = value;
        }
    }
}
