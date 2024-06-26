using CleanCore.Application.Commands;
using CleanCore.Application.Services;
using CleanCore.Domain.Common;
using CleanCore.Domain.Events;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CleanCore.Application.Handlers;

/// <summary>
/// The delete entity command handler class
/// </summary>
/// <seealso cref="IRequestHandler{DeleteEntityCommand, ErrorOr}"/>
public abstract class DeleteEntityCommandHandler<TId, TEntity, TDto> : IRequestHandler<DeleteEntityCommand<TId, TEntity, TDto>, ErrorOr<TDto>>
    where TId : IEquatable<TId>
    where TEntity : BaseEntity<TId, TEntity, TDto>
    where TDto : IEntityDto<TId, TEntity, TDto>
{
    /// <summary>
    /// The context
    /// </summary>
    private readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteEntityCommandHandler{TId,TEntity,TDto}"/> class
    /// </summary>
    /// <param name="context">The context</param>
    public DeleteEntityCommandHandler(DbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing an error or of t dto</returns>
    public virtual async Task<ErrorOr<TDto>> Handle(DeleteEntityCommand<TId, TEntity, TDto> request, CancellationToken cancellationToken)
    {
        return await _context.Set<TEntity>().Find(request.Id)
            .ToErrorOr()
            .FailIf(entity => entity == null, Error.NotFound(description: $"{request.Id} not found"))
            .Then(entity => entity!.AddDomainEvent(new EntityDeletedEvent<TId, TEntity, TDto>(entity)))
            .ThenAsync<TEntity>(async entity =>
            {
                try
                {
                    _context.Set<TEntity>().Remove(entity);
                    await _context.Set<TEntity>().AddAsync(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    return entity;
                }
                catch (Exception ex)
                {
                    return Error.Conflict(ex.Message);
                }
            })
            .Then(entity => entity.ToDto());
    }
}
/// <summary>
/// The delete elastic entity command handler class
/// </summary>
/// <seealso cref="BaseElasticCommandHandler{TId, TEntity, TDto}"/>
/// <seealso cref="IRequestHandler{DeleteEntityCommand, ErrorOr}"/>
public abstract class DeleteElasticEntityCommandHandler<TId, TEntity, TDto> : BaseElasticCommandHandler<TId, TEntity, TDto>, IRequestHandler<DeleteEntityCommand<TId, TEntity, TDto>, ErrorOr<TDto>>
    where TId : IEquatable<TId>
    where TEntity : BaseEntity<TId, TEntity, TDto>
    where TDto : IEntityDto<TId, TEntity, TDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteElasticEntityCommandHandler{TId,TEntity,TDto}"/> class
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="userProvider">The user provider</param>
    protected DeleteElasticEntityCommandHandler(IConfiguration configuration, IUserProvider userProvider) : base(configuration, userProvider)
    {
    }

    /// <summary>
    /// Handles the request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing an error or of t dto</returns>
    public virtual async Task<ErrorOr<TDto>> Handle(DeleteEntityCommand<TId, TEntity, TDto> request, CancellationToken cancellationToken)
    {
        return await (await GetAsync(request.Id))
            .FailIf(entity => entity == null, Error.NotFound(description: $"{request.Id} not found"))
            .Then(entity => entity!.AddDomainEvent(new EntityDeletedEvent<TId, TEntity, TDto>(entity)))
            .ThenAsync(async entity =>
            {
                try
                {
                    return (await DeleteAsync(entity.Id!)).Then(_ =>
                    {
                        entity.Deleted();
                        return entity;
                    });
                }
                catch (Exception ex)
                {
                    return Error.Conflict(ex.Message);
                }
            })
            .Then(entity => entity.ToDto());
    }
}