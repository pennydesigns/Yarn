﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Yarn.Extensions;

namespace Yarn.Adapters
{
    public class AuditableRepository : RepositoryAdapter
    {
        private readonly IPrincipal _principal;

        public AuditableRepository(IRepository repository, IPrincipal principal) 
            : base(repository)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }
            _principal = principal;
        }

        public override T Add<T>(T entity)
        {
            var auditable = entity as IAuditable;
            if (auditable != null)
            {
                auditable.AuditId = Guid.NewGuid();
                auditable.CreateDate = DateTime.UtcNow;
                if (_principal != null)
                {
                    auditable.CreatedBy = _principal.Identity.Name;
                }
                
                auditable.Cascade((root, item) =>
                {
                    if (item.CreateDate == DateTime.MinValue || !item.AuditId.HasValue)
                    {
                        item.CreateDate = root.CreateDate;
                        item.CreatedBy = root.CreatedBy;
                        item.AuditId = root.AuditId;
                    }
                });
            }
            return _repository.Add(entity);
        }

        public override T Update<T>(T entity)
        {
            BeforeUpdate(entity);
            return _repository.Update(entity);
        }

        private void BeforeUpdate<T>(T entity) where T : class
        {
            var auditable = entity as IAuditable;
            if (auditable != null)
            {
                auditable.AuditId = Guid.NewGuid();
                auditable.UpdateDate = DateTime.UtcNow;
                if (_principal != null)
                {
                    auditable.UpdatedBy = _principal.Identity.Name;
                }

                auditable.Cascade((root, item) =>
                {
                    if (item.CreateDate == DateTime.MinValue)
                    {
                        item.CreateDate = DateTime.UtcNow;
                        item.CreatedBy = root.CreatedBy;
                    }
                    else
                    {
                        auditable.UpdateDate = root.UpdateDate;
                        auditable.UpdatedBy = root.UpdatedBy;
                    }
                    item.AuditId = root.AuditId;
                });
            }
        }

        public override ILoadService<T> Load<T>()
        {
            var provider = _repository as ILoadServiceProvider;
            if (provider != null)
            {
                return new LoadService<T>(this, provider.Load<T>());
            }
            throw new InvalidOperationException();
        }

        private class LoadService<T> : ILoadService<T>
            where T : class
        {
            private readonly AuditableRepository _repository;
            private readonly ILoadService<T> _service;
            
            public LoadService(AuditableRepository repository, ILoadService<T> service)
            {
                _repository = repository;
                _service = service;
            }

            public ILoadService<T> Include<TProperty>(Expression<Func<T, TProperty>> path) where TProperty : class
            {
                _service.Include(path);
                return this;
            }

            public T Update(T entity)
            {
                _repository.BeforeUpdate(entity);
                return _service.Update(entity);
            }

            public T Find(Expression<Func<T, bool>> criteria)
            {
                return _service.Find(criteria);
            }

            public IEnumerable<T> FindAll(Expression<Func<T, bool>> criteria, int offset = 0, int limit = 0, Expression<Func<T, object>> orderBy = null)
            {
                return _service.FindAll(criteria, offset, limit, orderBy);
            }

            public T Find(ISpecification<T> criteria)
            {
                return _service.Find(criteria);
            }

            public IEnumerable<T> FindAll(ISpecification<T> criteria, int offset = 0, int limit = 0, Expression<Func<T, object>> orderBy = null)
            {
                return _service.FindAll(criteria, offset, limit, orderBy);
            }

            public IQueryable<T> All()
            {
                return _service.All();
            }

            public void Dispose()
            {
                _service.Dispose();
            }
        }
    }
}
