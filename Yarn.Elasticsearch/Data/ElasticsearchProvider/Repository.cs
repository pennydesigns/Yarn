﻿using System.Linq;
using Yarn.Reflection;
using Yarn.Specification;

namespace Yarn.Elasticsearch.Data.ElasticsearchProvider
{
    public class Repository : IRepository, IMetaDataProvider, IBulkOperationsProvider
    {
        private readonly DataContext _context;

        public Repository(string connection, string userName = null, string password = null)
        {
            _context = new DataContext(connection, userName, password);
        }

        public T GetById<T, ID>(ID id) where T : class
        {
            var response = _context.Session.Client.Get<T>(s => s.Id(id + ""));
            return response.Found ? response.Source : null;
        }

        public T Find<T>(ISpecification<T> criteria) where T : class
        {
            return Find(((Specification<T>)criteria).Predicate);
        }

        public T Find<T>(System.Linq.Expressions.Expression<System.Func<T, bool>> criteria) where T : class
        {
            return _context.Session.LinqClient.Query<T>().FirstOrDefault(criteria);
        }

        public System.Collections.Generic.IEnumerable<T> FindAll<T>(ISpecification<T> criteria, int offset = 0, int limit = 0, System.Linq.Expressions.Expression<System.Func<T, object>> orderBy = null) where T : class
        {
            return FindAll(((Specification<T>)criteria).Predicate, offset, limit, orderBy);
        }

        public System.Collections.Generic.IEnumerable<T> FindAll<T>(System.Linq.Expressions.Expression<System.Func<T, bool>> criteria, int offset = 0, int limit = 0, System.Linq.Expressions.Expression<System.Func<T, object>> orderBy = null) where T : class
        {
            var query = _context.Session.LinqClient.Query<T>().Where(criteria);
            if (offset > 0)
            {
                query = query.Skip(offset);
            }
            if (limit > 0)
            {
                query = query.Take(limit);
            }
            if (orderBy != null)
            {
                query = query.OrderBy(orderBy);
            }
            return query;
        }

        public System.Collections.Generic.IList<T> Execute<T>(string command, ParamList parameters) where T : class
        {
            throw new System.NotImplementedException();
        }

        public T Add<T>(T entity) where T : class
        {
            var response = _context.Session.Client.Index(entity);
            return response.Created ? entity : null;
        }

        public T Remove<T>(T entity) where T : class
        {
            var response = _context.Session.Client.Delete<T>(d => d.IdFrom(entity));
            return response.Found ? entity : null;
        }

        public T Remove<T, ID>(ID id) where T : class
        {
            _context.Session.Client.Delete<T>(d => d.Id(id + ""));
            return null;
        }

        public T Update<T>(T entity) where T : class
        {
            var response = _context.Session.Client.Update<T>(u => u.Doc(entity));
            return response.IsValid ? entity : null;
        }

        public long Count<T>() where T : class
        {
            return _context.Session.LinqClient.Query<T>().Count();
        }

        public long Count<T>(ISpecification<T> criteria) where T : class
        {
            return Count(((Specification<T>)criteria).Predicate);
        }

        public long Count<T>(System.Linq.Expressions.Expression<System.Func<T, bool>> criteria) where T : class
        {
            return _context.Session.LinqClient.Query<T>().Count(criteria);
        }

        public System.Linq.IQueryable<T> All<T>() where T : class
        {
            return _context.Session.LinqClient.Query<T>();
        }

        public void Detach<T>(T entity) where T : class
        {
            throw new System.NotImplementedException();
        }

        public void Attach<T>(T entity) where T : class
        {
            throw new System.NotImplementedException();
        }

        public IDataContext DataContext
        {
            get { return _context; }
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        string[] IMetaDataProvider.GetPrimaryKey<T>()
        {
            var response = _context.Session.Client.GetMapping<T>();
            return response.IsValid && response.Mapping != null && response.Mapping.IdFieldMappingDescriptor != null ? new[] { response.Mapping.IdFieldMappingDescriptor.Path } : new string[] { };
        }

        object[] IMetaDataProvider.GetPrimaryKeyValue<T>(T entity)
        {
            var idPropertyName = _context.Session.Client.Infer.Id(entity);
            return new[] { PropertyAccessor.Get(entity, idPropertyName) };
        }

        public System.Collections.Generic.IEnumerable<T> GetById<T, ID>(System.Collections.Generic.IEnumerable<ID> ids) where T : class
        {
            var response = _context.Session.Client.MultiGet(m => m.GetMany<T>(ids.Select(id => id + "")));
            return response.IsValid ? response.SourceMany<T>(ids.Select(id => id + "")) : Enumerable.Empty<T>();
        }

        public long Insert<T>(System.Collections.Generic.IEnumerable<T> entities) where T : class
        {
            var response = _context.Session.Client.Bulk(b => b.IndexMany(entities));
            return response.Items.LongCount();
        }

        public long Update<T>(System.Linq.Expressions.Expression<System.Func<T, bool>> criteria, System.Linq.Expressions.Expression<System.Func<T, T>> update) where T : class
        {
            throw new System.NotImplementedException();
        }

        public long Update<T>(params BulkUpdateOperation<T>[] bulkOperations) where T : class
        {
            throw new System.NotImplementedException();
        }

        public long Delete<T>(System.Collections.Generic.IEnumerable<T> entities) where T : class
        {
            var response = _context.Session.Client.Bulk(b => b.DeleteMany(entities));
            return response.Items.LongCount();
        }

        public long Delete<T, ID>(System.Collections.Generic.IEnumerable<ID> ids) where T : class
        {
            var response = _context.Session.Client.Bulk(b => b.DeleteMany(ids.Select(id => id + "")));
            return response.Items.LongCount();
        }

        public long Delete<T>(params System.Linq.Expressions.Expression<System.Func<T, bool>>[] criteria) where T : class
        {
            throw new System.NotImplementedException();
        }

        public long Delete<T>(params ISpecification<T>[] criteria) where T : class
        {
            throw new System.NotImplementedException();
        }
    }
}
