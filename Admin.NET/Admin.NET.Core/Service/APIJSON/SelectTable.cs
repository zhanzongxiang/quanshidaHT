// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using AspectCore.Extensions.Reflection;
using System.Dynamic;

namespace Admin.NET.Core.Service;

/// <summary>
///
/// </summary>
public class SelectTable : ISingleton
{
    private readonly IdentityService _identitySvc;
    private readonly TableMapper _tableMapper;
    private readonly ISqlSugarClient _db;

    public SelectTable(IdentityService identityService, TableMapper tableMapper, ISqlSugarClient dbClient)
    {
        _identitySvc = identityService;
        _tableMapper = tableMapper;
        _db = dbClient;
    }

    /// <summary>
    /// 判断表名是否正确，若不正确则抛异常
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public virtual bool IsTable(string table)
    {
        return _db.DbMaintenance.GetTableInfoList().Any(it => it.Name.Equals(table, StringComparison.CurrentCultureIgnoreCase))
            ? true
            : throw new Exception($"表名【{table}】不正确！");
    }

    /// <summary>
    /// 判断表的列名是否正确,如果不正确则抛异常，更早地暴露给调用方
    /// </summary>
    /// <param name="table"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public virtual bool IsCol(string table, string col)
    {
        return _db.DbMaintenance.GetColumnInfosByTableName(table).Any(it => it.DbColumnName.Equals(col, StringComparison.CurrentCultureIgnoreCase))
            ? true
            : throw new Exception($"表【{table}】不存在列【{col}】！请检查输入参数");
    }

    /// <summary>
    /// 查询列表数据
    /// </summary>
    /// <param name="subtable"></param>
    /// <param name="page"></param>
    /// <param name="count"></param>
    /// <param name="query"></param>
    /// <param name="json"></param>
    /// <param name="dd"></param>
    /// <returns></returns>
    public virtual Tuple<dynamic, int> GetTableData(string subtable, int page, int count, int query, string json, JObject dd)
    {
        var role = _identitySvc.GetSelectRole(subtable);
        if (!role.Item1)
            throw new Exception(role.Item2);

        var selectrole = role.Item2;
        subtable = _tableMapper.GetTableName(subtable);

        var values = JObject.Parse(json);
        page = values["page"] == null ? page : int.Parse(values["page"].ToString());
        count = values["count"] == null ? count : int.Parse(values["count"].ToString());
        query = values["query"] == null ? query : int.Parse(values["query"].ToString());
        values.Remove("page");
        values.Remove("count");
        // 构造查询过程
        var tb = SugarQueryable(subtable, selectrole, values, dd);

        // 实际会在这里执行
        if (query == 1) // 1-总数
        {
            return new Tuple<dynamic, int>(null, tb.MergeTable().Count());
        }
        else
        {
            if (page > 0) // 分页
            {
                int total = 0;
                if (query == 0)
                    return new Tuple<dynamic, int>(tb.ToPageList(page, count), total); // 0-对象
                else
                    return new Tuple<dynamic, int>(tb.ToPageList(page, count, ref total), total); // 2-以上全部
            }
            else // 列表
            {
                IList l = tb.ToList();
                return query == 0 ? new Tuple<dynamic, int>(l, 0) : new Tuple<dynamic, int>(l, l.Count);
            }
        }
    }

    /// <summary>
    /// 解析并查询
    /// </summary>
    /// <param name="queryJson"></param>
    /// <returns></returns>
    public virtual JObject Query(string queryJson)
    {
        var queryJobj = JObject.Parse(queryJson);
        return Query(queryJobj);
    }

    /// <summary>
    /// 单表查询
    /// </summary>
    /// <param name="queryObj"></param>
    /// <param name="nodeName">返回数据的节点名称  默认为 infos</param>
    /// <returns></returns>
    public virtual JObject QuerySingle(JObject queryObj, string nodeName = "infos")
    {
        var resultObj = new JObject();

        var total = 0;
        foreach (var item in queryObj)
        {
            var key = item.Key.Trim();
            if (key.EndsWith("[]"))
            {
                total = QuerySingleList(resultObj, item, nodeName);
            }
            else if (key.Equals("func"))
            {
                ExecFunc(resultObj, item);
            }
            else if (key.Equals("total@") || key.Equals("total"))
            {
                resultObj.Add("total", total);
            }
        }
        return resultObj;
    }

