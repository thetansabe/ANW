using Amnhac.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Amnhac.Interfaces
{
    public interface IRepository<Model>
    {
        /// <summary>
        /// Query to the database and return the target object
        /// </summary>
        /// <param name="id">The ID property of the record</param>
        /// <returns>Target object</returns>
        Task<Model> GetById(string id);

        /// <summary>
        /// Get all records from the database
        /// </summary>
        /// <returns>A list of records</returns>
        Task<List<Model>> GetAllAsync();
        /// <summary>
        /// Get all records from the database but limit the records
        /// </summary>
        /// <param name="limit">Number of result records</param>
        /// <param name="offset">Offset of the query set</param>
        /// <returns>A list of records</returns>
        Task<List<Model>> GetAll(int limit, int offset);
        /// <summary>
        /// Find all records that met the given expression.
        /// </summary>
        /// <param name="filter">Lambda Expression</param>
        /// <returns>A list of found records</returns>
        Task<List<Model>> Find(Expression<Func<Model, bool>> filter);
        /// <summary>
        /// Find all records that met the given expression. The extracted result will be limited.
        /// </summary>
        /// <param name="filter">Lambda Expression</param>
        /// <param name="limit">Maximum number of extracted records</param>
        /// <param name="offset">Offset of the query set</param>
        /// <returns></returns>
        Task<List<Model>> Find(Expression<Func<Model, bool>> filter, int limit, int offset);

        /// <summary>
        /// Insert into database the given object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>The result of execution</returns>
        Task<bool> Insert(Model obj);
        /// <summary>
        /// Update a target record.
        /// </summary>
        /// <param name="id">ID of the record</param>
        /// <param name="obj">The record data</param>
        /// <returns>The result of execution</returns>
        Task<bool> Update(string id, Model obj);
        /// <summary>
        /// Remove record from the collection
        /// </summary>
        /// <param name="id">ID of the record</param>
        /// <returns>The result of execution</returns>
        Task<bool> Delete(string id);
        /// <summary>
        /// Return number of records in the collection
        /// </summary>
        /// <returns>Number of records</returns>
        Task<long> CountAll();
        /// <summary>
        /// Return number of records which met the given expression.
        /// </summary>
        /// <param name="filter">Lambda Expression</param>
        /// <returns>Number of found records</returns>
        Task<long> CountIf(Expression<Func<Model, bool>> filter);
    }
}
