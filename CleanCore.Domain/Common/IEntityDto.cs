namespace CleanCore.Domain.Common;

/// <summary>
/// The entity dto interface
/// </summary>
public interface IEntityDto<TId, TEntity, TDto>
    where TId : IEquatable<TId>
    where TEntity : BaseEntity<TId, TEntity, TDto>
    where TDto : IEntityDto<TId, TEntity, TDto>
{
    /// <summary>
    /// Gets the value of the id
    /// </summary>
    TId? Id { get; set; }
    /// <summary>
    /// Gets the value of the created at
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }
    /// <summary>
    /// Gets the value of the created by
    /// </summary>
    string? CreatedBy { get; set; }
    /// <summary>
    /// Gets the value of the last modified at
    /// </summary>
    DateTimeOffset LastModifiedAt { get; set; }
    /// <summary>
    /// Gets the value of the last modified by
    /// </summary>
    string? LastModifiedBy { get; set; }
    /// <summary>
    /// Gets the value of the deleted at
    /// </summary>
    DateTimeOffset? DeletedAt { get; set; }
    /// <summary>
    /// Gets the value of the deleted by
    /// </summary>
    string? DeletedBy { get; set; }
    /// <summary>
    /// Returns the entity
    /// </summary>
    /// <returns>The entity</returns>
    TEntity ToEntity();
}
