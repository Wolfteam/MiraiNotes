﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MiraiNotes.Core.Dto;

namespace MiraiNotes.Abstractions.Data
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Adds a entity of type <paramref name="entity"/> to the db
        /// </summary>
        /// <param name="entity">TEntity to add</param>
        /// <returns><see cref="ResponseDto{TEntity}"/></returns>
        Task<ResponseDto<TEntity>> AddAsync(TEntity entity);

        /// <summary>
        /// Adds multiple entities of type <paramref name="entities"/> to the db
        /// </summary>
        /// <param name="entities">Entities to add</param>
        /// <returns><see cref="ResponseDto{IEnumerable{TEntity}}"/></returns>
        Task<ResponseDto<IEnumerable<TEntity>>> AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Checks if a entity matches with the <paramref name="predicate"/> passed
        /// </summary>
        /// <param name="predicate">A predicate for filtering</param>
        /// <returns>True in case of exists, otherwise false</returns>
        Task<ResponseDto<bool>> ExistsAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets the first entity that matches <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">A predicate for filtering</param>
        /// <returns><see cref="ResponseDto{TEntity}"/></returns>
        Task<ResponseDto<TEntity>> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets the first entity that matches <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">A predicate for filtering</param>
        /// <returns><see cref="ResponseDto{TEntity}"/></returns>
        Task<ResponseDto<TEntity>> FirstOrDefaultAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets an entity with pk equal to <paramref name="id"/>
        /// </summary>
        /// <param name="id">Entity pk</param>
        /// <returns><see cref="ResponseDto{TEntity}"/></returns>
        Task<ResponseDto<TEntity>> GetByIdAsync(object id);

        /// <summary>
        /// Gets an enumerable of TEntity according to <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">A predicate for filtering</param>
        /// <returns><see cref="ResponseDto{IEnumerable{TEntity}}"/></returns>
        Task<ResponseDto<IEnumerable<TEntity>>> GetAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets all the <see cref="TEntity"/> according to the filter
        /// </summary>
        /// <param name="filter">A predicate used to filter</param>
        /// <param name="includeProperties">Included properties</param>
        /// <returns><see cref="ResponseDto{TEntity}"/></returns>
        Task<ResponseDto<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter,
            string includeProperties = "");

        /// <summary>
        /// Gets all the <see cref="TEntity"/> according to the filter
        /// and may be ordered. 
        /// </summary>
        /// <param name="filter">A predicate used to filter</param>
        /// <param name="orderBy">An expresion used to order</param>
        /// <param name="includeProperties">Included properties</param>
        /// <returns><see cref="ResponseDto{IEnumerable{TEntity}}"/></returns>
        Task<ResponseDto<IEnumerable<TEntity>>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");

        /// <summary>
        /// Gets all the <see cref="TEntity"/> according to the filter
        /// and may be ordered. 
        /// </summary>
        /// <param name="filter">A predicate used to filter</param>
        /// <param name="orderBy">An expresion used to order</param>
        /// <param name="includeProperties">Included properties</param>
        /// <returns><see cref="ResponseDto{IEnumerable{TEntity}}"/></returns>
        Task<ResponseDto<IEnumerable<TEntity>>> GetAsNoTrackingAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");

        /// <summary>
        /// Gets all <see cref="TEntity"/>
        /// </summary>
        /// <returns><see cref="ResponseDto{IEnumerable{TEntity}}"/></returns>
        Task<ResponseDto<IEnumerable<TEntity>>> GetAllAsync();

        /// <summary>
        /// Gets all <see cref="TEntity"/>
        /// </summary>
        /// <returns><see cref="ResponseDto{IEnumerable{TEntity}}"/></returns>
        Task<ResponseDto<IEnumerable<TEntity>>> GetAllAsNoTrackingAsync();

        /// <summary>
        /// Removes the entity indicated by <paramref name="id"/>
        /// <paramref name="id"/> must be a PK
        /// </summary>
        /// <param name="id">Entity pk</param>
        /// <returns><see cref="EmptyResponseDto"/></returns>
        Task<EmptyResponseDto> RemoveAsync(object id);

        /// <summary>
        /// Removes all the entites that matches the <paramref name="filter"/>
        /// </summary>
        /// <param name="filter">The filter that will be applied</param>
        /// <returns><see cref="EmptyResponseDto"/></returns>
        Task<EmptyResponseDto> RemoveAsync(Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Removes a entity
        /// </summary>
        /// <param name="entity">Entity to remove</param>
        /// <returns><see cref="EmptyResponseDto"/></returns>
        Task<EmptyResponseDto> RemoveAsync(TEntity entity);

        /// <summary>
        /// Removes the entities specied in <paramref name="entities"/>
        /// </summary>
        /// <param name="entities">IEnumerable of entities to remove</param>
        /// <returns><see cref="EmptyResponseDto"/></returns>
        Task<EmptyResponseDto> RemoveRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Updates a TEntity
        /// </summary>
        /// <param name="entity"><see cref="TEntity"/> to update</param>
        /// <returns><see cref="EmptyResponseDto"/></returns>
        Task<ResponseDto<TEntity>> UpdateAsync(TEntity entity);

        /// <summary>
        /// Updates multiple entities
        /// </summary>
        /// <param name="entities"><see cref="TEntity"/> to update</param>
        /// <returns><see cref="ResponseDto{IEnumerable{TEntity}}"/></returns>
        Task<ResponseDto<IEnumerable<TEntity>>> UpdateRangeAsync(IEnumerable<TEntity> entities);
    }
}