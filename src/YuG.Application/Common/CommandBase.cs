using MediatR;

namespace YuG.Application.Common;

/// <summary>
/// 命令基类，所有写操作命令应继承此类
/// </summary>
/// <typeparam name="TResponse">命令响应类型</typeparam>
public abstract class CommandBase<TResponse> : IRequest<TResponse>
{
}