    /// <summary>
    /// 获取查询语句
    /// </summary>
    /// <param name="queryObj"></param>
    /// <returns></returns>
    public virtual string ToSql(JObject queryObj)
    {
        foreach (var item in queryObj)
        {
            if (item.Key.Trim().EndsWith("[]"))
                return ToSql(item);
        }
        return string.Empty;
    }

    /// <summary>
    /// 解析并查询
    /// </summary>
    /// <param name="queryObj"></param>
    /// <returns></returns>
    public virtual JObject Query(JObject queryObj)
    {
        var resultObj = new JObject();

        int total;
        foreach (var item in queryObj)
        {
            var key = item.Key.Trim();
            if (key.Equals("[]")) // 列表
            {
                total = QueryMoreList(resultObj, item);
                resultObj.Add("total", total); // 只要是列表查询都自动返回总数
            }
            else if (key.EndsWith("[]"))
            {
                total = QuerySingleList(resultObj, item);
            }
            else if (key.Equals("func"))
            {
                ExecFunc(resultObj, item);
            }
            else if (key.Equals("total@") || key.Equals("total"))
            {
                // resultObj.Add("total", total);
                continue;
            }
            else // 单条
            {
                var template = GetFirstData(key, item.Value.ToString(), resultObj);
                if (template != null)
                    resultObj.Add(key, JToken.FromObject(template));
            }
        }
        return resultObj;
    }

    // 动态调用方法
    private static object ExecFunc(string funcname, object[] param, Type[] types)
    {
        var method = typeof(FuncList).GetMethod(funcname);
        var reflector = method.GetReflector();
        var result = reflector.Invoke(new FuncList(), param);
        return result;
    }

    // 生成sql
    private string ToSql(string subtable, int page, int count, int query, string json)
    {
        var values = JObject.Parse(json);
        page = values["page"] == null ? page : int.Parse(values["page"].ToString());
        count = values["count"] == null ? count : int.Parse(values["count"].ToString());
        _ = values["query"] == null ? query : int.Parse(values["query"].ToString());
        values.Remove("page");
        values.Remove("count");
        subtable = _tableMapper.GetTableName(subtable);
        var tb = SugarQueryable(subtable, "*", values, null);
        var sqlObj = tb.Skip((page - 1) * count).Take(10).ToSql();
        return sqlObj.Key;
    }

    /// <summary>
    /// 查询第一条数据
    /// </summary>
    /// <param name="subtable"></param>
    /// <param name="json"></param>
    /// <param name="job"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private dynamic GetFirstData(string subtable, string json, JObject job)
    {
        var role = _identitySvc.GetSelectRole(subtable);
        if (!role.Item1)
            throw new Exception(role.Item2);

        var selectrole = role.Item2;
        subtable = _tableMapper.GetTableName(subtable);

        var values = JObject.Parse(json);
        values.Remove("page");
        values.Remove("count");
        var tb = SugarQueryable(subtable, selectrole, values, job).First();
        var dic = (IDictionary<string, object>)tb;
        foreach (var item in values.Properties().Where(it => it.Name.EndsWith("()")))
        {
            if (item.Value.IsNullOrEmpty())
            {
                var func = item.Value.ToString().Substring(0, item.Value.ToString().IndexOf("("));
                var param = item.Value.ToString().Substring(item.Value.ToString().IndexOf("(") + 1).TrimEnd(')');
                var types = new List<Type>();
                var paramss = new List<object>();
                foreach (var va in param.Split(','))
                {
                    types.Add(typeof(object));
                    paramss.Add(tb.Where(it => it.Key.Equals(va)).Select(i => i.Value));
                }
                dic[item.Name] = ExecFunc(func, paramss.ToArray(), types.ToArray());
            }
        }
        return tb;
    }

