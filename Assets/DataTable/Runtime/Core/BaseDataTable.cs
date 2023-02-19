using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace DataTable
{
    public abstract class BaseDataTable<TTableData> where TTableData : BaseTableData, new()
    {
        //private variable
        private const string VECTOR_SPLIT_TAG = ":";

        private static CultureInfo _cultureInfo = CultureInfo.InvariantCulture;
        private Dictionary<string, TTableData> _dataTableMap;


        //public variable
        public bool IsInitialized => _dataTableMap is null;


        //public method
        public void Initialize(IReadOnlyList<IDataUnit> dataUnits)
        {
            Assert.IsNotNull(dataUnits, $"[{GetType().Name}::Initialize] SheetContent is null.");

            _dataTableMap = new Dictionary<string, TTableData>();
            Parser(dataUnits);
        }

        public void Release()
        {
            _dataTableMap.Clear();
            _dataTableMap = null;
        }

        public bool TryGetTableData(string key, out TTableData tableData)
        {
            var hasValue = _dataTableMap.TryGetValue(key, out tableData);
            return hasValue;
        }


        //private method
        private void AddTableData(string key, TTableData tableData)
        {
            if (_dataTableMap.ContainsKey(key))
            {
                Debug.Log($"[{GetType().Name}::AddTableData] Already add key: {key}.");
                return;
            }

            _dataTableMap.Add(key, tableData);
        }

        private void Parser(IReadOnlyList<IDataUnit> dataUnits)
        {
            var tableDataPropertyInfoMap = CreateTableDataPropertyInfoMap();

            foreach (var dataUnit in dataUnits)
            {
                var tableData = new TTableData();

                var key = dataUnit.Key;
                var dataValues = dataUnit.DataValues;

                foreach (var dataValue in dataValues)
                {
                    var name = dataValue.Name;
                    var hasTableDataPropertyInfo =
                        tableDataPropertyInfoMap.TryGetValue(name, out var tableDataPropertyInfo);

                    if (!hasTableDataPropertyInfo)
                    {
                        Debug.LogWarning($"[{GetType().Name}::Parser] TableData hasnt property: {name}.");
                        continue;
                    }

                    var value = dataValue.Value;
                    SetValue(tableDataPropertyInfo, value, tableData);
                }

                AddTableData(key, tableData);
            }
        }

        private Dictionary<string, PropertyInfo> CreateTableDataPropertyInfoMap()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            var propertyInfos = typeof(TTableData).GetProperties(bindingFlags);

            var propertyInfoMap = new Dictionary<string, PropertyInfo>();

            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    var name = propertyInfo.Name;
                    propertyInfoMap.Add(name, propertyInfo);
                }
            }

            return propertyInfoMap;
        }

        private void SetValue(PropertyInfo propertyInfo, string valueString, TTableData tableData)
        {
            var propertyType = propertyInfo.PropertyType;

            try
            {
                if (propertyType == typeof(int))
                {
                    var value = string.IsNullOrWhiteSpace(valueString)
                        ? 0
                        : int.Parse(valueString, _cultureInfo);

                    propertyInfo.SetValue(tableData, value);
                    return;
                }

                if (propertyType == typeof(float))
                {
                    var value = string.IsNullOrWhiteSpace(valueString)
                        ? 0
                        : float.Parse(valueString, _cultureInfo);

                    propertyInfo.SetValue(tableData, value);
                    return;
                }

                if (propertyType == typeof(string))
                {
                    var value = string.IsNullOrWhiteSpace(valueString)
                        ? string.Empty
                        : valueString;

                    propertyInfo.SetValue(tableData, value);
                    return;
                }

                if (propertyType == typeof(bool))
                {
                    // ReSharper disable once SimplifyConditionalTernaryExpression
                    var value = string.IsNullOrWhiteSpace(valueString)
                        ? false
                        : bool.Parse(valueString);

                    propertyInfo.SetValue(tableData, value);
                    return;
                }

                if (propertyType == typeof(long))
                {
                    var value = string.IsNullOrWhiteSpace(valueString)
                        ? 0
                        : long.Parse(valueString, _cultureInfo);

                    propertyInfo.SetValue(tableData, value);
                    return;
                }

                if (propertyType.IsEnum)
                {
                    if (string.IsNullOrWhiteSpace(valueString))
                    {
                        propertyInfo.SetValue(tableData, Enum.ToObject(propertyType, 0));
                        return;
                    }

                    if (valueString.All(char.IsDigit))
                    {
                        propertyInfo.SetValue(tableData, Enum.ToObject(propertyType, int.Parse(valueString)));
                        return;
                    }

                    propertyInfo.SetValue(tableData, Enum.Parse(propertyType, valueString));
                    return;
                }

                if (propertyType == typeof(Vector2))
                {
                    var value = Vector2.zero;
                    if (!string.IsNullOrWhiteSpace(valueString))
                    {
                        var valueStrings = valueString.Split(VECTOR_SPLIT_TAG);

                        if (valueString.Length != 2)
                        {
                            Debug.LogError(
                                $"[{GetType().Name}::SetValue] TableDataKey: {tableData.Key} TableData: {tableData.GetType().Name}, PropertyName: {propertyInfo.Name}, value: {valueString} - Vector2's valueString length isnt 2");
                            return;
                        }

                        var x = float.Parse(valueStrings[0], _cultureInfo);
                        var y = float.Parse(valueStrings[1], _cultureInfo);

                        value = new Vector2(x, y);
                    }

                    propertyInfo.SetValue(tableData, value);
                    return;
                }

                if (propertyType == typeof(Vector3))
                {
                    var value = Vector3.zero;
                    if (!string.IsNullOrWhiteSpace(valueString))
                    {
                        var valueStrings = valueString.Split(VECTOR_SPLIT_TAG);

                        if (valueString.Length != 3)
                        {
                            Debug.LogError(
                                $"[{GetType().Name}::SetValue] TableDataKey: {tableData.Key} TableData: {tableData.GetType().Name}, PropertyName: {propertyInfo.Name}, value: {valueString} - Vector3's valueString length isnt 3");
                            return;
                        }

                        var x = float.Parse(valueStrings[0], _cultureInfo);
                        var y = float.Parse(valueStrings[1], _cultureInfo);
                        var z = float.Parse(valueStrings[2], _cultureInfo);

                        value = new Vector3(x, y, z);
                    }

                    propertyInfo.SetValue(tableData, value);
                    return;
                }
            }
            catch
            {
                Debug.LogError(
                    $"[{GetType().Name}::SetValue] TableDataKey: {tableData.Key} TableData: {tableData.GetType().Name}, PropertyName: {propertyInfo.Name}, value: {valueString}");
            }
        }
    }
}