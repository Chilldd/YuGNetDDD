using MediatR;
using YuG.Domain.Exceptions;
using YuG.Domain.Repositories;

namespace YuG.Application.Resource.Commands.Delete;

/// <summary>
/// 删除资源命令处理器
/// </summary>
public class DeleteResourceCommandHandler : IRequestHandler<DeleteResourceCommand, Unit>
{
    private readonly IResourceRepository _resourceRepository;

    /// <summary>
    /// 初始化删除资源命令处理器
    /// </summary>
    /// <param name="resourceRepository">资源仓储</param>
    public DeleteResourceCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    /// <summary>
    /// 处理删除资源命令
    /// </summary>
    /// <param name="request">删除资源命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Unit</returns>
    public async Task<Unit> Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        // 获取资源
        var resource = await _resourceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (resource == null)
        {
            throw new DomainException($"资源 '{request.Id}' 不存在");
        }

        // 删除资源
        _resourceRepository.Delete(resource);
        await _resourceRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
