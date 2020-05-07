using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BE.Models;

namespace BE.Util
{
    /// <summary>
    /// Helper Class
    /// </summary>
    public static class Helper
    {
        private static int FindEndStringToken(string st, int start)
        {
            for (var i = start + 1; i < st.Length; i++)
            {
                if (st[i] == '"' && st[i - 1] != '\\')
                {
                    return i;
                }
            }
            return -1;
        }
        private static int FindEndToken(string st, int start, char open, char close)
        {
            int count = 0;
            bool isInString = false;
            for (var i = start; i < st.Length; i++)
            {
                if (!isInString && st[i] == '"')
                {
                    isInString = true;
                }
                else if (isInString && st[i] == '"' && st[i - 1] != '\\')
                {
                    isInString = false;
                }
                else if (!isInString)
                {
                    if (st[i] == open)
                    {
                        count++;
                    }
                    else if (st[i] == close)
                    {
                        count--;
                        if (count < 0) return -1;
                        if (count == 0) return i;
                    }
                }
            }
            return -1;
        }
        private static List<string> ParseList(string st)
        {
            var list = new List<string>();
            int index = 0;
            int start = 0;
            while (index < st.Length)
            {
                if (st[index] == ',')
                {
                    list.Add(st.Substring(start, index - start));
                    start = index + 1;
                }
                else if (st[index] == '[')
                {
                    index = FindEndToken(st, index, '[', ']');
                }
                else if (st[index] == '{')
                {
                    index = FindEndToken(st, index, '{', '}');
                }
                index++;
            }
            if (start < index)
            {
                list.Add(st.Substring(start, index - start));
            }
            return list;
        }
        private static Dictionary<string, string> ParseObject(string st, bool lowerKey = false)
        {
            var list = new Dictionary<string, string>();
            int index = 0;
            int start = 0;
            while (index < st.Length)
            {
                if (st[index] == '"')
                {
                    string key = "";
                    string value = "";
                    start = index;
                    do
                    {
                        index++;
                        if (index < st.Length && st[index] == '"')
                        {
                            key = st.Substring(start + 1, index - 1 - start);
                            start = index + 1;
                            break;
                        }
                    }
                    while (index < st.Length);
                    do
                    {
                        index++;
                        if (index < st.Length && st[index] == ':')
                        {
                            index++;
                            start = index;
                            break;
                        }
                    } while (index < st.Length);
                    while (index < st.Length)
                    {
                        if (st[index] == '"')
                        {
                            index = FindEndStringToken(st, index);
                            value = st.Substring(start, index - start + 1);
                            start = index + 1;
                            break;
                        }
                        else if (st[index] == '{')
                        {
                            index = FindEndToken(st, index, '{', '}');
                            value = st.Substring(start, index - start + 1);
                            start = index + 1;
                            break;
                        }
                        else if (st[index] == '[')
                        {
                            index = FindEndToken(st, index, '[', ']');
                            value = st.Substring(start, index - start + 1);
                            start = index + 1;
                            break;
                        }
                        else if (!(char.IsWhiteSpace(st[index]) || char.IsControl(st[index])))
                        {
                            start = index;
                            while (index < st.Length && st[index] != ',' && !(char.IsWhiteSpace(st[index]) || char.IsControl(st[index])))
                            {
                                index++;
                            }
                            value = st.Substring(start, index - start);
                            start = index;
                            break;
                        }
                        index++;
                    }
                    if (key.Length > 0 && value.Length > 0)
                    {
                        if (lowerKey)
                        {
                            key = key.ToLower();
                        }
                        list.Add(key, value);
                    }
                }
                index++;
            }
            return list;
        }
        private static object GetObjectValue(string json, Type type)
        {
            object result;
            if (json[0] != '{' || json[json.Length - 1] != '}')
            {
                result = null;
            }
            else
            {
                var value = Activator.CreateInstance(type);
                if (json.Length > 2)
                {
                    var listToken = ParseObject(json.Substring(1, json.Length - 2), true);
                    var properties = type.GetProperties();
                    var jsonNameType = typeof(JsonNameAttribute);
                    var attributeNotMappedType = typeof(NotMappedAttribute);
                    foreach (var p in properties)
                    {
                        var notMap = p.GetCustomAttribute(attributeNotMappedType);
                        if (notMap != null)
                        {
                            continue;
                        }
                        var attr = p.GetCustomAttribute(jsonNameType);
                        string key = p.Name.ToLower();
                        if (attr != null)
                        {
                            key = ((JsonNameAttribute)attr).Name.ToLower();
                        }
                        if (listToken.ContainsKey(key))
                        {
                            object pvalue = GetValue(listToken[key], p.PropertyType);
                            p.SetValue(value, pvalue);
                        }
                    }
                    var fields = type.GetFields();
                    foreach (var field in fields)
                    {
                        if (!field.IsPrivate)
                        {
                            var notMap = field.GetCustomAttribute(attributeNotMappedType);
                            if (notMap != null)
                            {
                                continue;
                            }
                            var attr = field.GetCustomAttribute(jsonNameType);
                            string key = field.Name.ToLower();
                            if (attr != null)
                            {
                                key = ((JsonNameAttribute)attr).Name.ToLower();
                            }
                            if (listToken.ContainsKey(key))
                            {
                                object pvalue = GetValue(listToken[key], field.FieldType);
                                field.SetValue(value, pvalue);
                            }
                        }
                    }
                }
                result = value;
            }
            return result;
        }
        private static object GetValue(string json, Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            object result = null;
            if (type.IsEnum)
            {
                result = int.Parse(json);
            }
            else
            {
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        bool vBool;
                        bool.TryParse(json, out vBool);
                        result = vBool;
                        break;
                    case TypeCode.Byte:
                        Byte vByte;
                        byte.TryParse(json, out vByte);
                        result = vByte;
                        break;
                    case TypeCode.SByte:
                        SByte vsByte;
                        sbyte.TryParse(json, out vsByte);
                        result = vsByte;
                        break;
                    case TypeCode.UInt16:
                        UInt16 vUInt16;
                        UInt16.TryParse(json, out vUInt16);
                        result = vUInt16;
                        break;
                    case TypeCode.UInt32:
                        UInt32 vUInt32;
                        UInt32.TryParse(json, out vUInt32);
                        result = vUInt32;
                        break;
                    case TypeCode.UInt64:
                        UInt64 vUInt64;
                        UInt64.TryParse(json, out vUInt64);
                        result = vUInt64;
                        break;
                    case TypeCode.Int16:
                        Int16 vInt16;
                        Int16.TryParse(json, out vInt16);
                        result = vInt16;
                        break;
                    case TypeCode.Int32:
                        Int32 vInt32;
                        Int32.TryParse(json, out vInt32);
                        result = vInt32;
                        break;
                    case TypeCode.Int64:
                        Int64 vInt64;
                        Int64.TryParse(json, out vInt64);
                        result = vInt64;
                        break;
                    case TypeCode.Decimal:
                        Decimal vDecimal;
                        Decimal.TryParse(json, out vDecimal);
                        result = vDecimal;
                        break;
                    case TypeCode.Single:
                        Single vSingle;
                        Single.TryParse(json, out vSingle);
                        result = vSingle;
                        break;
                    case TypeCode.Double:
                        Double vDouble;
                        Double.TryParse(json, out vDouble);
                        result = vDouble;
                        break;
                    case TypeCode.Char:
                        Char vChar;
                        Char.TryParse(json, out vChar);
                        result = vChar;
                        break;
                    case TypeCode.String:
                        if (json == "null")
                        {
                            result = null;
                        }
                        else
                        {
                            if (json.Length > 0 && json[0] == '"')
                            {
                                json = json.Substring(1);
                            }
                            if (json.Length > 0 && json[json.Length - 1] == '"')
                            {
                                json = json.Substring(0, json.Length - 1);
                            }
                            result = json;
                        }
                        break;
                    case TypeCode.DateTime:
                        DateTime vDate;
                        DateTime.TryParse(json, out vDate);
                        result = vDate;
                        break;
                    default:
                        json = json.Trim();
                        if (type.IsArray)
                        {
                            if (json[0] != '[' || json[json.Length - 1] != ']')
                            {
                                result = null;
                            }
                            else
                            {
                                var elementType = type.GetElementType();
                                if (json.Length > 2)
                                {
                                    var listToken = ParseList(json.Substring(1, json.Length - 2));
                                    var list = Array.CreateInstance(elementType, listToken.Count);
                                    for (var indexE = 0; indexE < listToken.Count; indexE++)
                                    {
                                        list.SetValue(GetValue(listToken[indexE], elementType), indexE);
                                    }
                                    result = list;
                                }
                                else
                                {
                                    result = Array.CreateInstance(elementType, 0);
                                }
                            }
                        }
                        else if (type.IsGenericType)
                        {
                            if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                            {
                                if (json[0] != '{' || json[json.Length - 1] != '}')
                                {
                                    result = null;
                                }
                                else
                                {
                                    Type keyType = type.GetGenericArguments()[0];
                                    if (keyType == typeof(string))
                                    {
                                        Type valueType = type.GetGenericArguments()[1];
                                        var dic = (IDictionary)Activator.CreateInstance(type);
                                        if (json.Length > 2)
                                        {
                                            var listToken = ParseObject(json.Substring(1, json.Length - 2));
                                            foreach (var key in listToken.Keys)
                                            {
                                                dic.Add(key, GetValue(listToken[key], valueType));
                                            }
                                        }
                                        result = dic;
                                    }
                                }
                            }
                            else if (type.GetGenericTypeDefinition() == typeof(List<>))
                            {
                                var instance = (IList)Activator.CreateInstance(type);
                                if (json.Length > 2)
                                {
                                    Type valueType = type.GetGenericArguments()[0];
                                    var listToken = ParseList(json.Substring(1, json.Length - 2));
                                    for (var indexE = 0; indexE < listToken.Count; indexE++)
                                    {
                                        instance.Add(GetValue(listToken[indexE], valueType));
                                    }
                                }
                                result = instance;
                            }
                            else
                            {
                                result = GetObjectValue(json, type);
                            }
                        }
                        else if (type.IsClass)
                        {
                            result = GetObjectValue(json, type);
                        }
                        break;
                }
            }
            return result;
        }
        public static T ToObject<T>(this string json)
        {
            var type = typeof(T);
            object value = GetValue(json, type);
            return (T)Convert.ChangeType(value, type);
        }
        private static void WriteObject(Type type, object value, StringBuilder builder)
        {
            builder.Append("{");
            List<string> list = new List<string>();
            List<string> listFields = new List<string>();
            var jsonNameType = typeof(JsonNameAttribute);
            var notMappedType = typeof(NotMappedAttribute);
            var properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                var notMap = properties[i].GetCustomAttribute(notMappedType);
                if (notMap != null)
                {
                    continue;
                }
                var propertyValue = properties[i].GetValue(value);
                if (propertyValue != null)
                {
                    var arrayName = properties[i].Name.ToCharArray();
                    var attr = properties[i].GetCustomAttribute(jsonNameType);
                    if (attr != null)
                    {
                        arrayName = ((JsonNameAttribute)attr).Name.ToCharArray();
                    }
                    arrayName[0] = arrayName[0].ToString().ToLower()[0];
                    string propertyName = new string(arrayName);
                    list.Add("\"" + propertyName + "\":" + propertyValue.ToJson());
                    listFields.Add(propertyName.ToLower());
                }
            }
            var fields = type.GetFields(BindingFlags.Public);
            for (int i = 0; i < fields.Length; i++)
            {
                if (!fields[i].IsPrivate && !listFields.Contains(fields[i].Name.ToLower()))
                {
                    var notMap = fields[i].GetCustomAttribute(notMappedType);
                    if (notMap != null)
                    {
                        continue;
                    }
                    var propertyValue = fields[i].GetValue(value);
                    if (propertyValue != null)
                    {
                        var arrayName = fields[i].Name.ToCharArray();
                        var attr = fields[i].GetCustomAttribute(jsonNameType);
                        if (attr != null)
                        {
                            arrayName = ((JsonNameAttribute)attr).Name.ToCharArray();
                        }
                        arrayName[0] = arrayName[0].ToString().ToLower()[0];
                        string propertyName = new string(arrayName);
                        list.Add("\"" + propertyName + "\":" + propertyValue.ToJson());
                    }
                }
            }
            builder.Append(string.Join(",", list.ToArray()));
            builder.Append("}");
        }
        internal static void DefaultIncludeFields<T>(RequestData<T> request) where T : BEModel
        {
            if (request.Filters != null && request.Filters.Count > 0)
            {
                for (var i = 0; i < request.Filters.Count; i++)
                {
                    if (!request.Filters[i].StartsWith("*"))
                    {
                        //User sepecify field
                        return;
                    }
                }
                List<string> listFields = new List<string>();
                Type type = typeof(T);
                var jsonNameType = typeof(JsonNameAttribute);
                var notMappedType = typeof(NotMappedAttribute);
                var properties = type.GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    if (Type.GetTypeCode(properties[i].PropertyType) == TypeCode.Object) continue;
                    var notMap = properties[i].GetCustomAttribute(notMappedType);
                    if (notMap != null)
                    {
                        continue;
                    }
                    var arrayName = properties[i].Name;
                    var attr = properties[i].GetCustomAttribute(jsonNameType);
                    if (attr != null)
                    {
                        arrayName = ((JsonNameAttribute)attr).Name;
                    }
                    request.GetFields(arrayName);
                }
                var fields = type.GetFields(BindingFlags.Public);
                for (int i = 0; i < fields.Length; i++)
                {
                    if (Type.GetTypeCode(fields[i].FieldType) == TypeCode.Object) continue;
                    if (!fields[i].IsPrivate && !listFields.Contains(fields[i].Name.ToLower()))
                    {
                        var notMap = fields[i].GetCustomAttribute(notMappedType);
                        if (notMap != null)
                        {
                            continue;
                        }
                        var arrayName = fields[i].Name;
                        var attr = fields[i].GetCustomAttribute(jsonNameType);
                        if (attr != null)
                        {
                            arrayName = ((JsonNameAttribute)attr).Name;
                        }
                        request.GetFields(arrayName);
                    }
                }
            }
        }
        public static string ToJson(this object value)
        {
            if (value == null) return "";
            var type = value.GetType();
            var typeCode = Type.GetTypeCode(type);
            if (type.IsEnum)
            {
                return ((int)value).ToString();
            }
            switch (typeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Char:
                    return value.ToString();
                case TypeCode.String:
                    return "\"" + value.ToString().Replace("\"", "\\\"") + "\"";
                case TypeCode.DateTime:
                    return "\"" + ((DateTime)value).ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff") + "\"";
                default:
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    if (type.IsEnum)
                    {
                        var vl = (int)Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
                        builder.Append(vl.ToString());
                    }
                    else if (type.IsArray)
                    {
                        builder.Append("[");
                        System.Array array = (System.Array)value;
                        List<string> list = new List<string>();
                        for (int i = 0; i < array.Length; i++)
                        {
                            list.Add(array.GetValue(i).ToJson());
                        }
                        builder.Append(string.Join(",", list.ToArray()));
                        builder.Append("]");
                    }
                    else if (type.IsGenericType)
                    {
                        if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        {
                            Type keyType = type.GetGenericArguments()[0];
                            if (keyType == typeof(string))
                            {
                                IDictionary dict = value as IDictionary;
                                builder.Append("{");
                                List<string> list = new List<string>();
                                foreach (var key in dict.Keys)
                                {
                                    var v = dict[key];
                                    if (v != null)
                                    {
                                        var arrayName = key.ToString().ToCharArray();
                                        arrayName[0] = arrayName[0].ToString().ToLower()[0];
                                        list.Add("\"" + new string(arrayName) + "\":" + v.ToJson());
                                    }

                                }
                                builder.Append(string.Join(",", list.ToArray()));
                                builder.Append("}");
                            }
                        }
                        else if (type.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            IList listValue = value as IList;
                            builder.Append("[");
                            List<string> list = new List<string>();
                            foreach (var o in listValue)
                            {
                                list.Add(o.ToJson());
                            }
                            builder.Append(string.Join(",", list.ToArray()));
                            builder.Append("]");
                        }
                        else
                        {
                            WriteObject(type, value, builder);
                        }
                    }
                    else if (type.IsClass)
                    {

                        WriteObject(type, value, builder);
                    }
                    return builder.ToString();
            }
        }
        /// <summary>
        /// Convert list condition to a json string
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public static void MakeWhereRequest(Type type, object o, RequestData request,bool idOnly=true)
        {
            var attributeType = typeof(ColumnAttribute);
            List<string> listKeys = new List<string>();
            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                var typeCode = Type.GetTypeCode(p.PropertyType);
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Decimal:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Char:
                    case TypeCode.String:
                    case TypeCode.DateTime:
                        var attr = p.GetCustomAttribute(attributeType);
                        var name = p.Name;
                        if (attr != null)
                        {
                            name = ((ColumnAttribute)attr).Name;
                        }
                        if (name.ToLower() == "id"||!idOnly)
                        {
                            listKeys.Add(name.ToLower());
                            object value = p.GetValue(o);
                            if (value != null)
                            {
                                request.Where(x => x[name].Equals(value));
                            }
                        }
                        break;
                }
            }
            var fields = type.GetFields();
            foreach (var p in fields)
            {
                if (!p.IsPrivate)
                {
                    var typeCode = Type.GetTypeCode(p.FieldType);
                    switch (typeCode)
                    {
                        case TypeCode.Boolean:
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Char:
                        case TypeCode.String:
                        case TypeCode.DateTime:
                            var attr = p.GetCustomAttribute(attributeType);
                            var name = p.Name;
                            if (attr != null)
                            {
                                name = ((ColumnAttribute)attr).Name;
                            }
                            if (name.ToLower() == "id" || !idOnly)
                            {
                                listKeys.Add(name.ToLower());
                                object value = p.GetValue(o);
                                request.Where(x => x[name].Equals(value));
                            }
                            break;
                    }
                }
            }
        }
        public static void SetValueRequest(Type type, object o, RequestData request, bool ignoreId = false)
        {
            var attributeType = typeof(ColumnAttribute);
            var attributeNotMappedType = typeof(NotMappedAttribute);
            List<string> listKeys = new List<string>();
            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                var typeCode = Type.GetTypeCode(p.PropertyType);
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Decimal:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Char:
                    case TypeCode.String:
                    case TypeCode.DateTime:
                        var notMap = p.GetCustomAttribute(attributeNotMappedType);
                        if (notMap != null)
                        {
                            continue;
                        }
                        var attr = p.GetCustomAttribute(attributeType);
                        var name = p.Name;
                        if (attr != null)
                        {
                            name = ((ColumnAttribute)attr).Name;
                        }
                        if (ignoreId && name.ToLower() == "id")
                        {
                            continue;
                        }
                        listKeys.Add(name.ToLower());
                        object value = p.GetValue(o);
                        request.SetValue(name, value);
                        break;
                }
            }
            var fields = type.GetFields();
            foreach (var p in fields)
            {
                if (!p.IsPrivate)
                {
                    var typeCode = Type.GetTypeCode(p.FieldType);
                    switch (typeCode)
                    {
                        case TypeCode.Boolean:
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Char:
                        case TypeCode.String:
                        case TypeCode.DateTime:
                            var notMap = p.GetCustomAttribute(attributeNotMappedType);
                            if (notMap != null)
                            {
                                continue;
                            }
                            var attr = p.GetCustomAttribute(attributeType);
                            var name = p.Name;
                            if (attr != null)
                            {
                                name = ((ColumnAttribute)attr).Name;
                            }
                            if (ignoreId && name.ToLower() == "id")
                            {
                                continue;
                            }
                            listKeys.Add(name.ToLower());
                            object value = p.GetValue(o);
                            request.SetValue(name, value);
                            break;
                    }
                }
            }
        }
        public static string GetRequestString(string query, string schema, RequestData requestData = null)
        {
            string json = "{\"query\":\"" + query + "\",\"schema\":\"" + schema + "\"" + (requestData != null ? ",\"data\":" + requestData.ToJson() : "");
            json += "}";
            return json;
        }


    }
    public class JsonNameAttribute : Attribute
    {
        private string _Name;
        public JsonNameAttribute(string name)
        {
            _Name = name;
        }
        public string Name { get { return _Name; } }
    }
}

public class ColumnAttribute : Attribute
{
    private string _Name;
    public ColumnAttribute(string name)
    {
        _Name = name;
    }
    public string Name { get { return _Name; } }
}
public class NotMappedAttribute : Attribute
{
    public NotMappedAttribute()
    {
    }
}
public class SchemaAttribute : Attribute
{
    private string _Name;
    public SchemaAttribute(string name)
    {
        _Name = name;
    }
    public string Name { get { return _Name; } }
}