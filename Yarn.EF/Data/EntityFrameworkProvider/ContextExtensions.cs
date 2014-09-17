﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Entity.Core.Metadata.Edm;

namespace Yarn.Data.EntityFrameworkProvider
{
    public static class ContextExtensions
    {
        public static string GetTableName<T>(this DbContext context) where T : class
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            return objectContext.GetTableName<T>();
        }

        //public static string GetTableName<T>(this ObjectContext context) where T : class
        //{
        //    var sql = context.CreateObjectSet<T>().ToTraceString();
        //    var regex = new Regex("FROM\\s+(?<table>.*)\\s+AS");
        //    var match = regex.Match(sql);

        //    var table = match.Groups["table"].Value;
        //    return table;
        //}

        //public static string GetColumnName<T>(this DbContext context, string name) where T : class
        //{
        //    var objectContext = ((IObjectContextAdapter)context).ObjectContext;
        //    var tableName = objectContext.GetTableName<T>();
            
        //    var itemCollection = objectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
        //    GlobalItem i;
        //    if (itemCollection == null)
        //    {
        //        return null;
        //    }

        //    return null;
        //}

        public static string GetTableName<T>(this ObjectContext context) where T : class
        {
            var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);
            var sets = container.BaseEntitySets.Where(e => e.BuiltInTypeKind == BuiltInTypeKind.EntitySet);
            return sets.Cast<EntitySet>().Where(e => e.ElementType.Name == typeof(T).Name).Select(e => e.Name).FirstOrDefault();
        }
    }
}