    // 单表查询,返回的数据在指定的NodeName节点
    private int QuerySingleList(JObject resultObj, KeyValuePair<string, JToken> item, string nodeName)
    {
        var key = item.Key.Trim();
        var jb = JObject.Parse(item.Value.ToString());
        int page = jb["page"] == null ? 0 : int.Parse(jb["page"].ToString());
        int count = jb["count"] == null ? 10 : int.Parse(jb["count"].ToString());
        int query = jb["query"] == null ? 2 : int.Parse(jb["query"].ToString()); // 默认输出数据和数量
        int total = 0;

        jb.Remove("page"); jb.Remove("count"); jb.Remove("query");

        var htt = new JArray();
        foreach (var t in jb)
        {
            var datas = GetTableData(t.Key, page, count, query, t.Value.ToString(), null);
            if (query > 0)
                total = datas.Item2;

            foreach (var data in datas.Item1)
            {
                htt.Add(JToken.FromObject(data));
            }
        }

        if (!string.IsNullOrEmpty(nodeName))
            resultObj.Add(nodeName, htt);
        else
            resultObj.Add(key, htt);

        return total;
    }

    // 生成sql
    private string ToSql(KeyValuePair<string, JToken> item)
    {
        var jb = JObject.Parse(item.Value.ToString());
        int page = jb["page"] == null ? 0 : int.Parse(jb["page"].ToString());
        int count = jb["count"] == null ? 10 : int.Parse(jb["count"].ToString());
        int query = jb["query"] == null ? 2 : int.Parse(jb["query"].ToString()); // 默认输出数据和数量

        jb.Remove("page"); jb.Remove("count"); jb.Remove("query");
        foreach (var t in jb)
        {
            return ToSql(t.Key, page, count, query, t.Value.ToString());
        }
        return string.Empty;
    }

    // 单表查询
    private int QuerySingleList(JObject resultObj, KeyValuePair<string, JToken> item)
    {
        var key = item.Key.TrimEnd("[]");
        return QuerySingleList(resultObj, item, key);
    }

    /// <summary>
    /// 多列表查询
    /// </summary>
    /// <param name="resultObj"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private int QueryMoreList(JObject resultObj, KeyValuePair<string, JToken> item)
    {
        int total = 0;

        var jb = JObject.Parse(item.Value.ToString());
        var page = jb["page"] == null ? 0 : int.Parse(jb["page"].ToString());
        var count = jb["count"] == null ? 10 : int.Parse(jb["count"].ToString());
        var query = jb["query"] == null ? 2 : int.Parse(jb["query"].ToString()); // 默认输出数据和数量
        jb.Remove("page"); jb.Remove("count"); jb.Remove("query");
        var htt = new JArray();
        List<string> tables = new List<string>(), where = new List<string>();
        foreach (var t in jb)
        {
            tables.Add(t.Key); where.Add(t.Value.ToString());
        }
        if (tables.Count > 0)
        {
            string table = tables[0].TrimEnd("[]");
            var temp = GetTableData(table, page, count, query, where[0], null);
            if (query > 0)
                total = temp.Item2;

            // 关联查询，先查子表数据，再根据外键循环查询主表
            foreach (var dd in temp.Item1)
            {
                var zht = new JObject
                {
                    { table, JToken.FromObject(dd) }
                };
                for (int i = 1; i < tables.Count; i++) // 从第二个表开始循环
                {
                    string subtable = tables[i];
                    // 有bug，暂不支持[]分支
                    //if (subtable.EndsWith("[]"))
                    //{
                    //   string tableName = subtable.TrimEnd("[]".ToCharArray());
                    //    var jbb = JObject.Parse(where[i]);
                    //    page = jbb["page"] == null ? 0 : int.Parse(jbb["page"].ToString());
                    //    count = jbb["count"] == null ? 0 : int.Parse(jbb["count"].ToString());

                    //    var lt = new JArray();
                    //    foreach (var d in GetTableData(tableName, page, count, query, item.Value[subtable].ToString(), zht).Item1)
                    //    {
                    //        lt.Add(JToken.FromObject(d));
                    //    }
                    //    zht.Add(tables[i], lt);
                    //}
                    //else
                    //{
                    var ddf = GetFirstData(subtable, where[i].ToString(), zht);
                    if (ddf != null)
                        zht.Add(subtable, JToken.FromObject(ddf));
                }
                htt.Add(zht);
            }
        }
        if (query != 1)
            resultObj.Add("[]", htt);

        // 分页自动添加当前页数和数量
        if (page > 0 && count > 0)
        {
            resultObj.Add("page", page);
            resultObj.Add("count", count);
            resultObj.Add("max", (int)Math.Ceiling((decimal)total / count));
        }

        return total;
    }

