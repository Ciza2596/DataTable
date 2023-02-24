using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace GoogleSpreadsheetLoader.Editor
{
    [Serializable]
    public class SpreadsheetContentInfo
    {
        //private variable
        [LabelText("Sheet Content存放路徑(建立物件時使用)")] [ReadOnly] [SerializeField]
        private string _sheetContentPath;

        [TableList(IsReadOnly = true)] [SerializeField]
        private List<SheetContentInfo> _sheetContentInfos = new List<SheetContentInfo>();

        private readonly string _spreadsheetInfoId;


        //public variable
        public string SheetContentPath => _sheetContentPath;

        public SheetContentInfo[] SheetContentInfos => _sheetContentInfos.ToArray();

        public string SpreadsheetInfoId => _spreadsheetInfoId;


        //constructor
        public SpreadsheetContentInfo(string spreadsheetInfoId) =>
            _spreadsheetInfoId = spreadsheetInfoId;


        //public method
        public void RemoveAll()
        {
            var subSheetContentInfos = SheetContentInfos;

            foreach (var subSheetContentInfo in subSheetContentInfos)
            {
                _sheetContentInfos.Remove(subSheetContentInfo);
                subSheetContentInfo.Remove();
            }
        }


        public void SetSheetContentPath(string sheetContentPath) => _sheetContentPath = sheetContentPath;

        public SheetContentInfo FindSheetContentInfo(string sheetInfoId)
        {
            var sheetContentInfo =
                _sheetContentInfos.Find(sheetContentInfo => sheetContentInfo.SheetInfoId == sheetInfoId);
            return sheetContentInfo;
        }

        public SheetContentInfo CreateSheetContentInfo(string sheetInfoId, string spreadSheetId, string sheetId,
            string spreadSheetName,
            GoogleSpreadsheetLoader googleSpreadsheetLoader)
        {
            var sheetContent = CreateScriptableObjectToAssets<SheetContent>(spreadSheetName);

            var sheetContentInfo = new SheetContentInfo(sheetInfoId, spreadSheetId, sheetId, sheetContent,
                googleSpreadsheetLoader);
            _sheetContentInfos.Add(sheetContentInfo);
            return sheetContentInfo;
        }


        //private method
        private T CreateScriptableObjectToAssets<T>(string spreadSheetName) where T : ScriptableObject
        {
            var fileName = $"{Guid.NewGuid().ToString()}.asset";

            var fullPath = _sheetContentPath + '/' + spreadSheetName + fileName;

            if (!Directory.Exists(_sheetContentPath))
                Directory.CreateDirectory(_sheetContentPath);

            var scriptableObject = ScriptableObject.CreateInstance(typeof(T));

            AssetDatabase.CreateAsset(scriptableObject, fullPath);
            AssetDatabase.SaveAssets();
            return scriptableObject as T;
        }
    }
}