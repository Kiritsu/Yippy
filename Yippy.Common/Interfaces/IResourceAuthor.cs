namespace Yippy.Common.Interfaces;

public interface IResourceAuthor : IResource
{
    Guid UserId { get; }
}

public interface IResource
{
    Guid Id { get; }
}