    // 执行方法
    private void ExecFunc(JObject resultObj, KeyValuePair<string, JToken> item)
    {
        var jb = JObject.Parse(item.Value.ToString());

        var dataJObj = new JObject();
        foreach (var f in jb)
        {
            var types = new List<Type>();
            var param = new List<object>();
            foreach (var va in JArray.Parse(f.Value.ToString()))
            {
                types.Add(typeof(object));
                param.Add(va);
            }
            dataJObj.Add(f.Key, JToken.FromObject(ExecFunc(f.Key, param.ToArray(), types.ToArray())));
        }
        resultObj.Add("func", dataJObj);
    }

    /// <summary>
    /// 构造查询过程
    /// </summary>
    /// <param name="subtable"></param>
    /// <param name="selectrole"></param>
    /// <param name="values"></param>
    /// <param name="dd"></param>
    /// <returns></returns>
    private ISugarQueryable<ExpandoObject> SugarQueryable(string subtable, string selectrole, JObject values, JObject dd)
    {
        IsTable(subtable);

        var tb = _db.Queryable(subtable, "tb");

        // select
        if (!values["@column"].IsNullOrEmpty())
        {
            ProcessColumn(subtable, selectrole, values, tb);
        }
        else
        {
            tb.Select(selectrole);
        }

        // 前几行
        ProcessLimit(values, tb);

        // where
        ProcessWhere(subtable, values, tb, dd);

        // 排序
        ProcessOrder(subtable, values, tb);

        // 分组
        PrccessGroup(subtable, values, tb);

        // Having
        ProcessHaving(values, tb);

        return tb;
    }

    // 处理字段重命名 "@column":"toId:parentId"，对应SQL是toId AS parentId，将查询的字段toId变为parentId返回
    private void ProcessColumn(string subtable, string selectrole, JObject values, ISugarQueryable<ExpandoObject> tb)
    {
        var str = new System.Text.StringBuilder(100);
        foreach (var item in values["@column"].ToString().Split(','))
        {
            var ziduan = item.Split(':');
            var colName = ziduan[0];
            var ma = new Regex(@"\((\w+)\)").Match(colName);
            // 处理max、min这样的函数
            if (ma.Success && ma.Groups.Count > 1)
                colName = ma.Groups[1].Value;

            // 判断列表是否有权限  sum(1)、sum(*)、Count(1)这样的值直接有效
            if (colName == "*" || int.TryParse(colName, out int colNumber) || (IsCol(subtable, colName) && _identitySvc.ColIsRole(colName, selectrole.Split(','))))
            {
                if (ziduan.Length > 1)
                {
                    if (ziduan[1].Length > 20)
                        throw new Exception("别名不能超过20个字符");

                    str.Append(ziduan[0] + " as `" + ReplaceSQLChar(ziduan[1]) + "`,");
                }
                // 不对函数加``，解决sum(*)、Count(1)等不能使用的问题
                else if (ziduan[0].Contains('('))
                {
                    str.Append(ziduan[0] + ",");
                }
                else
                    str.Append("`" + ziduan[0] + "`" + ",");
            }
        }
        if (string.IsNullOrEmpty(str.ToString()))
            throw new Exception($"表名{subtable}没有可查询的字段！");

        tb.Select(str.ToString().TrimEnd(','));
    }

