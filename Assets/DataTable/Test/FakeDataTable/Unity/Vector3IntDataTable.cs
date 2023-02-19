using DataTable;
using UnityEngine;

public class Vector3IntDataTable: BaseDataTable<Vector3IntTableData>
{
}

public class Vector3IntTableData : BaseTableData
{
    public Vector3Int Value { get; private set; }
}