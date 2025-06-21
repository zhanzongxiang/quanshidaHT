﻿// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core.Service;

/// <summary>
/// 系统菜单返回结果
/// </summary>
public class MenuOutput
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父Id
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 菜单类型（0目录 1菜单 2按钮）
    /// </summary>
    public MenuTypeEnum Type { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 路由地址
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    /// 权限标识
    /// </summary>
    public string Permission { get; set; }

    /// <summary>
    /// 重定向
    /// </summary>
    public string Redirect { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public virtual DateTime UpdateTime { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public virtual string CreateUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public virtual string UpdateUserName { get; set; }

    /// <summary>
    /// 菜单Meta
    /// </summary>
    public SysMenuMeta Meta { get; set; }

    /// <summary>
    /// 菜单子项
    /// </summary>
    public List<MenuOutput> Children { get; set; }
}

/// <summary>
/// 菜单Meta配置
/// </summary>
public class SysMenuMeta
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 是否内嵌
    /// </summary>
    public bool IsIframe { get; set; }

    /// <summary>
    /// 外链链接
    /// </summary>
    public string IsLink { get; set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    public bool IsHide { get; set; }

    /// <summary>
    /// 是否缓存
    /// </summary>
    public bool IsKeepAlive { get; set; }

    /// <summary>
    /// 是否固定
    /// </summary>
    public bool IsAffix { get; set; }
}

/// <summary>
/// 配置菜单对象映射
/// </summary>
public class SysMenuMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<SysMenu, MenuOutput>()
            .Map(t => t.Meta.Title, o => o.Title)
            .Map(t => t.Meta.Icon, o => o.Icon)
            .Map(t => t.Meta.IsIframe, o => o.IsIframe)
            .Map(t => t.Meta.IsLink, o => o.OutLink)
            .Map(t => t.Meta.IsHide, o => o.IsHide)
            .Map(t => t.Meta.IsKeepAlive, o => o.IsKeepAlive)
            .Map(t => t.Meta.IsAffix, o => o.IsAffix);
    }
}