    /// <summary>
    /// 构造查询条件 where
    /// </summary>
    /// <param name="subtable"></param>
    /// <param name="values"></param>
    /// <param name="tb"></param>
    /// <param name="dd"></param>
    private void ProcessWhere(string subtable, JObject values, ISugarQueryable<ExpandoObject> tb, JObject dd)
    {
        var conModels = new List<IConditionalModel>();
        if (!values["identity"].IsNullOrEmpty())
            conModels.Add(new ConditionalModel() { FieldName = values["identity"].ToString(), ConditionalType = ConditionalType.Equal, FieldValue = _identitySvc.GetUserIdentity() });

        foreach (var va in values)
        {
            string key = va.Key.Trim();
            string fieldValue = va.Value.ToString();
            if (key.StartsWith("@"))
            {
                continue;
            }
            if (key.EndsWith("$")) // 模糊查询
            {
                FuzzyQuery(subtable, conModels, va);
            }
            else if (key.EndsWith("{}")) // 逻辑运算
            {
                ConditionQuery(subtable, conModels, va);
            }
            else if (key.EndsWith("%")) // bwtween查询
            {
                ConditionBetween(subtable, conModels, va, tb);
            }
            else if (key.EndsWith("@")) // 关联上一个table
            {
                if (dd == null)
                    continue;

                var str = fieldValue.Split('/');
                var lastTableRecord = ((JObject)dd[str[^2]]);
                if (!lastTableRecord.ContainsKey(str[^1]))
                    throw new Exception($"找不到关联列:{str}，请在{str[^2]}@column中设置");

                var value = lastTableRecord[str[^1]].ToString();
                conModels.Add(new ConditionalModel() { FieldName = key.TrimEnd('@'), ConditionalType = ConditionalType.Equal, FieldValue = value });
            }
            else if (key.EndsWith("~")) // 不等于（应该是正则匹配）
            {
                //conModels.Add(new ConditionalModel() { FieldName = key.TrimEnd('~'), ConditionalType = ConditionalType.NoEqual, FieldValue = fieldValue });
            }
            else if (IsCol(subtable, key.TrimEnd('!'))) // 其他where条件
            {
                ConditionEqual(subtable, conModels, va);
            }
        }
        if (conModels.Any())
            tb.Where(conModels);
    }

