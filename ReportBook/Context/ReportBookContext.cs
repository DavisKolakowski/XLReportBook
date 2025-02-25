namespace ReportBook.Context
{
    using System.Collections.Generic;
    using System;
    using ReportBook.Models;

    public abstract class ReportBookContext
    {
        protected Dictionary<Type, ReportStore> reportStoresByType = new Dictionary<Type, ReportStore>();

        protected ReportBookContext()
        {
            InitializeStores();
        }

        private void InitializeStores()
        {
            Type type = GetType();
            foreach (var prop in type.GetProperties())
            {
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ReportStore<>))
                {
                    var reportType = prop.PropertyType.GetGenericArguments()[0];
                    var storeName = prop.Name;
                    var store = (ReportStore)Activator.CreateInstance(prop.PropertyType, storeName);
                    prop.SetValue(this, store);
                    reportStoresByType[reportType] = store;
                }
            }
        }

        public void Refresh<TSchema>(IEnumerable<TSchema> data) where TSchema : Report, new()
        {
            if (reportStoresByType.TryGetValue(typeof(TSchema), out var store))
            {
                if (store is ReportStore<TSchema> typedStore)
                {
                    typedStore.Refresh(data);
                }
            }
            else
            {
                throw new ArgumentException($"No ReportStore registered for type {typeof(TSchema).Name}");
            }
        }

        public void Clear()
        {
            foreach (var store in reportStoresByType.Values)
            {
                store.Data.Rows.Clear();
            }
        }
    }
}
