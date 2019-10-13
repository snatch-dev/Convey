using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Convey.CQRS.Queries;
using Convey.Types;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Convey.Persistence.MongoDB.Repositories
{
	internal class MongoRepository<TEntity, TIdentifiable> : IMongoRepository<TEntity, TIdentifiable>
		where TEntity : IIdentifiable<TIdentifiable>
	{
		public MongoRepository(IMongoDatabase database, string collectionName)
		{
			Collection = database.GetCollection<TEntity>(collectionName);
		}

		public IMongoCollection<TEntity> Collection { get; }

		public async Task<TEntity> GetAsync(TIdentifiable id)
			=> await GetAsync(e => e.Id.Equals(id));

		public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
			=> await Collection.Find(predicate).SingleOrDefaultAsync();

		public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
			=> await Collection.Find(predicate).ToListAsync();

		public async Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate,
			TQuery query) where TQuery : IPagedQuery
			=> await Collection.AsQueryable().Where(predicate).PaginateAsync(query);

		public async Task AddAsync(TEntity entity)
			=> await Collection.InsertOneAsync(entity);

		public async Task UpdateAsync(TEntity entity)
			=> await Collection.ReplaceOneAsync(e => e.Id.Equals(entity.Id), entity);

		public async Task DeleteAsync(TIdentifiable id)
			=> await Collection.DeleteOneAsync(e => e.Id.Equals(id));

		public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
			=> await Collection.Find(predicate).AnyAsync();
	}
}