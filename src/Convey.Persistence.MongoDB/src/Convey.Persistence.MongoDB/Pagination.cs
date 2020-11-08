using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Convey.CQRS.Queries;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Convey.Persistence.MongoDB
{
    public static class Pagination
    {
        public static async Task<PagedResult<T>> PaginateAsync<T>(this IMongoQueryable<T> collection, IPagedQuery query)
            => await collection.PaginateAsync(query.OrderBy, query.SortOrder, query.Page, query.Results);

        public static async Task<PagedResult<T>> PaginateAsync<T>(this IMongoQueryable<T> collection, string orderBy,
            string sortOrder, int page = 1, int resultsPerPage = 10)
        {
            if (page <= 0)
            {
                page = 1;
            }

            if (resultsPerPage <= 0)
            {
                resultsPerPage = 10;
            }

            var isEmpty = await collection.AnyAsync() == false;
            if (isEmpty)
            {
                return PagedResult<T>.Empty;
            }

            var totalResults = await collection.CountAsync();
            var totalPages = (int) Math.Ceiling((decimal) totalResults / resultsPerPage);

            List<T> data;
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                data = await collection.Limit(page, resultsPerPage).ToListAsync();
                return PagedResult<T>.Create(data, page, resultsPerPage, totalPages, totalResults);
            }

            if (sortOrder?.ToLowerInvariant() == "asc")
            {
                data = await collection.OrderBy(ToLambda<T>(orderBy)).Limit(page, resultsPerPage).ToListAsync();
            }
            else
            {
                data = await collection.OrderByDescending(ToLambda<T>(orderBy)).Limit(page, resultsPerPage).ToListAsync();
            }

            return PagedResult<T>.Create(data, page, resultsPerPage, totalPages, totalResults);
        }

        public static IMongoQueryable<T> Limit<T>(this IMongoQueryable<T> collection, IPagedQuery query)
            => collection.Limit(query.Page, query.Results);

        public static IMongoQueryable<T> Limit<T>(this IMongoQueryable<T> collection,
            int page = 1, int resultsPerPage = 10)
        {
            if (page <= 0)
            {
                page = 1;
            }
            if (resultsPerPage <= 0)
            {
                resultsPerPage = 10;
            }
            var skip = (page - 1) * resultsPerPage;
            var data = collection.Skip(skip)
                .Take(resultsPerPage);

            return data;
        }

        private static Expression<Func<T, object>> ToLambda<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, propertyName);
            var propAsObject = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<T, object>>(propAsObject, parameter);
        }
    }
}