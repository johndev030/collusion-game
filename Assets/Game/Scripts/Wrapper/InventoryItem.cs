using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem 
{
}
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Inventory
{
    public string ItemId { get; set; }
    public string ItemInstanceId { get; set; }
    public string ItemClass { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string Annotation { get; set; }
    public string CatalogVersion { get; set; }
    public string DisplayName { get; set; }
    public int UnitPrice { get; set; }
}

public class Root
{
    public string PlayFabId { get; set; }
    public List<Inventory> Inventory { get; set; }
    public VirtualCurrency VirtualCurrency { get; set; }
    public VirtualCurrencyRechargeTimes VirtualCurrencyRechargeTimes { get; set; }
}

public class VirtualCurrency
{
    public int CL { get; set; }
}

public class VirtualCurrencyRechargeTimes
{
}