    // "@having":"function0(...)?value0;function1(...)?value1;function2(...)?value2..."，
    // SQL函数条件，一般和 @group一起用，函数一般在 @column里声明
    private static void ProcessHaving(JObject values, ISugarQueryable<ExpandoObject> tb)
    {
        if (!values["@having"].IsNullOrEmpty())
        {
            var hw = new List<IConditionalModel>();
            var havingItems = new List<string>();
            if (values["@having"].HasValues)
            {
                havingItems = values["@having"].Select(p => p.ToString()).ToList();
            }
            else
            {
                havingItems.Add(values["@having"].ToString());
            }
            foreach (var item in havingItems)
            {
                var and = item.ToString();
                var model = new ConditionalModel();
                if (and.Contains(">="))
                {
                    model.FieldName = and.Split(new string[] { ">=" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    model.ConditionalType = ConditionalType.GreaterThanOrEqual;
                    model.FieldValue = and.Split(new string[] { ">=" }, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                else if (and.Contains("<="))
                {
                    model.FieldName = and.Split(new string[] { "<=" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    model.ConditionalType = ConditionalType.LessThanOrEqual;
                    model.FieldValue = and.Split(new string[] { "<=" }, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                else if (and.Contains('>'))
                {
                    model.FieldName = and.Split(new string[] { ">" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    model.ConditionalType = ConditionalType.GreaterThan;
                    model.FieldValue = and.Split(new string[] { ">" }, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                else if (and.Contains('<'))
                {
                    model.FieldName = and.Split(new string[] { "<" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    model.ConditionalType = ConditionalType.LessThan;
                    model.FieldValue = and.Split(new string[] { "<" }, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                else if (and.Contains("!="))
                {
                    model.FieldName = and.Split(new string[] { "!=" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    model.ConditionalType = ConditionalType.NoEqual;
                    model.FieldValue = and.Split(new string[] { "!=" }, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                else if (and.Contains('='))
                {
                    model.FieldName = and.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    model.ConditionalType = ConditionalType.Equal;
                    model.FieldValue = and.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                hw.Add(model);
            }
            //var d = db.Context.Utilities.ConditionalModelToSql(hw);
            //tb.Having(d.Key, d.Value);
            tb.Having(string.Join(",", havingItems));
        }
    }

    // "@group":"column0,column1..."，分组方式。如果 @column里声明了Table的id，则id也必须在 @group中声明；其它情况下必须满足至少一个条件:
    // 1.分组的key在 @column里声明
    // 2.Table主键在 @group中声明
    private void PrccessGroup(string subtable, JObject values, ISugarQueryable<ExpandoObject> tb)
    {
        if (!values["@group"].IsNullOrEmpty())
        {
            var groupList = new List<GroupByModel>(); // 多库兼容写法
            foreach (var col in values["@group"].ToString().Split(','))
            {
                if (IsCol(subtable, col))
                {
                    // str.Append(and + ",");
                    groupList.Add(new GroupByModel() { FieldName = col });
                }
            }
            if (groupList.Any())
                tb.GroupBy(groupList);
        }
    }

    // 处理排序 "@order":"name-,id"查询按 name降序、id默认顺序 排序的User数组
    private void ProcessOrder(string subtable, JObject values, ISugarQueryable<ExpandoObject> tb)
    {
        if (!values["@order"].IsNullOrEmpty())
        {
            var orderList = new List<OrderByModel>(); // 多库兼容写法
            foreach (var item in values["@order"].ToString().Split(','))
            {
                string col = item.Replace("-", "").Replace("+", "").Replace(" desc", "").Replace(" asc", ""); // 增加对原生排序的支持
                if (IsCol(subtable, col))
                {
                    orderList.Add(new OrderByModel()
                    {
                        FieldName = col,
                        OrderByType = item.EndsWith("-") || item.EndsWith(" desc") ? OrderByType.Desc : OrderByType.Asc
                    });
                }
            }

            if (orderList.Any())
                tb.OrderBy(orderList);
        }
    }

    /// <summary>
    /// 表内参数"@count"(int)：查询前几行，不能同时使用count和@count函数
    /// </summary>
    /// <param name="values"></param>
    /// <param name="tb"></param>
    private static void ProcessLimit(JObject values, ISugarQueryable<ExpandoObject> tb)
    {
        if (!values["@count"].IsNullOrEmpty())
        {
            int c = values["@count"].ToObject<int>();
            tb.Take(c);
        }
    }

    // 条件查询 "key{}":"条件0,条件1..."，条件为任意SQL比较表达式字符串，非Number类型必须用''包含条件的值，如'a'
    // &, |, ! 逻辑运算符，对应数据库 SQL 中的 AND, OR, NOT。
    // 横或纵与：同一字段的值内条件默认 | 或连接，不同字段的条件默认 & 与连接。
    // ① & 可用于"key&{}":"条件"等
    // ② | 可用于"key|{}":"条件", "key|{}":[] 等，一般可省略
    // ③ ! 可单独使用，如"key!":Object，也可像&,|一样配合其他功能符使用
    private void ConditionQuery(string subtable, List<IConditionalModel> conModels, KeyValuePair<string, JToken> va)
    {
        var vakey = va.Key.Trim();
        var field = vakey.TrimEnd("{}".ToCharArray());
        var columnName = field.TrimEnd(new char[] { '&', '|' });
        IsCol(subtable, columnName);
        var ddt = new List<KeyValuePair<WhereType, ConditionalModel>>();
        foreach (var and in va.Value.ToString().Split(','))
        {
            var model = new ConditionalModel
            {
                FieldName = columnName
            };

            if (and.StartsWith(">="))
            {
                model.ConditionalType = ConditionalType.GreaterThanOrEqual;
                model.FieldValue = and.TrimStart(">=".ToCharArray());
            }
            else if (and.StartsWith("<="))
            {
                model.ConditionalType = ConditionalType.LessThanOrEqual;
                model.FieldValue = and.TrimStart("<=".ToCharArray());
            }
            else if (and.StartsWith(">"))
            {
                model.ConditionalType = ConditionalType.GreaterThan;
                model.FieldValue = and.TrimStart('>');
            }
            else if (and.StartsWith("<"))
            {
                model.ConditionalType = ConditionalType.LessThan;
                model.FieldValue = and.TrimStart('<');
            }
            model.CSharpTypeName = FuncList.GetValueCSharpType(model.FieldValue);
            ddt.Add(new KeyValuePair<WhereType, ConditionalModel>(field.EndsWith("!") ? WhereType.Or : WhereType.And, model));
        }
        conModels.Add(new ConditionalCollections() { ConditionalList = ddt });
    }

    /// <summary>
    /// "key%":"start,end" => "key%":["start,end"]，其中 start 和 end 都只能为 Boolean, Number, String 中的一种，如 "2017-01-01,2019-01-01" ，["1,90000", "82001,100000"] ，可用于连续范围内的筛选
    /// 目前不支持数组形式
    /// </summary>
    /// <param name="subtable"></param>
    /// <param name="conModels"></param>
    /// <param name="va"></param>
    /// <param name="tb"></param>
    private static void ConditionBetween(string subtable, List<IConditionalModel> conModels, KeyValuePair<string, JToken> va, ISugarQueryable<ExpandoObject> tb)
    {
        var vakey = va.Key.Trim();
        var field = vakey.TrimEnd("%".ToCharArray());
        var inValues = new List<string>();
        if (va.Value.HasValues)
        {
            foreach (var cm in va.Value)
            {
                inValues.Add(cm.ToString());
            }
        }
        else
        {
            inValues.Add(va.Value.ToString());
        }

        for (var i = 0; i < inValues.Count; i++)
        {
            var fileds = inValues[i].Split(',');
            if (fileds.Length == 2)
            {
                var type = FuncList.GetValueCSharpType(fileds[0]);
                ObjectFuncModel f = ObjectFuncModel.Create("between", field, $"{{{type}}}:{fileds[0]}", $"{{{type}}}:{fileds[1]}");
                tb.Where(f);
            }
        }
    }

    /// <summary>
    /// 等于、不等于、in 、not in
    /// </summary>
    /// <param name="subtable"></param>
    /// <param name="conModels"></param>
    /// <param name="va"></param>
    private void ConditionEqual(string subtable, List<IConditionalModel> conModels, KeyValuePair<string, JToken> va)
    {
        var key = va.Key;
        var fieldValue = va.Value.ToString();
        // in / not in
        if (va.Value is JArray)
        {
            conModels.Add(new ConditionalModel()
            {
                FieldName = key.TrimEnd('!'),
                ConditionalType = key.EndsWith("!") ? ConditionalType.NotIn : ConditionalType.In,
                FieldValue = va.Value.ToObject<string[]>().Aggregate((a, b) => a + "," + b)
            });
        }
        else
        {
            if (string.IsNullOrEmpty(fieldValue))
            {
                // is not null or ''
                if (key.EndsWith("!"))
                {
                    conModels.Add(new ConditionalModel() { FieldName = key.TrimEnd('!'), ConditionalType = ConditionalType.IsNot, FieldValue = null });
                    conModels.Add(new ConditionalModel() { FieldName = key.TrimEnd('!'), ConditionalType = ConditionalType.IsNot, FieldValue = "" });
                }
                //is null or ''
                else
                {
                    conModels.Add(new ConditionalModel() { FieldName = key.TrimEnd('!'), FieldValue = null });
                }
            }
            // = / !=
            else
            {
                conModels.Add(new ConditionalModel()
                {
                    FieldName = key.TrimEnd('!'),
                    ConditionalType = key.EndsWith("!") ? ConditionalType.NoEqual : ConditionalType.Equal,
                    FieldValue = fieldValue
                });
            }
        }
    }

    // 模糊搜索  "key$":"SQL搜索表达式" => "key$":["SQL搜索表达式"]，任意SQL搜索表达式字符串，如 %key%(包含key), key%(以key开始), %k%e%y%(包含字母k,e,y) 等，%表示任意字符
    private void FuzzyQuery(string subtable, List<IConditionalModel> conModels, KeyValuePair<string, JToken> va)
    {
        var vakey = va.Key.Trim();
        var fieldValue = va.Value.ToString();
        var conditionalType = ConditionalType.Like;
        if (IsCol(subtable, vakey.TrimEnd('$')))
        {
            // 支持三种like查询
            if (fieldValue.StartsWith("%") && fieldValue.EndsWith("%"))
            {
                conditionalType = ConditionalType.Like;
            }
            else if (fieldValue.StartsWith("%"))
            {
                conditionalType = ConditionalType.LikeRight;
            }
            else if (fieldValue.EndsWith("%"))
            {
                conditionalType = ConditionalType.LikeLeft;
            }
            conModels.Add(new ConditionalModel() { FieldName = vakey.TrimEnd('$'), ConditionalType = conditionalType, FieldValue = fieldValue.TrimEnd("%".ToArray()).TrimStart("%".ToArray()) });
        }
    }

    // 处理sql注入
    private string ReplaceSQLChar(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return string.Empty;

        str = str.Replace("'", "");
        str = str.Replace(";", "");
        str = str.Replace(",", "");
        str = str.Replace("?", "");
        str = str.Replace("<", "");
        str = str.Replace(">", "");
        str = str.Replace("(", "");
        str = str.Replace(")", "");
        str = str.Replace("@", "");
        str = str.Replace("=", "");
        str = str.Replace("+", "");
        str = str.Replace("*", "");
        str = str.Replace("&", "");
        str = str.Replace("#", "");
        str = str.Replace("%", "");
        str = str.Replace("$", "");
        str = str.Replace("\"", "");

        // 删除与数据库相关的词
        str = Regex.Replace(str, "delete from", "", RegexOptions.IgnoreCase);
        str = Regex.Replace(str, "drop table", "", RegexOptions.IgnoreCase);
        str = Regex.Replace(str, "truncate", "", RegexOptions.IgnoreCase);
        str = Regex.Replace(str, "xp_cmdshell", "", RegexOptions.IgnoreCase);
        str = Regex.Replace(str, "exec master", "", RegexOptions.IgnoreCase);
        str = Regex.Replace(str, "net localgroup administrators", "", RegexOptions.IgnoreCase);
        str = Regex.Replace(str, "net user", "", RegexOptions.IgnoreCase);
        str = Regex.Replace(str, "-", "", RegexOptions.IgnoreCase);
        str = Regex.Replace(str, "truncate", "", RegexOptions.IgnoreCase);
        return str;
    }

    /// <summary>
    /// 单条插入
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="cols"></param>
    /// <param name="role"></param>
    /// <returns>（各种类型的）id</returns>
    public object InsertSingle(string tableName, JObject cols, APIJSON_Role role = null)
    {
        role ??= _identitySvc.GetRole();
        var dt = new Dictionary<string, object>();

        foreach (var f in cols) // 遍历字段
        {
            if (//f.Key.ToLower() != "id" &&   //是否一定要传id
                IsCol(tableName, f.Key) &&
                (role.Insert.Column.Contains("*") || role.Insert.Column.Contains(f.Key, StringComparer.CurrentCultureIgnoreCase)))
                dt.Add(f.Key, FuncList.TransJObjectToSugarPara(f.Value));
        }
        // 如果外部没传Id，就后端生成或使用数据库默认值，如果都没有会出错
        object id;
        if (!dt.ContainsKey("id"))
        {
            id = YitIdHelper.NextId();//自己生成id的方法，可以由外部传入
            dt.Add("id", id);
        }
        else
        {
            id = dt["id"];
        }
        _db.Insertable(dt).AS(tableName).ExecuteCommand();//根据主键类型设置返回雪花或自增,目前返回条数

        return id;
    }

    /// <summary>
    /// 为每天记录创建udpate sql
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="record"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    public int UpdateSingleRecord(string tableName, JObject record, APIJSON_Role role = null)
    {
        role ??= _identitySvc.GetRole();
        if (!record.ContainsKey("id"))
            throw Oops.Bah("未传主键id");

        var dt = new Dictionary<string, object>();
        var sb = new StringBuilder(100);
        object id = null;
        foreach (var f in record)//遍历每个字段
        {
            if (f.Key.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                if (f.Value is JArray)
                {
                    sb.Append($"{f.Key} in (@{f.Key})");
                    id = FuncList.TransJArrayToSugarPara(f.Value);
                }
                else
                {
                    sb.Append($"{f.Key}=@{f.Key}");
                    id = FuncList.TransJObjectToSugarPara(f.Value);
                }
            }
            else if (IsCol(tableName, f.Key) && (role.Update.Column.Contains("*") || role.Update.Column.Contains(f.Key, StringComparer.CurrentCultureIgnoreCase)))
            {
                dt.Add(f.Key, FuncList.TransJObjectToSugarPara(f.Value));
            }
        }
        string whereSql = sb.ToString();
        int count = _db.Updateable(dt).AS(tableName).Where(whereSql, new { id }).ExecuteCommand();
        return count;
    }

    /// <summary>
    /// 更新单表，支持同表多条记录
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="records"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    public int UpdateSingleTable(string tableName, JToken records, APIJSON_Role role = null)
    {
        role ??= _identitySvc.GetRole();
        int count = 0;
        if (records is JArray)
        {
            foreach (var record in records.ToObject<JObject[]>())
            {
                count += UpdateSingleRecord(tableName, record, role);
            }
        }
        else
        {
            count = UpdateSingleRecord(tableName, records.ToObject<JObject>(), role);
        }
        return count;
    }
}