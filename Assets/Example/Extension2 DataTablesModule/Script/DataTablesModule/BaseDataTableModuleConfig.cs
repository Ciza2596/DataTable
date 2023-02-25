using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GoogleSpreadsheetLoader;

namespace DataTable
{
    public abstract class BaseDataTableModuleConfig : IDataTableModuleConfig
    {
        private AddressablesModule.AddressablesModule _addressablesModule;
        private Dictionary<Type, object> _dataTables;

        private List<UniTask> _installTasks = new List<UniTask>();
        private List<string> _dataTableNames = new List<string>();

        protected BaseDataTableModuleConfig(AddressablesModule.AddressablesModule addressablesModule) =>
            _addressablesModule = addressablesModule;


        public async void Install(Dictionary<Type, object> dataTables)
        {
            _dataTables = dataTables;

            await ExecuteInstallTasks();
            ReleaseInstallTasks();
            
            ReleaseSheetContents();
        }

        protected void AddDataTable<TTableData>(BaseDataTable<TTableData> dataTable) where TTableData : BaseTableData =>
            _installTasks.Add(InitializeDataTable(dataTable));
        
        
        private async UniTask ExecuteInstallTasks()
        {
            var installTasks = _installTasks.ToArray();
            await UniTask.WhenAll(installTasks);
        }
        
        private void ReleaseInstallTasks()
        {
            _installTasks.Clear();
            _installTasks = null;
        }

        private async UniTask InitializeDataTable<TTableData>(BaseDataTable<TTableData> dataTable)
            where TTableData : BaseTableData
        {
            var dataTableName = dataTable.Name;
            _dataTableNames.Add(dataTableName);
            
            var sheetContent = await _addressablesModule.GetAssetAsync<SheetContent>(dataTableName);
            var dataUnits = sheetContent.DataUnits.ToArray();
            dataTable.Initialize(dataUnits);

            _dataTables.Add(dataTable.GetType(), dataTable);
        }

        private void ReleaseSheetContents()
        {
            var dataTableNames = _dataTableNames.ToArray();
            _addressablesModule.ReleaseAssets(dataTableNames);

            _dataTableNames.Clear();
            _dataTableNames = null;
        }
    }
}